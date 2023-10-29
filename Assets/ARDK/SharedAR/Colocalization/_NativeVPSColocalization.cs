// Copyright 2022 Niantic, Inc. All Rights Reserved.

#pragma warning disable 0067
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;

using AOT;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.WayspotAnchors;
using Niantic.ARDK.Internals;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Logging;

using UnityEngine;

namespace Niantic.Experimental.ARDK.SharedAR
{
  public enum VPSColocalizerFailureCode {
    None = 0,
    Unknown = 1,
    VPSDependencyMissing = 2,
    DatastoreDependencyMissing = 3,
    VPSDependencyFailure = 4
  }

  public struct VPSColocalizerFailureReason: IArdkEventArgs {
    public VPSColocalizerFailureReason(VPSColocalizerFailureCode colocError,
                                        LocalizationFailureReason localizationError):
      this()
    {
      ColocalizationError = colocError;
      VPSError = localizationError;
    }
    public VPSColocalizerFailureCode ColocalizationError;
    public LocalizationFailureReason VPSError;
  }

  internal interface INARVpsColocalization {
    delegate void NARVpsColocalization_FailureReasonCallback
    (
      IntPtr context,
      byte vpsColocErrorCode,
      byte optionalVpsErrorCode
    );

    IntPtr NARVpsColocalization_Init(IntPtr applicationHandle, byte[] stageId, IntPtr networkingHandle);

    IntPtr NARVpsColocalization_InitWithNode
    (
      IntPtr applicationHandle,
      byte[] stageId,
      IntPtr networkingHandle,
      byte[] data,
      ulong dataSize
    );

    void NARVpsColocalization_SetFailureReasonCallback
    (
      IntPtr applicationHandle,
      IntPtr nativeHandle,
      NARVpsColocalization_FailureReasonCallback callback
    );
  }

  internal class NativeNARVpsColocalization :
    INARVpsColocalization
  {
    public IntPtr NARVpsColocalization_Init
    (
      IntPtr applicationHandle,
      byte[] stageId,
      IntPtr networkingHandle
    )
    {
      return _NARVpsColocalizer_Init(applicationHandle, stageId, networkingHandle);
    }

    public IntPtr NARVpsColocalization_InitWithNode
    (
      IntPtr applicationHandle,
      byte[] stageId,
      IntPtr networkingHandle,
      byte[] data,
      ulong dataSize
    )
    {
      return _NARVpsColocalizer_InitWithNode(applicationHandle, stageId, networkingHandle, data, dataSize);
    }

    public void NARVpsColocalization_SetFailureReasonCallback
    (
      IntPtr applicationHandle,
      IntPtr nativeHandle,
      INARVpsColocalization.NARVpsColocalization_FailureReasonCallback callback
    )
    {
      _NARVpsColocalizer_SetFailureReasonCallback(applicationHandle, nativeHandle, callback);
    }

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARVpsColocalizer_Init
    (
      IntPtr applicationHandle,
      byte[] stageId,
      IntPtr networkingHandle
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARVpsColocalizer_InitWithNode
    (
      IntPtr applicationHandle,
      byte[] stageId,
      IntPtr networkingHandle,
      byte[] data,
      ulong dataSize
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVpsColocalizer_SetFailureReasonCallback
    (
      IntPtr applicationHandle,
      IntPtr nativeHandle,
      INARVpsColocalization.NARVpsColocalization_FailureReasonCallback callback
    );
  }

  /// @note This is an experimental feature. Experimental features should not be used in
  /// production products as they are subject to breaking changes, not officially supported, and
  /// may be deprecated without notice
  public class _NativeVPSColocalization :
    IColocalization
  {
    private bool _isDestroyed;
    private INARVpsColocalization _nativeAPI;
    private readonly float[] _reusableMatrixForMarshalling;

    internal readonly INetworking Networking;

    public _NativeVPSColocalization
    (
      INetworking networking,
      IARSession arSession,
      WayspotAnchorPayload node = null
    ) : this(networking, arSession, node, new NativeNARVpsColocalization()) {}

    internal _NativeVPSColocalization
    (
      INetworking networking,
      IARSession arSession,
      WayspotAnchorPayload node,
      INARVpsColocalization nativeAPI
    )
    {
      _nativeAPI = nativeAPI;
      Networking = networking;

      if (Networking is INativeNetworking nativeNeworking)
      {
        _nativeHandle = (node != null)
          ? _nativeAPI.NARVpsColocalization_InitWithNode
          (
            _applicationHandle,
            arSession.StageIdentifier.ToByteArray(),
            nativeNeworking.GetNativeHandle(),
            node._Blob,
            (ulong)node._Blob.Length
          )
          : _nativeAPI.NARVpsColocalization_Init
          (
            _applicationHandle,
            arSession.StageIdentifier.ToByteArray(),
            nativeNeworking.GetNativeHandle()
          );

        GC.AddMemoryPressure(GCPressure);
        SubscribeToNativeCallbacks();
      }
      else
      {
        ARLog._Error("Can only use NativeVPSColocalization with NativeNetworking for now");
      }

      _reusableMatrixForMarshalling = new float[16];
    }

    public void Start()
    {
      _NARVpsColocalizer_StartColocalization(_nativeHandle);
    }

    public void Stop()
    {
      _NARVpsColocalizer_PauseColocalization(_nativeHandle);
    }

    private readonly Dictionary<IPeerID, ColocalizationState> _colocalizationStates;
    public ReadOnlyDictionary<IPeerID, ColocalizationState> ColocalizationStates { get; }

    public Matrix4x4 AlignedSpaceOrigin
    {
      get
      {
        lock (_reusableMatrixForMarshalling)
        {
          _NARVpsColocalizer_GetAlignedSpaceOrigin(_nativeHandle, _reusableMatrixForMarshalling);

          return NARConversions.FromNARToUnity(_Convert.InternalToMatrix4x4(_reusableMatrixForMarshalling));
        }
      }
    }

    public event ArdkEventHandler<ColocalizationStateUpdatedArgs> ColocalizationStateUpdated;

    public event ArdkEventHandler<VPSColocalizerFailureReason> ColocalizationFailure;

    public void LocalPoseToAligned(Matrix4x4 poseInLocalSpace, out Matrix4x4 poseInAlignedSpace)
    {
      var poseArray = _Convert.Matrix4x4ToInternalArray
        (NARConversions.FromUnityToNAR(poseInLocalSpace));

      lock (_reusableMatrixForMarshalling)
      {
        _NARVpsColocalizer_LocalPoseToAligned(_nativeHandle, poseArray, _reusableMatrixForMarshalling);
        poseInAlignedSpace = NARConversions.FromNARToUnity(_Convert.InternalToMatrix4x4(_reusableMatrixForMarshalling));
      }
    }

    public ColocalizationAlignmentResult AlignedPoseToLocal(IPeerID id, Matrix4x4 poseInAlignedSpace, out Matrix4x4 poseInLocalSpace)
    {
      var poseArray = _Convert.Matrix4x4ToInternalArray
        (NARConversions.FromUnityToNAR(poseInAlignedSpace));

      var peerGuid = id.Identifier.ToByteArray();

      lock (_reusableMatrixForMarshalling)
      {
        var capiResult = _NARVpsColocalizer_AlignedPoseToLocal(_nativeHandle, peerGuid, poseArray, _reusableMatrixForMarshalling);
        var result = (ColocalizationAlignmentResult)capiResult;

        poseInLocalSpace = (result == ColocalizationAlignmentResult.Success) ?
          NARConversions.FromNARToUnity(_Convert.InternalToMatrix4x4(_reusableMatrixForMarshalling)) :
          Matrix4x4.identity;

        return result;
      }
    }

    public void Dispose()
    {
      if (_nativeHandle != IntPtr.Zero)
      {
        _NARVpsColocalizer_Release(_nativeHandle);
        GC.RemoveMemoryPressure(GCPressure);
        _nativeHandle = IntPtr.Zero;
        _isDestroyed = true;
      }
      _cachedHandle.Free();
    }

    #region Handles
    // Below here are private fields and methods to handle native code and callbacks

    // The pointer to the C++ object handling functionality at the native level
    private IntPtr _nativeHandle;

    private IntPtr _cachedHandleIntPtr = IntPtr.Zero;
    private SafeGCHandle<_NativeVPSColocalization> _cachedHandle;

    // Approx memory size of native object
    // Magic number for 64KB
    private const long GCPressure = 64L * 1024L;

    // Used to round-trip a pointer through c++,
    // so that we can keep our this pointer even in c# functions
    // marshaled and called by native code
    private IntPtr _applicationHandle
    {
      get
      {
        if (_cachedHandleIntPtr != IntPtr.Zero)
          return _cachedHandleIntPtr;

        lock (this)
        {
          if (_cachedHandleIntPtr != IntPtr.Zero)
            return _cachedHandleIntPtr;

          // https://msdn.microsoft.com/en-us/library/system.runtime.interopservices.gchandle.tointptr.aspx
          _cachedHandle = SafeGCHandle.Alloc(this);
          _cachedHandleIntPtr = _cachedHandle.ToIntPtr();
        }

        return _cachedHandleIntPtr;
      }
    }
#endregion
    private bool _didSubscribeToNativeEvents;

    private void SubscribeToNativeCallbacks()
    {
      if (_didSubscribeToNativeEvents)
        return;

      lock (this)
      {
        if (_didSubscribeToNativeEvents)
          return;

        _NARVpsColocalizer_Set_colocalizationStateCallback
          (_applicationHandle, _nativeHandle, _colocalizationStateCallbackNative);

        _nativeAPI.NARVpsColocalization_SetFailureReasonCallback
          (_applicationHandle, _nativeHandle, _failureReasonCallbackNative);

        _didSubscribeToNativeEvents = true;
      }
    }

    // PInvoke
    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARVpsColocalizer_Init
    (
      IntPtr applicationHandle,
      byte[] stageId,
      IntPtr networkingHandle
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern IntPtr _NARVpsColocalizer_InitWithNode
    (
      IntPtr applicationHandle,
      byte[] stageId,
      IntPtr networkingHandle,
      byte[] data,
      ulong dataSize
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVpsColocalizer_Release(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVpsColocalizer_StartColocalization(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVpsColocalizer_PauseColocalization(IntPtr nativeHandle);

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVpsColocalizer_GetAlignedSpaceOrigin
    (
      IntPtr nativeHandle,
      float[] outPose
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern byte _NARVpsColocalizer_AlignedPoseToLocal
    (
      IntPtr nativeHandle,
      byte[] peerId,
      float[] alignedPose,
      float[] outPose
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVpsColocalizer_LocalPoseToAligned
    (
      IntPtr nativeHandle,
      float[] localPose,
      float[] outPose
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVpsColocalizer_Set_peerPoseCallback
    (
      IntPtr applicationHandle,
      IntPtr nativeHandle,
      _NARVpsColocalizer_peerPoseCallback callback
    );

    [DllImport(_ARDKLibrary.libraryName)]
    private static extern void _NARVpsColocalizer_Set_colocalizationStateCallback
    (
      IntPtr applicationHandle,
      IntPtr nativeHandle,
      _NARVpsColocalizer_colocalizationStateCallback callback
    );

    // C++ -> C# callbacks
    private delegate void _NARVpsColocalizer_colocalizationStateCallback
    (
      IntPtr context,
      UInt32 state,
      byte[] peerId
    );

    private delegate void _NARVpsColocalizer_peerPoseCallback
    (
      IntPtr context,
      float[] pose,
      byte[] peerId
    );

    [MonoPInvokeCallback(typeof(_NARVpsColocalizer_colocalizationStateCallback))]
    private static void _colocalizationStateCallbackNative(IntPtr context, UInt32 state, byte[] peerId)
    {
      var instance = SafeGCHandle.TryGetInstance<_NativeVPSColocalization>(context);

      if (instance == null || instance._isDestroyed)
        return;

      _CallbackQueue.QueueCallback
      (
        () =>
        {
          if (instance == null || instance._isDestroyed)
          {
            ARLog._Warn
            (
              "Queued _colocalizationStateCallbackNative invoked after C# instance was destroyed."
            );

            return;
          }

          var handler = instance.ColocalizationStateUpdated;
          if (handler != null)
          {
            ARLog._DebugFormat("Surfacing ColocalizationState event: {0}", false, state);
            var args = new ColocalizationStateUpdatedArgs(null, (ColocalizationState)state);
            handler(args);
          }
        }
      );
    }

    [MonoPInvokeCallback(typeof(INARVpsColocalization.NARVpsColocalization_FailureReasonCallback))]
    private static void _failureReasonCallbackNative(IntPtr context, byte vpsColocErrorCode, byte optionalVpsErrorCode)
    {
      var instance = SafeGCHandle.TryGetInstance<_NativeVPSColocalization>(context);

      if (instance == null || instance._isDestroyed)
        return;

      _CallbackQueue.QueueCallback
      (
        () =>
        {
          if (instance == null || instance._isDestroyed)
          {
            ARLog._Warn
            (
              "Queued _failureReasonCallbackNative invoked after C# instance was destroyed."
            );

            return;
          }

          var handler = instance.ColocalizationFailure;
          if (handler != null)
          {
            ARLog._DebugFormat("Surfacing ColocalizationFailure event: {0} {1}", false, vpsColocErrorCode, optionalVpsErrorCode);
            var args = new VPSColocalizerFailureReason((VPSColocalizerFailureCode)vpsColocErrorCode, (LocalizationFailureReason)optionalVpsErrorCode);
            handler(args);
          }
        }
      );
    }
  }
}
#pragma warning restore 0067

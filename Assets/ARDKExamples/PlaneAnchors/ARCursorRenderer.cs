// Copyright 2022 Niantic, Inc. All Rights Reserved.

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.Utilities;

using UnityEngine;

namespace Niantic.ARDKExamples.Helpers
{
    //! Helper script that spawns a cursor on a plane if it finds one
    /// <summary>
    /// A sample class that can be added to a scene to demonstrate basic plane finding and hit
    ///   testing usage. On each updated frame, a hit test will be applied from the middle of the
    ///   screen and spawn a cursor if it finds a plane.
    /// </summary>
    public class ARCursorRenderer :
      MonoBehaviour
    {
        /// The camera used to render the scene. Used to get the center of the screen.
        public Camera Camera;

        /// The object we will place to represent the cursor!
        public GameObject CursorObject;

        public GameObject landSeed;

        public GameObject transluSeed;

        public GameObject transluBot;

        [HideInInspector]
        public bool donePlacing;

        // No can use
        //public GameMode curMode;

        /// A reference to the spawned cursor in the center of the screen.
        public GameObject _spawnedCursorObject; // public to be used in SlideScript

        private IARSession _session;

        private void Start()
        {
            ARSessionFactory.SessionInitialized += _SessionInitialized;
            donePlacing = false;
            transluBot.transform.localScale = new Vector3(0f,0f,0f); // null-ified because we no longer need it; TODO Delete line if we go back


        }

        private void OnDestroy()
        {
            ARSessionFactory.SessionInitialized -= _SessionInitialized;

            var session = _session;
            if (session != null)
                session.FrameUpdated -= _FrameUpdated;

            DestroySpawnedCursor();
        }

        private void DestroySpawnedCursor()
        {
            if (_spawnedCursorObject == null)
                return;

            Destroy(_spawnedCursorObject);
            _spawnedCursorObject = null;
        }

        // Yeah setting alpha to 0.0f does nuttin, this shi deprecated

        //void ChangeAlpha(Material mat, float alphaVal)
        //{
        //    Color oldColor = mat.color;
        //    Color newColor = new Color(1.0f, 1.0f, 1.0f,0.0f);
        //    mat.SetColor("_Color", newColor);
        //}


        // REIMPLEMENT IF MORE LANDS ARE ADDED
        private bool isLand()
        {
            return landSeed.transform.GetChild(0).gameObject.activeSelf == true || landSeed.transform.GetChild(1).gameObject.activeSelf == true;
        }

        // USE ALONG WITH IsLAND() SINCE CURMODE ISN'T IN NAMESPACE
        private bool isGame()
        {
            return landSeed.transform.GetChild(7).gameObject.activeSelf == true;
        }

        // my new method for destroying th current cursor and adding in a new one
        public void SetNewCursor()
        {
            // Change newCursor's opacity
            //Debug.Log("Prior cursor color: " + newCursor.transform.Find("default").gameObject.GetComponent<Renderer>().material.color);
            //newCursor = Instantiate(newCursor, Vector2.one, Quaternion.identity);

            //GameObject newCursor = landSeed;
            GameObject newCursor = transluSeed;

            if (!isLand()) {
                if (!isGame()) {
                    newCursor = null;
                } else if (isGame())
                {
                    newCursor = transluBot;
                }
            }



            // this code is at least partially buggy, because of the seeded cursor glitch
            DestroySpawnedCursor();
            CursorObject = newCursor;
            if (isLand() || isGame()) {
                _spawnedCursorObject = Instantiate(CursorObject, Vector2.one, Camera.transform.rotation);
            }

            //Debug.Log("Current cursor color: "+ newCursor.transform.Find("default").gameObject.GetComponent<Renderer>().material.color);

        }

        // Useful for minigame after bot is placed; DON'T FORGET TO DEACTIVATE
        public void SetNewCursor(bool donkeyPlacing)
        {
            donePlacing = donkeyPlacing;
            if (donkeyPlacing)
            {
                // we're done with cursor
                DestroySpawnedCursor();
            } else
            {
                SetNewCursor();
            }
        }

        public Vector3 getLoc()
        {
            //if (_spawnedCursorObject != null) {
                return _spawnedCursorObject.transform.position;
            //}
            //return Vector3.one;
        }

        public Quaternion getRot()
        {
            return _spawnedCursorObject.transform.rotation;
        }

        private void _SessionInitialized(AnyARSessionInitializedArgs args)
        {
            var oldSession = _session;
            if (oldSession != null)
                oldSession.FrameUpdated -= _FrameUpdated;

            var newSession = args.Session;
            _session = newSession;
            newSession.FrameUpdated += _FrameUpdated;
            newSession.Deinitialized += _OnSessionDeinitialized;
        }

        private void _OnSessionDeinitialized(ARSessionDeinitializedArgs args)
        {
            DestroySpawnedCursor();
        }

        private void _FrameUpdated(FrameUpdatedArgs args)
        {
            // checks for need for cursor
            if (!donePlacing && (isLand()||isGame())) {
                var camera = Camera;
                if (camera == null)
                    return;

                var viewportWidth = camera.pixelWidth;
                var viewportHeight = camera.pixelHeight;

                // Hit testing for cursor in the middle of the screen
                var middle = new Vector2(viewportWidth / 2f, viewportHeight / 2f);

                var frame = args.Frame;
                // Perform a hit test and either estimate a horizontal plane, or use an existing plane and its
                // extents!
                var hitTestResults =
                  frame.HitTest
                  (
                    viewportWidth,
                    viewportHeight,
                    middle,
                    ARHitTestResultType.ExistingPlaneUsingExtent |
                    ARHitTestResultType.EstimatedHorizontalPlane
                  );

                if (hitTestResults.Count == 0)
                    return;

                if (_spawnedCursorObject == null)
                    _spawnedCursorObject = Instantiate(CursorObject, Vector2.one, Camera.transform.rotation);


                if (isLand()) {
                    Vector3 eulers = hitTestResults[0].Anchor.Transform.rotation.eulerAngles;
                    // Set the cursor object to the hit test result's position
                    _spawnedCursorObject.transform.position = hitTestResults[0].WorldTransform.ToPosition();


                    // TODO Uncomment if you want normal to plane placement back
                    _spawnedCursorObject.transform.rotation = hitTestResults[0].Anchor.Transform.rotation;


                    //Debug.Log(hitTestResults[0].Anchor.Transform.rotation.eulerAngles);

                    
                    // ONLY PERFORM IF PLACING ON THE FLOOR (45f for rough adjustment)
                    if (eulers.y >= eulers.x + eulers.z + 5f) {
                        // Orient the cursor object to look at the user, but remain flat on the "ground", aka
                        // only rotate about the y-axis

                        // TODO Uncomment if you want rotating with camera back
                        _spawnedCursorObject.transform.LookAt
                        (
                          new Vector3
                          (
                            frame.Camera.Transform[0, 3],
                            _spawnedCursorObject.transform.position.y,
                            frame.Camera.Transform[2, 3]
                          )
                        );
                    }
                } else
                {
                    Vector3 eulers = hitTestResults[0].Anchor.Transform.rotation.eulerAngles;
                    //if (eulers.y >= eulers.x + eulers.z + 5f)
                    //{
                        // Set the cursor object to the hit test result's position
                        _spawnedCursorObject.transform.position = hitTestResults[0].WorldTransform.ToPosition();



                        _spawnedCursorObject.transform.rotation = hitTestResults[0].Anchor.Transform.rotation;
                    //}

                        _spawnedCursorObject.transform.LookAt(
                      new Vector3
                      (
                        frame.Camera.Transform[0, 3],
                        _spawnedCursorObject.transform.position.y,
                        frame.Camera.Transform[2, 3]
                      )
                    );
                    
                }
            }
        }
    }
}

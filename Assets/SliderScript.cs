using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;

public class SliderScript : MonoBehaviour
{

    public GameObject camerar;
    private Niantic.ARDKExamples.Helpers.ARHitTester hmm;
    private Niantic.ARDKExamples.Helpers.ARCursorRenderer rinder;

    private Slider slidey;
    private float lastValue = 0;

    public GameObject background;
    private Image backgroundImage;
    public GameObject fillarea;
    private Image fillareaImage;
    public GameObject handle;

    private Color32 onColor = new Color32(255, 255, 255, 255);
    private Color32 offColor = new Color32(255, 255, 255, 0);

    // Start is called before the first frame update
    void Start()
    {
        slidey = gameObject.GetComponent<Slider>();
        hmm = camerar.GetComponent<Niantic.ARDKExamples.Helpers.ARHitTester>();
        rinder = camerar.GetComponent<Niantic.ARDKExamples.Helpers.ARCursorRenderer>();
        backgroundImage = background.gameObject.GetComponent<Image>();
        fillareaImage = fillarea.gameObject.GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 toScale = new Vector3(0f, 1f, 0f); // vertical because relative to whatever it loads
        if (hmm.justPlaced) // just placed
        {
            hmm.justPlaced = false;
            slidey.value = 0;
            lastValue = 0;
            backgroundImage.color = onColor;
            fillareaImage.color = onColor;
            handle.transform.localScale = Vector3.one;
        }
        if (hmm._placedObjects.Count > 0) {
            // offset based on placed plane

            // Possible values; the unity vector of the euler angles; the unity vector of the scaled world position (from hittest[0] in rinder) minus the actual
            hmm._placedObjects[hmm._placedObjects.Count - 1].transform.position += hmm._placedObjects[hmm._placedObjects.Count - 1].transform.GetChild(0).TransformDirection(toScale * (slidey.value - lastValue)); // shifts based on scale change during update

            backgroundImage.color = Color32.Lerp(onColor, offColor, slidey.value);
            fillareaImage.color = Color32.Lerp(onColor, offColor, slidey.value);
            handle.transform.localScale = new Vector3(1+slidey.value, 1 + slidey.value, 1 + slidey.value);
        }
        lastValue = slidey.value;
    }
}

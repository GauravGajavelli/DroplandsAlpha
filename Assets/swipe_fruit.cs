using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using UnityEngine.EventSystems;


    public class swipe_fruit : MonoBehaviour
{
    public GameObject scrollbar;
    public GameObject viewie;
    private RectTransform shifts;
    private RectTransform scrols;

    private int lastClicked = -1; // the index of the button last clicked
    private int curDex = 0; // Start is called before the first frame update

    public GameObject canvas;
    private RectTransform canvasTrans;

    private Vector2 startPos;
    private Vector2 direction;
    private bool directionChosen;

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    public GameObject fruitZone;
    
    public bool inFruitZone;

    void Start()
    {
        shifts = viewie.gameObject.GetComponent<RectTransform>();
        scrols = gameObject.GetComponent<RectTransform>();
        canvasTrans = canvas.gameObject.GetComponent<RectTransform>(); // gets the bot's bod

        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();

        inFruitZone = false;

    }

    // Update is called once per frame
    void Update()
    {
        //Check if the left Mouse button is clicked
        if (Input.GetKey(KeyCode.Mouse0))
        {
            //Set up the new Pointer Event
            m_PointerEventData = new PointerEventData(m_EventSystem);
            //Set the Pointer Event Position to that of the mouse position
            m_PointerEventData.position = Input.mousePosition;

            //Create a list of Raycast Results
            List<RaycastResult> results = new List<RaycastResult>();

            //Raycast using the Graphics Raycaster and mouse click position
            m_Raycaster.Raycast(m_PointerEventData, results);

            inFruitZone = false;

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                if (result.gameObject == fruitZone)
                {
                    inFruitZone = true;
                }
            }
        }


        var touch = PlatformAgnosticInput.GetTouch(0);

        if (touch.phase == TouchPhase.Began)
        {
            startPos = touch.position;
            directionChosen = false;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            direction = touch.position - startPos;
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            directionChosen = true;
            Debug.Log("CHANGE: " + (direction));
        }
        if (directionChosen) // Prevents people from holding to move menu
        {
            if (inFruitZone)
            {
                if (direction.x > 0)
                {
                    if (curDex > 0)
                    {
                        float shifnum = (1f / (transform.childCount)) * scrols.rect.width;
                        Debug.Log("SHIFNUM: " + shifnum);
                        //shifts.anchoredPosition += new Vector2(shifnum, 0);
                        curDex--;
                    }
                }
                else if (direction.x < 0)
                {
                    if (curDex < transform.childCount - 1)
                    {
                        float shifnum = (1f / (transform.childCount)) * scrols.rect.width;
                        Debug.Log("SHIFNUM: " + shifnum);
                        //shifts.anchoredPosition -= new Vector2(shifnum, 0);
                        curDex++;
                    }
                }
            }

            // gotta reset b4 next touch
            startPos = new Vector2();
            directionChosen = false;
            direction = new Vector2();
        }

        // TODO Uncomment this if its resizing is needed
        //// Selected
        //if (transform.childCount > 0) {
        //    transform.GetChild(curDex).localScale = Vector2.Lerp(transform.GetChild(curDex).localScale, new Vector2(1.6f, 1.6f), 0.1f); // this is THE button;
        //}

        //// calls appropriate button click (NOTHING IN THIS VERSION OF THE SCRIPT)
        //if (curDex != lastClicked) // different place
        //{
        //}
        //lastClicked = curDex;

        //// Resizes the rest
        //for (int a = 0; a < transform.childCount; a++)
        //{
        //    if (a != curDex)
        //    {
        //        transform.GetChild(a).localScale = Vector2.Lerp(transform.GetChild(a).localScale, new Vector2(0.8f, 0.8f), 0.1f);

        //    }
        //}



    }
    // resets menu
    void OnDisable()
    {
        float shifnum = (1f / (transform.childCount)) * scrols.rect.width;
        shifts.anchoredPosition += new Vector2(shifnum * curDex, 0);
        curDex -= curDex;
    }

    //// NOTE: THIS IS THE PLACEMENT DEADZONE; FALSE IS THE LIFEZONE FOR THE FRUIT MENU
    //private bool notInDeadzone(Touch toca)
    //{
    //    Debug.Log("Touch x: " + toca.position.x);
    //    Debug.Log("Touch y: " + toca.position.y);

    //    // since touch is always a pair of nonnegative values, we can assume that it's dead about 1/5 up the screen


    //    // so... there's a scaling factor to account for here: the numbers given by touche's position are actually about 2 times the canvas dimensions;
    //    // ergo we should divide our touch position by 2 before using it in the 1/6 calculation
    //    float converTouch = toca.position.y / 2;
    //    if (converTouch < (1 * canvasTrans.rect.height / 2))
    //    {
    //        Debug.Log("Bottom 1/5!");
    //        return true;
    //    }
    //    Debug.Log("Top 4/5!");
    //    return false;
    //}
}

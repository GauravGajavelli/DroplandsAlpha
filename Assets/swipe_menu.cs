using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;
using System;
using UnityEngine.EventSystems;

public class swipe_menu : MonoBehaviour
{
    public GameObject scrollbar;
    public GameObject inventoryButton; // to get inventoryscrip reference to call the button click methods based on the current scroll
    private Niantic.ARDKExamples.Helpers.inventoryScrip hmm;
    public GameObject viewie;
    private RectTransform shifts ;
    private RectTransform scrols;

    private int lastClicked = -1; // the index of the button last clicked
    private int curDex = -1; // Start is called before the first frame update

    public GameObject canvas;
    private RectTransform canvasTrans;

    private Vector2 startPos;
    private Vector2 direction;
    private bool directionChosen;

    GraphicRaycaster m_Raycaster;
    PointerEventData m_PointerEventData;
    EventSystem m_EventSystem;

    public GameObject fruitZone;
    public GameObject menuZone;
    public GameObject gradientZone;
    public GameObject sliderZone;

    public bool inSliderZone;
    public bool inFruitZone;
    public bool inMenuZone;
    public bool inGradientZone;

    public GameObject glasContent;

    // Land Seed template
    public GameObject placeSeedHere;

    SortedSet<int> deadDexes = new SortedSet<int>();

    public Camera _mainCamera;  //This will reference the MainCamera in the scene, so the ARDK can leverage the device camera
    public GameMode curMode; // scriptable object containing the current game mode (seed or land time)

    public GameObject instructions;

    public bool instrucShow; // whether or not we should show the instruction
    public GameObject instructionsOnButton;
    public GameObject instructionsOffButton;
    public bool instructionActive; // unlike instrucShow, it's not prescriptive; it simply describes whether the instruction is active

    public bool grownPanelActive;
    public GameObject grownPanel;
    private Image grownPanelImage;
    private Color32 earnColor = new Color32(47, 255, 130, 157);
    private Color32 noneColor = new Color32(255, 70, 47, 157);

    public bool inTutorial;
    public GameObject miniArrow;
    public bool menuSetup;
    public GameObject blockingPanel;
    private int goalLoc;
    public GameObject skipButton;

    void Awake()
    {
        scrols = gameObject.GetComponent<RectTransform>();
    }

    void Start()
    {
        hmm = inventoryButton.GetComponent<Niantic.ARDKExamples.Helpers.inventoryScrip>();
        shifts = viewie.gameObject.GetComponent<RectTransform>();
        canvasTrans = canvas.gameObject.GetComponent<RectTransform>(); // gets the bot's bod
        grownPanelImage = grownPanel.gameObject.GetComponent<Image>();

        //Fetch the Raycaster from the GameObject (the Canvas)
        m_Raycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
        //Fetch the Event System from the Scene
        m_EventSystem = GetComponent<EventSystem>();

        inFruitZone = false;
        inMenuZone = false;
        inGradientZone = false;
        inSliderZone = false;
        instrucShow = false;
        instructionActive = false;
        grownPanelActive = false;
        inTutorial = false;
        menuSetup = false;

        loadItems();
        curDex = firstAliveItem();
        string curStr = glasContent.transform.GetChild(curDex).gameObject.name;
        //if (PlayerPrefs.GetInt("num" + curStr) <= 0)
        //{
        //    hmm.ButtonClicked(curDex);
        //}
        //else
        //{
        //    hmm.ButtonClicked(-1);
        //}


        Invoke("setupMenu",0.01f);
    }

    public void iCanListen()
    {
        Debug.Log("Mr. Jones and me");
    }

    void setupMenu()
    {
        float shifnum = (1f / (transform.childCount)) * scrols.rect.width;
        shifts.anchoredPosition -= new Vector2(shifnum * (curDex), 0);
        transform.GetChild(curDex).localScale = new Vector3(1.2f, 1.2f, 1.2f);

        // calls appropriate button click
        if (curDex != lastClicked) // different place
        {
            if (deadDexes.Contains(curDex))
            {
                hmm.ButtonClicked(-1); // TODO Uncomment this because it is proper behavior
                //hmm.ButtonClicked(curDex); // like you clicked the button at the corresponding child
            }
            else
            {
                hmm.ButtonClicked(curDex); // like you clicked the button at the corresponding child
            }

        }
        lastClicked = curDex;

        // Resizes the rest
        for (int a = 0; a < transform.childCount; a++)
        {
            Transform aTrans = transform.GetChild(a);
            if (a != curDex)
            {
                //aTrans.localScale = Vector2.Lerp(transform.GetChild(a).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                aTrans.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }
            //    if (System.Math.Abs(curDex-a) > 1)
            //    {
            //    aTrans.gameObject.SetActive(false);
            //    } else
            //    {
            //    aTrans.gameObject.SetActive(true);
            //}
        }

        // sets up instructions
        int struction = PlayerPrefs.GetInt("instruct");
        Debug.Log("instructplaya: " + struction);
        if (struction == 1 || (struction == 0 && instrucShow))
        {
            if (!instructionActive)
            {
                Debug.Log("instructivation 2: ");
                instructions.gameObject.SetActive(true);
                activateInstruction(curDex); // TODO See phone notes for zpecification
                instructionActive = true;
            }
            instructionsOnButton.gameObject.SetActive(true);
        }
        else
        {
            if (instructionActive)
            {
                Debug.Log("instructdeactivation 2: ");
                deactivateCurstruction();
            }
            instructionsOffButton.gameObject.SetActive(true);
        }

        menuSetup = true;
    }

    // Handles loading the proper items; TODO optimize like with lands, loading and storing only at start and end of use
    void loadItems()
    {

        if (!PlayerPrefs.HasKey("instruct"))
        {
            PlayerPrefs.SetInt("instruct", 1); // 1 means it's on; 0 means it's toggleable; always on until first toggle
            Debug.Log("gliolingon struct 1: " + PlayerPrefs.GetInt("instruct"));
        }
        if (!PlayerPrefs.HasKey("tutStage"))
        {
            PlayerPrefs.SetInt("tutStage",0);
        }
        curMode.tutorialStage = PlayerPrefs.GetInt("tutStage");
        // Adding all items initially if in tutorial mode
        if (curMode.tutorialStage == 0)
        {
            curMode.tutorialStage++;
            PlayerPrefs.SetInt("tutStage", PlayerPrefs.GetInt("tutStage")+1);
            PlayerPrefs.SetInt("numGrass Button", 1);
            PlayerPrefs.SetInt("numWater Button", 1);
            PlayerPrefs.SetInt("numSeed Button", 0);
            PlayerPrefs.SetInt("numWood Seed Button", 0);
            PlayerPrefs.SetInt("numFan Seed Button", 1);
            PlayerPrefs.SetInt("numDroplet Button", 1);
        }

        for (int a = 0; a < glasContent.transform.childCount; a++)
        {
            string curStr = glasContent.transform.GetChild(a).gameObject.name;
            if (!PlayerPrefs.HasKey("num"+curStr))
            {
                PlayerPrefs.SetInt("num"+curStr, 1);
            }
            if (PlayerPrefs.GetInt("num" + curStr) <= 0)
            {
                //glasContent.transform.GetChild(a).gameObject.SetActive(false);
                deadDexes.Add(a);
                glasContent.transform.GetChild(a).gameObject.GetComponent<Image>().color = new Color32(239, 239, 240, 122);
            }
        }


        deadDexes.Add(100);
    }

    // Update is called once per frame
    void Update()
    {
        if (!menuSetup)
        {
            Debug.Log("Still getting dressed");
            return;
        }
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
            inMenuZone = false;
            inGradientZone = false;
            inSliderZone = false;

            //For every result returned, output the name of the GameObject on the Canvas hit by the Ray
            foreach (RaycastResult result in results)
            {
                if (result.gameObject == menuZone)
                {
                    inMenuZone = true;
                }
                if (result.gameObject == fruitZone)
                {
                    inFruitZone = true;
                }
                if (result.gameObject == gradientZone)
                {
                    inGradientZone = true;
                }
                if (result.gameObject == sliderZone)
                {
                    inSliderZone = true;
                }
            }
        }

        // Checking fully grown plants and showing grown panel
        if (!grownPanelActive && curMode.prizeTime)
        {

            if (curMode.plantType == 0) // cactus brained
            {
                grownPanelImage.color = earnColor;
            }
            else
            { // final state of the non-fruiting plants
                grownPanelImage.color = noneColor;
            }
            grownPanel.gameObject.SetActive(true);
            activateGrownPanel(curMode.plantType);

            grownPanelActive = true;
        }
        else if (grownPanelActive)
        {
            if (!curMode.prizeTime)
            {
                grownPanelActive = false;
                grownPanel.gameObject.SetActive(false);
            }

            // Essentially nullifies all touch detection while it's active
            inFruitZone = false;
            inMenuZone = false;
            inGradientZone = false;
            inSliderZone = false;
        }

        // Checking in tutorial
        if (!inTutorial && curMode.tutorialStage < 6)
        {
            inTutorial = true;
            blockingPanel.gameObject.SetActive(true); // blocks the minigame panel
            skipButton.gameObject.SetActive(true);
        }
        else if (inTutorial)
        {
            if (curMode.tutorialStage >= 6)
            {
                inTutorial = false;
                skipButton.gameObject.SetActive(false);
                blockingPanel.gameObject.SetActive(false);
                miniArrow.gameObject.SetActive(false);
            } else
            {
                if (curMode.tutorialStage == 1) // gotta do land
                {
                   goalLoc = 0; // grassland
                }
                else if (curMode.tutorialStage == 2) // gotta do seed
                {
                    goalLoc = 4; // fanseed
                }
                else if (curMode.tutorialStage == 3) // gotta do water
                {
                    goalLoc = 5; // droplet
                }
                else if (curMode.tutorialStage == 4) // gotta do delete
                {
                    goalLoc = 6; // delete
                }
                else if (curMode.tutorialStage == 5) // gotta click minigame
                {
                    blockingPanel.gameObject.SetActive(false);
                    miniArrow.gameObject.SetActive(true);
                    skipButton.gameObject.SetActive(false);
                }
            }
            //inGradientZone = false;
            inMenuZone = false;
        }

        var touch = PlatformAgnosticInput.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            startPos = touch.position;
            directionChosen = false;
            if (!inFruitZone && !inMenuZone && !inGradientZone && !inSliderZone) // this means we clicked something
            {
                if ((seedHit(touch) && curMode.seedTime) || (dropHit(touch) && curMode.dropTime))
                {
                    // BECAUSE CORRESPONDING PLACE IN ARHITTESTER TODO Update if it gets changed lol (gotta love that coupling)

                    GameObject curJect = transform.GetChild(curDex).gameObject;
                    int curFreq = PlayerPrefs.GetInt("num" + curJect.name);
                    if (curFreq > 0)
                    {
                        PlayerPrefs.SetInt("num" + curJect.name, curFreq - 1);
                    }
                    if (curFreq - 1 == 0) // aka if we empty now
                    {
                        Debug.Log("ButtSomething died");
                        deadDexes.Add(curDex);
                        glasContent.transform.GetChild(curDex).gameObject.GetComponent<Image>().color = new Color32(239, 239, 240, 122);
                        Invoke("deathOfCurrentItem", 0f); // gets it on next update
                    }

                    // seed or droplet got placed
                    if (curMode.seedTime) {
                        curMode.tutorialStage++;
                        PlayerPrefs.SetInt("tutStage", PlayerPrefs.GetInt("tutStage") + 1);
                    } else if (curMode.dropTime)
                    {
                        curMode.tutorialStage++;
                        PlayerPrefs.SetInt("tutStage", PlayerPrefs.GetInt("tutStage") + 1);
                    }
                }
                else if (isLand() && PlayerPrefs.GetInt("zeroIfLandNotPlaced") == 1)
                {
                    PlayerPrefs.SetInt("zeroIfLandNotPlaced", 0);
                    GameObject curJect = transform.GetChild(curDex).gameObject;
                    int curFreq = PlayerPrefs.GetInt("num" + curJect.name);
                    if (curFreq > 0)
                    {
                        PlayerPrefs.SetInt("num" + curJect.name, curFreq - 1);
                    }
                    if (curFreq - 1 == 0) // aka if we empty now
                    {
                        Debug.Log("Something died");
                        deadDexes.Add(curDex);
                        glasContent.transform.GetChild(curDex).gameObject.GetComponent<Image>().color = new Color32(239, 239, 240, 122);
                        Invoke("deathOfCurrentItem", 0f); // gets it on next update
                    }

                    // land got placed
                    curMode.tutorialStage++;
                    PlayerPrefs.SetInt("tutStage", PlayerPrefs.GetInt("tutStage") + 1);
                }
            }
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
        if (directionChosen || inTutorial) // Prevents people from holding to move menu
        {
            if (inMenuZone || inTutorial) {
                if ((!inTutorial && direction.x > 0) || (inTutorial && goalLoc < curDex))
                {
                    if (curDex > 0) // TODO replace with checking this against the continuous next value being -1, then do nothing
                    {
                        float shifnum = (1f / (transform.childCount)) * scrols.rect.width;
                        Debug.Log("SHIFNUM: " + shifnum);


                        if (!inTutorial && direction.x > 0)
                        {
                            shifts.anchoredPosition += new Vector2(shifnum, 0);
                            curDex--;
                        } else
                        {
                            Vector2 toMult = new Vector2(shifnum, 0);
                            shifts.anchoredPosition += toMult*(curDex-goalLoc);
                            curDex = goalLoc;
                        }
                    }
                }
                else if ((!inTutorial && direction.x < 0) || (inTutorial && goalLoc > curDex))
                {
                    if (curDex < transform.childCount - 1) // TODO same as above but with continuous prev being -1, p easy for loop with if for consecutive
                    {
                        float shifnum = (1f / (transform.childCount)) * scrols.rect.width;
                        Debug.Log("SHIFNUM: " + shifnum);

                        if (!inTutorial && direction.x < 0)
                        {
                            shifts.anchoredPosition -= new Vector2(shifnum, 0);
                            curDex++;
                        } else
                        {
                            Vector2 toMult = new Vector2(shifnum, 0);
                            shifts.anchoredPosition += toMult * (curDex - goalLoc);
                            curDex = goalLoc;
                        }
                    }
                }
            }

            // gotta reset b4 next touch
            startPos = new Vector2();
            directionChosen = false;
            direction = new Vector2();
        }

        // Selected
        //transform.GetChild(curDex).localScale = Vector2.Lerp(transform.GetChild(curDex).localScale, new Vector2(1.2f, 1.2f), 0.1f); // this is THE button;
        transform.GetChild(curDex).localScale = new Vector3(1.2f, 1.2f, 1.2f);

        // calls appropriate button click
        if (curDex != lastClicked) // different place
        {
            Debug.Log("ButtChula vista: "+curDex);
            Debug.Log("Butt deadDexes: "+printDeadDexes());
            if (deadDexes.Contains(curDex)) {
                Debug.Log("Butt omfg it was dead");
                hmm.ButtonClicked(-1); // TODO Uncomment this because it is proper behavior
                //hmm.ButtonClicked(curDex); // like you clicked the button at the corresponding child
            }
            else
            {
                hmm.ButtonClicked(curDex); // like you clicked the button at the corresponding child
            }

            // Deactivate previous structions
            deactivateCurstruction();

            //// TODO NEXT Get rid of if statements if the lastClicked and curDex have equivalents in instructions children
            //if (lastClicked < instructions.transform.childCount)
            //{
            //    instructions.transform.GetChild(lastClicked).gameObject.SetActive(false);
            //}
            //if (curDex < instructions.transform.childCount)
            //{
            //    activateInstruction(curDex); // activates only the specific instruction
            //}

        }
        lastClicked = curDex;

        // Resizes the rest
        for (int a = 0; a < transform.childCount; a++)
        {
            Transform aTrans = transform.GetChild(a);
            if (a != curDex)
            {
                //aTrans.localScale = Vector2.Lerp(transform.GetChild(a).localScale, new Vector2(0.8f, 0.8f), 0.1f);
                aTrans.localScale = new Vector3(0.8f, 0.8f, 0.8f);
            }
            //    if (System.Math.Abs(curDex-a) > 1)
            //    {
            //    aTrans.gameObject.SetActive(false);
            //    } else
            //    {
            //    aTrans.gameObject.SetActive(true);
            //}
        }

        // sets up instructions
        int struction = PlayerPrefs.GetInt("instruct");
        //Debug.Log("instructplaya: " + struction);
        if (struction == 1 || (struction == 0 && instrucShow))
        {
            if (!instructionActive) {
                Debug.Log("instructivation 1: ");
                instructions.gameObject.SetActive(true);
                activateInstruction(curDex); // TODO See phone notes for zpecification
                instructionActive = true;
            }
        } else
        {
            if (instructionActive)
            {
                Debug.Log("instructdeactivation 1: ");
                deactivateCurstruction();
            }
        }
    }

    void deathOfCurrentItem()
    {
        Debug.Log("Butt death");
        hmm.ButtonClicked(-1);
    }

    // Returns the index of the first active item in the list, for seletion after losing an item
    // returns -1 if no item is active
    // Assums deadDexes populated
    int firstAliveItem()
    { // menu edge cases: it properly nexts when: 1) completely empty, 2) first is gone, 3) Some at start are gone and some at end are gone
        int current = 0;
        SortedSet<int>.Enumerator em = deadDexes.GetEnumerator();
        while (em.MoveNext()) // essentially checks that, starting from beginning we increase if they are still dead
        {
            if (current == em.Current)
            {
                current++;
            } else if (current < em.Current) // we're done
            {
                return current==transform.childCount?0:current;
            }
        }
        return 0;
    }

    string printDeadDexes()
    { // menu edge cases: it properly nexts when: 1) completely empty, 2) first is gone, 3) Some at start are gone and some at end are gone
        SortedSet<int>.Enumerator em = deadDexes.GetEnumerator();
        string gum = "";
        while (em.MoveNext()) // essentially checks that, starting from beginning we increase if they are still dead
        {
            gum += em.Current.ToString()+", ";
        }
        return gum;
    }

    // resets menu
    void OnDisable()
    {
        float shifnum = (1f / (transform.childCount)) * scrols.rect.width;
        shifts.anchoredPosition += new Vector2(shifnum * curDex, 0);
        curDex -= curDex;
    }

    // REIMPLEMENT IF MORE LANDS ARE ADDED
    private bool isLand()
    {
        return placeSeedHere.transform.GetChild(0).gameObject.activeSelf == true || placeSeedHere.transform.GetChild(1).gameObject.activeSelf == true;
    }

    private bool seedHit(Touch touch)
    {
        //        RaycastHit hitData;
        //        Ray ray = _mainCamera.ScreenPointToRay(touch.position);
        //        if (Physics.Raycast(ray, out hitData)) // as per gamedevbeginner's raycast tutorial
        //        {
        //            if (hitData.transform.parent.GetChild(2).gameObject.activeSelf == false &&
        //                    hitData.transform.parent.GetChild(9).gameObject.activeSelf == false &&
        //                    hitData.transform.parent.GetChild(13).gameObject.activeSelf == false &&
        //                    // seeds above
        //                    hitData.transform.parent.GetChild(3).gameObject.activeSelf == false &&
        //                    hitData.transform.parent.GetChild(4).gameObject.activeSelf == false &&
        //                    hitData.transform.parent.GetChild(5).gameObject.activeSelf == false &&
        //                    hitData.transform.parent.GetChild(6).gameObject.activeSelf == false &&
        //                    // cacti above
        //                    hitData.transform.parent.GetChild(10).gameObject.activeSelf == false &&
        //                    hitData.transform.parent.GetChild(11).gameObject.activeSelf == false &&
        //                    hitData.transform.parent.GetChild(12).gameObject.activeSelf == false &&
        //                    // wood above, fan below
        //                    hitData.transform.parent.GetChild(14).gameObject.activeSelf == false &&
        //                    hitData.transform.parent.GetChild(15).gameObject.activeSelf == false)
        //            {
        //                Debug.Log("Damn that seed hits");
        //                return true; // its seed time
        //            }
        //            Debug.Log("Goethe here");
        //            Debug.Log("First quarter: " +
        //    (hitData.transform.parent.GetChild(2).gameObject.name));
        //// So it's apparent that the reason why it's doin this is because it's already placed and it's checking that
        //        }
        //        Debug.Log("Damn that seed doesn't hit");
        //        return false;
        int seedNum = PlayerPrefs.GetInt("zeroIfSeedNotPlaced");
        PlayerPrefs.SetInt("zeroIfSeedNotPlaced", 0);
        return (seedNum == 1);
    }
    private bool dropHit(Touch touch)
    {
        //RaycastHit hitData;
        //Ray ray = _mainCamera.ScreenPointToRay(touch.position);
        //if (Physics.Raycast(ray, out hitData)) // as per gamedevbeginner's raycast tutorial
        //{
        //    if (hitData.transform.parent.GetChild(2).gameObject.activeSelf
        //        || hitData.transform.parent.GetChild(3).gameObject.activeSelf
        //        || hitData.transform.parent.GetChild(4).gameObject.activeSelf
        //        || hitData.transform.parent.GetChild(5).gameObject.activeSelf
        //        //|| hitData.transform.parent.GetChild(6).gameObject.activeSelf // TODO reactivate if we make watering max cactus have any meaning in the future
        //        || hitData.transform.parent.GetChild(9).gameObject.activeSelf
        //        || hitData.transform.parent.GetChild(10).gameObject.activeSelf
        //        || hitData.transform.parent.GetChild(11).gameObject.activeSelf
        //        || hitData.transform.parent.GetChild(13).gameObject.activeSelf
        //        || hitData.transform.parent.GetChild(14).gameObject.activeSelf)
        //    {
        //        Debug.Log("Damn that drop hits");
        //        return true; // its drop time, I guess these are all of the plant sizes
        //    }
        //}
        //Debug.Log("Damn that drop doesn't hit");
        //return false;
        int dropNum = PlayerPrefs.GetInt("zeroIfDropNotPlaced");
        PlayerPrefs.SetInt("zeroIfDropNotPlaced", 0);
        return (dropNum == 1);
    }

    // For use when a deletion occurs, reactivates appropriate items and if the current item is reactivated, then it does the corresponding reactivation
    public void refreshMenu ()
    {
        for (int a = 0; a < glasContent.transform.childCount; a++)
        {
            string curStr = glasContent.transform.GetChild(a).gameObject.name;
            if (PlayerPrefs.GetInt("num" + curStr) <= 0)
            {
                //glasContent.transform.GetChild(a).gameObject.SetActive(false);
                deadDexes.Add(a);
                glasContent.transform.GetChild(a).gameObject.GetComponent<Image>().color = new Color32(239, 239, 240, 122);
            } else
            {
                deadDexes.Remove(a);
                glasContent.transform.GetChild(a).gameObject.GetComponent<Image>().color = new Color32(255,255,255,255);

                // TODO prolly delete
                ////  revived ones... yeahh I just realized this case doesn't really exist cuz we're always on reharvest when we reharvested
                //if (curDex == a) // we are sitting on a revived one, was previously on hmm.ButtonClicked(-1)
                //{
                //    hmm.ButtonClicked(curDex);
                //}
            }
        }
    }

    public bool grasslandActive() {
        return PlayerPrefs.GetInt("numGrass Button") > 0;
    }

    // activates only the instruction at the given index
    private void activateInstruction(int curdex)
    {
        for (int a = 0; a < instructions.transform.childCount; a++)
        {
            instructions.transform.GetChild(a).gameObject.SetActive(false);
        }

        instructions.transform.GetChild(curdex).gameObject.SetActive(true);
        if (instructions.transform.GetChild(curdex).transform.childCount > 0) {
            instructions.transform.GetChild(curdex).transform.GetChild(0).gameObject.SetActive(true);
        }
    }

    // activates only the panel at the given index
    private void activateGrownPanel(int curdex)
    {
        for (int a = 0; a < grownPanel.transform.childCount; a++)
        {
            grownPanel.transform.GetChild(a).gameObject.SetActive(false);
        }

        grownPanel.transform.GetChild(curdex).gameObject.SetActive(true);
        //if (instructions.transform.GetChild(curdex).transform.childCount > 0)
        //{
        //    instructions.transform.GetChild(curdex).transform.GetChild(0).gameObject.SetActive(true);
        //}
    }

    // Deprecated in favor of more efficient boolean representation of same idea (instructionActive)
    private bool instructioneActive(int curdex)
    {
        return instructions.transform.GetChild(curdex).gameObject.activeSelf;
    }

    public void toggleInstructions(bool toggleOn)
    {
        Debug.Log("instructoggle: "+toggleOn);
        PlayerPrefs.SetInt("instruct", 0); // now it's no longer perma on
        if (toggleOn)
        {
            instrucShow = true;
            instructionsOnButton.gameObject.SetActive(true);
            instructionsOffButton.gameObject.SetActive(false);
        }
        else // toggle off
        {
            instrucShow = false;
            instructionsOnButton.gameObject.SetActive(false);
            instructionsOffButton.gameObject.SetActive(true);
        }
    }

    public void deactivateCurstruction()
    {
        instructions.gameObject.SetActive(false);
        instructionActive = false;
    }
}

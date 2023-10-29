// Copyright 2022 Niantic, Inc. All Rights Reserved.

// TODO when implementing final tutorial screens, have instructional text boxes pop up as paused (true pause, clock slowed to 0) and then disappear forever when acknowledged

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.External;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;

using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using UnityEngine;
using UnityEngine.EventSystems;

// CURRENTLY WHERE VIRTUAL TO REAL-WORLD COLLISIONS ARE HANDLED

namespace Niantic.ARDKExamples.Helpers
{
    //! A helper class that demonstrates hit tests based on user input
    /// <summary>
    /// A sample class that can be added to a scene and takes user input in the form of a screen touch.
    ///   A hit test is run from that location. If a plane is found, spawn a game object at the
    ///   hit location.
    /// </summary>
    public class ARHitTester : MonoBehaviour
    {


        /// The camera used to render the scene. Used to get the center of the screen.
        public Camera Camera;

        /// The types of hit test results to filter against when performing a hit test.
        [EnumFlagAttribute]
        public ARHitTestResultType HitTestType = ARHitTestResultType.ExistingPlane;

        /// The object we will place when we get a valid hit test result!
        public GameObject PlacementObjectPf;

        // Materials
        public Material invisible;

        // Land Seed template
        public GameObject placeSeedHere;
        public GameObject goalie;
        public GameObject sandgoal;
        public GameObject watergoal;
        public GameObject plats;
        public GameObject grassplat;
        public GameObject sandplat;

        private ARCursorRenderer rinder;

        public GameObject gamestructions;
        public GameObject swipestructions;
        public GameObject winstructions;

        /// A list of placed game objects to be destroyed in the OnDestroy method.
        public List<GameObject> _placedObjects = new List<GameObject>();

        private Vector2 startPos;
        private Vector2 direction;
        private bool directionChosen;

        private GameObject placedGoal = null;

        // set equal to number of plats placed
        private int numPlats = 0;

        Vector3 ogPlaced;

        public int jumpMag;

        /// Internal reference to the session, used to get the current frame to hit test against.
        private IARSession _session;

        public Camera _mycamera;

        // for adding items to scroll panel
        public GameObject panelie;
        public GameObject fruitImg;

        // used for platform placement distance; everything has been calculated with gravity as always 9.81 in mind, but we can change these
        private float deltaY = 0.27f;
        private float deltaXZ = 0.1f;
        private float cubeScale = 0.1f;
        private Rigidbody robobody = null;
        private bool jumpinUp = false;
        private bool jumpinDown = false;

        // necessary to establish deadzone below/on buttons
        public GameObject canvas;
        private RectTransform canvasTrans;

        // Global time
        private int trime = 0;

        private bool won = false;
        public bool paused = false;

        private int numFails = 0;
        public bool justPlaced = false; // necessary to reset slider

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

        private float botStartY = -999f;

        public GameObject gameOverScreen;
        public GameObject prizePanel;
        private Vector3 botDisplacement;
        private Transform camTransform;
        private Vector3 toScale;
        private float botScale = 1.1f;

        private void Start()
        {

            ARSessionFactory.SessionInitialized += OnAnyARSessionDidInitialize;
            Debug.Log("I'm attached to " + gameObject.name);

            rinder = _mycamera.GetComponent<ARCursorRenderer>();
            camTransform = _mycamera.GetComponent<Transform>();
            loadPositions();

            Physics.gravity = new Vector3(0, -9.81f, 0);

            canvasTrans = canvas.gameObject.GetComponent<RectTransform>(); // gets the bot's bod
                                                                           //Fetch the Raycaster from the GameObject (the Canvas)
            m_Raycaster = canvas.gameObject.GetComponent<GraphicRaycaster>();
            //Fetch the Event System from the Scene
            m_EventSystem = GetComponent<EventSystem>();

            inFruitZone = false;
            inMenuZone = false;
            inGradientZone = false;
            inSliderZone = false;

            PlayerPrefs.SetInt("zeroIfLandNotPlaced", 0);
            //PlayerPrefs.SetInt("negOneIfNoSeedReward", -1); // if reward == -1, nothing, if == 0 then cactus seed, if == 1 then wood seed, if == 2 then fan seed

            botDisplacement = new Vector3(0f,0f,0f);
            toScale = Vector3.forward;
        }

        // Methods to support save loading (based on code by gamedevbeginner)

        // MUST LOAD ALL SAVED CHARACTERISTICS FROM EACH LANDSEED'S DATA (as of Week 4: location, rotation, land type, and plant level)
        // Updated for deletion optimization Week like 20 (Sept 16 2023)
        public void loadPositions()
        {
            //// Comment again when cleared

            //PlayerPrefs.DeleteAll(); // deleting all doesn't work since it resets mock environment to nothing
            //                         // ; useful for clearing all prefs after updating system though, just set
            //                         // Virtual Studio -> mock environment back to Living Room or Interior Scene

            //// TODO ALWAYS ACKNOWLEDGE THIS: IT CREATES AN INFINITE LOOP ON PC BUT IS USEFUL ON PHONE FOR CLEANING UP
            if (!PlayerPrefs.HasKey("sokka")) // TODO change clear code (e.g., zabumafu) every build
            {
                PlayerPrefs.DeleteAll(); // deleting all doesn't work since it resets mock environment to nothing
                                         // ; useful for clearing all prefs after updating system though, just set
                                         // Virtual Studio -> mock environment back to Living Room
                PlayerPrefs.SetString("sokka", "doesn'tmatter");
            }


            if (!PlayerPrefs.HasKey("numFruits"))
            {
                PlayerPrefs.SetInt("numFruits", 0);
            }
            else
            {
                // add all the froots to the inventory
                int froots = PlayerPrefs.GetInt("numFruits");
                if (panelie.transform.childCount == 0) { // only run if the apprpriate fruits have not already been added
                    for (int i = 0; i < froots && i < 6; i++) // TODO Adjust limits # to 7 displayed
                    {
                        GameObject newFruit = Instantiate(fruitImg); // TODO FIGURE OUT NAMESPACES
                        newFruit.transform.SetParent(panelie.transform, false);
                        newFruit.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                    }
                }
            }

            if (!PlayerPrefs.HasKey("numPlaced"))
            {
                PlayerPrefs.SetInt("numPlaced", 0); // NOTE Not the number of visible lands; but the number of total times placement occurs in the game, regardless of deletion
            }

            // Deletion optimization algorithm (scrubs deleteds from prefs)
            int erLand = 1; // In honor of Erling Haaland
            int numPlaced = PlayerPrefs.GetInt("numPlaced");
            Debug.Log("# was placed: "+numPlaced);
            for (int i = 1; i <= numPlaced; i++) // loops through all placed objects; <= because 1-indexed
            {
                if (!PlayerPrefs.HasKey("LandSeed" + i + "isNothing")) // current land is dead
                {
                    if (erLand != i) { // makes sure we aren't deleting where we stand after
                        transplantLand(erLand,i); // copies to erLand from curLand
                        PlayerPrefs.SetString("LandSeed" + i + "isNothing", "doesn'tmatter"); // marks curLand for deletion

                        Debug.Log("# erling: " + (erLand));
                    } else
                    {
                        // do nothing
                    }
                    erLand++;
                }
                if (PlayerPrefs.HasKey("LandSeed" + i + "isNothing")) { // dlete if we set it to nothing
                    purgeLand(i); // dlete curLand
                }
            }
            PlayerPrefs.SetInt("numPlaced", erLand-1); // updates numPlaced for the future; erLand points to the next place we should put things, and since 1 indexed the last used index (erLand-1) is the number of placed
            Debug.Log("# placed now: " + (erLand-1));

            int curLand = 1;
            while (PlayerPrefs.HasKey("LandSeed" + curLand))
            {
                Vector3 loadedPosition = new Vector3(
                PlayerPrefs.GetFloat("LandSeed" + curLand + "posX"),
                PlayerPrefs.GetFloat("LandSeed" + curLand + "posY"),
                PlayerPrefs.GetFloat("LandSeed" + curLand + "posZ"));

                //Debug.Log("LandSeed" + curLand + "posX");
                //Debug.Log("LandSeed" + curLand + "posY"+
                //    "LOADED why: " + PlayerPrefs.GetFloat("LandSeed" + curLand + "posY")
                //    );
                //Debug.Log("LandSeed" + curLand + "posZ");

                Quaternion loadedRotation = new Quaternion(
                PlayerPrefs.GetFloat("LandSeed" + curLand + "rotX"),
                PlayerPrefs.GetFloat("LandSeed" + curLand + "rotY"),
                PlayerPrefs.GetFloat("LandSeed" + curLand + "rotZ"),
                PlayerPrefs.GetFloat("LandSeed" + curLand + "rotW"));

                GameObject depends = placeSeedHere; // then modify it based on the file save; covers all
                                                    // permutations of characteristics

                int plantLevel = 0;
                try
                {
                    plantLevel = PlayerPrefs.GetInt("LandSeed" + curLand + "growthLevel");
                }
                catch 
                {
                }

                int woodPlantLevel = 0;
                try
                {
                    woodPlantLevel = PlayerPrefs.GetInt("LandSeed" + curLand + "growthLevelA");
                }
                catch 
                {
                }

                int fanPlantLevel = 0;
                try
                {
                    fanPlantLevel = PlayerPrefs.GetInt("LandSeed" + curLand + "growthLevelB");
                }
                catch 
                {
                }

                if (!PlayerPrefs.HasKey("LandSeed" + curLand + "isNothing")) // INTEGRAL TO CHECK FIRST, SINCE IT MAY ALSO BE WATER OR GRASS ETC
                {
                    if (PlayerPrefs.HasKey("LandSeed" + curLand + "isGrass"))
                    {
                        depends.transform.GetChild(0).gameObject.SetActive(true); // 0 is GrassLand, 1 is WaterLand, and 2 is Seed
                        depends.transform.GetChild(1).gameObject.SetActive(false); // 0 is GrassLand, 1 is WaterLand, and 2 is Seed
                    }
                    else if (PlayerPrefs.HasKey("LandSeed" + curLand + "isWater"))
                    {
                        depends.transform.GetChild(0).gameObject.SetActive(false);
                        depends.transform.GetChild(1).gameObject.SetActive(true);
                    }

                    for (int i = 2; i < depends.transform.childCount; i++)
                    {
                        depends.transform.GetChild(i).gameObject.SetActive(false);
                    }

                    // have a systematic way of gauging with the degree of growth of
                    // the plant (0 : no egg, 1 : egg, 2 : first droplet added, 3 : second, 4 : third/full grown, 5 : fruit)
                    if (plantLevel == 0) // no egg or plant
                    {
                    }
                    else if (plantLevel == 1) // just egg
                    {
                        // egg
                        depends.transform.GetChild(2).gameObject.SetActive(true);

                        // no plant
                        depends.transform.GetChild(3).gameObject.SetActive(false);
                        depends.transform.GetChild(4).gameObject.SetActive(false);
                        depends.transform.GetChild(5).gameObject.SetActive(false);
                        depends.transform.GetChild(6).gameObject.SetActive(false);
                    }
                    else if (plantLevel == 2) // short plant
                    {
                        // no egg
                        depends.transform.GetChild(2).gameObject.SetActive(false);

                        // short plant
                        depends.transform.GetChild(3).gameObject.SetActive(true);

                        // no taller plant
                        depends.transform.GetChild(4).gameObject.SetActive(false);
                        depends.transform.GetChild(5).gameObject.SetActive(false);
                        depends.transform.GetChild(6).gameObject.SetActive(false);
                    }
                    else if (plantLevel == 3) // medium plant
                    {
                        // no egg
                        depends.transform.GetChild(2).gameObject.SetActive(false);

                        // no short plant
                        depends.transform.GetChild(3).gameObject.SetActive(false);

                        // medium plant
                        depends.transform.GetChild(4).gameObject.SetActive(true);

                        // no taller plant
                        depends.transform.GetChild(5).gameObject.SetActive(false);
                        depends.transform.GetChild(6).gameObject.SetActive(false);
                    }
                    else if (plantLevel == 4) // tall plant
                    {
                        // no egg
                        depends.transform.GetChild(2).gameObject.SetActive(false);

                        // no short plant
                        depends.transform.GetChild(3).gameObject.SetActive(false);

                        // no medium plant
                        depends.transform.GetChild(4).gameObject.SetActive(false);

                        // tall plant
                        depends.transform.GetChild(5).gameObject.SetActive(true);

                        // no taller plant
                        depends.transform.GetChild(6).gameObject.SetActive(false);

                    }
                    else if (plantLevel == 5) // fruit plant
                    {
                        // no egg
                        depends.transform.GetChild(2).gameObject.SetActive(false);

                        // no shorter plant
                        depends.transform.GetChild(3).gameObject.SetActive(false);
                        depends.transform.GetChild(4).gameObject.SetActive(false);
                        depends.transform.GetChild(5).gameObject.SetActive(false);

                        // fruit plant
                        depends.transform.GetChild(6).gameObject.SetActive(true);
                    }
                    else if (plantLevel == 6) // dry plant
                    {
                        // no egg
                        depends.transform.GetChild(2).gameObject.SetActive(false);

                        // no shorter plant
                        depends.transform.GetChild(3).gameObject.SetActive(false);
                        depends.transform.GetChild(4).gameObject.SetActive(false);
                        depends.transform.GetChild(5).gameObject.SetActive(false);

                        // fruit plant
                        depends.transform.GetChild(6).gameObject.SetActive(false);

                        // dry plant
                        depends.transform.GetChild(16).gameObject.SetActive(true);
                    }

                    // TODO will include more elses for more grown plants in the future
                    if (woodPlantLevel == 0) // no egg or plant
                    {
                    }
                    else if (woodPlantLevel == 1) // just egg
                    {
                        depends.transform.GetChild(9).gameObject.SetActive(true);
                    }
                    else if (woodPlantLevel == 2) // short plant
                    {
                        depends.transform.GetChild(10).gameObject.SetActive(true);
                    }
                    else if (woodPlantLevel == 3) // medium plant
                    {
                        depends.transform.GetChild(11).gameObject.SetActive(true);
                    }
                    else if (woodPlantLevel == 4) // medium plant
                    {
                        depends.transform.GetChild(12).gameObject.SetActive(true);
                    }

                    if (fanPlantLevel == 0) // no egg or plant
                    {
                    }
                    else if (fanPlantLevel == 1) // just egg
                    {
                        depends.transform.GetChild(13).gameObject.SetActive(true);
                    }
                    else if (fanPlantLevel == 2) // short plant
                    {
                        depends.transform.GetChild(14).gameObject.SetActive(true);
                    }
                    else if (fanPlantLevel == 3) // medium plant
                    {
                        depends.transform.GetChild(15).gameObject.SetActive(true);
                    }
                }
                else
                {
                    //// nothing should be visible
                    for (int i = 0; i < depends.transform.childCount; i++)
                    {
                        depends.transform.GetChild(i).gameObject.SetActive(false);
                    }
                }
                // Places and iterates
                // Place the objects and add to _placedObjects
                GameObject toPlace = Instantiate(placeSeedHere /*equivalent to depends since same reference*/, loadedPosition, loadedRotation);
                _placedObjects.Add(toPlace);
                toPlace.name = "LandSeed" + curLand;
                curLand++;
            }


            //// Now that I think about it, not even necessary
            //clearSavedPrefs(); // more stable deletion of all saved LandSeeds
        }

        // Eliminates any trace of the land; also a nice list of every key we have for the lands
        // TODO Expand if we grow the key list
        private void purgeLand(int curLand)
        {
            PlayerPrefs.DeleteKey("LandSeed" + curLand);
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "isNothing");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "posX");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "posY");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "posZ");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "rotX");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "rotY");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "rotZ");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "rotW");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "growthLevel");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "growthLevelA");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "growthLevelB");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "isGrass");
            PlayerPrefs.DeleteKey("LandSeed" + curLand + "isWater");
        }

        // to: the number of the land to copy to
        // from: the number of the land to copy from
        // TODO Expand if we grow the key list
        private void transplantLand(int to, int from)
        {

            // Getting all the stuff from from
            // moves the floats
            Vector3 loadedPosition = new Vector3(
            PlayerPrefs.GetFloat("LandSeed" + from + "posX"),
            PlayerPrefs.GetFloat("LandSeed" + from + "posY"),
            PlayerPrefs.GetFloat("LandSeed" + from + "posZ"));

            Quaternion loadedRotation = new Quaternion(
            PlayerPrefs.GetFloat("LandSeed" + from + "rotX"),
            PlayerPrefs.GetFloat("LandSeed" + from + "rotY"),
            PlayerPrefs.GetFloat("LandSeed" + from + "rotZ"),
            PlayerPrefs.GetFloat("LandSeed" + from + "rotW"));

            // moves the ints
            int plantLevel = 0;
            try
            {
                plantLevel = PlayerPrefs.GetInt("LandSeed" + from + "growthLevel");
            }
            catch
            {
            }

            int woodPlantLevel = 0;
            try
            {
                woodPlantLevel = PlayerPrefs.GetInt("LandSeed" + from + "growthLevelA");
            }
            catch
            {
            }

            int fanPlantLevel = 0;
            try
            {
                fanPlantLevel = PlayerPrefs.GetInt("LandSeed" + from + "growthLevelB");
            }
            catch
            {
            }

            // moves the booleans, and sets the booleans
            if (!PlayerPrefs.HasKey("LandSeed" + from + "isNothing")) // INTEGRAL TO CHECK FIRST, SINCE IT MAY ALSO BE WATER OR GRASS ETC
            {
                if (PlayerPrefs.HasKey("LandSeed" + from + "isGrass"))
                {
                    PlayerPrefs.SetString("LandSeed" + to + "isGrass", "doesn'tmatter");
                }
                else if (PlayerPrefs.HasKey("LandSeed" + from + "isWater"))
                {
                    PlayerPrefs.SetString("LandSeed" + to + "isWater", "doesn'tmatter");
                }
            } else // nothing lel
            {
                    PlayerPrefs.SetString("LandSeed" + to + "isNothing", "doesn'tmatter");
            }

            // "creates" the to land
            PlayerPrefs.SetString("LandSeed" + to, "doesn'tmatter");

            // sets the ints and floats

            // Growth levels
            PlayerPrefs.SetInt("LandSeed" + to + "growthLevel",plantLevel);
            PlayerPrefs.SetInt("LandSeed" + to + "growthLevelA",woodPlantLevel);
            PlayerPrefs.SetInt("LandSeed" + to + "growthLevelB",fanPlantLevel);
            // position
            PlayerPrefs.SetFloat("LandSeed" + to + "posX", loadedPosition.x);
            PlayerPrefs.SetFloat("LandSeed" + to + "posY", loadedPosition.y);
            PlayerPrefs.SetFloat("LandSeed" + to + "posZ", loadedPosition.z);
            // rotation
            PlayerPrefs.SetFloat("LandSeed" + to + "rotX", loadedRotation.x);
            PlayerPrefs.SetFloat("LandSeed" + to + "rotY", loadedRotation.y);
            PlayerPrefs.SetFloat("LandSeed" + to + "rotZ", loadedRotation.z);
            PlayerPrefs.SetFloat("LandSeed" + to + "rotW", loadedRotation.w);
        }

        private void OnAnyARSessionDidInitialize(AnyARSessionInitializedArgs args)
        {
            _session = args.Session;
            _session.Deinitialized += OnSessionDeinitialized;
        }

        private void OnSessionDeinitialized(ARSessionDeinitializedArgs args)
        {
            Debug.Log("onsesh"); // proves this happens first
            if (_placedObjects.Count > 0)
            {
                // Saves last in order to preserve slider shift
                saveLast();
            }
            ClearObjects();
        }

        private void OnDestroy()
        {
            ARSessionFactory.SessionInitialized -= OnAnyARSessionDidInitialize;

            _session = null;
            Debug.Log("ondest");
            ClearObjects();
        }

        private void ClearObjects()
        {
            Debug.Log("I was called!" + (_placedObjects.Count > 0));
            foreach (var placedObject in _placedObjects)
            {
                Destroy(placedObject);
            }

            _placedObjects.Clear();
        }

        private void FixedUpdate()
        {
            if (!paused && isGame() && won)
            {
                Debug.Log("I'm pausing with the kids");
                // celebrate
                gamestructions.SetActive(true); // no more need for instructions
                swipestructions.SetActive(true); // need for instructions
                winstructions.SetActive(true); // need for instructions
                PauseGame();
            }
            // controls landing
            if (robobody != null)
            {
                //if (robobody.velocity.y != 0f) {
                //    Debug.Log("Robobody velocitee: " + robobody.velocity.y);
                //}
                float hmm = robobody.velocity.y;
                if (jumpinUp && hmm < 0f)
                {
                    // freezes mid air after zenith
                    Vector3 newVeloc = new Vector3(0f, robobody.velocity.y, 0f);
                robobody.velocity = newVeloc;
                jumpinUp = false;
                

                } else if (hmm == 0f)
                {
                    jumpinUp = false;
                    jumpinDown = false;
                }
                if (trime > 75)
                {
                    trime = 0;
                }
                if (trime == 60)
                {
                    trime = 0;
                    // flips all active states for plats
                    for (int i = 2; i < 2 + (numPlats*2); i+=2)
                    {
                        _placedObjects[i].gameObject.SetActive(!_placedObjects[i].gameObject.activeSelf);
                        _placedObjects[i + 1].gameObject.SetActive(!_placedObjects[i + 1].gameObject.activeSelf);
                    }
                }

                // Checks for loss/fails
                if (robobody.position.y < botStartY - 0.5f) // if you dropped a bit
                {
                    gameOverScreen.gameObject.SetActive(true);
                    //prizePanel.gameObject.SetActive(false);
                    for (int a = 0; a < prizePanel.transform.childCount; a++)
                    {
                        prizePanel.transform.GetChild(a).gameObject.SetActive(false);
                    }

                        _placedObjects[1].gameObject.SetActive(false);
                    // TODO this depends on robot being the second placed object
                    //PlayerPrefs.SetInt("negOneIfNoSeedReward", -1);
                }
            }
            trime++;
        }

        private void Update()
        {
            if (_session == null)
            {
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

            if (PlatformAgnosticInput.touchCount <= 0)
            {
                return;
            }
            if (!paused)
            {
                var touch = PlatformAgnosticInput.GetTouch(0);

                if (touch.phase == TouchPhase.Began)
                {
                    startPos = touch.position;
                    directionChosen = false;
                    TouchBegan(touch);
                }
                else if (touch.phase == TouchPhase.Moved)
                {
                    direction = touch.position - startPos;
                }
                else if (touch.phase == TouchPhase.Ended)
                {
                    directionChosen = true;
                    //Debug.Log("CHANGE: " + (direction));
                }
                if (directionChosen) // sort of unnecessary, but this way it's more like docs
                {
                    TouchEnded(touch);

                    // gotta reset b4 next touch
                    startPos = new Vector2();
                    directionChosen = false;
                    direction = new Vector2();
                }
            } else // what do when paused
            {
                Debug.Log("paused update");
                // instructions read
                ResumeGame();

                // deactivate instructions
                // celebrate
                gamestructions.SetActive(false); // no more need for instructions
                swipestructions.SetActive(false); // need for instructions
                winstructions.SetActive(false); // need for instructions
            }
        }

        private void TouchEnded(Touch touch)
        {
            if (!paused) {
                if (rinder.donePlacing) // platforming time
                {

                    if (direction.y > 100 && !jumpinUp) // if at least a little up
                    {
                        jumpinUp = true;
                        jumpinDown = false;
                        // the bot will always be the second thing placed, after the goal
                        //Debug.Log("The name of our one and only seed: "+ _placedObjects[1].name);
                        var bod = _placedObjects[1].transform.GetChild(7).gameObject.GetComponent<Rigidbody>(); // gets the bot's bod
                        bod.constraints = RigidbodyConstraints.None; // enables physics
                        bod.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;

                        //float unitV = jumpMag; // how much we scale right direction by
                        //Vector3 absoluteUnit = placedGoal.transform.position - _placedObjects[1].transform.GetChild(7).position; // first plat loc - start

                        //Vector3 newVeloc = absoluteUnit; 
                        ////newVeloc.x = 0.3f;
                        ////newVeloc.z = 0.3f;

                        //while (newVeloc.magnitude < unitV)
                        //{
                        //    newVeloc *= 1.1f;
                        //}

                        // TODO Make y some function of platform height difference, and x and z based on the path to the next plat or sumn

                        // Based on Maple-emailed calculations
                        float tiempo = 1f;
                        // velocity = distance to zenith/time to zenith
                        float velocXZ = 3*deltaXZ;

                        Vector3 newVeloc = new Vector3(velocXZ, (float)System.Math.Sqrt(9.81f), velocXZ);

                        bod.velocity = newVeloc;
                        Debug.Log("Robobody velocity: " + robobody.velocity.y);
                        //Debug.Log("My demons: " + _placedObjects[1].GetComponent<Rigidbody>().constraints);
                    } else if (direction.y < -100 && !jumpinDown)
                    {
                        jumpinDown = true;
                        jumpinUp = false;
                        // the bot will always be the second thing placed, after the goal
                        Debug.Log("GOING DOWN");
                        var bod = _placedObjects[1].transform.GetChild(7).gameObject.GetComponent<Rigidbody>(); // gets the bot's bod
                        bod.constraints = RigidbodyConstraints.None; // enables physics
                        bod.constraints = RigidbodyConstraints.FreezeRotationZ | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationX;

                        // Based on Maple-emailed calculations
                        float tiempo = 0.03058103976f * (float)System.Math.Sqrt(218f * deltaY + 109f * cubeScale);
                        // velocity = distance to zenith/time to zenith
                        float velocXZ = -1f * deltaXZ / tiempo;

                        Vector3 newVeloc = (new Vector3(velocXZ, 0.01f, velocXZ));

                        bod.velocity = newVeloc;
                        //Debug.Log("My demons: " + _placedObjects[1].GetComponent<Rigidbody>().constraints);
                    }
                }
            } else // what to when paused
            {
                Debug.Log("paused touch ended"); // never runs?

            }
        }


        // SAVE EVERY ASPECT OF THE LAND (as of Week 3: location, land type, and plant level [handled in seed manager])
        private bool savePosition(GameObject toPlace)
        {
            if (toPlace == null)
            {
                return false;
            }
            // SAVE LANDS HERE
            int curLand = 1;
            while (PlayerPrefs.HasKey("LandSeed" + curLand))
            {
                curLand++;
            }

            PlayerPrefs.SetString("LandSeed" + curLand, "doesn'tmatter"); // using this as a boolean flag, sort of

            // position
            PlayerPrefs.SetFloat("LandSeed" + curLand + "posX", toPlace.transform.position.x);
            PlayerPrefs.SetFloat("LandSeed" + curLand + "posY", toPlace.transform.position.y);
            PlayerPrefs.SetFloat("LandSeed" + curLand + "posZ", toPlace.transform.position.z);

            // rotation
            PlayerPrefs.SetFloat("LandSeed" + curLand + "rotX", toPlace.transform.rotation.x);
            PlayerPrefs.SetFloat("LandSeed" + curLand + "rotY", toPlace.transform.rotation.y);
            PlayerPrefs.SetFloat("LandSeed" + curLand + "rotZ", toPlace.transform.rotation.z);
            PlayerPrefs.SetFloat("LandSeed" + curLand + "rotW", toPlace.transform.rotation.w);

            // add landseed descriptors based on activations
            // 0 is GrassLand, 1 is WaterLand, and 2 is Seed
            if (toPlace.transform.GetChild(0).gameObject.activeSelf)
            {
                //Debug.Log("IT'S A GRASS!");
                PlayerPrefs.SetString("LandSeed" + curLand + "isGrass", "doesn'tmatter");
            }
            else if (toPlace.transform.GetChild(1).gameObject.activeSelf)
            {
                //Debug.Log("IT'S A WATER!");
                PlayerPrefs.SetString("LandSeed" + curLand + "isWater", "doesn'tmatter");
            }

            toPlace.name = "LandSeed" + curLand;

            // there's no egg, etc. by default
            PlayerPrefs.SetInt("LandSeed" + curLand + "growthLevel", 0);

            //Debug.Log(toPlace);
            return true;
        }

        private void TouchBegan(Touch touch)
        {

            var currentFrame = _session.CurrentFrame;
            if (currentFrame == null)
            {
                return;
            }

            var results = currentFrame.HitTest // A list of surfaces
            (
              Camera.pixelWidth,
              Camera.pixelHeight,
              touch.position,
              HitTestType
            );

            int count = results.Count;
            //Debug.Log("Hit test results: " + count);

            if (count <= 0)
                return;

            // Get the closest result
            var result = results[0];

            var hitPosition = result.WorldTransform.ToPosition();

            //// custom offset position for testing new object
            //var offsetPosition = new Vector3(hitPosition.x, hitPosition.y+2.0f, hitPosition.z);

            // paused or not
            if (!paused)
            {
                // instantiates the prefab at the given position of the hit
                // Quaternion.identity is no rotation, I think

                // change hitPosition to something derived from rinder's cursor location
                GameObject toPlace = null;
                Vector3 newPosition = rinder.getLoc();
                Quaternion newRotation = rinder.getRot();
                if (isLand() || (isGame() && _placedObjects.Count == 0))
                { // we're limiting to one placement if in game mode

                    if (isLand())
                    {
                        // placement code
                        if (!inFruitZone && !inMenuZone && !inGradientZone && !inSliderZone) { // makes sure to only place in valid parts of the viewport

                            saveLast(); // saves to playerprefs



                            toPlace = Instantiate(PlacementObjectPf, newPosition, newRotation);
                            Debug.Log("toPlace #: "+toPlace.name.Substring(8));
                            _placedObjects.Add(toPlace);
                            justPlaced = true;
                            PlayerPrefs.SetInt("zeroIfLandNotPlaced", 1);
                            PlayerPrefs.SetInt("numPlaced", PlayerPrefs.GetInt("numPlaced")+1);
                            Debug.Log("# numero placed: "+ PlayerPrefs.GetInt("numPlaced"));
                        }
                    }
                    else if (isGame() && _placedObjects.Count == 0)
                    {// we know the first object was just placed, so the platforms should be too
                        

                        GameObject goal = autoGoalPlacer(result.Anchor, newRotation);
                        if (goal != null) // goal found: successful
                        {
                            Vector3 newP = camTransform.position+camTransform.TransformDirection(toScale * botScale)+botDisplacement;
                            Quaternion newR = new Quaternion();

                            toPlace = Instantiate(PlacementObjectPf, newP, newR); // drop start, since goal is already there
                            // should be ready for the physicsing, since landseed is rigidbody

                            _placedObjects.Add(toPlace); // add to placed objects list
                            robobody = _placedObjects[1].transform.GetChild(7).gameObject.GetComponent<Rigidbody>();

                            ogPlaced = new Vector3(toPlace.transform.position.x, toPlace.transform.position.y, toPlace.transform.position.z); // we use this instead of object since object moves
                            placedGoal = goal;

                            //// Create the floor
                            //createFloor(result.Anchor);

                            placePlatforms(toPlace, goal); // interpolate platforms between the goal and bot
                            rinder.SetNewCursor(true); // indicates we're done placing

                        }
                        else // no goal found: unsuccessful
                        {
                            // indicate that they should make sure to scan at least two heights, then place their bot; goal 
                            // will be generated or they will be told to re-place bot and/ or rescan until they get it

                            numFails++;



                            //_placedObjects.Remove(toPlace);
                            //Destroy(toPlace);

                            // Try again sucka
                            Debug.Log("Try again with the placement, sucka");
                        }
                    }
                    else
                    {
                        RaycastHit hitData;
                        Ray ray = Camera.ScreenPointToRay(touch.position);
                        Physics.Raycast(ray, out hitData);

                        if (Physics.Raycast(ray, out hitData)) // as per gamedevbeginner's raycast tutorial
                        {
                            Debug.Log("Where you struck: " + hitData.point);
                        }
                    }
                }

                if (!isGame())
                { // no need to save when impermanent like this
                    savePosition(toPlace);
                    Debug.Log("toPlace #: " + toPlace.name.Substring(8));
                }

                //// testing new lands
                //GameObject toPlace = Instantiate(catchAll, offsetPosition, Quaternion.identity);
                //_placedObjects.Add(toPlace);
            } else
            { // what happens when paused
                Debug.Log("paused touch began"); // never runs ig?
            }
            var anchor = result.Anchor;
            Debug.LogFormat
            (
              "Spawning cube at {0} (anchor: {1})",
              hitPosition.ToString("F4"),
              anchor == null
                ? "none"
                : anchor.AnchorType + " " + anchor.Identifier
            );

        }

        // TODO yeah this is some serious code duplication, SAME METHOD IN ARHITTESTER, SWIPE_MENU, AND SWIPE_FRUIT
        // NOTE: THIS IS THE PLACEMENT DEADZONE; TRUE IS THE LIFEZONE FOR PLACEMENT
        //private bool notInDeadzone(Touch toca)
        //{
        //    Debug.Log("Touch x: "+toca.position.x);
        //    Debug.Log("Touch y: " + toca.position.y);

        //    // since touch is always a pair of nonnegative values, we can assume that it's dead about 1/5 up the screen


        //    // so... there's a scaling factor to account for here: the numbers given by touche's position are actually about 2 times the canvas dimensions;
        //    // ergo we should divide our touch position by 2 before using it in the 1/6 calculation
        //    float converTouch = toca.position.y / 2;
        //    if (converTouch > (canvasTrans.rect.height / 3) && converTouch < (4 * canvasTrans.rect.height / 5))
        //    {
        //        Debug.Log("Bottom 1/5!");
        //        return true;
        //    }
        //    Debug.Log("Top 4/5!");
        //    return false;
        //}

        // Programmatically determines the location to place the goal relative to dropbot start selected, working backward
        private GameObject autoGoalPlacer(Niantic.ARDK.AR.Anchors.IARAnchor anchorPlane, Quaternion toPlace) // note the elite namespacing based on error msg
        {
            //GameObject goal = null;
            //float yToBeat = anchorPlane.Transform.ToPosition().y+1; // how high the placement was

            //var foundPlanes = _session.CurrentFrame.Anchors; // all the "anchors" (planes etc that link virtual and real world) of current frame

            //foreach (var eye in foundPlanes)
            //{
            //    if (eye.Transform.ToPosition().y > (yToBeat + 0.5)) // TODO there should probably be a threshold on how much higher it must be to be a valid goal
            //    {
            //        Vector3 goalPos = eye.Transform.ToPosition();  // okay, so the y value will obviously be eye's, but the x and z likely should depend on the height difference (like perhaps maintaining a slope of 45 degrees is a good idea)
            //        goal = Instantiate(goalie, goalPos, toPlace);
            //        _placedObjects.Add(goal);
            //        //Debug.Log("Goal placed at: "+goal.transform.position);
            //        break;
            //    }
            //}

            //// If it failed to place
            //if (goal == null && numFails == 1)
            //{
            //    Vector3 goalPos = new Vector3(0, yToBeat+ 0.5f,0);  // okay, so the y value will obviously be eye's, but the x and z likely should depend on the height difference (like perhaps maintaining a slope of 45 degrees is a good idea)
            //    goal = Instantiate(goalie, goalPos, toPlace);
            //    _placedObjects.Add(goal);
            //}


            //return goal;
            GameObject goal = null;
            //float yToBeat = anchorPlane.Transform.ToPosition().y+0.5f; // how high the placement was

            // TODO UNCOMMENT IF NEW PLACING DUN'T WORK
            //Vector3 goalPos = new Vector3(0, yToBeat + 0.5f, 0);  // okay, so the y value will obviously be eye's, but the x and z likely should depend on the height difference (like perhaps maintaining a slope of 45 degrees is a good idea)
            Vector3 goalPos = new Vector3(0, 0.75f, 0);
            goalPos = goalPos + (camTransform.position + camTransform.TransformDirection(toScale * botScale) + botDisplacement);

            System.Random rnd = new System.Random();
            int num = rnd.Next();
            if (num%3 == 0) { // one third chance of water todo modify if you wanna change that
                goal = Instantiate(sandgoal, goalPos, toPlace);
            } else
            {
                num = rnd.Next();
                goal = Instantiate(watergoal, goalPos, toPlace);
                Debug.Log("New random: "+num%3);
                PlayerPrefs.SetInt("negOneIfNoSeedReward", num%3);
            }
            _placedObjects.Add(goal);
            return goal;
        }

        // TODO Implement this
        // interpolate platforms between the goal and bot
        private void placePlatforms(GameObject start, GameObject goal)
        {

            Vector3 absoluteUnit = goal.transform.position - start.transform.position;

            float numsPlats = absoluteUnit.y/deltaY;
            numPlats = 0; // NOTE: Two different variables lol

            // Create like a unit Vector3 (goal-start) along which the calculate the interpolated "points" for the prisms, as well as their sizes
            for (int i = 0; i < numsPlats; i++)
            {
                // use createPrimitives to make some right lovely rectangular prisms
                /*GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);*/ // TODO UNCOMMENT IF WE END UP REVERTING TO CREATEPRIMITIVE
                GameObject cube = Instantiate(plats);
                GameObject platland = Instantiate(grassplat);

                Vector3 newPos = start.transform.position + (new Vector3(deltaXZ, deltaY, deltaXZ)) * i ;
                //newPos.y = (start.transform.position + (absoluteUnit / (numsPlats + 1)) * (i)).y; // make it more reachable
                cube.transform.position = newPos;
                platland.transform.position = newPos;

                //cube.transform.rotation = start.transform.rotation;

                // Rotates each next platform slightly, but could complicate jump calcs
                //cube.transform.Rotate(0, angleLeft/(numPlats + 1) * (i+1), 0);

                //cube.transform.localScale = new Vector3(0.3f, 0.03f, 0.3f);

                /*cube.transform.localScale = new Vector3(cubeScale, cubeScale, cubeScale);
                cube.AddComponent<Rigidbody>(); */ // TODO UNCOMMENT IF WE END UP REVERTING TO CREATEPRIMITIVE

                var bod = cube.GetComponent<Rigidbody>();

                // freezes that bod
                bod.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ
                    | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY
                    | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX
                    ;

                //bod.isKinematic = true;



                //// Testing collisions with new spawns

                //GameObject cube2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                //cube2.transform.position = start.transform.position + (absoluteUnit / (numPlats + 1)) * (i + 1);
                //cube2.transform.localScale = new Vector3(0.3f, 0.03f, 0.3f);
                //cube2.AddComponent<Rigidbody>();
                //cube2.transform.position += Vector3.up * 10;
                //_placedObjects.Add(cube2);


                //GameObject invisiCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //invisiCube.transform.position = start.transform.position + (new Vector3(deltaXZ, deltaY, deltaXZ)) * i;
                //invisiCube.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
                //invisiCube.transform.rotation = start.transform.rotation;
                //invisiCube.GetComponent<MeshRenderer>().material = invisible;
                //invisiCube.AddComponent<Rigidbody>();
                //var bod2 = invisiCube.GetComponent<Rigidbody>();
                //// freezes that bod
                //bod2.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationZ
                //    | RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezeRotationY
                //    | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotationX
                //    ;

                //bod2.isKinematic = false;
                //_placedObjects.Add(invisiCube);




                _placedObjects.Add(cube);
                _placedObjects.Add(platland);
                if ((i + 1) % 2 == 0) // every other is born invisible
                {
                    cube.gameObject.SetActive(false);
                    platland.gameObject.SetActive(false);
                }

                if (i == 0) // First platform
                {
                    botStartY = cube.transform.position.y;
                }

                numPlats++; // updates global number of platforms
            }

            // Shifts start little above starting platform
            start.transform.position += new Vector3(0f, cubeScale/2, 0f);

        // random switching code (broken)
            //bool multByNeg = trime % 2 == 0;
            //if (multByNeg)
            //{
            //    deltaXZ *= -1f; // fine since we only call once
            //}
            if (deltaXZ > 0f) {
                start.transform.Rotate(0.0f, 45.0f, 0.0f);
            } else
            {
                start.transform.Rotate(0.0f, -45.0f, 0.0f);
            }

            // Shifts goal to after final platform
            Vector3 goalPos = start.transform.position + (new Vector3(deltaXZ, deltaY, deltaXZ)) * (numsPlats);
            goal.transform.position = goalPos;

            // TODO control idea: bot walks towards wherever you are tapping relative to it, and then swiping (has to be a straight line if you don't want to deviate) allows for jumps

            
        }

        // Call to preserve location; typically used in order to preserve slider shift upon next placement
        public void saveLast()
        {
            if (_placedObjects.Count > 0) {
                //Debug.Log(
                //    "BEFORESET why: " + _placedObjects[_placedObjects.Count - 1].name + ", " + PlayerPrefs.GetFloat(_placedObjects[_placedObjects.Count - 1].name + "posY")
                //);

                PlayerPrefs.SetFloat(_placedObjects[_placedObjects.Count - 1].name + "posX",
                            _placedObjects[_placedObjects.Count - 1].transform.position.x);
                PlayerPrefs.SetFloat(_placedObjects[_placedObjects.Count - 1].name + "posY",
                    _placedObjects[_placedObjects.Count - 1].transform.position.y);
                PlayerPrefs.SetFloat(_placedObjects[_placedObjects.Count - 1].name + "posZ",
                    _placedObjects[_placedObjects.Count - 1].transform.position.z);

                //Debug.Log(
                //    "AFTERSET why: " + _placedObjects[_placedObjects.Count - 1].name + ", " + PlayerPrefs.GetFloat(_placedObjects[_placedObjects.Count - 1].name + "posY")
                //);
            }
        }

        void PauseGame()
        {
            Time.timeScale = 0;
            paused = true;
        }
        void ResumeGame()
        {
            Time.timeScale = 1;
            paused = false;
        }

        // USE ALONG WITH ISLAND() SINCE CURMODE ISN'T IN NAMESPACE
        private bool isGame()
        {
            return placeSeedHere.transform.GetChild(7).gameObject.activeSelf == true;
        }

        // REIMPLEMENT IF MORE LANDS ARE ADDED
        private bool isLand()
        {
            return placeSeedHere.transform.GetChild(0).gameObject.activeSelf == true || placeSeedHere.transform.GetChild(1).gameObject.activeSelf == true;
        }

        // Deletes and saves all placed object
        public void flattenAllPlaced()
        {
            ClearObjects();
            PlayerPrefs.Save();
        }

        // DON'T SAVE LANDS OR GAMEOBJECTS HERE, I THINK IT THROWS ERRORS!
        void OnApplicationQuit()
        {
            PlayerPrefs.Save();
            Debug.Log("THIS APPLICATION QUITTED!");
        }
    }
}

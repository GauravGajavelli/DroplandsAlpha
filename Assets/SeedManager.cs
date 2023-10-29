/**
 * 
 * Based on "The Basics" Lightship tutorial video with throwing ball demo
 * 
 * 
 */

//Standard Unity/C# functionality
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

//These tell our project to use pieces from the Lightship ARDK
using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;

//Define our main class; THIS CLASS WILL ESSENTIALLY HANDLE IN-GAME VIRTUAL COLLISIONS, I THINK
public class SeedManager : MonoBehaviour
{
    //Variables we'll need to reference other objects in our game
    public Camera _mainCamera;  //This will reference the MainCamera in the scene, so the ARDK can leverage the device camera
    IARSession _ARsession;  //An ARDK ARSession is the main piece that manages the AR experience

    public GameMode curMode; // scriptable object containing the current game mode (seed or land time)

    // for adding items to scroll panel
    public GameObject panelie;
    public GameObject fruitImg;
    public GameObject inventoryButton; // to get inventoryscrip reference to call the button click methods based on the current scroll
    private Niantic.ARDKExamples.Helpers.inventoryScrip hmm;
    public GameObject glasContent;
    private swipe_menu swipey;

    // Start is called before the first frame update
    void Start()
    {
        //ARSessionFactory helps create our AR Session. Here, we're telling our 'ARSessionFactory' to listen to when a new ARSession is created, then call an 'OnSessionInitialized' function when we get notified of one being created
        ARSessionFactory.SessionInitialized += OnSessionInitialized;

        hmm = inventoryButton.GetComponent<Niantic.ARDKExamples.Helpers.inventoryScrip>();
        swipey = glasContent.GetComponent<swipe_menu>();
        PlayerPrefs.SetInt("zeroIfDropNotPlaced", 0);
        PlayerPrefs.SetInt("zeroIfSeedNotPlaced", 0);
    }

    // Update is called once per frame
    void Update()
    {
        //If there is no touch, we're not going to do anything
        if (PlatformAgnosticInput.touchCount <= 0)
        {
            return;
        }

        //If we detect a new touch, call our 'TouchBegan' function
        var touch = PlatformAgnosticInput.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            TouchBegan(touch);
        }
    }

    //This function will be called when a new AR Session has been created, as we instructed our 'ARSessionFactory' earlier
    private void OnSessionInitialized(AnyARSessionInitializedArgs args)
    {
        //Now that we've initiated our session, we don't need to do this again so we can remove the callback
        ARSessionFactory.SessionInitialized -= OnSessionInitialized;

        //Here we're saving our AR Session to our '_ARsession' variable, along with any arguments our session contains
        _ARsession = args.Session;
    }

    //This function will be called when the player touches the screen. For us, we'll have this trigger the shooting of our ball from where we touch.
    private void TouchBegan(Touch touch)
    {
        // checking how the touch position is stored (seems like a pair of points)
        //Debug.Log(_mainCamera.ScreenToWorldPoint(touch.position));

        //Ray ray = _mainCamera.ScreenPointToRay(touch.position);
        RaycastHit hitData;
        //// Construct a ray from the current touch coordinates
        //Physics.Raycast(ray, out hitData);
        //Debug.Log("CHEKING OUT HIT DATA: COLLOSIION : " + hitData);
        //Debug.Log("POSITION: " + touch.position);
        //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hitData.distance, Color.yellow);

        //Ray ray = new Ray(transform.position, transform.forward);
        //RaycastHit hitData;
        //Physics.Raycast(ray, out hitData);

        Ray ray = _mainCamera.ScreenPointToRay(touch.position);
        Physics.Raycast(ray, out hitData);
        //Debug.Log("CHEKING OUT HIT DATA: " + ray + " COLLOSIION : " + hitData.transform);
        //Debug.Log("POSITION: " + touch.position);
        //Debug.Log(Physics.Raycast(ray, out hitData));
        // Create a particle if hit
        //if (Physics.Raycast(ray))
        //    Instantiate(particle, transform.position, transform.rotation);



        if (Physics.Raycast(ray, out hitData)) // as per gamedevbeginner's raycast tutorial
        {
            //// proves that it's hitting the default of each thing
            Debug.Log("Who we hittin boys: "+hitData.transform.gameObject);

            Debug.Log("Seed time? " + curMode.seedTime);

            // Deletion
            if (curMode.deleteTime)
            {
                int whichKid = -1; // the part of the transform that's there
                for (int i = 0; i < hitData.transform.parent.childCount; i++)
                {
                    // TODO this is where I'm stickin in rehatvestin. Still have to add in the reapparance of like reticles based on whatever you're hangin over
                    if (hitData.transform.parent.GetChild(i).gameObject == hitData.transform.gameObject)
                    {
                        Debug.Log("I've found you, you fool: " + hitData.transform.gameObject.name);
                        whichKid = i;
                        break;
                    }
                }

                // number of each item (corresponding to a button), to be accumulated for restoration in playerprefs
                int numDrops = 0;
                int numGrasslands = 0;
                int numSandlands = 0;
                int numCactusseeds = 0;
                int numWoodseeds = 0;
                int numFanseeds = 0;
                // Turns off the current instance
                for (int i = 0; i < hitData.transform.parent.childCount; i++)
                {
                    // TODO this is where I'm stickin in rehatvestin. Still have to add in the reapparance of like reticles based on whatever you're hangin over
                    if (hitData.transform.parent.GetChild(i).gameObject.activeSelf)
                    {
                        //Debug.Log("Chilluns: " + hitData.transform.parent.GetChild(i).gameObject.name);
                        if (i == 0)
                        {
                            numGrasslands++;
                        }
                        else if (i == 1)
                        {
                            numSandlands++;
                        }
                        else if (i >= 2 && i <= 6)
                        { //  cacti lineage, of which there will be only one
                            numCactusseeds++;
                            numDrops += i - 2; // 0 if seed, 1 if small, etc.

                            //if (i == 6) {
                            //    // TODO probably add fruit harvesting here (prolly have it appear in the menu when this action is completed)
                            //    // incremenents the number of fruits
                            //    PlayerPrefs.SetInt("numFruits", PlayerPrefs.GetInt("numFruits") + 1);
                            //    Debug.Log("NUMBERS OF FRUITS: " + (PlayerPrefs.GetInt("numFruits")));

                            //    if (PlayerPrefs.GetInt("numFruits") < 7)
                            //    {
                            //        // Adds it to the scroll panel by making it its child
                            //        GameObject newFruit = Instantiate(fruitImg); // TODO FIGURE OUT NAMESPACES
                            //        newFruit.transform.SetParent(panelie.transform, false);
                            //        newFruit.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                            //    }
                            //}
                        }
                        else if (i >= 9 && i <= 12)
                        { // wood lineage
                            numWoodseeds++;
                            numDrops += i - 9;
                        }
                        else if (i >= 13 && i <= 15)
                        { // fan lineage
                            numFanseeds++;
                            numDrops += i - 13;
                        } else if (i == 16)
                        { // dry cactus, post fruit
                            numCactusseeds++;
                            // no droplets received, just a cactus
                        }
                    }
                    hitData.transform.parent.GetChild(i).gameObject.SetActive(false);
                }

                // Adding all reharvested items TODO never change item button names in canvas, used here
                PlayerPrefs.SetInt("numGrass Button", PlayerPrefs.GetInt("numGrass Button") + numGrasslands);
                PlayerPrefs.SetInt("numWater Button", PlayerPrefs.GetInt("numWater Button") + numSandlands);
                PlayerPrefs.SetInt("numSeed Button", PlayerPrefs.GetInt("numSeed Button") + numCactusseeds);
                PlayerPrefs.SetInt("numWood Seed Button", PlayerPrefs.GetInt("numWood Seed Button") + numWoodseeds);
                PlayerPrefs.SetInt("numFan Seed Button", PlayerPrefs.GetInt("numFan Seed Button") + numFanseeds);
                PlayerPrefs.SetInt("numDroplet Button", PlayerPrefs.GetInt("numDroplet Button") + numDrops);

                // Won't show up on future saves
                PlayerPrefs.SetString(hitData.transform.parent.gameObject.name + "isNothing", "doesn'tmatter"); // subs out "LandSeed"+curLand for name

                curMode.tutorialStage++; // finished deletion
                PlayerPrefs.SetInt("tutStage", PlayerPrefs.GetInt("tutStage") + 1);

                //// I can't do this since it could disrupt the
                //// continuous number of LandSeeds, which is necessary for me to index and loop through them
                //Destroy(hitData.transform.parent.gameObject); // destroys the LandSeed

                swipey.refreshMenu();
            }
            else // not deleting, then the door is open for harvesting
            {
                if (!curMode.prizeTime && hitData.transform.parent.GetChild(6).gameObject.activeSelf == true)
                {
                    hitData.transform.parent.GetChild(6).gameObject.SetActive(false); // turns off the fruit
                    hitData.transform.parent.GetChild(16).gameObject.SetActive(true);// turns on the dry

                    PlayerPrefs.SetInt("numFruits", PlayerPrefs.GetInt("numFruits") + 1);
                    if (PlayerPrefs.GetInt("numFruits") < 7)
                    {
                        // Adds it to the scroll panel by making it its child
                        GameObject newFruit = Instantiate(fruitImg); // TODO FIGURE OUT NAMESPACES
                        newFruit.transform.SetParent(panelie.transform, false);
                        newFruit.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                    }

                    PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevel", 6); // final "growth" level
                }
            }

            // Seed placement
            if (curMode.seedTime) // TODO now needs to be split into 3 cases on the basis of currently selected seed
            {
                if (hitData.transform.parent.GetChild(2).gameObject.activeSelf == false &&
                    hitData.transform.parent.GetChild(9).gameObject.activeSelf == false &&
                    hitData.transform.parent.GetChild(13).gameObject.activeSelf == false &&
                    // seeds above
                    hitData.transform.parent.GetChild(3).gameObject.activeSelf == false &&
                    hitData.transform.parent.GetChild(4).gameObject.activeSelf == false &&
                    hitData.transform.parent.GetChild(5).gameObject.activeSelf == false &&
                    hitData.transform.parent.GetChild(6).gameObject.activeSelf == false &&
                    // cacti above
                    hitData.transform.parent.GetChild(10).gameObject.activeSelf == false &&
                    hitData.transform.parent.GetChild(11).gameObject.activeSelf == false &&
                    hitData.transform.parent.GetChild(12).gameObject.activeSelf == false &&
                    // wood above, fan below
                    hitData.transform.parent.GetChild(14).gameObject.activeSelf == false &&
                    hitData.transform.parent.GetChild(15).gameObject.activeSelf == false
                    /*MAKE THIS CHECK FOR ALL PLANT STAGES*/)
                { // ensures that we don't make a seed appear where a plant already is
                    PlayerPrefs.SetInt("zeroIfSeedNotPlaced", 1);
                    if (hmm.landSeed.transform.GetChild(2).gameObject.activeSelf == true) // cactus seed
                    {
                        Debug.Log("TWOOOOO");
                        hitData.transform.parent.GetChild(2).gameObject.SetActive(true); // turns on the seed
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevel", 1); // subs out "LandSeed"+curLand for name
                    }
                    else if (hmm.landSeed.transform.GetChild(9).gameObject.activeSelf == true)
                    {
                        Debug.Log("NINTHHH");
                        hitData.transform.parent.GetChild(9).gameObject.SetActive(true); // turns on the seed
                                                                                         //PlayerPrefs.SetInt("LandSeed" + curLand + "growthLevel", 1);
                                                                                         // updates the growth level in the save
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevelA", 1); // subs out "LandSeed"+curLand for name
                    }
                    else if (hmm.landSeed.transform.GetChild(13).gameObject.activeSelf == true)
                    {
                        Debug.Log("THIRTEENTHHHH");
                        hitData.transform.parent.GetChild(13).gameObject.SetActive(true); // turns on the seed
                                                                                         //PlayerPrefs.SetInt("LandSeed" + curLand + "growthLevel", 1);
                                                                                         // updates the growth level in the save
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevelB", 1); // subs out "LandSeed"+curLand for name
                    }
                }
            }

            // adding and removing of all plants and fruits from inventory will happen in these cases by removing images from the scrollbar

            // Droplet placement (for plant)
            if (curMode.dropTime)
            {
                if (hitData.transform.parent.GetChild(2).gameObject.activeSelf
                    || hitData.transform.parent.GetChild(3).gameObject.activeSelf
                    || hitData.transform.parent.GetChild(4).gameObject.activeSelf
                    || hitData.transform.parent.GetChild(5).gameObject.activeSelf
                    //|| hitData.transform.parent.GetChild(6).gameObject.activeSelf // TODO reactivate if we make watering max cactus have any meaning in the future
                    || hitData.transform.parent.GetChild(9).gameObject.activeSelf
                    || hitData.transform.parent.GetChild(10).gameObject.activeSelf
                    || hitData.transform.parent.GetChild(11).gameObject.activeSelf
                    || hitData.transform.parent.GetChild(13).gameObject.activeSelf
                    || hitData.transform.parent.GetChild(14).gameObject.activeSelf)
                { // yes, this is duplicated code, but it's easier than duplicating the playerprefs for the droplet, which in turn is the onl way to get system-wide message passing with the assembly directive issues of having modified a pre-existing class
                    PlayerPrefs.SetInt("zeroIfDropNotPlaced", 1);
                    // DOES IT FOR CACTI
                    if (hitData.transform.parent.GetChild(2).gameObject.activeSelf == true) // ig we don't NEED the like 4 conditionals, right?
                    {
                        hitData.transform.parent.GetChild(3).gameObject.SetActive(true); // turns on the plant
                        hitData.transform.parent.GetChild(2).gameObject.SetActive(false);// turns off the seed

                        // updates the growth level in the save
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevel", 2); // subs out "LandSeed"+curLand for name
                    }
                    else if (hitData.transform.parent.GetChild(3).gameObject.activeSelf == true)
                    {
                        hitData.transform.parent.GetChild(4).gameObject.SetActive(true); // turns on the plant
                        hitData.transform.parent.GetChild(3).gameObject.SetActive(false);// turns off the seed

                        // updates the growth level in the save
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevel", 3); // subs out "LandSeed"+curLand for name
                    }
                    else if (hitData.transform.parent.GetChild(4).gameObject.activeSelf == true)
                    {
                        hitData.transform.parent.GetChild(5).gameObject.SetActive(true); // turns on the plant
                        hitData.transform.parent.GetChild(4).gameObject.SetActive(false);// turns off the seed

                        // updates the growth level in the save
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevel", 4); // subs out "LandSeed"+curLand for name
                    }
                    else if (hitData.transform.parent.GetChild(5).gameObject.activeSelf == true)
                    {
                        hitData.transform.parent.GetChild(6).gameObject.SetActive(true); // turns on the plant
                        hitData.transform.parent.GetChild(5).gameObject.SetActive(false);// turns off the seed

                        // insert code for adding harvested seed to inventory AND SAVING IT here

                        // updates the growth level in the save
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevel", 5); // subs out "LandSeed"+curLand for name

                        //  TODO HAVE THESE TWO FOR ALL FINAL PLANT STAGES
                        curMode.prizeTime = true;
                        curMode.plantType = 0;
                    }
                    else if (hitData.transform.parent.GetChild(6).gameObject.activeSelf == true) // once you harvest the seed, it loops around
                    {
                        //// incremenents the number of fruits
                        //PlayerPrefs.SetInt("numFruits", PlayerPrefs.GetInt("numFruits")+1);
                        //Debug.Log("NUMBERS OF FRUITS: "+ (PlayerPrefs.GetInt("numFruits") ));

                        //if (PlayerPrefs.GetInt("numFruits") < 7) {
                        //    // Adds it to the scroll panel by making it its child
                        //    GameObject newFruit = Instantiate(fruitImg); // TODO FIGURE OUT NAMESPACES
                        //    newFruit.transform.SetParent(panelie.transform, false);
                        //    newFruit.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
                        //}

                        //hitData.transform.parent.GetChild(6).gameObject.SetActive(false); // turns off the fruit plant

                        //PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevel", 0); // resets growth level
                    } // continue for further plant levels

                    // DOES IT FOR WOOD PLANTS
                    if (hitData.transform.parent.GetChild(9).gameObject.activeSelf == true) // ig we don't NEED the like 4 conditionals, right?
                    {
                        hitData.transform.parent.GetChild(10).gameObject.SetActive(true); // turns on the plant
                        hitData.transform.parent.GetChild(9).gameObject.SetActive(false);// turns off the seed

                        // updates the growth level in the save
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevelA", 2); // subs out "LandSeed"+curLand for name
                    }
                    else if (hitData.transform.parent.GetChild(10).gameObject.activeSelf == true)
                    {
                        hitData.transform.parent.GetChild(11).gameObject.SetActive(true); // turns on the plant
                        hitData.transform.parent.GetChild(10).gameObject.SetActive(false);// turns off the seed

                        // updates the growth level in the save
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevelA", 3); // subs out "LandSeed"+curLand for name
                    }
                    else if (hitData.transform.parent.GetChild(11).gameObject.activeSelf == true)
                    {
                        hitData.transform.parent.GetChild(12).gameObject.SetActive(true); // turns on the plant
                        hitData.transform.parent.GetChild(11).gameObject.SetActive(false);// turns off the seed

                        // updates the growth level in the save
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevelA", 4); // subs out "LandSeed"+curLand for name

                        // FOR ALL FINAL PLANT STAGES
                        curMode.prizeTime = true;
                        curMode.plantType = 1;
                    }

                    // DOES IT FOR FAN PLANTS
                    if (hitData.transform.parent.GetChild(13).gameObject.activeSelf == true) // ig we don't NEED the like 4 conditionals, right?
                    {
                        hitData.transform.parent.GetChild(14).gameObject.SetActive(true); // turns on the plant
                        hitData.transform.parent.GetChild(13).gameObject.SetActive(false);// turns off the seed

                        // updates the growth level in the save
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevelB", 2); // subs out "LandSeed"+curLand for name
                    }
                    else if (hitData.transform.parent.GetChild(14).gameObject.activeSelf == true)
                    {
                        hitData.transform.parent.GetChild(15).gameObject.SetActive(true); // turns on the plant
                        hitData.transform.parent.GetChild(14).gameObject.SetActive(false);// turns off the seed

                        // updates the growth level in the save
                        PlayerPrefs.SetInt(hitData.transform.parent.gameObject.name + "growthLevelB", 3); // subs out "LandSeed"+curLand for name

                        // FOR ALL FINAL PLANT STAGES
                        curMode.prizeTime = true;
                        curMode.plantType = 2;
                    }
                }
            }

        }
    }
}


// TODO IMPLEMENT A CLEANING FUNCTION THAT WIPES ALL EMPTY LANDS FROM THE PROGRAM TO OPTIMIZE BATTERY USAGE
// TODO CHANGE THE HIERARCHY SYSTEM OF THE LANDSEED BY PLACING THE LINEAGE OF PLANTS INTO IT'S OWN SUBFOLDER, ALSO THE LINEAGES OF EACH OF THE TYPES OF PLANTS GET THEIR OWN SUBFOLDER (LESS CONFUSING THAT WAY AND ALLOWS FOR MASS DEACTIVATIONS RELATIVELY QUICK [ALSO DO THIS FOR ALL THE BUTTONS AND INSTRUCTIONS])
   // This one's a maybe, since there's something to say to always being
   // able to look to parent and then back down to other children to check for information. Alternatively...
// TODO INSTEAD OF MEMORIZING THESE NUMBERS, DO RESEARCH ON WHAT ENUMERATED TYPES ARE AND WHETHER I COULD USE THEM OR SOMETHING LIKE THEM
    // This one's better I say

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

using Niantic.ARDK.AR;
using Niantic.ARDK.AR.ARSessionEventArgs;
using Niantic.ARDK.AR.HitTest;
using Niantic.ARDK.External;
using Niantic.ARDK.Utilities;
using Niantic.ARDK.Utilities.Input.Legacy;

// CURRENTLY WHERE ALL MENUING IS CONTROLLED

// To speed this sucker up: put all the GetComponent methods inside of Start() instead of button clicks
// Also getting rid of logs, potentially

namespace Niantic.ARDKExamples.Helpers
{
    public class inventoryScrip : MonoBehaviour
    {

        public Camera mainy;
        //public GameObject grassLand;
        //public GameObject waterLand;
        public GameObject instructions;
        public GameObject seedstructions;
        public GameObject deletstructions;
        public GameObject dropstructions;
        public GameObject gamestructions;
        public GameObject swipestructions;
        public GameObject winstructions;

        public GameObject landSeed; // a prefab representing all the possible permutations of game forms
        public GameMode curMode;
        public GameObject droplet;

        // things to invisiblize
        public GameObject harvestScrollie;
        public GameObject scrollView;
        public Slider slider;

        private const int updatePeriod = 3; // how long it takes before you can update again, a la Joust game

        public Button gameButton;
        public Button quitButton;
        public Button retryButton;

        public Button menuPlacementButton;
        public Button menuGameButton;

        public GameObject menuPanel;
        public GameObject zonePanels;

        public GameObject collectNothingButton;
        public GameObject createNothingButton;

        public GameObject fruitViewporter;
        private RectTransform fruitTrans;

        public GameObject gameOverPanel;

        public GameObject glasContent;

        private swipe_menu swopey;

        public Button instructionsOnButton;
        public Button instructionsOffButton;
        public GameObject gamepillstructions;

        public Button cactusGrownButton;
        public Button woodGrownButton;
        public Button fanGrownButton;

        public Button skipButton;
        public GameObject miniArrow;

        void Start()
        {

            fruitTrans = fruitViewporter.gameObject.GetComponent<RectTransform>();

            quitButton.onClick.AddListener(() => ButtonClicked(22)); // essentially acts as inventory button
            gameButton.onClick.AddListener(() => ButtonClicked(88));
            retryButton.onClick.AddListener(() => ButtonClicked(110)); // essentially acts as inventory button then minigame button

            menuPlacementButton.onClick.AddListener(() => ButtonClicked(33)); // essentially acts as inventory button
            menuGameButton.onClick.AddListener(() => ButtonClicked(44));

            instructionsOnButton.onClick.AddListener(() => ButtonClicked(77)); // toggles instruction
            instructionsOffButton.onClick.AddListener(() => ButtonClicked(99));  // also toggles instruction

            cactusGrownButton.onClick.AddListener(() => ButtonClicked(11)); // deactivates the screens
            woodGrownButton.onClick.AddListener(() => ButtonClicked(11));
            fanGrownButton.onClick.AddListener(() => ButtonClicked(11));

            skipButton.onClick.AddListener(() => ButtonClicked(66));

            deactiveAllInLandSeed(); // so placement isn't possible in the menu state

            swopey = glasContent.GetComponent<swipe_menu>();
        }
        void TaskOnClick()
        {
            //Output this to console when Button1 or Button3 is clicked
            Debug.Log("You have clicked the button!");
        }

        void TaskWithParameters(string message)
        {
            //Output this to console when the Button2 is clicked
            Debug.Log(message);
        }

        // What happens whenever a button is clicked
        private void standardButtonStuff()
        {
            
            //blueButton.gameObject.SetActive(!blueButton.gameObject.activeSelf);
            //greenButton.gameObject.SetActive(!greenButton.gameObject.activeSelf);
            //tealButton.gameObject.SetActive(!tealButton.gameObject.activeSelf);
            //redButton.gameObject.SetActive(!redButton.gameObject.activeSelf);
            //yellowButton.gameObject.SetActive(!yellowButton.gameObject.activeSelf);
            //scrollPanel.gameObject.SetActive(!scrollPanel.gameObject.activeSelf); // necessary to invizabilize all items collected

            // Deactivate instructions
            Debug.Log(gameObject.name);
            instructions.SetActive(false);
            seedstructions.SetActive(false);
            deletstructions.SetActive(false);
            dropstructions.SetActive(false);
        }

        private void deactiveAllInLandSeed()
        {
            // maybe just make a forloop

            for (int i = 0; i < landSeed.transform.childCount; i++)
            {
                landSeed.transform.GetChild(i).gameObject.SetActive(false);
            }

            // Old version

            //// 0 is GrassLand, 1 is WaterLand, 2 is Seed, 3 is Sprout (Small Plant), 4 is Medium Plant, 5 is Tall Plant, 6 is Fruiting Plant, 7 is Bot, 8 is Deprecated Bot, 9 is Wood Seed, 10-12 is Wood Plants, 13 is Fan Seed, 14-15 is Fan Plants
        }

        // TODO don't mess with the unused button inputs because it's necessary for swipe_menu.cs to function
        public void ButtonClicked(int buttonNo)
        {
            Debug.Log("Inventory button's position: " + transform.position);
            //Output this to console when the Button3 is clicked
            Debug.Log("Button clicked = " + buttonNo);
            if (buttonNo == 22) // choosin inventory
            {
                deactiveAllInLandSeed();

                standardButtonStuff();

                ARCursorRenderer rinder = mainy.GetComponent<ARCursorRenderer>();
                //rinder.SetNewCursor(null); // temporarily eliminating cursor for testing

                // Cursor or no cursor
                rinder.SetNewCursor();

                // undoing game time
                if (curMode.gameTime)
                {
                    Debug.Log("SHOULD BE FLATTENING");
                    ARHitTester hitty = mainy.GetComponent<ARHitTester>();
                    hitty.flattenAllPlaced(); // allows for the clearing of everything, undoing the stage for the minigame
                    hitty.loadPositions();
                    //gamestructions.SetActive(false);
                    //swipestructions.SetActive(false); handled by arhittester now
                    rinder.SetNewCursor(false);

                    // swaps out buttons
                    gameButton.gameObject.SetActive(true);
                    harvestScrollie.gameObject.SetActive(true);
                    scrollView.gameObject.SetActive(true);

                    slider.gameObject.SetActive(true);

                    quitButton.gameObject.SetActive(false);
                    retryButton.gameObject.SetActive(false);

                    gamepillstructions.gameObject.SetActive(false);

                    // We need to set the instructions button appropriately
                    int struction = PlayerPrefs.GetInt("instruct");
                    if (struction == 1 || (struction == 0 && swopey.instrucShow))
                    {
                        instructionsOnButton.gameObject.SetActive(true);
                        instructionsOffButton.gameObject.SetActive(false);
                    } else
                    {
                        instructionsOnButton.gameObject.SetActive(false);
                        instructionsOffButton.gameObject.SetActive(true);
                    }
                }

                curMode.seedTime = false;
                curMode.deleteTime = false;
                curMode.dropTime = false;
                curMode.landTime = false;
                curMode.gameTime = false;

                zonePanels.gameObject.SetActive(true);
                createNothingButton.gameObject.SetActive(true);
                collectNothingButton.gameObject.SetActive(false);
                //ButtonClicked(0);

                // Both of these are playerprefs things, but regardless I'd prefer to delegate responsibility more evenly
                swopey.refreshMenu(); // order shouldn't matter here, this needed in case we gain something in minigame
                if (swopey.grasslandActive()) {
                    ButtonClicked(0);
                } else {
                    ButtonClicked(-1);
                }

                gameOverPanel.gameObject.SetActive(false);

                // Fixes NaN UI bug
                fruitTrans.anchoredPosition = new Vector2(-126,-31);
                // TODO modify the location of fruit panel here too if you do
            }
            else if (buttonNo == 0) // choosin grassland
            {
                // Activate only the grass land
                deactiveAllInLandSeed();
                landSeed.transform.GetChild(0).gameObject.SetActive(true); // 0 is GrassLand, 1 is WaterLand, and 2 is Seed

                standardButtonStuff(); // What happens whenever a button is clicked

                // change the camera's ARHitTester and ARCursorRenderer components
                ARHitTester hitty = mainy.GetComponent<ARHitTester>();
                //hitty.PlacementObjectPf = grassLand;
                hitty.PlacementObjectPf = landSeed;

                ARCursorRenderer rinder = mainy.GetComponent<ARCursorRenderer>();
                //rinder.CursorObject = grassLand; // deprecated... I think
                //rinder.SetNewCursor(grassLand);


                // Cursor or no cursor
                rinder.SetNewCursor();

                curMode.seedTime = false;
                curMode.deleteTime = false;
                curMode.dropTime = false;
                curMode.landTime = true;
                curMode.gameTime = false;
            }
            else if (buttonNo == 1) // choosin waterland
            {
                //grassLand.gameObject.SetActive(false);
                //waterLand.gameObject.SetActive(true);

                // activate only water land
                deactiveAllInLandSeed();
                landSeed.transform.GetChild(1).gameObject.SetActive(true); // 0 is GrassLand, 1 is WaterLand, and 2 is Seed

                standardButtonStuff(); // What happens whenever a button is clicked

                // change the camera's ARHitTester and ARCursorRenderer components
                ARHitTester hitty = mainy.GetComponent<ARHitTester>();
                //hitty.PlacementObjectPf = waterLand;
                hitty.PlacementObjectPf = landSeed;

                ARCursorRenderer rinder = mainy.GetComponent<ARCursorRenderer>();
                //rinder.CursorObject = waterLand; // deprecated... I think
                //rinder.SetNewCursor(waterLand);

                // Cursor or no cursor
                rinder.SetNewCursor();

                curMode.seedTime = false;
                curMode.deleteTime = false;
                curMode.dropTime = false;
                curMode.landTime = true;
                curMode.gameTime = false;
            }
            else if (buttonNo == 2) // choosin' seed
            {
                deactiveAllInLandSeed();
                landSeed.transform.GetChild(2).gameObject.SetActive(true); // 0 is GrassLand, 1 is WaterLand, and 2 is Seed

                standardButtonStuff(); // What happens whenever a button is clicked

                ARHitTester hitty = mainy.GetComponent<ARHitTester>();
                //hitty.PlacementObjectPf = waterLand;
                hitty.PlacementObjectPf = null;

                ARCursorRenderer rinder = mainy.GetComponent<ARCursorRenderer>();
                //rinder.CursorObject = waterLand; // deprecated... I think
                //rinder.SetNewCursor(waterLand);


                // Cursor or no cursor
                rinder.SetNewCursor();

                curMode.seedTime = true;
                curMode.deleteTime = false;
                curMode.dropTime = false;
                curMode.landTime = false;
                curMode.gameTime = false;
                Debug.Log("Seed tiempo? " + curMode.seedTime); Debug.Log("Seed tiempo? " + curMode.seedTime); Debug.Log("Seed tiempo? " + curMode.seedTime);

            }
            else if (buttonNo == 6) // TODO CHANGE EVERY TIME YOU ADD MORE BUTTONS! SHOULD BE FINAL INDEX
            {
                deactiveAllInLandSeed();

                standardButtonStuff(); // What happens whenever a button is clicked

                ARHitTester hitty = mainy.GetComponent<ARHitTester>();
                //hitty.PlacementObjectPf = waterLand;
                hitty.PlacementObjectPf = null;

                ARCursorRenderer rinder = mainy.GetComponent<ARCursorRenderer>();
                //rinder.CursorObject = waterLand; // deprecated... I think
                //rinder.SetNewCursor(waterLand);
                //rinder.SetNewCursor(null);

                // Cursor or no cursor
                rinder.SetNewCursor();

                curMode.seedTime = false;
                curMode.deleteTime = true;
                curMode.dropTime = false;
                curMode.landTime = false;
                curMode.gameTime = false;
                Debug.Log("Delete tiempo? " + curMode.deleteTime);
            }
            else if (buttonNo == -1) // No buttons left!
            {
                deactiveAllInLandSeed();

                standardButtonStuff(); // What happens whenever a button is clicked

                ARHitTester hitty = mainy.GetComponent<ARHitTester>();
                //hitty.PlacementObjectPf = waterLand;
                hitty.PlacementObjectPf = null;

                ARCursorRenderer rinder = mainy.GetComponent<ARCursorRenderer>();
                //rinder.CursorObject = waterLand; // deprecated... I think
                //rinder.SetNewCursor(waterLand);
                //rinder.SetNewCursor(null);

                // Cursor or no cursor
                rinder.SetNewCursor();

                curMode.seedTime = false;
                curMode.deleteTime = false;
                curMode.dropTime = false;
                curMode.landTime = false;
                curMode.gameTime = false;
                Debug.Log("Nothing tiempo");
            }
            else if (buttonNo == 5) // drop tiempo
            {
                deactiveAllInLandSeed();

                standardButtonStuff(); // What happens whenever a button is clicked

                ARHitTester hitty = mainy.GetComponent<ARHitTester>();
                //hitty.PlacementObjectPf = waterLand;
                hitty.PlacementObjectPf = null;

                ARCursorRenderer rinder = mainy.GetComponent<ARCursorRenderer>();
                //rinder.SetNewCursor(droplet);
                // Cursor or no cursor
                rinder.SetNewCursor();

                curMode.seedTime = false;
                curMode.deleteTime = false;
                curMode.dropTime = true;
                curMode.landTime = false;
                curMode.gameTime = false;
                Debug.Log("Drop tiempo? " + curMode.dropTime);
            }
            else if (buttonNo == 3) // Wood seed
            {
                deactiveAllInLandSeed();
                landSeed.transform.GetChild(9).gameObject.SetActive(true); // 0 is GrassLand, 1 is WaterLand, and 2 is Seed, 9 is Wood Seed

                standardButtonStuff(); // What happens whenever a button is clicked

                ARHitTester hitty = mainy.GetComponent<ARHitTester>();
                //hitty.PlacementObjectPf = waterLand;
                hitty.PlacementObjectPf = null;

                ARCursorRenderer rinder = mainy.GetComponent<ARCursorRenderer>();
                //rinder.CursorObject = waterLand; // deprecated... I think
                //rinder.SetNewCursor(waterLand);


                // Cursor or no cursor
                rinder.SetNewCursor();

                curMode.seedTime = true;
                curMode.deleteTime = false;
                curMode.dropTime = false;
                curMode.landTime = false;
                curMode.gameTime = false;
                Debug.Log("Seed tiempo? " + curMode.seedTime);
                Debug.Log("Wood seed!!");
            }
            else if (buttonNo == 4) // Fan seed
            {
                deactiveAllInLandSeed();
                landSeed.transform.GetChild(13).gameObject.SetActive(true); // 0 is GrassLand, 1 is WaterLand, and 2 is Seed, 13 is Fan Seed

                standardButtonStuff(); // What happens whenever a button is clicked

                ARHitTester hitty = mainy.GetComponent<ARHitTester>();
                //hitty.PlacementObjectPf = waterLand;
                hitty.PlacementObjectPf = null;

                ARCursorRenderer rinder = mainy.GetComponent<ARCursorRenderer>();
                //rinder.CursorObject = waterLand; // deprecated... I think
                //rinder.SetNewCursor(waterLand);


                // Cursor or no cursor
                rinder.SetNewCursor();

                curMode.seedTime = true;
                curMode.deleteTime = false;
                curMode.dropTime = false;
                curMode.landTime = false;
                curMode.gameTime = false;
                Debug.Log("Seed tiempo? " + curMode.seedTime);
                Debug.Log("Fan seed!!");
            }
            else if (buttonNo == 88) // Game button
            {
                deactiveAllInLandSeed();
                landSeed.transform.GetChild(7).gameObject.SetActive(true); // 7 is Dropbot

                standardButtonStuff();

                // activated based on whether instructions were active and whether gamestructions have been seen
                int struction = PlayerPrefs.GetInt("instruct");
                bool seenGameStructions = PlayerPrefs.HasKey("seenGameStructions");
                PlayerPrefs.SetString("seenGameStructions", "doesn'tmatter");

                if (!seenGameStructions || struction == 1 || (struction == 0 && swopey.instrucShow))
                {
                    toggleInstructions(true);
                }
                else
                {
                    toggleInstructions(false);
                }

                // TODO Gotta deactivate all of the stuff on screen: add translucent bot cursor, add bot to landseed
                ARHitTester hitty = mainy.GetComponent<ARHitTester>();

                Debug.Log("SUPER WHY");
                hitty.saveLast(); // preserve the last thing slider shift before entering minigame
                Debug.Log("SUPER WHY");

                hitty.PlacementObjectPf = landSeed; // add robot to landseed
                hitty.flattenAllPlaced(); // allows for the clearing of everything, setting the stage for the minigame

                ARCursorRenderer rinder = mainy.GetComponent<ARCursorRenderer>();
                //rinder.SetNewCursor(null); // temporarily eliminating cursor for testing

                // Cursor or no cursor
                rinder.SetNewCursor();

                //gamestructions.SetActive(true); see arhittester

                // swaps out buttons
                gameButton.gameObject.SetActive(false);
                harvestScrollie.gameObject.SetActive(false);
                scrollView.gameObject.SetActive(false);

                //slider.value = 0; NO NEED, UNDOES MY SAVE
                slider.gameObject.SetActive(false);

                quitButton.gameObject.SetActive(true);
                retryButton.gameObject.SetActive(true);

                curMode.seedTime = false;
                curMode.deleteTime = false;
                curMode.dropTime = false;
                curMode.landTime = false;
                curMode.gameTime = true;
                curMode.tutorialStage++;
                PlayerPrefs.SetInt("tutStage", PlayerPrefs.GetInt("tutStage") + 1);
                Debug.Log("Game tiempo? " + curMode.gameTime);

                zonePanels.gameObject.SetActive(false);
                createNothingButton.gameObject.SetActive(false);
                collectNothingButton.gameObject.SetActive(true);

                skipButton.gameObject.SetActive(false);
                miniArrow.gameObject.SetActive(false);

                swopey.deactivateCurstruction();
            }
            else if (buttonNo == 110) // Retry
            {
                Debug.Log("TRIED TO RETRY");
                ButtonClicked(22); // inventory
                ButtonClicked(88); // minigame
            }
            else if (buttonNo == 33) // Menu Placement Button
            {
                // swaps out buttons
                gameButton.gameObject.SetActive(true);
                harvestScrollie.gameObject.SetActive(true);
                scrollView.gameObject.SetActive(true);
                slider.gameObject.SetActive(true);
                menuPanel.SetActive(false);

                // P sure the activated swipe menu will call the appropriate button click
            }
            else if (buttonNo == 44) // Menu Game Button
            {
                // swaps out buttons
                //menuPanel.SetActive(false);


            }
            else if (buttonNo == 77) // on turns instructions off
            {
                swopey.toggleInstructions(false);
                if (curMode.gameTime)
                {
                    Debug.Log("I am still around");
                    toggleInstructions(false);
                }
            }
            else if (buttonNo == 99) // off turns instructions on
            {
                swopey.toggleInstructions(true);
                if (curMode.gameTime)
                {
                    Debug.Log("I'll always be around");
                    toggleInstructions(true);
                }
            }
            else if (buttonNo == 11) // deactivate grown panel by acknowledging button
            {
                curMode.prizeTime = false;
                curMode.plantType = -1; // sorta redundant with prizetime, but idk it's more readable
            }
            else if (buttonNo == 66)
            {
                curMode.tutorialStage = 6;
                skipButton.gameObject.SetActive(false);
            }
        }

        void toggleInstructions(bool toggleOn)
        {
            if (toggleOn)
            {
                instructionsOnButton.gameObject.SetActive(true);
                instructionsOffButton.gameObject.SetActive(false);
                gamepillstructions.gameObject.SetActive(true);
                Debug.Log("instructoggle: " + toggleOn);
            }
            else // toggle off
            {
                instructionsOnButton.gameObject.SetActive(false);
                instructionsOffButton.gameObject.SetActive(true);
                gamepillstructions.gameObject.SetActive(false);
                Debug.Log("instructoggle: " + toggleOn);
            }
        }

        // Triggered when app ends?
        void OnApplicationQuit()
        {
            // resets scriptable object curMode
            curMode.seedTime = false;
            curMode.deleteTime = false;
            curMode.dropTime = false;
            curMode.landTime = false; // as long as we start off with landseed as grassland
            curMode.gameTime = false;
            curMode.prizeTime = false; // showing the winning of the prize
            curMode.plantType = -1;

        // saved playerprefs
        PlayerPrefs.Save();
        }
    }
}
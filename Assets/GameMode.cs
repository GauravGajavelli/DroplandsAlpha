using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu]
public class GameMode : ScriptableObject
{
    public bool landTime; // enables placement of lands
    public bool seedTime; // enables the placement of seeds
    public bool deleteTime; // whether or not we're deleting LandSeeds
    public bool dropTime; // enables the placement of seeds/growth of plants
    public bool gameTime; // indicates that we are currently playing the minigame
    public bool prizeTime = false; // showing the winning of the prize
    public int plantType = -1; // int plantType: 0 is cactus, 1 is wood, and 2 is fan
    public int tutorialStage = 0; // int tutorialStage: 1 is land, 2 is seed, 3 is water, 4 is delete, 5 is minigame, 6 is done
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class dropletGoalScript : MonoBehaviour
{
    public GameObject gameOverScreen;

    public GameObject dropletPrizeSquare;
    public GameObject cactusPrizeSquare;
    public GameObject woodPrizeSquare;
    public GameObject fanPrizeSquare;
    private bool alreadyEarned;

    // Start is called before the first frame update
    void Start()
    {
        alreadyEarned = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    //Upon collision with another GameObject, the other disappears
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Rosencrantz: ");
        if (!alreadyEarned && other.gameObject.tag == "bot")
        {
            alreadyEarned = true;
            // boop... bot is absorbed
            other.gameObject.SetActive(false);

            dropletPrizeSquare.gameObject.SetActive(false);
            fanPrizeSquare.gameObject.SetActive(false);
            woodPrizeSquare.gameObject.SetActive(false);
            cactusPrizeSquare.gameObject.SetActive(false);

            dropletPrizeSquare.gameObject.SetActive(true);
            PlayerPrefs.SetInt("numDroplet Button", PlayerPrefs.GetInt("numDroplet Button") + 1);
            
            gameOverScreen.gameObject.SetActive(true);
            //PlayerPrefs.SetInt("negOneIfNoSeedReward", -1); // SETS TO NEGATIVE ONE IN THE WIN STATE, THE LOSE STATE, IN THE LEAVES GAMEMODE STATE, AND IN THE QUIT'S GAME STATE
            // COMMENTED Because won't break the game if it's not -1
        }
    }
}

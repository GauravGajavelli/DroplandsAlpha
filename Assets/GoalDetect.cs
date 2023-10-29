using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalDetect : MonoBehaviour
{
    public GameObject gameOverScreen;
    public GameObject prizeSquare;

    // for adding items to scroll panel
    public GameObject panelie;
    public GameObject fruitImg;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //Upon collision with another GameObject, the other disappears
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Rosencrantz: "+ other.gameObject.tag);
        if (other.gameObject.tag == "bot") {
            // boop... bot is absorbed
            other.gameObject.SetActive(false);

            prizeSquare.gameObject.SetActive(true);
            gameOverScreen.gameObject.SetActive(true);

            // TODO Add some randomization in what prize is displayed (via more prizeSquares and a rng) 
            // incremenents the number of fruits
            PlayerPrefs.SetInt("numFruits", PlayerPrefs.GetInt("numFruits") + 1);
            if (PlayerPrefs.GetInt("numFruits") < 7)
            {
                // Adds it to the scroll panel by making it its child
                GameObject newFruit = Instantiate(fruitImg); // TODO FIGURE OUT NAMESPACES
                newFruit.transform.SetParent(panelie.transform, false);
                newFruit.transform.localScale = new Vector3(0.9f, 0.9f, 0.9f);
            }
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TouchInput : MonoBehaviour
{
    public GameObject particle;
    public Button blueButton;

    void Start()
    {
        Debug.Log("TouchInput working");
    }

    void Update()
    {
        Debug.Log(Input.touches.Length);
        foreach (Touch touch in Input.touches)
        {
            if (touch.phase == TouchPhase.Began)
            {
                Debug.Log("How they became great");
                // Construct a ray from the current touch coordinates
                Ray ray = Camera.main.ScreenPointToRay(touch.position);
                if (Physics.Raycast(ray)) // if we hit a collider
                {
                    if (blueButton.gameObject.activeSelf == false) {
                        // Create a particle if hit
                        Instantiate(particle, transform.position, transform.rotation);
                    }
                }
            }
        }
    }
}
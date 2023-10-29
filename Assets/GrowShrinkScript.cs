using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrowShrinkScript : MonoBehaviour
{
    bool shrinkin;
    Vector3 increment;

    // Start is called before the first frame update
    void Start()
    {
        shrinkin = true;
        increment = new Vector3(.01f, .01f, .01f);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 curScale = transform.localScale;
        if (shrinkin) // growin
        {
            if (curScale.x <= 1f) // min shrink
            {
                shrinkin = false;
            } else
            {
                curScale -= increment;
                transform.localScale = curScale;
            }
        } else
        {
            if (curScale.x >= 1.5f) // max grow
            {
                shrinkin = true;
            }
            else
            {
                curScale += increment;
                transform.localScale = curScale;
            }
        }
    }
}

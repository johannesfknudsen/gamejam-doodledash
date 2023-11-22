using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Exit : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        if (Collectables.allCollected == false)
        {
            GetComponent<SpriteRenderer>().color = new Color(0, 0, 0, 0.1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Collectables.allCollected == true)
        {
            GetComponent<SpriteRenderer>().color = new Color(1, 1, 1, 1);
        }
    }
}

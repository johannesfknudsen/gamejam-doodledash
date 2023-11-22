using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Collectables : MonoBehaviour
{
    public static int collectableCount;
    public static bool allCollected = false;
    private void Start()
    {
        collectableCount = FindObjectsOfType<Collectable>().Length;
    }
    private void Update()
    {
        Debug.Log(collectableCount);
        if (collectableCount == 0)
        {
            allCollected = true;
            Debug.Log("Gå til exit!");
        }
    }
}
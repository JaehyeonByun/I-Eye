using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorLogger : MonoBehaviour
{
    public Transform wrist; 
    public Transform position; 
    
    public float logInterval = 1f;

    private float timeSinceLastLog = 0f;

    void Update()
    {
        timeSinceLastLog += Time.deltaTime;

        if (timeSinceLastLog >= logInterval)
        {
            if (wrist != null)
            {
                GameManager.HyperActivity_a.Add(wrist.position); 
            }
            if (position != null)
            {
                GameManager.HyperActivity_e.Add(position.position);
            }

            timeSinceLastLog = 0f;
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            PrintPositionHistory();
        }
    }

    public void PrintPositionHistory()
    {
        Debug.Log("Wrist Positions:");
        foreach (var wristPos in GameManager.HyperActivity_a)
        {
            Debug.Log(wristPos);
        }

        Debug.Log("Position Positions:");
        foreach (var pos in GameManager.HyperActivity_e)
        {
            Debug.Log(pos);
        }
    }
}

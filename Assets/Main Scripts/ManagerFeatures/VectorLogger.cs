using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorLogger : MonoBehaviour
{
    public Transform wristLeft; 
    public Transform wristRight;
    public Transform bodyposition; 
    
    public float logInterval = 0.1f;
    private float timeSinceLastLog = 0f;

    void Update()
    {
        if (GameManager._onIntroducing)
        {
            timeSinceLastLog += Time.deltaTime;

            if (timeSinceLastLog >= logInterval)
            {
                if (wristLeft != null && wristRight != null)
                {
                    // Round the time to 1 decimal place
                    float roundedTime = Mathf.Round(Time.time * 10f) / 10f;
                    GameManager.HyperActivity_a.Add((roundedTime, (wristLeft.position + wristRight.position) / 2));
                }

                if (bodyposition != null)
                {
                    // Round the time to 1 decimal place
                    float roundedTime = Mathf.Round(Time.time * 10f) / 10f;
                    GameManager.HyperActivity_e.Add((roundedTime, bodyposition.position));
                }

                timeSinceLastLog = 0f; 
            }
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
            Debug.Log("Time: " + wristPos.Item1.ToString("F1") + ", Position: " + wristPos.Item2);
        }

        Debug.Log("Body Positions:");
        foreach (var pos in GameManager.HyperActivity_e)
        {
            Debug.Log("Time: " + pos.Item1.ToString("F1") + ", Position: " + pos.Item2);
        }
    }
    
    public void StartIntroducing()
    {
        GameManager._onIntroducing = true;
    }

    public void StopIntroducing()
    {
        GameManager._onIntroducing = false;
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorLogger : MonoBehaviour
{
  public Transform wristLeft; 
    public Transform wristRight;
    public Transform bodyposition; 
    public GameObject targetObject; 

    public float logInterval = 0.1f;
    private float timeSinceLastLog = 0f;
    private float startTime; // Time when the scene starts

    void Start()
    {
        // Set startTime to the time the scene starts
        startTime = Time.time;

        // Optionally log the start time
        Debug.Log("Scene started at time: " + startTime);
    }

    void Update()
    {
        // Check if the target object is active before proceeding with logging
        if (targetObject != null && targetObject.activeInHierarchy)
        {
            // We will log based on time since the scene started
            timeSinceLastLog += Time.deltaTime;

            if (timeSinceLastLog >= logInterval)
            {
                if (wristLeft != null && wristRight != null)
                {
                    // Calculate elapsed time since scene start and round it to 1 decimal place
                    float elapsedTime = Mathf.Round((Time.time - startTime) * 10f) / 10f;
                    Debug.Log("Wrist log time: " + elapsedTime);
                    GameManager.HyperActivity_a_List.Add((elapsedTime, (wristLeft.position + wristRight.position) / 2));
                }

                if (bodyposition != null)
                {
                    // Calculate elapsed time since scene start and round it to 1 decimal place
                    float elapsedTime = Mathf.Round((Time.time - startTime) * 10f) / 10f;
                    GameManager.HyperActivity_e_List.Add((elapsedTime, bodyposition.position));
                }

                timeSinceLastLog = 0f; 
            }
        }
    }

    public void StartLogging()
    {
        // Start logging right away when the scene begins (no condition for targetObject)
        timeSinceLastLog = 0f; 
    }

    public void StopLogging()
    {
        // Stop logging by disabling the target object
        targetObject.SetActive(false);
    }
}

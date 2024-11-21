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
                    GameManager.HyperActivity_a_List.Add((roundedTime, (wristLeft.position + wristRight.position) / 2));
                }

                if (bodyposition != null)
                {
                    // Round the time to 1 decimal place
                    float roundedTime = Mathf.Round(Time.time * 10f) / 10f;
                    GameManager.HyperActivity_e_List.Add((roundedTime, bodyposition.position));
                }

                timeSinceLastLog = 0f; 
            }
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
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

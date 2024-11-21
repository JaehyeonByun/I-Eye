using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHyperactivity : MonoBehaviour
{
    public GameObject objectToActivate;    
    public GameObject objectToMonitor;     
    public float countdownTime = 15f;     

    public int isHyperActivity = 0;  
    private bool countdownStarted = false; 

    private float currentCountdownTime;    

    void Update()
    {
        if (objectToActivate.activeSelf && !countdownStarted)
        {
            StartCountdown();
        }
        
        if (countdownStarted)
        {
            currentCountdownTime -= Time.deltaTime;

            if (currentCountdownTime <= 0)
            {
                Debug.Log("Countdown finished!");
                countdownStarted = false;
            }
        }
        
        if (objectToMonitor != null && !objectToMonitor.activeSelf)
        {
            if (countdownStarted && currentCountdownTime > 0)
            {
                isHyperActivity += 1;
                Debug.Log("HyperActivity triggered! isHyperActivity = 1");
                countdownStarted = false;
            }
        }
    }
    
    private void StartCountdown()
    {
        countdownStarted = true;
        currentCountdownTime = countdownTime;
        Debug.Log("Countdown started for " + countdownTime + " seconds.");
    }
    
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalculateDistance : MonoBehaviour
{
    private Vector3 lastPosition; 
    private float totalDistance;  

    void Start()
    {
        lastPosition = new Vector3(transform.position.x, 0, transform.position.z);
        totalDistance = 0f;
    }

    void Update()
    {
        Vector3 currentPosition = new Vector3(transform.position.x, 0, transform.position.z);
        
        float distance = Vector3.Distance(currentPosition, lastPosition);
        totalDistance += distance;
        lastPosition = currentPosition;
    }
    public float GetTotalDistance()
    {
        return totalDistance;
    }
    public void ResetTotalDistance()
    {
        totalDistance = 0f;
    }
}

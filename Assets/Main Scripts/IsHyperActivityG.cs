using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsHyperActivityG : MonoBehaviour
{
    public GameObject targetObject;      
    public bool _isCount = false;   
    public int isHyperActivity = 0; 
    
    // Specific GameObject to check if it is active
    public GameObject conditionObject;

    private Vector3 previousPosition;     

    void Start()
    {
        previousPosition = targetObject.transform.position;
    }

    void Update()
    {
        if (conditionObject != null && conditionObject.activeInHierarchy)
        {
            // Check if the target object has moved
            if (targetObject.transform.position != previousPosition)
            {
                if (!_isCount)
                {
                    isHyperActivity += 1;
                    Debug.Log("Movement detected, HyperActivity is now 1.");
                    _isCount = true;
                }
            }
            previousPosition = targetObject.transform.position;
        }
    }
}

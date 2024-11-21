using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsHyperActivityG : MonoBehaviour
{
    public GameObject targetObject;      
    public bool _isCount = false;   
    public float movementThreshold = 0.1f; 

    private Vector3 previousPosition;     
    public int isHyperActivity = 0; 

    void Start()
    {
        previousPosition = targetObject.transform.position;
    }

    void Update()
    {
        if (GameManager._onIntroducing)
        {
            if (Vector3.Distance(targetObject.transform.position, previousPosition) > movementThreshold)
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

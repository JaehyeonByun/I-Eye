using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OthersBelonging : MonoBehaviour
{
    public int OtherBelongingTouch = 0;
    private Dictionary<Transform, Vector3> childInitialPositions = new Dictionary<Transform, Vector3>();

    void Start()
    {
        foreach (Transform child in transform)
        {
            childInitialPositions[child] = child.position;
        }
    }

    void Update()
    {
        foreach (Transform child in transform)
        {
            if (childInitialPositions.ContainsKey(child))
            {
                if (child.position != childInitialPositions[child])
                {
                    childInitialPositions.Remove(child); 
                    OtherBelongingTouch++; 
                }
            }
        }
    }
    

}

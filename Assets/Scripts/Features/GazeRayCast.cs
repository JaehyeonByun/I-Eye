using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeRayCast : MonoBehaviour
{
    [SerializeField, Range(10f, 50f)] private float lineSize = 10f;

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, lineSize))
        {
                Debug.Log(hit.collider.gameObject.name);
        }
    }
}

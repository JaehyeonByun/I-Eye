using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Brick : MonoBehaviour
{
    private Vector3 fixedPosition;
    private Quaternion fixedRotation;
    private bool isFixed = false;

    private void OnTriggerEnter(Collider other)
    {
        if (gameObject.CompareTag("빨간벽돌") && other.CompareTag("빨간벽돌정답"))
        {
            fixedPosition = other.transform.position;
            fixedRotation = other.transform.rotation; 
            LockBlockPosition();
        }
        else if (gameObject.CompareTag("노란벽돌") && other.CompareTag("노란벽돌정답"))
        {
            fixedPosition = other.transform.position;
            fixedRotation = other.transform.rotation; 
            LockBlockPosition();
        }
        else if (gameObject.CompareTag("파란벽돌") && other.CompareTag("파란벽돌정답"))
        {
            fixedPosition = other.transform.position;
            fixedRotation = other.transform.rotation;
            LockBlockPosition();
        }
    }

    private void LockBlockPosition()
    {
        transform.position = fixedPosition;
        transform.rotation = fixedRotation; 
        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true; 
        }
        isFixed = true; 
    }

    private void Update()
    {
        if (isFixed)
        {
            transform.position = fixedPosition;
            transform.rotation = fixedRotation; 
        }
    }
}

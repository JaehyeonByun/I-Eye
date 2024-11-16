using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HouseBricks : MonoBehaviour
{
    private void Start()
    {
        foreach (Transform child in transform)
        {
            Collider collider = child.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;
            }
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        string tag = other.gameObject.tag;

        if (tag == "빨간벽돌정답" || tag == "파란벽돌정답" || tag == "노란벽돌정답")
        {
            foreach (Transform child in transform)
            {
                if (child.CompareTag("빨간벽돌") && tag == "빨간벽돌정답")
                {
                    Debug.Log("정답");
                    FixBlockPosition(child, other.transform.position);
                }
                else if (child.CompareTag("파란벽돌") && tag == "파란벽돌정답")
                {
                    Debug.Log("정답");
                    FixBlockPosition(child, other.transform.position);
                }
                else if (child.CompareTag("노란벽돌") && tag == "노란벽돌정답")
                {
                    Debug.Log("정답");
                    FixBlockPosition(child, other.transform.position);
                }
            }
        }
    }
    
    private void FixBlockPosition(Transform block, Vector3 targetPosition)
    {
        block.position = targetPosition;
        block.GetComponent<Collider>().enabled = false;
    }
}

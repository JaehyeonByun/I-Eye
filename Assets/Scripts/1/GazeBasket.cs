using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GazeBasket : MonoBehaviour
{
    [SerializeField, Range(10f, 50f)] private float lineSize = 10f;
    private float collisionStartTime;
    private bool isColliding = false;

    void Update()
    {
        Debug.DrawRay(transform.position, transform.forward * lineSize, Color.yellow);
        
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, lineSize))
        {
            if (hit.collider.CompareTag("오븐"))
            {
                if (!isColliding)
                {
                    isColliding = true;
                    collisionStartTime = Time.time;
                }
                if (Time.time - collisionStartTime >= 10f)
                {
                    Debug.Log(hit.collider.gameObject.name + " - 10초 이상 충돌 유지됨");
                }
            }
            else
            {
                ResetCollision();
            }
        }
        else if (isColliding)
        {
            if (Time.time - collisionStartTime < 10f)
            {
                Debug.Log("충돌이 10초 미만으로 유지됨 - " + collisionStartTime + "초 동안 유지됨");
            }
            ResetCollision();
        }
    }

    private void ResetCollision()
    {
        isColliding = false;
        collisionStartTime = 0f;
    }
}

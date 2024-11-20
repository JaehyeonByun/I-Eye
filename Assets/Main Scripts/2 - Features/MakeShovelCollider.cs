using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MakeShovelCollider : MonoBehaviour
{
    public Transform target; // 따라갈 대상

    void Update()
    {
        if (target != null)
        {
            // 대상의 위치를 따라가게 설정
            transform.position = target.position;
        }
    }
}

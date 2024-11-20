using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalyGrab : MonoBehaviour
{
    private Vector3 lastPosition; // 이전 프레임의 위치 저장
    private bool isFirstFrame = true; // 첫 프레임인지 확인

    void Update()
    {
        // 첫 프레임에는 위치를 초기화만 함
        if (isFirstFrame)
        {
            lastPosition = transform.position;
            isFirstFrame = false;
            return;
        }

        // 현재 위치가 이전 위치와 다른지 확인
        if (transform.position != lastPosition)
        {
            Debug.Log($"{gameObject.name}가 움직여서 삭제됩니다.");
            Destroy(gameObject); // 오브젝트 삭제
        }

        // 이전 위치 업데이트
        lastPosition = transform.position;
    }
}

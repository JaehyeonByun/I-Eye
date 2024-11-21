using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DistanceAndTime : MonoBehaviour
{
    public GameObject activeObject; // 활성화 상태를 추적할 오브젝트
    public GameObject inactiveObject; // 비활성화 상태를 추적할 오브젝트
    public GameObject playerObject; // 플레이어 오브젝트

    private bool isGaming = false; // 게임 상태
    public float gamingTime = 0f; // isGaming 동안의 경과 시간
    private Vector3 lastPlayerPosition; // 이전 플레이어 위치
    public float totalDistance = 0f; // X, Z축 총 이동 거리

    private bool hasStarted = false; // activeObject가 처음 비활성화된 상태 확인
    private bool hasStopped = false; // inactiveObject가 처음 활성화된 상태 확인

    void Start()
    {
        if (playerObject != null)
            lastPlayerPosition = playerObject.transform.position;
    }

    void Update()
    {
        // activeObject가 처음 비활성화되었을 때
        if (activeObject != null && !activeObject.activeSelf && !hasStarted)
        {
            hasStarted = true; // 시작 상태 설정
            isGaming = true; // 게임 상태 시작
            gamingTime = 0f; // 시간 초기화
            totalDistance = 0f; // 거리 초기화
            lastPlayerPosition = playerObject.transform.position; // 플레이어 위치 초기화
        }

        // inactiveObject가 처음 활성화되었을 때
        if (inactiveObject != null && inactiveObject.activeSelf && !hasStopped && isGaming)
        {
            hasStopped = true; // 정지 상태 설정
            isGaming = false; // 게임 상태 종료
        }

        // 게임 상태에서 시간과 거리 계산
        if (isGaming)
        {
            gamingTime += Time.deltaTime;

            if (playerObject != null)
            {
                Vector3 currentPosition = playerObject.transform.position;
                totalDistance += Vector3.Distance(
                    new Vector3(lastPlayerPosition.x, 0, lastPlayerPosition.z),
                    new Vector3(currentPosition.x, 0, currentPosition.z)
                );
                lastPlayerPosition = currentPosition;
            }
        }
    }
}

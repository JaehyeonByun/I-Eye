using UnityEngine;

public class HeadLockedCanvas : MonoBehaviour
{
    [SerializeField]
    private Transform playerHead; // 플레이어의 카메라 또는 헤드 트랜스폼

    [SerializeField, Range(0f, 5f)]
    private float followDistance = 0.6f; // 플레이어와의 거리 유지

    [SerializeField, Range(1f, 10f)]
    private float followSpeed = 5f; // 따라가는 속도

    private void Update()
    {
        // 목표 위치 계산 (플레이어 헤드에서 일정 거리 유지)
        Vector3 targetPosition = playerHead.position + playerHead.forward * followDistance;
        
        // 캔버스를 목표 위치로 부드럽게 이동
        transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

        // 캔버스가 플레이어 헤드를 바라보도록 회전 설정
        Quaternion targetRotation = Quaternion.LookRotation(transform.position - playerHead.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * followSpeed);
    }
}
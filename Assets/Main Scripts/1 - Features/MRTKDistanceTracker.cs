using TMPro;
using UnityEngine;

public class MRTKDistanceTracker : MonoBehaviour
{
    [SerializeField] public TextMeshProUGUI DistanceText;
    
    private Transform cameraTransform; // 메인 카메라의 Transform
    private Vector3 previousPosition; // 이전 카메라 위치
    private float totalDistance = 0f; // 누적 이동 거리
    private bool isTracking = false; // 측정 활성화 여부

    private void Start()
    {
        // MRTK에서 메인 카메라 가져오기
        cameraTransform = Camera.main.transform;

        if (cameraTransform == null)
        {
            Debug.LogError("[MRTKDistanceTracker] Main Camera is not assigned.");
            return;
        }

        // 초기 위치 설정
        previousPosition = cameraTransform.position;
    }

    private void Update()
    {
        if (cameraTransform == null || !isTracking)
            return;

        // 현재 위치 가져오기
        Vector3 currentPosition = cameraTransform.position;

        // 이전 위치와의 거리 계산
        float distance = Vector3.Distance(previousPosition, currentPosition);

        // 이동 거리 누적
        if (distance > 0.01f) // 1cm 이상 이동했을 때만 거리 계산
        {
            totalDistance += distance;
        }

        // 이전 위치 갱신
        previousPosition = currentPosition;
    }

    /// <summary>
    /// 이동 거리 측정 시작
    /// </summary>
    public void StartTracking()
    {
        Debug.Log("[MRTKDistanceTracker] Tracking started.");
        totalDistance = 0f; // 거리 초기화
        previousPosition = cameraTransform.position; // 현재 위치 설정
        isTracking = true; // 측정 활성화
    }

    /// <summary>
    /// 이동 거리 측정 종료
    /// </summary>
    public void StopTracking()
    {
        DistanceText.text = totalDistance.ToString("0.00");
        Debug.Log($"[MRTKDistanceTracker] Tracking stopped. Total Distance: {totalDistance:F2} meters");
        isTracking = false; // 측정 비활성화
    }

    /// <summary>
    /// 누적된 이동 거리 반환
    /// </summary>
    public float GetTotalDistance()
    {
        return totalDistance;
    }
}
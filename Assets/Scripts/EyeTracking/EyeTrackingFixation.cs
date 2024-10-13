using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTrackingFixation : MonoBehaviour
{
    /*
    public enum FixationThreshold // 위치 변화 임계 
    {
        Low = 1,  
        Medium = 3, 
        High = 5 
    }
    
    public enum FixationTimeThreshold // 응시 시간 임계 
    {
        Short = 200, //0.2초 
        Medium = 350,  // 0.35초
        Long = 500  // 0.5초
    }

    public FixationThreshold fixationThreshold = FixationThreshold.Medium; 
    public FixationTimeThreshold fixationTimeThreshold = FixationTimeThreshold.Medium; 

    private Vector3 lastEyePosition; 
    private float fixationTimer = 0.0f; 
    private bool isFixating = false;

    void Start()
    {
        lastEyePosition = Vector3.zero;
    }

    void Update()
    {
        Vector3 currentEyePosition = GetEyeTrackingData();

        float angleChange = Vector3.Angle(lastEyePosition, currentEyePosition); // 변화각

        if (angleChange < (float)fixationThreshold)
        {
            fixationTimer += Time.deltaTime;
            if (fixationTimer >= (float)fixationTimeThreshold / 1000 && !isFixating) // ms 단위를 초로 변환
            {
                isFixating = true;
                OnFixationStart();
            }
        }
        else
        {           
            fixationTimer = 0.0f;
            if (isFixating)
            {
                isFixating = false;
                OnFixationEnd();
            }
        }
        lastEyePosition = currentEyePosition;
    }

    private Vector3 GetEyeTrackingData()
    {
        // 메타코어로부터 3차원 벡터 형태로 위치값 실시간 받아오기
    }

    private void OnFixationStart()
    {
        Debug.Log("Fixation started");
    }
    private void OnFixationEnd()
    {
        Debug.Log("Fixation ended");
    }
}*/
}

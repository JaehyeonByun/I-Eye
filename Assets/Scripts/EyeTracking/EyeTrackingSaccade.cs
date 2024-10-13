using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeTrackingSaccade : MonoBehaviour
{
    public float speedThreshold = 30.0f; 
    public float accelerationThreshold = 100.0f; 
    public int sampleCountThreshold = 5; 

    private Queue<Vector2> gazeDataQueue = new Queue<Vector2>();
    private Queue<float> speedDataQueue = new Queue<float>();
    private Vector2 lastGazePosition;
    private float lastTime;

    void Update()
    {
        // 현재 시선 데이터 가져오기
        Vector2 currentGazePosition = GetCurrentGazePosition();
        Debug.Log(currentGazePosition);
        float currentTime = Time.time;

        if (gazeDataQueue.Count == 0)
        {
            lastGazePosition = currentGazePosition;
            lastTime = currentTime;
            gazeDataQueue.Enqueue(currentGazePosition);
            return;
        }

        // 시선 속도 계산
        float timeDelta = currentTime - lastTime;
        float distance = Vector2.Distance(lastGazePosition, currentGazePosition);
        float speed = distance / timeDelta;

        // 속도가 임계값을 초과하는지 확인
        if (speed > speedThreshold)
        {
            gazeDataQueue.Enqueue(currentGazePosition);
            speedDataQueue.Enqueue(speed);
        }

        // 사카드 판단
        if (gazeDataQueue.Count >= sampleCountThreshold)
        {
            float lowestSpeed = float.MaxValue;
            foreach (float s in speedDataQueue)
            {
                if (s < lowestSpeed)
                {
                    lowestSpeed = s;
                }
            }

            float acceleration = (lowestSpeed - speed) / timeDelta;

            if (lowestSpeed > speedThreshold && acceleration > accelerationThreshold)
            {
                Debug.Log("Saccade detected!");
            }

            gazeDataQueue.Dequeue();
            speedDataQueue.Dequeue();
        }

        lastGazePosition = currentGazePosition;
        lastTime = currentTime;
    }

    // 임시로 렌덤벡터 반환하게함
    private Vector2 GetCurrentGazePosition()
    {
        return new Vector2(Random.Range(0f, Screen.width), Random.Range(0f, Screen.height));
    }
}


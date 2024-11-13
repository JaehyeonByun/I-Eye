using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Puzzle : MonoBehaviour
{
    // 빨간벽돌위치들을 배열로 관리
    [SerializeField] private Transform[] redBrickTargets;
    private bool[] isOccupied;  

    void Start()
    {
        isOccupied = new bool[redBrickTargets.Length];
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("빨간벽돌"))
        {
            for (int i = 0; i < redBrickTargets.Length; i++)
            {
                if (!isOccupied[i] && other.CompareTag("빨간벽돌") &&
                    other.bounds.Intersects(redBrickTargets[i].GetComponent<Collider>().bounds))
                {
                    other.transform.position = redBrickTargets[i].position;
                    other.transform.rotation = redBrickTargets[i].rotation;
                    isOccupied[i] = true;
                    Debug.Log("빨간 벽돌 올바른 위치에 고정됨");
                    return;
                }
            }

            Debug.Log("올바르지 않은 위치");
        }
    }
}

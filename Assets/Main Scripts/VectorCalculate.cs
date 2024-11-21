using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VectorCalculate : MonoBehaviour
{
// 3차원 벡터 배열 예시 (x, y, z)
    Vector3[] vectors = new Vector3[]
    {
        new Vector3(1, 2, 3),
        new Vector3(4, 5, 6),
        new Vector3(7, 8, 9),
        new Vector3(10, 11, 12)
    };

    void Start()
    {
        Vector3 totalMovement = CalculateTotalMovement(vectors);
        Debug.Log("총 이동량: " + totalMovement);
    }

    Vector3 CalculateTotalMovement(Vector3[] vectorArray)
    {
        Vector3 totalMovement = Vector3.zero;
        for (int i = 1; i < vectorArray.Length; i++)
        {
            Vector3 movement = vectorArray[i] - vectorArray[i - 1];
            totalMovement += movement;
        }
        return totalMovement;
    }
}

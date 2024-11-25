using Unity.Barracuda;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using System.Collections;
using System.Collections.Generic;

public class ADHDPreprocessors : MonoBehaviour
{
    public ADHDParameters adhdParameters;
    
    private int[] inputArray = new int[18];
    public float[] normalizedArray = new float[18];
    public bool isOver = false;

    public IEnumerator HandleStatistics()
    {
        Dictionary<string, Dictionary<string, float>> statistics = null;
        
        yield return StartCoroutine(DatabaseManager.GetStatistics(result =>
        {
            statistics = result;
        }));
        if (statistics != null)
        {
            Debug.Log("Statistics fetched successfully!");
        }
        else
        {
            Debug.LogError("Failed to fetch statistics.");
        }
        
        for (int i = 0; i < adhdParameters.inattentionMetrics.Length; i++)
        {
            inputArray[i] = adhdParameters.inattentionMetrics[i].value;
        }
        
        for (int i = 0; i < adhdParameters.hyperActivityMetric.Length; i++)
        {
            inputArray[i + 9] = adhdParameters.hyperActivityMetric[i].value;
        }

        for (int i = 0; i < inputArray.Length; i++)
        {
            normalizedArray[i] = Sigmoid(inputArray[i]); // Used Sigmoid instead of Normalized
        }
        isOver = true;
    }
    private static float Sigmoid(float x)
    {
        return 1f / (1f + Mathf.Exp(-x));
    }
}


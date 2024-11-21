using Unity.Barracuda;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;
using System.Collections;
using System.Collections.Generic;

public class DataPreprocessor : MonoBehaviour
{
    private int[] inputArray = new int[18];
    public float[] normalizedArray = new float[18];
    public bool isOver = false;
    // private int[] distances = new int[2];
    
    // Method to preprocess data for all 18 symptoms into a 6x3 tensor
    // public Tensor PreprocessData()
    // {
    //     // Initialize a float array to store 18 symptom scores
    //     float[] symptomScores = new float[18];

    //     // Step 2: Reshape the symptomScores array into a 6x3 tensor for model input
    //     Tensor inputTensor = new Tensor(1, 1, 6, 3);
    //     for (int i = 0; i < 18; i++)
    //     {
    //         int row = i / 3;
    //         int col = i % 3;
    //         inputTensor[0, 0, row, col] = symptomScores[i];
    //     }

    //     return inputTensor;
    // }

    public IEnumerator HandleStatistics()
    {
        Dictionary<string, Dictionary<string, float>> statistics = null;

        // Call the GetStatistics coroutine and wait for it to finish
        yield return StartCoroutine(DatabaseManager.GetStatistics(result =>
        {
            statistics = result;
        }));

        // Now statistics is available, proceed with your code
        if (statistics != null)
        {
            Debug.Log("Statistics fetched successfully!");
            // foreach (var outerKey in statistics.Keys)
            // {
            //     Debug.Log($"Category: {outerKey}");
            //     foreach (var innerKey in statistics[outerKey].Keys)
            //     {
            //         Debug.Log($"{innerKey}: {statistics[outerKey][innerKey]}");
            //     }
            // }
        }
        else
        {
            Debug.LogError("Failed to fetch statistics.");
        }

        // Place additional logic here that depends on statistics being fetched
        inputArray[0] = GameManager.Inattention_a[0];
        inputArray[1] = GameManager.Inattention_b[0];
        inputArray[2] = GameManager.Inattention_c[0];
        inputArray[3] = GameManager.Inattention_d[0];
        inputArray[4] = GameManager.Inattention_e[0];
        inputArray[5] = GameManager.Inattention_f[0];
        inputArray[6] = GameManager.Inattention_g[0];
        inputArray[7] = GameManager.Inattention_h[0];
        inputArray[8] = GameManager.Inattention_i[0];

        inputArray[9] = GameManager.HyperActivity_a[0];
        // inputArray[0] = distances[0];
        inputArray[10] = GameManager.HyperActivity_b[0];
        inputArray[11] = GameManager.HyperActivity_c[0];
        inputArray[12] = GameManager.HyperActivity_d[0];
        inputArray[13] = GameManager.HyperActivity_e[0];
        // inputArray[4] = distances[1];
        inputArray[14] = GameManager.HyperActivity_f[0];
        inputArray[15] = GameManager.HyperActivity_g[0];
        // inputArray[7] = GameManager.HyperActivity_h[0];
        inputArray[16] = 0;
        inputArray[17] = GameManager.HyperActivity_i[0];

        for (int i = 0; i < inputArray.Length; i++)
        {
            normalizedArray[i] = Sigmoid(inputArray[i]);
        }
        
        isOver = true;
        // normalizedArray[0] = (float)inputArray[0];
        // normalizedArray[1] = (float)inputArray[1];
        // normalizedArray[2] = (float)inputArray[2];
        // normalizedArray[3] = (float)inputArray[3];
        // normalizedArray[4] = (float)inputArray[4];
        // normalizedArray[5] = (float)inputArray[5];
        // normalizedArray[6] = (float)inputArray[6];
        // normalizedArray[7] = (float)inputArray[7];
        // normalizedArray[8] = (float)inputArray[8];

        // normalizedArray[9] = (float)inputArray[9];
        // normalizedArray[10] = (float)inputArray[10];
        // normalizedArray[11] = (float)inputArray[11];
        // normalizedArray[12] = (float)inputArray[12];
        // normalizedArray[13] = (float)inputArray[13];
        // normalizedArray[14] = (float)inputArray[14];
        // normalizedArray[15] = (float)inputArray[15];
        // normalizedArray[16] = (float)inputArray[16];
        // normalizedArray[17] = (float)inputArray[17];
    }

    // Sigmoid function
    private static float Sigmoid(float x)
    {
        return 1f / (1f + Mathf.Exp(-x));
    }
}

using Unity.Barracuda;
using UnityEngine;
using UnityEngine.InputSystem.Interactions;

public class DataPreprocessor
{
    private int[] inputArray = new int[18];
    private float[] normalizedArray = new float[18];
    // private int[] distances = new int[2];
    
    // Method to preprocess data for all 18 symptoms into a 6x3 tensor
    public Tensor PreprocessData()
    {
        

        // Initialize a float array to store 18 symptom scores
        float[] symptomScores = new float[18];

        // Step 2: Reshape the symptomScores array into a 6x3 tensor for model input
        Tensor inputTensor = new Tensor(1, 1, 6, 3);
        for (int i = 0; i < 18; i++)
        {
            int row = i / 3;
            int col = i % 3;
            inputTensor[0, 0, row, col] = symptomScores[i];
        }

        return inputTensor;
    }

    private float[] Normalize() {
        // distances = DistanceCalculator.Calculate();
        
        inputArray[0] = GameManager.Inattention_a[0];
        inputArray[1] = GameManager.Inattention_b[0];
        inputArray[2] = GameManager.Inattention_c[0];
        inputArray[3] = GameManager.Inattention_d[0];
        inputArray[4] = GameManager.Inattention_e[0];
        inputArray[5] = GameManager.Inattention_f[0];
        inputArray[6] = GameManager.Inattention_g[0];
        inputArray[7] = GameManager.Inattention_h[0];
        inputArray[8] = GameManager.Inattention_i[0];

        inputArray[0] = GameManager.HyperActivity_a[0];
        // inputArray[0] = distances[0];
        inputArray[1] = GameManager.HyperActivity_b[0];
        inputArray[2] = GameManager.HyperActivity_c[0];
        inputArray[3] = GameManager.HyperActivity_d[0];
        inputArray[4] = GameManager.HyperActivity_e[0];
        // inputArray[4] = distances[1];
        inputArray[5] = GameManager.HyperActivity_f[0];
        inputArray[6] = GameManager.HyperActivity_g[0];
        // inputArray[7] = GameManager.HyperActivity_h[0];
        inputArray[7] = 0;
        inputArray[8] = GameManager.HyperActivity_i[0];
        
        for (int i = 0; i < 9; i++)
        {
            ;
        }
        for (int i = 0; i < 9; i++)
        {
            ;
        }

        return normalizedArray;
    }
}

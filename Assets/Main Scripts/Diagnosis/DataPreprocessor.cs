using Unity.Barracuda;
using UnityEngine;

public class DataPreprocessor
{
    // Method to preprocess data for all 18 symptoms into a 6x3 tensor
    public Tensor PreprocessData()
    {
        // Initialize a float array to store 18 symptom scores
        float[] symptomScores = new float[18];

        Debug.Log(GameManager.Inattention_a);

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
}

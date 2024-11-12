using Unity.Barracuda;
using UnityEngine;

public class ADHDDataPreprocessor
{
    // Method to preprocess data for all 18 symptoms into a 6x3 tensor
    public Tensor PreprocessData(
        float responseTime, int errors, int omissions, 
        float headMovement, float handMovement, int interactionCount, 
        float voiceVolume, float voiceActivityDuration,
        float prematureResponseTime, int interruptions,
        int misplacements, float searchTime, int instructionRepetitions,
        float focusOnUnrelatedObjects, int quietActivityDisturbances, 
        int climbAttempts, int prematureActions, float taskCompletionRate)
    {
        // Initialize a float array to store 18 symptom scores
        float[] symptomScores = new float[18];

        // Symptom 1: Careless mistakes (Normalized error count)
        symptomScores[0] = Mathf.Clamp01(errors / 5f);

        // Symptom 2: Difficulty sustaining attention (Normalized response time and accuracy)
        symptomScores[1] = Mathf.Clamp01(responseTime / 10f);

        // Symptom 3: Does not seem to listen when spoken to (Instruction repetition count)
        symptomScores[2] = Mathf.Clamp01(instructionRepetitions / 3f);

        // Symptom 4: Does not follow through on instructions and fails to finish tasks
        symptomScores[3] = 1f - Mathf.Clamp01(taskCompletionRate);

        // Symptom 5: Difficulty organizing tasks (Time taken to start/organize)
        symptomScores[4] = Mathf.Clamp01(responseTime / 15f);

        // Symptom 6: Avoids tasks requiring sustained mental effort
        symptomScores[5] = Mathf.Clamp01(quietActivityDisturbances / 3f);

        // Symptom 7: Loses things (Misplacements)
        symptomScores[6] = Mathf.Clamp01(misplacements / 5f);

        // Symptom 8: Easily distracted (Time focused on unrelated objects)
        symptomScores[7] = Mathf.Clamp01(focusOnUnrelatedObjects / 10f);

        // Symptom 9: Forgetful in daily activities (Instruction repetitions)
        symptomScores[8] = Mathf.Clamp01(instructionRepetitions / 3f);

        // Hyperactivity-Impulsivity Symptoms

        // Symptom 10: Fidgets or squirms (Hand movement frequency)
        symptomScores[9] = Mathf.Clamp01(handMovement / 100f);

        // Symptom 11: Leaves seat (Movement or standing time)
        symptomScores[10] = Mathf.Clamp01(headMovement / 100f);

        // Symptom 12: Runs or climbs excessively (Climb attempts)
        symptomScores[11] = Mathf.Clamp01(climbAttempts / 3f);

        // Symptom 13: Difficulty with quiet activities (Voice volume level)
        symptomScores[12] = Mathf.Clamp01(voiceVolume);

        // Symptom 14: "On the go" (Overall movement frequency)
        symptomScores[13] = Mathf.Clamp01((headMovement + handMovement) / 200f);

        // Symptom 15: Talks excessively (Voice activity duration)
        symptomScores[14] = Mathf.Clamp01(voiceActivityDuration / 60f);

        // Symptom 16: Blurts out answers (Premature response time)
        symptomScores[15] = Mathf.Clamp01(prematureResponseTime / 1f);

        // Symptom 17: Difficulty awaiting turn (Premature actions)
        symptomScores[16] = Mathf.Clamp01(prematureActions / 3f);

        // Symptom 18: Interrupts or intrudes (Interruptions count)
        symptomScores[17] = Mathf.Clamp01(interruptions / 5f);

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

using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class DatabaseManager : MonoBehaviour
{
    // Random integer array of size 18
    private int[] inputArray;

    // URL for the POST request
    private string url = "http://127.0.0.1:8000/users/1/trials/";

    void Start()
    {
        // Initialize the inputArray with random integers
        inputArray = GenerateRandomArray(18, 1, 10000); // 18 random integers between 1 and 10,000
    }

    void Update()
    {
        // Listen for 'E' key press
        if (Input.GetKeyDown(KeyCode.E))
        {
            StartCoroutine(PostData());
        }
    }

    IEnumerator PostData()
    {
        // Construct the JSON body manually
        string jsonData = "{"
            + "\"inattention_a\":" + inputArray[0] + ","
            + "\"inattention_b\":" + inputArray[1] + ","
            + "\"inattention_c\":" + inputArray[2] + ","
            + "\"inattention_d\":" + inputArray[3] + ","
            + "\"inattention_e\":" + inputArray[4] + ","
            + "\"inattention_f\":" + inputArray[5] + ","
            + "\"inattention_g\":" + inputArray[6] + ","
            + "\"inattention_h\":" + inputArray[7] + ","
            + "\"inattention_i\":" + inputArray[8] + ","
            + "\"hyperactivity_a\":" + inputArray[9] + ","
            + "\"hyperactivity_b\":" + inputArray[10] + ","
            + "\"hyperactivity_c\":" + inputArray[11] + ","
            + "\"hyperactivity_d\":" + inputArray[12] + ","
            + "\"hyperactivity_e\":" + inputArray[13] + ","
            + "\"hyperactivity_f\":" + inputArray[14] + ","
            + "\"hyperactivity_g\":" + inputArray[15] + ","
            + "\"hyperactivity_h\":" + inputArray[16] + ","
            + "\"hyperactivity_i\":" + inputArray[17] + "}";

        Debug.Log("JSON Payload: " + jsonData);

        using (UnityWebRequest webRequest = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.SetRequestHeader("Content-Type", "application/json");

            // Send the request
            yield return webRequest.SendWebRequest();

            // Handle the response
            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("POST successful: " + webRequest.downloadHandler.text);
            }
            else
            {
                Debug.LogError("POST failed: " + webRequest.error);
                Debug.LogError("Response Code: " + webRequest.responseCode);
                Debug.LogError("Response Body: " + webRequest.downloadHandler.text);
            }
        }
    }
        

    // Generate a random integer array
    private int[] GenerateRandomArray(int size, int minValue, int maxValue)
    {
        int[] randomArray = new int[size];
        for (int i = 0; i < size; i++)
        {
            randomArray[i] = Random.Range(minValue, maxValue);
        }
        return randomArray;
    }

    // Wrapper for serializing dictionary to JSON
    [System.Serializable]
    private class JsonWrapper
    {
        public Dictionary<string, int> data;

        public JsonWrapper(Dictionary<string, int> dictionary)
        {
            data = dictionary;
        }
    }
}

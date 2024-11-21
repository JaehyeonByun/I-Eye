using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class DatabaseManager : MonoBehaviour
{
    // Random integer array of size 18
    private int[] inputArray = new int[18];
    // private int[] distances = new int[2];

    // URL for the POST request
    private string url = "http://124.5.166.85:8000/users/1/trials/";
    private static string statisticsUrl = "http://124.5.166.85:8000/statistics/";

    void Start()
    {
        // Initialize the inputArray with random integers
        // inputArray = GenerateRandomArray(18, 1, 10000); // 18 random integers between 1 and 10,000
    }

    void Update()
    {
        // // Listen for 'P' key press
        // if (Input.GetKeyDown(KeyCode.P))
        // {
        //     // StartCoroutine(PostData());
        //     // Fetch statistics and log mean of inattention_a
        //     StartCoroutine(GetStatistics((statistics) =>
        //     {
        //         if (statistics != null)
        //         {
        //             Debug.Log("Statistics fetched successfully!");
                    
        //             // Access mean of inattention_a
        //             if (statistics.ContainsKey("mean") && statistics["mean"].ContainsKey("inattention_a"))
        //             {
        //                 float inattentionAMean = statistics["mean"]["inattention_a"];
        //                 Debug.Log($"Mean of inattention_a: {inattentionAMean}");
        //             }
        //             else
        //             {
        //                 Debug.LogError("Key 'inattention_a' not found in 'mean'.");
        //             }
        //         }
        //         else
        //         {
        //             Debug.LogError("Failed to fetch statistics.");
        //         }
        //     }));
        // }
        
    }

    public IEnumerator PostData()
    {
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

    public static IEnumerator GetStatistics(System.Action<Dictionary<string, Dictionary<string, float>>> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(statisticsUrl))
        {
            // Send the request
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.Success)
            {
                try
                {
                    // Parse the JSON response using SimpleJSON
                    string jsonResponse = webRequest.downloadHandler.text;
                    var jsonObject = JSON.Parse(jsonResponse);

                    // Convert JSON to nested dictionary
                    var statistics = new Dictionary<string, Dictionary<string, float>>
                    {
                        { "mean", ParseNestedDictionary(jsonObject["mean"]) },
                        { "std", ParseNestedDictionary(jsonObject["std"]) }
                    };

                    // Invoke the callback with the parsed data
                    callback(statistics);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to parse JSON: " + e.Message);
                    callback(null);
                }
            }
            else
            {
                Debug.LogError("GET failed: " + webRequest.error);
                Debug.LogError("Response Code: " + webRequest.responseCode);
                callback(null);
            }
        }
    }

    // Helper method to convert SimpleJSON JSONNode to Dictionary<string, float>
    private static Dictionary<string, float> ParseNestedDictionary(JSONNode node)
    {
        var result = new Dictionary<string, float>();
        foreach (var key in node.Keys)
        {
            result[key] = node[key].AsFloat;
        }
        return result;
    }
}

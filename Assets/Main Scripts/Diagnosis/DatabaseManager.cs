using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using SimpleJSON;

public class DatabaseManager : MonoBehaviour
{
    public ADHDParameters adhdParameters;
    
    private int[] inputArray = new int[18];
    private string url = "http://124.5.166.85:8000/users/1/trials/";
    private static string statisticsUrl = "http://124.5.166.85:8000/statistics/";

    public IEnumerator PostData()
    {
        for (int i = 0; i < adhdParameters.inattentionMetrics.Length; i++)
        {
            inputArray[i] = adhdParameters.inattentionMetrics[i].value;
        }
        
        for (int i = 0; i < adhdParameters.hyperActivityMetric.Length; i++)
        {
            inputArray[i + 9] = adhdParameters.hyperActivityMetric[i].value;
        }
        string[] inattentionKeys = new string[] {
            "inattention_a", "inattention_b", "inattention_c", "inattention_d", "inattention_e",
            "inattention_f", "inattention_g", "inattention_h", "inattention_i"
        };

        string[] hyperactivityKeys = new string[] {
            "hyperactivity_a", "hyperactivity_b", "hyperactivity_c", "hyperactivity_d", "hyperactivity_e",
            "hyperactivity_f", "hyperactivity_g", "hyperactivity_h", "hyperactivity_i"
        };

        string jsonData = "{";

// Add inattention values
        for (int i = 0; i < inattentionKeys.Length; i++)
        {
            jsonData += $"\"{inattentionKeys[i]}\":{inputArray[i]},";
        }

// Add hyperactivity values
        for (int i = 0; i < hyperactivityKeys.Length; i++)
        {
            jsonData += $"\"{hyperactivityKeys[i]}\":{inputArray[i + inattentionKeys.Length]},";
        }

// Add prediction values
        jsonData += $"\"predict_a\":\"{ADHDModelRunner.resultArray[0]}\",";
        jsonData += $"\"predict_b\":\"{ADHDModelRunner.resultArray[1]}\",";
        jsonData += $"\"predict_c\":\"{ADHDModelRunner.resultArray[2]}\",";
        jsonData += $"\"predict_d\":\"{ADHDModelRunner.resultArray[3]}\"";

// Close JSON string
        jsonData += "}";

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

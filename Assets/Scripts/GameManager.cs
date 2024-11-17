using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;

public class GameManager : MonoBehaviour
{
    //The GameManager records 18 biometric data points necessary for DSM-5 diagnosis.

    // Inattention Parameter
    public static List<float> Inattention_a = new List<float>();
    public static List<float> Inattention_b = new List<float>();
    public static List<float> Inattention_c = new List<float>();
    public static List<float> Inattention_d = new List<float>();
    public static List<float> Inattention_e = new List<float>();
    public static List<float> Inattention_f = new List<float>();
    public static List<float> Inattention_g = new List<float>();
    public static List<float> Inattention_i = new List<float>();

    // HyperActivity Parameter
    public static List<float> HyperActivity_a = new List<float>();
    public static List<float> HyperActivity_b = new List<float>();
    public static List<float> HyperActivity_c = new List<float>();
    public static List<float> HyperActivity_d = new List<float>();
    public static List<float> HyperActivity_e = new List<float>();
    public static List<float> HyperActivity_f = new List<float>();
    public static List<float> HyperActivity_g = new List<float>();
    public static List<float> HyperActivity_i = new List<float>();


    public static GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Debug.LogWarning("씬에 두개 이상의 게임 매니저가 존재합니다!");
            Destroy(gameObject);
        }
    }

    public static void MakeInputTensorCSV()
    {
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = "ArraysData_" + timestamp + ".csv";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);

        string[] arrayPrefixes = { "Inattention", "HyperActivity" };

        using (StreamWriter writer = new StreamWriter(filePath))
        {

            foreach (string prefix in arrayPrefixes)
            {
                foreach (FieldInfo field in typeof(GameManager).GetFields(BindingFlags.Public | BindingFlags.Static))
                {
                    if (field.Name.StartsWith(prefix) && field.FieldType == typeof(float[]))
                    {
                        writer.WriteLine(field.Name);

                        float[] array = (float[])field.GetValue(null);
                        foreach (float value in array)
                        {
                            writer.WriteLine(value);
                        }

                        writer.WriteLine();
                    }
                }
            }
        }

        Debug.Log("CSV 파일 저장 완료: " + filePath);
    }
}



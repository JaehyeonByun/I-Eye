using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;

public class GameManager : MonoBehaviour
{
    public static float[] Inattention_a;
    public static float[] Inattention_b;
    public static float[] Inattention_c;
    public static float[] Inattention_d;
    public static float[] Inattention_e;
    public static float[] Inattention_f;
    public static float[] Inattention_g;
    public static float[] Inattention_i;
    
    public static float[] HyperActivity_a;
    public static float[] HyperActivity_b;
    public static float[] HyperActivity_c;
    public static float[] HyperActivity_d;
    public static float[] HyperActivity_e;
    public static float[] HyperActivity_f;
    public static float[] HyperActivity_g;
    public static float[] HyperActivity_i;
    
    
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
    }}

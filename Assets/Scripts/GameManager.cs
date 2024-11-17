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
    public static List<int> Inattention_f = new List<int>();
    public static List<float> Inattention_g = new List<float>();
    public static List<int> Inattention_h = new List<int>();
    public static List<float> Inattention_i = new List<float>();

    // HyperActivity Parameter
    public static List<float> HyperActivity_a = new List<float>();
    public static List<float> HyperActivity_b = new List<float>();
    public static List<float> HyperActivity_c = new List<float>();
    public static List<int> HyperActivity_d = new List<int>();
    public static List<float> HyperActivity_e = new List<float>();
    public static List<float> HyperActivity_f = new List<float>();
    public static List<float> HyperActivity_g = new List<float>();
    public static List<float> HyperActivity_i = new List<float>();
    
    
    /*
    //inattentionA (Level 1)
    public static int Ia_WrongMudCount = 0;
    
    //inattentionB (Level 2)
    public static float Ib_DontSeeOven = 0f;
    
    //inattentionC (Level 0, Level 1, Level 2)
    public static int Ic_SpeakAgain = 0;
    
    //inattentionD (Level 1)
    public static int Id_Hint = 0;
    
    //inattentionE (Level 0)
    public static int Ia_IsFun = 0;
    
    //inattentionF (Level 0, Level 1)
    public static float Hd_Skip = 0f;
    
    //inattentionG
    
    //inattentionH (Level 1)
    public static int Hh_Distracted = 0;
    
    //inattentionI (Level 1)
    public static float Hh_ClearTime = 0f;
    
    //hyperactivityA (Level 1)
    public static int Ia_WrongMudCount = 0;
    
    //hyperactivityB (Level 2)
    public static float Ib_DontSeeOven = 0f;
    
    //hyperactivityC (Level 0, Level 1, Level 2)
    public static int Ic_SpeakAgain = 0;
    
    //hyperactivityD (Level 1)
    public static int Id_Hint = 0;
    
    //hyperactivityE (Level 0)
    public static int Ia_IsFun = 0;
    
    //hyperactivityF (Level 0, Level 1)
    public static float Hd_Skip = 0f;
    
    //hyperactivityG
    
    //hyperactivityH (Level 1)
    public static int Hh_Distracted = 0;
    
    //hyperactivityI (Level 1)
    public static float Hh_ClearTime = 0f;
*/

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
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            MakeInputTensorCSV();
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



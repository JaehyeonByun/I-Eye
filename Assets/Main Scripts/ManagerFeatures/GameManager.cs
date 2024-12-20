using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Reflection;

public class GameManager : MonoBehaviour
{ 
    /// <summary>
    /// The GameManager records 18 biometric data points necessary for DSM-5 diagnosis.
    /// ---------------------------------------------------------------------------------------
    /// Inattention a : Fails to give close attention to details or mistakes in schoolwork
    /// <param name="Inattention_a"> 0단계 에서 잘못된 진흙을 주운 횟수 (Count)</param>
    /// 
    /// Inattention b : Has difficulty sustaining attention in tasks or play activities
    /// <param name="Inattention_b">1단계에서 오븐이 아닌 다른 곳을 본 시간 (Time) </param>
    /// 
    /// Inattention c : Does not seem to listen when spoken to directly
    /// <param name="Inattention_c">돼지가 설명하면서 말을 걸 때, 알겠어가 아닌 다시 들려줘를 누른 횟수 (Count)</param>
    /// 
    /// Inattention d : Does not follow through on instructions and fails to finish work
    /// <param name="Inattention_d">0단계에서 힌트를 누른 횟수 (Count)</param>
    /// 
    /// Inattention e : Has difficulty organizing tasks and activities
    /// <param name="Inattention_e">????</param>
    /// 
    /// Inattention f : Avoids tasks (e.g. schoolwork and homework) that require sustained mental effort
    /// <param name="Inattention_f">긴 설명이 있는 UI 내에서 일정 시간 (5초) 안에 대충 넘어감 (Selection)</param>
    /// 
    /// Inattention g : Loses things necessary for tasks or activities
    /// <param name="Inattention_g">????</param>
    /// 
    /// Inattention h : Is easily distracted
    /// <param name="Inattention_h">1단계에서 돼지가 말을 걸면서 방해할 때 집중력이 흐트러진 횟수 (Count)</param>
    /// 
    /// Inattention i : Is forgetful in daily activities
    /// <param name="Inattention_i">첫0단계를 클리어하는데 걸린 시간 (Time)</param>
    /// ---------------------------------------------------------------------------------------
    /// Hyperactivity a: Fidgets with hands or feet or squirms in seat
    /// <param name="HyperActivity_a">설명을 듣는 동안 손목의 움직임 (Number of times sensors detect frequent repetitive movements)</param>
    /// 
    /// Hyperactivity b: Leaves seat in classroom or in other situations in which remaining seated is expected
    /// <param name="HyperActivity_b">0단계에서 플레이어가 움직인 총 누적 거리 (Distance moved during task)</param>
    /// 
    /// Hyperactivity c: Runs about or climbs excessively in situations in which it is inappropriate
    /// <param name="HyperActivity_c">0단계에서 플레이어가 뛴 시간 (Distance after given instruction to walk)</param>
    /// 
    /// Hyperactivity d: Has difficulty playing or engaging in leisure activities quietly
    /// <param name="HyperActivity_d">1단계에서 삽이 지정된 위치를 벗어난 횟수 (Length of time between beginning ending of scene)</param>
    /// 
    /// Hyperactivity e: Is “on the go” or acts as if “driven by a motor”
    /// <param name="HyperActivity_e">설명을 듣는 동안 몸의 움직임 (Position Vector)</param>
    /// 
    /// Hyperactivity f: Talks excessively
    /// <param name="HyperActivity_f">돼지가 말하는 동안 말을 한 횟수 (Number of times speech exceeds threshold of 60 dB when others are speaking)</param>
    /// 
    /// Hyperactivity g: Blurts out answers before questions have been completed
    /// <param name="HyperActivity_g">설명이 끝나기도 전에 일정 범위 이상 움직인 횟수 (Count)</param>
    /// 
    /// Hyperactivity h: Has difficulty awaiting turn
    /// <param name="HyperActivity_h">????</param>
    /// 
    /// Hyperactivity i: Interrupts or intrudes on others
    /// <param name="HyperActivity_i">내 삽이 아닌 다른 삽을 건드린 횟수 (Count)</param>
    /// 
    /// </summary>

// 시간초 b,i b(거리),f
    
    // Inattention Parameter
    public static List<int> Inattention_a = new List<int>();
    public static List<int> Inattention_b = new List<int>();
    public static List<int> Inattention_c = new List<int>();
    public static List<int> Inattention_d = new List<int>();
    public static List<int> Inattention_e = new List<int>();
    public static List<int> Inattention_f = new List<int>();
    public static List<int> Inattention_g = new List<int>();
    public static List<int> Inattention_h = new List<int>();
    public static List<int> Inattention_i = new List<int>();

    // HyperActivity Parameter
    public static List<int> HyperActivity_a = new List<int>();
    public static List<int> HyperActivity_b = new List<int>();
    public static List<int> HyperActivity_c = new List<int>();
    public static List<int> HyperActivity_d = new List<int>();
    public static List<int> HyperActivity_e = new List<int>();
    public static List<int> HyperActivity_f = new List<int>();
    public static List<int> HyperActivity_h = new List<int>();
    public static List<int> HyperActivity_g = new List<int>();
    public static List<int> HyperActivity_i = new List<int>();
    
    public static bool _onIntroducing = false;
    
    public static List<(float time, Vector3 position)> HyperActivity_a_List = new List<(float, Vector3)>();
    public static List<(float time, Vector3 position)> HyperActivity_e_List = new List<(float, Vector3)>();
    
    public static GameManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("There is more than one GameManager in scene.");
            Destroy(gameObject);
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            PrintAllArrays();
        }
    }
    
    public static void PrintAllArrays()
    {
        // Get the GameManager type
        Type gameManagerType = typeof(GameManager);

        // Create a stream to write the output
        string timestamp = System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string fileName = "ArrayDataOutput_" + timestamp + ".txt";
        string filePath = Path.Combine(Application.persistentDataPath, fileName);
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            // Iterate over all public static fields
            foreach (FieldInfo field in gameManagerType.GetFields(BindingFlags.Public | BindingFlags.Static))
            {
                // Only process List or array types
                if (field.FieldType.IsGenericType && field.FieldType.GetGenericTypeDefinition() == typeof(List<>))
                {
                    writer.WriteLine("List: " + field.Name);

                    // Get the list and its elements
                    var list = (IList)field.GetValue(null);

                    // Write the list's elements to the file
                    foreach (var item in list)
                    {
                        writer.WriteLine(item.ToString());
                    }
                    writer.WriteLine(); // Add a blank line for separation
                }
            }
        }

        Debug.Log("Array data saved to: " + filePath);
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



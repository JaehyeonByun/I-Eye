using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class DiagnosisDataManger1 : MonoBehaviour
{
    [SerializeField] private ClayPickupManager _clayPickupManager;
    [SerializeField] private UIScenario _slideManager;
    [SerializeField] private TutorialManager _tutorialManager ;
    [SerializeField] private UIHyperactivity _hyperActivityChecker ;
    [SerializeField] private DistanceAndTime _distanceAndTime ;
    
    [SerializeField] private GameObject clearUI; // Reference to the clearUI object

    private bool hasSavedData = false; // Flag to ensure SaveData only runs once
    
    public void SaveData()
    {
        float clearTime = _distanceAndTime.GetComponent<DistanceAndTime>().gamingTime;
        int resultClearTime = Mathf.RoundToInt(clearTime * 100f); // 소수점 두 자리까지 반올림 후 정수형으로 변환
        
        float totalDistance = _distanceAndTime.GetComponent<DistanceAndTime>().totalDistance;
        int resultTotalDistance = Mathf.RoundToInt(totalDistance * 100f);
        
        GameManager.Inattention_a.Add(_clayPickupManager.GetComponent<ClayPickupManager>().incorrectClayCount); // 함정 진흙을 주운 횟수 --
        GameManager.Inattention_c.Add(_clayPickupManager.GetComponent<ClayPickupManager>().SayAgainCount); // 다시 들은 횟수
        GameManager.Inattention_d.Add(_clayPickupManager.GetComponent<ClayPickupManager>().HintClicekdCount/2); // 힌트 누른 횟수
        GameManager.Inattention_e.Add(_clayPickupManager.GetComponent<ClayPickupManager>().incorrectSequenceClayCount); // 잘못된 순서로 진흙을 주운 횟수 --
        GameManager.Inattention_g.Add(_clayPickupManager.GetComponent<ClayPickupManager>().WrongButtonClicked); // 진흙 횟수를 기억 못함
        GameManager.Inattention_f.Add(_hyperActivityChecker.GetComponent<UIHyperactivity>().isHyperActivity); // 성급하게 안읽고 넘긴 횟수
        GameManager.Inattention_i.Add(resultClearTime); // 클리어하는데 걸린 시간
        
        GameManager.HyperActivity_a.Add(WristMovementSum()); //
        GameManager.HyperActivity_b.Add(resultTotalDistance); // 총 이동거리
        GameManager.HyperActivity_c.Add(_clayPickupManager.GetComponent<ClayPickupManager>().jumpCount); // 점프 횟수
        GameManager.HyperActivity_e.Add(BodyMovementSum());
        
        hasSavedData = true; // Mark that data has been saved
    }
    public void DebugSavedData()
    {
        Debug.Log("----------------------------------------------------");
        Debug.Log($"Incorrect Clay Count: {_clayPickupManager.GetComponent<ClayPickupManager>().incorrectClayCount}");
        Debug.Log($"Say Again Count: {_clayPickupManager.GetComponent<ClayPickupManager>().SayAgainCount}");
        Debug.Log($"Hint Clicked Count: {_clayPickupManager.GetComponent<ClayPickupManager>().HintClicekdCount}");
        Debug.Log($"Incorrect Sequence Clay Count: {_clayPickupManager.GetComponent<ClayPickupManager>().incorrectSequenceClayCount}");
        Debug.Log($"Wrong Button Clicked: {_clayPickupManager.GetComponent<ClayPickupManager>().WrongButtonClicked}");
        Debug.Log($"Is Hyper Activity: {_hyperActivityChecker.GetComponent<UIHyperactivity>().isHyperActivity}");

        float clearTime = _distanceAndTime.GetComponent<DistanceAndTime>().gamingTime;
        int resultClearTime = Mathf.RoundToInt(clearTime * 100f);
        Debug.Log($"Clear Time (Rounded): {resultClearTime}");

        float totalDistance = _distanceAndTime.GetComponent<DistanceAndTime>().totalDistance;
        int resultTotalDistance = Mathf.RoundToInt(totalDistance * 100f);
        Debug.Log($"Total Distance (Rounded): {resultTotalDistance}");

        Debug.Log($"Jump Count: {_clayPickupManager.GetComponent<ClayPickupManager>().jumpCount}");
        Debug.Log("----------------------------------------------------");
    }

    void Update()
    
    {  if (clearUI.activeSelf && !hasSavedData)
        {
            SaveData();
        }
        if (Input.GetKeyDown(KeyCode.O))
        {
            DebugSavedData();
        }
    }
    
    public int WristMovementSum()
    {
        float totalDisplacement = 0f;

        for (int i = 1; i < GameManager.HyperActivity_a_List.Count; i++)
        {
            Vector3 currentPosition = GameManager.HyperActivity_a_List[i].position;
            Vector3 previousPosition = GameManager.HyperActivity_a_List[i - 1].position;

            float displacement = Vector3.Distance(currentPosition, previousPosition);

            totalDisplacement += displacement;
        }
        return Mathf.RoundToInt(totalDisplacement * 100);
    }
    public int BodyMovementSum()
    {
        float totalDisplacement = 0f;

        for (int i = 1; i < GameManager.HyperActivity_e_List.Count; i++)
        {
            Vector3 currentPosition = GameManager.HyperActivity_e_List[i].position;
            Vector3 previousPosition = GameManager.HyperActivity_e_List[i - 1].position;

            float displacement = Vector3.Distance(currentPosition, previousPosition);

            totalDisplacement += displacement;
        }
        return Mathf.RoundToInt(totalDisplacement * 100);
    }
}

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
    
    public void SaveData()
    {
        float clearTime = _distanceAndTime.GetComponent<DistanceAndTime>().gamingTime;
        int resultClearTime = Mathf.RoundToInt(clearTime * 100f); // 소수점 두 자리까지 반올림 후 정수형으로 변환
        
        float totalDistance = _distanceAndTime.GetComponent<DistanceAndTime>().totalDistance;
        int resultTotalDistance = Mathf.RoundToInt(totalDistance * 100f);
        
        GameManager.Inattention_a.Add(_clayPickupManager.GetComponent<ClayPickupManager>().incorrectClayCount); // 함정 진흙을 주운 횟수 --
        GameManager.Inattention_c.Add(_clayPickupManager.GetComponent<ClayPickupManager>().SayAgainCount); // 다시 들은 횟수
        GameManager.Inattention_d.Add(_clayPickupManager.GetComponent<ClayPickupManager>().HintClicekdCount); // 힌트 누른 횟수
        GameManager.Inattention_e.Add(_clayPickupManager.GetComponent<ClayPickupManager>().incorrectSequenceClayCount); // 잘못된 순서로 진흙을 주운 횟수 --
        GameManager.Inattention_g.Add(_clayPickupManager.GetComponent<ClayPickupManager>().WrongButtonClicked); // 진흙 횟수를 기억 못함
        GameManager.Inattention_f.Add(_hyperActivityChecker.GetComponent<UIHyperactivity>().isHyperActivity); // 성급하게 안읽고 넘긴 횟수
        GameManager.Inattention_i.Add(resultClearTime); // 클리어하는데 걸린 시간
        
        GameManager.HyperActivity_b.Add(resultTotalDistance); // 총 이동거리
        GameManager.HyperActivity_c.Add(_clayPickupManager.GetComponent<ClayPickupManager>().jumpCount); // 점프 횟수
        
        
        
        
    }
}

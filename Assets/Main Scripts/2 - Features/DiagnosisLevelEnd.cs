using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class DiagnosisLevelEnd : MonoBehaviour
{
    public enum ADHDLevel
    {
        Low,
        Middle,
        High
    }
    
    public ADHDLevel _AdhdLevel = ADHDLevel.Middle;
    
    [SerializeField] private GameObject _oven;
    [SerializeField] private GameObject _othersBelonging;
    [SerializeField] private GameObject _isHyperActivityG;
    
    private void LoadSceneBasedOnADHDLevel()
    {
        switch (_AdhdLevel)
        {
            case ADHDLevel.Low:
                SceneManager.LoadScene("3. Building House(L)");
                break;
            case ADHDLevel.Middle:
                SceneManager.LoadScene("3. Building House(M)");
                break;
            case ADHDLevel.High:
                SceneManager.LoadScene("3. Building House(H)"); 
                break;
            default:
                Debug.LogWarning("Unknown SceneState");
                break;
        }
    }
    
    public void OnButtonClicked() 
    { 
        SaveData();
        LoadSceneBasedOnADHDLevel();
    }
    
    
    private void SaveData()
    {
        float distractedTime = _oven.GetComponent<Oven>()._distractedTime;
        int result = Mathf.RoundToInt(distractedTime * 100f); // 소수점 두 자리까지 반올림 후 정수형으로 변환
        GameManager.Inattention_b.Add(result);
        GameManager.Inattention_h.Add(_oven.GetComponent<Oven>()._distractedWhenCall);
        GameManager.HyperActivity_d.Add(_oven.GetComponent<Oven>()._shovelOutCount);
        GameManager.HyperActivity_i.Add(_othersBelonging.GetComponent<OthersBelonging>().OtherBelongingTouch);
        GameManager.HyperActivity_g.Add(_isHyperActivityG.GetComponent<IsHyperActivityG>().isHyperActivity);

        foreach (var item in GameManager.Inattention_b)
        {
            Debug.Log("부주의 b_Raw Data: " + item);
        }
        foreach (var item in GameManager.Inattention_h)
        {
            Debug.Log("부주의 h_Raw Data: " + item);
        }
        foreach (var item in GameManager.HyperActivity_d)
        {
            Debug.Log("부주의 d_Raw Data: " + item);
        }
        foreach (var item in GameManager.HyperActivity_i)
        {
            Debug.Log("부주의 i_Raw Data:: " + item);
        }
    }
}

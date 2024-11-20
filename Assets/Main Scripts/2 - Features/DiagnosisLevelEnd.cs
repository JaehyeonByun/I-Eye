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
    
    private void LoadSceneBasedOnADHDLevel()
    {
        switch (_AdhdLevel)
        {
            case ADHDLevel.Low:
                SceneManager.LoadScene("3. Building House(Low)");
                break;
            case ADHDLevel.Middle:
                SceneManager.LoadScene("3. Building House(Middle)");
                break;
            case ADHDLevel.High:
                SceneManager.LoadScene("3. Building House(High)"); 
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
        GameManager.Inattention_b.Append(_oven.GetComponent<Oven>()._distractedTime);
        GameManager.Inattention_h.Append(_oven.GetComponent<Oven>()._distractedWhenCall);
        GameManager.HyperActivity_d.Append(_oven.GetComponent<Oven>()._shovelOutCount);
        GameManager.HyperActivity_i.Append(_othersBelonging.GetComponent<OthersBelonging>().OtherBelongingTouch);

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

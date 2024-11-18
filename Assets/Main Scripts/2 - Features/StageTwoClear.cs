using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class StageTwoClear : MonoBehaviour
{
    [SerializeField] private GameObject _oven;
    [SerializeField] private GameObject _othersBelonging;
    
    void Update() // Debuging
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            SaveData();
        }
    }
    
    public void SaveData()
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

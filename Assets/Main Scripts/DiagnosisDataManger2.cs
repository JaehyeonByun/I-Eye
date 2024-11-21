using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiagnosisDataManger2 : MonoBehaviour
{
   [SerializeField] private GameObject _oven;
    [SerializeField] private GameObject _othersBelonging;
    [SerializeField] private GameObject _isHyperActivityG;
    [SerializeField] private GameObject clearUI; // Reference to the clearUI object
    [SerializeField] private GameObject databaseManager;

    private bool hasSavedData = false; // Flag to ensure SaveData only runs once

    public void SaveData()
    {
        if (hasSavedData || clearUI.activeSelf == false) // If data has already been saved or clearUI is not active
            return;

        float distractedTime = _oven.GetComponent<Oven>()._distractedTime;
        int result = Mathf.RoundToInt(distractedTime * 100f);
        GameManager.Inattention_b.Add(result);
        GameManager.Inattention_h.Add(_oven.GetComponent<Oven>()._distractedWhenCall);
        GameManager.HyperActivity_d.Add(_oven.GetComponent<Oven>()._shovelOutCount);
        GameManager.HyperActivity_i.Add(_othersBelonging.GetComponent<OthersBelonging>().OtherBelongingTouch);
        GameManager.HyperActivity_g.Add(_isHyperActivityG.GetComponent<IsHyperActivityG>().isHyperActivity);

        hasSavedData = true; // Mark that data has been saved
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.O))
        {
            DebugSaveData();
        }

        // Save data when clearUI is active and only once
        if (clearUI.activeSelf && !hasSavedData)
        {
            StartCoroutine(databaseManager.GetComponent<DatabaseManager>().PostData());
            SaveData();
        }
    }

    private void DebugSaveData()
    {
        Debug.Log("_____________________________________________");
        float distractedTime = _oven.GetComponent<Oven>()._distractedTime;
        int result = Mathf.RoundToInt(distractedTime * 100f);
        Debug.Log($"Distracted Time (Rounded): {result}");
        
        int distractedWhenCall = _oven.GetComponent<Oven>()._distractedWhenCall;
        Debug.Log($"Distracted When Call: {distractedWhenCall}");
        
        int shovelOutCount = _oven.GetComponent<Oven>()._shovelOutCount;
        Debug.Log($"Shovel Out Count: {shovelOutCount}");
        
        int otherBelongingTouch = _othersBelonging.GetComponent<OthersBelonging>().OtherBelongingTouch;
        Debug.Log($"Other Belonging Touch: {otherBelongingTouch}");
 
        int isHyperActivity = _isHyperActivityG.GetComponent<IsHyperActivityG>().isHyperActivity;
        Debug.Log($"Is Hyper Activity: {isHyperActivity}");
        Debug.Log("_____________________________________________");
    }
}

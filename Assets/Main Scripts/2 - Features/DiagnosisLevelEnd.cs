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

        LoadSceneBasedOnADHDLevel();
    }
    
}

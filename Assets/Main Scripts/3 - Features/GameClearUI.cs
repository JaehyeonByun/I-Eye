using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameClearUI : MonoBehaviour
{
    public void onbuttonClick()
    {
        LoadScene("Clear Scene");
    }
    
    public void LoadScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }
}

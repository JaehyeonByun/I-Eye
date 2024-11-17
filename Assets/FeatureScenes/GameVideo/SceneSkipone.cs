using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSkipone : MonoBehaviour
{
    void Start()
    {
        // 5초 후에 ChangeScene 함수 호출
        Invoke("ChangeScene", 20f);
    }

    void ChangeScene()
    {
        SceneManager.LoadScene("Video 1");
    }
}

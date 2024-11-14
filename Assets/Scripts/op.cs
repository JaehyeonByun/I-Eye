using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class op : MonoBehaviour
{
    void Start()
    {
        // 5초 후에 ChangeScene 함수 호출
        Invoke("ChangeScene", 5f);
    }

    void ChangeScene()
    {
        // "Video 1" 씬으로 전환
        SceneManager.LoadScene("Video 1");
    }
}

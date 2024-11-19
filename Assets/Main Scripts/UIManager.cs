using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private float timer = 0f;
    private string[] scenes = { "Video", "Video 1", "Video 2", "Video 3" };
    private int currentSceneIndex = 0;

    void Start()
    {
        SceneManager.LoadScene(scenes[currentSceneIndex]);
    }

    void Update()
    {
        timer += Time.deltaTime;

        if (timer >= 20f)
        {
            timer = 0f; // 타이머 초기화
            SwitchScene();
        }
    }

    void SwitchScene()
    {
        currentSceneIndex = (currentSceneIndex + 1) % scenes.Length;
        SceneManager.LoadScene(scenes[currentSceneIndex]);
    }
}

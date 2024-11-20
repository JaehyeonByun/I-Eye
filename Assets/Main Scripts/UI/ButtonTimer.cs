using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ButtonTimer : MonoBehaviour
{
    public Button firstButton;  
    public Button secondButton; 
    public int variable = 0;    
    private float timer = 0f;   
    private bool isTimerActive = false; 

    void Start()
    {
     
        firstButton.onClick.AddListener(OnFirstButtonClick);
        secondButton.onClick.AddListener(OnSecondButtonClick);
    }

    void Update()
    {
        if (isTimerActive)
        {
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                isTimerActive = false;
            }
        }
    }

    void OnFirstButtonClick()
    {
        timer = 5f;
        isTimerActive = true;
    }

    void OnSecondButtonClick()
    {
        if (isTimerActive)
        {
            variable++;
            Debug.Log("변수가 추가되었습니다. 현재 값: " + variable);
            isTimerActive = false;
        }
        else
        {
            Debug.Log("타이머가 만료되었습니다. 변수가 추가되지 않습니다.");
        }
    }
}

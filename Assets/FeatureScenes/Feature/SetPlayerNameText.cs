using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 추가

public class SetPlayerNameText : MonoBehaviour
{
    public TextMeshProUGUI targetText; 
    public string slideTextTemplate = "{0}의 도움이 필요해\n집 짓기를 도와줄래?!"; 

    private void Start()
    {
        // TextMeshPro 객체가 설정되지 않은 경우
        if (targetText == null)
        {
            Debug.LogError("[SetPlayerNameText] TextMeshProUGUI component is not assigned!");
            return;
        }

        // 텍스트 설정
        targetText.text = string.Format(slideTextTemplate, GlobalVariables.PlayerName);
        Debug.Log($"[SetPlayerNameText] Text set to: {targetText.text}");
    }

    // 외부에서 텍스트를 갱신할 때 호출
    public void UpdatePlayerNameText()
    {
        if (targetText != null)
        {
            targetText.text = string.Format(slideTextTemplate, GlobalVariables.PlayerName);
            Debug.Log($"[SetPlayerNameText] Updated text to: {targetText.text}");
        }
    }
}
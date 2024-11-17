using UnityEngine;
using UnityEngine.SceneManagement; // 씬 전환에 필요
using UnityEngine.UI; // UI 버튼에 필요

public class ButtonActions : MonoBehaviour
{
    public UIScenario uiScenario;
    public Button yesButton; // YES 버튼
    public Button noButton; // NO 버튼

    public string nextSceneName = "NextScene"; // 전환할 씬 이름

    private void Start()
    {
        // 버튼 클릭 이벤트 등록
        yesButton.onClick.AddListener(OnYesButtonClicked);
        noButton.onClick.AddListener(OnNoButtonClicked);
    }

    private void OnYesButtonClicked()
    {
        // YES 버튼을 누르면 씬 전환
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            SceneManager.LoadScene(nextSceneName);
            Debug.Log("Scene changed to: " + nextSceneName);
        }
        else
        {
            Debug.LogError("Next scene name is not set.");
        }
    }

    private void OnNoButtonClicked()
    {
        // NO 버튼을 누르면 Slide 3으로 돌아가고 녹음을 다시 시작
        if (uiScenario != null)
        {
            Debug.Log("Returning to Slide 3 and restarting recording...");

            // UIScenario에서 Slide 3으로 이동하고 다시 시작
            uiScenario.ReturnToSlide(3);
        }
        else
        {
            Debug.LogError("[ButtonActions] UIScenario is not assigned.");
        }
    }
}
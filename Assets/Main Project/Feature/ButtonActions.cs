using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonActions : MonoBehaviour
{
    public UIScenario uiScenario; // UIScenario 스크립트 참조
    public string nextSceneName = "NextScene"; // 전환할 씬 이름

    // Unity OnClick() 이벤트에 연결할 메서드
    public void OnNextSceneButtonClicked()
    {
        // NextScene 버튼을 누르면 씬 전환
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

    // Unity OnClick() 이벤트에 연결할 메서드
    public void OnNoButtonClicked()
    {
        // NO 버튼을 누르면 Slide 3으로 돌아가고 녹음을 다시 시작
        if (uiScenario != null)
        {
            Debug.Log("Returning to Slide 3 and restarting recording...");

            // UIScenario에서 Slide 3으로 이동하고 다시 시작
            uiScenario.ReturnToSlide(2);
        }
        else
        {
            Debug.LogError("[ButtonActions] UIScenario is not assigned.");
        }
    }
}
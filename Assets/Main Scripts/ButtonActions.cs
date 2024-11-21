using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonActions : MonoBehaviour
{
    public UIScenario uiScenario; // UIScenario 스크립트 참조
    public string nextSceneName = "NextScene"; // 전환할 씬 이름
    public ClayPickupManager clayPickupManager;

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
            Debug.Log("Returning to Slide 3");

            // ClayPickupManager가 null인지 확인하고 안전하게 증가
            if (clayPickupManager != null)
            {
                clayPickupManager.SayAgainCount++;
                Debug.Log("returnToexplanation number " + clayPickupManager.SayAgainCount);
            }
            else
            {
                Debug.LogWarning("[ButtonActions] ClayPickupManager is null. Skipping returnToExplanation increment.");
            }

            // UIScenario에서 Slide 3으로 이동하고 다시 시작
            uiScenario.ReturnToSlide(2);
        }
        else
        {
            Debug.LogError("[ButtonActions] UIScenario is not assigned.");
        }
    }

    public void onWrongNumberButtonClicked()
    {
        // ClayPickupManager가 null인지 확인하고 안전하게 호출
        if (clayPickupManager != null)
        {
            clayPickupManager.WrongButtonClicked++;
            Debug.Log("onWrongNumberButtonClicked number " + clayPickupManager.WrongButtonClicked);
            clayPickupManager.PlayWrongPickupSound();
        }
        else
        {
            Debug.LogWarning("[ButtonActions] ClayPickupManager is null. Skipping onWrongNumberButtonClicked actions.");
        }
    }

    public void onRightNumberButtonClicked()
    {
        // ClayPickupManager가 null인지 확인하고 안전하게 호출
        if (clayPickupManager != null)
        {
            clayPickupManager.PlayCorrectPickupSound();
            clayPickupManager.ShowClearUI();
        }
        else
        {
            Debug.LogWarning("[ButtonActions] ClayPickupManager is null. Skipping onRightNumberButtonClicked actions.");
        }
    }
}

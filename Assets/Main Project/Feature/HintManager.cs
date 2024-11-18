using UnityEngine;
using UnityEngine.UI;

public class HintManager : MonoBehaviour
{
    [SerializeField] private GameObject hintUI; // 힌트 UI 오브젝트
    [SerializeField] private ClayPickupManager clayPickupManager; // ClayPickupManager 참조

    /// <summary>
    /// 힌트 버튼 클릭 시 동작.
    /// </summary>
    public void OnHintButtonClicked()
    {
        ShowHintUI();

        // ClayPickupManager의 UseHint 메서드 호출
        if (clayPickupManager != null)
        {
            clayPickupManager.UseHint();
        }
        else
        {
            Debug.LogWarning("ClayPickupManager reference is not set!");
        }
    }

    /// <summary>
    /// 힌트 UI를 표시합니다.
    /// </summary>
    public void ShowHintUI()
    {
        if (hintUI != null)
        {
            hintUI.SetActive(true);
            Debug.Log("Hint UI displayed.");
        }
    }

    /// <summary>
    /// 힌트 UI를 숨깁니다.
    /// </summary>
    public void HideHintUI()
    {
        if (hintUI != null)
        {
            hintUI.SetActive(false);
            Debug.Log("Hint UI hidden.");
        }
    }
}
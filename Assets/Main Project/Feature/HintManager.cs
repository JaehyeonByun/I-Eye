using UnityEngine;
using TMPro;

public class HintManager : MonoBehaviour
{
    [SerializeField] private GameObject hintUI; // 힌트 UI 오브젝트
    [SerializeField] private ClayPickupManager clayPickupManager; // ClayPickupManager 참조
    [SerializeField] private TextMeshProUGUI hintText; // 힌트 텍스트를 표시할 TextMeshProUGUI

    /// <summary>
    /// 힌트 버튼 클릭 시 동작.
    /// </summary>
    public void OnHintButtonClicked()
    {
        if (clayPickupManager != null)
        {
            // ClayPickupManager의 UseHint 메서드 호출
            clayPickupManager.UseHint();

            // 힌트 UI를 업데이트하고 표시
            UpdateHintUI();
            ShowHintUI();
        }
        else
        {
            Debug.LogWarning("ClayPickupManager reference is not set!");
        }
    }

    /// <summary>
    /// 힌트 UI를 업데이트합니다.
    /// </summary>
    private void UpdateHintUI()
    {
        if (clayPickupManager != null && hintText != null)
        {
            // 현재 주워야 할 색상 가져오기
            string nextColor = clayPickupManager.GetNextClayColor();
            

            // 힌트 텍스트 업데이트
            hintText.text = $"힌트를 줄게,\n 다음은 {nextColor} 진흙을 주울 차례야!";
        }
        else
        {
            Debug.LogWarning("Hint UI or ClayPickupManager reference is missing!");
        }
    }

    /// <summary>
    /// 힌트 UI를 표시합니다.
    /// </summary>
    public void ShowHintUI()
    {
        if (hintUI != null)
        {
            UpdateHintUI();
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
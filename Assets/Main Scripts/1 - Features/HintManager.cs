using UnityEngine;
using System.Collections;
using TMPro;

public class HintManager : MonoBehaviour
{
    [SerializeField] private GameObject hintUI; // 힌트 UI 오브젝트
    [SerializeField] private ClayPickupManager clayPickupManager; // ClayPickupManager 참조
    [SerializeField] private TextMeshProUGUI hintText; // 힌트 텍스트를 표시할 TextMeshProUGUI

    private Coroutine fadeCoroutine; // 페이드 코루틴 저장

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
            Debug.LogWarning("Use Hint!");
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
    /// 힌트 UI를 페이드인하며 표시합니다.
    /// </summary>
    public void ShowHintUI()
    {
        if (hintUI != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            UpdateHintUI();
            hintUI.SetActive(true);
            fadeCoroutine = StartCoroutine(FadeCanvasGroup(hintUI.GetComponent<CanvasGroup>(), 0, 1, 0.5f));
        }
    }

    /// <summary>
    /// 힌트 UI를 페이드아웃하며 숨깁니다.
    /// </summary>
    public void HideHintUI()
    {
        if (hintUI != null)
        {
            if (fadeCoroutine != null)
            {
                StopCoroutine(fadeCoroutine);
            }

            fadeCoroutine = StartCoroutine(FadeCanvasGroup(hintUI.GetComponent<CanvasGroup>(), 1, 0, 0.5f, () => hintUI.SetActive(false)));
        }
    }

    /// <summary>
    /// CanvasGroup의 alpha 값을 변경하여 페이드 효과를 구현합니다.
    /// </summary>
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        if (canvasGroup == null)
        {
            canvasGroup = hintUI.AddComponent<CanvasGroup>(); // CanvasGroup 동적 추가
        }

        float elapsedTime = 0f;
        canvasGroup.alpha = startAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;

        // 페이드 완료 후 동작 실행 (예: UI 비활성화)
        onComplete?.Invoke();
    }
}

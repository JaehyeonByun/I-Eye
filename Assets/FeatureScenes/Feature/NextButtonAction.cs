using UnityEngine;
using UnityEngine.UI;

public class NextButtonAction : MonoBehaviour
{
    public UIScenario uiScenario; // UIScenario 참조
    public Button actionButton; // 현재 슬라이드의 버튼

    private void Start()
    {
        if (actionButton != null)
        {
            actionButton.onClick.AddListener(OnActionButtonClicked);
        }
        else
        {
            Debug.LogError("[NextButtonAction] Action button is not assigned.");
        }
    }

    private void OnActionButtonClicked()
    {
        if (uiScenario != null)
        {
            Debug.Log("[NextButtonAction] Button clicked. Attempting to transition to the next slide...");
            uiScenario.TriggerFadeToNextSlide();
        }
        else
        {
            Debug.LogError("[NextButtonAction] UIScenario reference is not assigned.");
        }
    }
}
using UnityEngine;

public class NextButtonAction : MonoBehaviour
{
    public UIScenario uiScenario; // UIScenario 참조

    // Unity의 OnClick 이벤트에서 호출될 메서드
    public void OnActionButtonClicked()
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
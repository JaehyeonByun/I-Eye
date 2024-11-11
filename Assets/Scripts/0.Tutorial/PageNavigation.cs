#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;

public class PageNavigation : MonoBehaviour
{
    [SerializeField]
    private GameObject nextPage; // 넘어갈 페이지의 UI GameObject

    [SerializeField]
    private InputActionReference TriggerAction;

    private void Start()
    {
        TriggerAction.action.performed += OnTriggerAction;
    }

    private void OnDestroy()
    {
        TriggerAction.action.performed -= OnTriggerAction;
    }

#if UNITY_EDITOR
    public void OnEditorButtonClick()
    {
        GoToNextPage();
    }
#endif

    private void OnTriggerAction(InputAction.CallbackContext context)
    {
        GoToNextPage();
    }

    private void GoToNextPage()
    {
        // TriggerAction 이벤트에서 OnTriggerAction 해제
        TriggerAction.action.performed -= OnTriggerAction;
        
        gameObject.SetActive(false);
        if (nextPage != null)
        {
            nextPage.SetActive(true);

            // AutoSlideShow를 찾아서 자동 슬라이드 재개
            AutoSlideShow autoSlideShow = nextPage.GetComponent<AutoSlideShow>();
            if (autoSlideShow != null)
            {
                int currentSlideIndex = autoSlideShow.GetCurrentSlideIndex();
                int targetIndex = currentSlideIndex + 1;

                // targetIndex가 슬라이드 범위 내인지 확인
                if (targetIndex >= 0 && targetIndex < autoSlideShow.slides.Count && targetIndex <= 13)
                {
                    GameObject targetSlide = autoSlideShow.slides[targetIndex];

                    // 슬라이드가 비활성화된 경우 강제로 활성화
                    if (!targetSlide.activeSelf)
                    {
                        targetSlide.SetActive(true);
                    }

                    // 페이드인 효과 적용
                    autoSlideShow.StartCoroutine(autoSlideShow.FadeIn(targetSlide));

                    // 슬라이드를 보여주는 로직 호출
                    autoSlideShow.SetCurrentSlideIndex(targetIndex);
                    autoSlideShow.ShowSlide(targetIndex);
                    autoSlideShow.ResumeAutoSlide();
                }
                else
                {
                    Debug.LogWarning("Target slide index is out of range.");
                }
            }
            else
            {
                Debug.LogWarning("AutoSlideShow component not found on next page.");
            }
        }
    }
}

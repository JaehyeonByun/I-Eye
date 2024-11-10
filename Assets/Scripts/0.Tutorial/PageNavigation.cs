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
                Debug.Log("Attempting to resume AutoSlide from PageNavigation");
                
                autoSlideShow.SetCurrentSlideIndex(targetIndex); // 강제로 슬라이드 9로 설정
                autoSlideShow.ShowSlide(targetIndex);
                autoSlideShow.ResumeAutoSlide(); 
            }
            else
            {
                Debug.LogWarning("AutoSlideShow component not found on next page.");
            }
        }
    }
}

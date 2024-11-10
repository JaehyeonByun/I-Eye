using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCanvas; // 튜토리얼 UI 캔버스
    [SerializeField] private GameObject dialogueCanvas;

    private void Start()
    {
        // tutorialCanvas는 처음에 비활성화
        tutorialCanvas.SetActive(false);

        // AutoSlideShow의 OnTutorialMode 이벤트 구독
        AutoSlideShow slideShow = FindObjectOfType<AutoSlideShow>();
        if (slideShow != null)
        {
            slideShow.OnTutorialMode += ActivateTutorialCanvas;
        }
    }

    private void ActivateTutorialCanvas()
    {
        Debug.Log("Activating Tutorial Canvas...");
        tutorialCanvas.SetActive(true); // 튜토리얼 캔버스를 활성화
        dialogueCanvas.SetActive(false);
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        AutoSlideShow slideShow = FindObjectOfType<AutoSlideShow>();
        if (slideShow != null)
        {
            slideShow.OnTutorialMode -= ActivateTutorialCanvas;
        }
    }
}
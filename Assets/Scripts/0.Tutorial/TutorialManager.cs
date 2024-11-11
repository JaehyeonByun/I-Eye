using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCanvas; // 튜토리얼 UI 캔버스
    [SerializeField] private GameObject dialogueCanvas;
    [SerializeField] private GameObject clayPickupManagerObject; // ClayPickupManager 오브젝트
    [SerializeField] private GameObject pageNavigationObject; // PageNavigation 오브젝트

    private void Start()
    {
        // tutorialCanvas와 clayPickupManagerObject, pageNavigationObject는 처음에 비활성화
        tutorialCanvas.SetActive(false);
        clayPickupManagerObject.SetActive(false);

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
        
        // ClayPickupManager 오브젝트 활성화
        clayPickupManagerObject.SetActive(true);

        // PageNavigation 오브젝트 종료 (비활성화)
        if (pageNavigationObject != null)
        {
            pageNavigationObject.SetActive(false);
            Debug.Log("PageNavigation has been disabled by TutorialManager.");
        }
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
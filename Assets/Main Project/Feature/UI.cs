using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement; 

[System.Serializable]
public class UIPanel
{
    public GameObject panelObject; // 패널 GameObject
    public float interval = 3f; // 자동 전환 시간 (초)
    public AudioSource audioSource; // 패널 별 오디오 소스
    public int motionIndex; // 애니메이션 컨트롤러의 scenario 값
    public bool requiresButtonPress = false; // 버튼 클릭 필요 여부
}
public class UI : MonoBehaviour
{
      public List<UIPanel> panels; // 패널 리스트
    public Button nextButton; // "다음" 버튼
    public Button backButton; // "되돌아가기" 버튼
    public float fadeDuration = 1f; // 페이드 시간
    public Animator animator; // 애니메이션 컨트롤러
    public string tutorialSceneName = "1.Tutorial"; // 이동할 씬 이름

    private int currentPanelIndex = 0;
    private bool isTransitioning = false;

    private void Start()
    {
        foreach (var panel in panels)
        {
            SetPanelAlpha(panel.panelObject, 0); // 모든 패널을 투명하게 설정
            panel.panelObject.SetActive(false);
        }

        if (panels.Count > 0)
        {
            ShowPanel(0);
        }

        nextButton.onClick.AddListener(NextButtonClicked);
        backButton.onClick.AddListener(PreviousPanel);
    }

    private void ShowPanel(int index)
    {
        if (index < 0 || index >= panels.Count) return;

        UIPanel panel = panels[index];
        panel.panelObject.SetActive(true);

        // 애니메이션 컨트롤러의 scenario 값 설정
        if (animator != null)
        {
            animator.SetInteger("scenario", panel.motionIndex);
        }

        // 패널 보이스 재생
        if (panel.audioSource != null)
        {
            panel.audioSource.Play();
        }

        // 페이드 인
        StartCoroutine(FadeIn(panel.panelObject));

        // 자동 전환
        if (!panel.requiresButtonPress)
        {
            StartCoroutine(AutoTransition(panel.interval));
        }
    }

    private void HidePanel(int index)
    {
        if (index < 0 || index >= panels.Count) return;

        UIPanel panel = panels[index];
        StartCoroutine(FadeOut(panel.panelObject, () =>
        {
            panel.panelObject.SetActive(false);

            // 오디오 중지
            if (panel.audioSource != null)
            {
                panel.audioSource.Stop();
            }
        }));
    }

    private IEnumerator AutoTransition(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        NextButtonClicked();
    }

    public void NextButtonClicked()
    {
        // 마지막 패널에서 버튼 클릭 시 씬 변경
        if (currentPanelIndex == panels.Count - 1)
        {
            SceneManager.LoadScene(tutorialSceneName);
        }
        else
        {
            NextPanel();
        }
    }

    public void NextPanel()
    {
        if (isTransitioning) return;

        if (currentPanelIndex < panels.Count - 1)
        {
            StartCoroutine(TransitionToPanel(currentPanelIndex + 1));
        }
    }

    public void PreviousPanel()
    {
        if (isTransitioning) return;

        if (currentPanelIndex > 0)
        {
            StartCoroutine(TransitionToPanel(currentPanelIndex - 1));
        }
    }

    private IEnumerator TransitionToPanel(int targetIndex)
    {
        isTransitioning = true;

        // 현재 패널 숨기기
        HidePanel(currentPanelIndex);

        // 페이드 효과가 끝날 때까지 대기
        yield return new WaitForSeconds(fadeDuration);

        // 다음 패널 표시
        currentPanelIndex = targetIndex;
        ShowPanel(currentPanelIndex);

        isTransitioning = false;
    }

    private IEnumerator FadeIn(GameObject panel)
    {
        var images = panel.GetComponentsInChildren<Image>();
        var texts = panel.GetComponentsInChildren<Text>();

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(0, 1, elapsedTime / fadeDuration);

            SetAlpha(images, texts, alpha);
            yield return null;
        }

        SetAlpha(images, texts, 1f); // 완전히 표시
    }

    private IEnumerator FadeOut(GameObject panel, System.Action onComplete)
    {
        var images = panel.GetComponentsInChildren<Image>();
        var texts = panel.GetComponentsInChildren<Text>();

        float elapsedTime = 0f;
        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.deltaTime;
            float alpha = Mathf.Lerp(1, 0, elapsedTime / fadeDuration);

            SetAlpha(images, texts, alpha);
            yield return null;
        }

        SetAlpha(images, texts, 0f); // 완전히 숨기기
        onComplete?.Invoke();
    }

    private void SetAlpha(Image[] images, Text[] texts, float alpha)
    {
        foreach (var img in images)
        {
            Color color = img.color;
            color.a = alpha;
            img.color = color;
        }

        foreach (var txt in texts)
        {
            Color color = txt.color;
            color.a = alpha;
            txt.color = color;
        }
    }

    private void SetPanelAlpha(GameObject panel, float alpha)
    {
        var images = panel.GetComponentsInChildren<Image>();
        var texts = panel.GetComponentsInChildren<Text>();

        SetAlpha(images, texts, alpha);
    }
}

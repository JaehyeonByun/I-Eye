using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlideData
{
    public float interval = 2f;        // 간격
    public bool isStop;                // 멈춰야 하는지
    public GameObject slide;           // 슬라이드 오브젝트
    public AudioSource Voice;          // 음성 배열
    public int AnimatorCount;
}

public class UIScenario : MonoBehaviour
{
 [SerializeField]
    private List<SlideData> _slides;

    [SerializeField]
    private float fadeDuration = 1f; // 페이드 인/아웃 지속 시간

    [SerializeField]
    private Animator animator; 

    private int currentSlideIndex = 0;
    private bool isPaused = false;
    private Coroutine autoSlideCoroutine;

    private void Start()
    {
        ShowSlide(currentSlideIndex); // 첫 슬라이드 활성화
        autoSlideCoroutine = StartCoroutine(AutoNextSlide()); // 자동 슬라이드 시작
    }

    private IEnumerator AutoNextSlide()
    {
        while (currentSlideIndex < _slides.Count - 1)
        {
            Debug.Log("AutoNextSlide - Current Slide: " + currentSlideIndex);

            // 특정 인덱스에서 자동 슬라이드 중단
            if ((_slides[currentSlideIndex].isStop == true) && !isPaused)
            {
                isPaused = true;
                Debug.Log("Paused at Slide " + (currentSlideIndex + 1) + ", waiting for manual navigation.");
                yield break; // 코루틴 종료
            }

            // 현재 슬라이드의 interval 값만큼 대기
            yield return new WaitForSeconds(_slides[currentSlideIndex].interval);

            // 페이드 아웃 및 다음 슬라이드로 전환
            yield return StartCoroutine(FadeToNextSlide());
        }
    }

    private IEnumerator FadeToNextSlide()
    {
        yield return StartCoroutine(FadeOutSlide(currentSlideIndex));
        ShowNextSlide();
        yield return StartCoroutine(FadeInSlide(currentSlideIndex));
    }

    private IEnumerator FadeOutSlide(int index)
    {
        CanvasGroup canvasGroup = _slides[index].slide.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            float startAlpha = canvasGroup.alpha;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, 0, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 0; // 확실히 투명하게 설정
        }
        _slides[index].slide.SetActive(false);// 페이드 아웃 후 비활성화
    }

    private IEnumerator FadeInSlide(int index)
    {
        _slides[index].slide.SetActive(true); 
        CanvasGroup canvasGroup = _slides[index].slide.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            float startAlpha = canvasGroup.alpha;
            for (float t = 0; t < fadeDuration; t += Time.deltaTime)
            {
                canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeDuration);
                yield return null;
            }
            canvasGroup.alpha = 1; 
        }
        _slides[index].Voice.Play();
    }

    public void ResumeAutoSlide()
    {
        if (isPaused)
        {
            isPaused = false;
            Debug.Log("Resuming AutoSlide from Slide " + (currentSlideIndex + 1));
            autoSlideCoroutine = StartCoroutine(AutoNextSlide());
        }
        else
        {
            Debug.LogWarning("Attempted to resume AutoSlide while it was not paused. Current slide index: " + currentSlideIndex);
        }
    }

    public int GetCurrentSlideIndex()
    {
        return currentSlideIndex;
    }

    public void SetCurrentSlideIndex(int index)
    {
        currentSlideIndex = index;
        Debug.Log("Manually set slide index to: " + currentSlideIndex);
        ShowSlide(currentSlideIndex);
    }

    public void ShowNextSlide()
    {
        currentSlideIndex++;
        ShowSlide(currentSlideIndex); 
    }

    public void ShowSlide(int index)
    {
        foreach (var slideData in _slides)
        {
            slideData.slide.SetActive(false);
        }

        if (index >= 0 && index < _slides.Count)
        {
            _slides[index].slide.SetActive(true);
            _slides[index].Voice.Play();
            animator.SetInteger("Scenario", _slides[index].AnimatorCount); 
            Debug.Log("Showing Slide: " + index);
        }
        else
        {
            Debug.LogWarning("Slide index out of range: " + index);
        }
    }
}

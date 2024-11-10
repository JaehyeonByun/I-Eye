using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoSlideShow : MonoBehaviour
{
    [SerializeField, Range(1, 10)]
    private float interval = 5f; // 기본 간격을 5초로 설정

    [SerializeField]
    private List<GameObject> slides; // 슬라이드들을 인스펙터에 추가

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
        while (currentSlideIndex < slides.Count - 1)
        {
            Debug.Log("AutoNextSlide - Current Slide: " + currentSlideIndex);

            // 특정 인덱스에서 자동 슬라이드 중단
            if ((currentSlideIndex == 7 || currentSlideIndex == 9 || currentSlideIndex == 10 || currentSlideIndex == 11) && !isPaused)
            {
                isPaused = true;
                Debug.Log("Paused at Slide " + (currentSlideIndex + 1) + ", waiting for manual navigation.");
                yield break; // 코루틴 종료
            }

            yield return new WaitForSeconds(interval);
            ShowNextSlide();
        }
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
        ShowSlide(currentSlideIndex); // 인덱스 변경 후 슬라이드 표시
    }

    public void ShowNextSlide()
    {
        slides[currentSlideIndex].SetActive(false);
        currentSlideIndex++;
        ShowSlide(currentSlideIndex);
    }

    public void ShowSlide(int index)
    {
        // 모든 슬라이드 비활성화 후 현재 인덱스 슬라이드만 활성화
        foreach (var slide in slides)
        {
            slide.SetActive(false);
        }

        if (index >= 0 && index < slides.Count)
        {
            slides[index].SetActive(true);
            Debug.Log("Showing Slide: " + index);
        }
        else
        {
            Debug.LogWarning("Slide index out of range: " + index);
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlideData
{
    public float interval = 2f;        // 간격
    public bool isStop;           // 멈춰야 하는지
    public GameObject slide;      // 슬라이드 오브젝트
    public AudioSource Voice;  // 음성 배열
}
public class UIScenario : MonoBehaviour
{
    [SerializeField]
    private List<SlideData> _slides; 
    
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
        _slides[currentSlideIndex].slide.SetActive(false); // 현재 슬라이드 비활성화
        currentSlideIndex++;
        ShowSlide(currentSlideIndex); // 다음 슬라이드 활성화
    }

    public void ShowSlide(int index)
    {
        // 모든 슬라이드 비활성화 후 현재 인덱스 슬라이드만 활성화
        foreach (var slideData in _slides)
        {
            slideData.slide.SetActive(false);
        }

        if (index >= 0 && index < _slides.Count)
        {
            _slides[index].slide.SetActive(true);
            _slides[index].Voice.Play();
            Debug.Log("Showing Slide: " + index);
        }
        else
        {
            Debug.LogWarning("Slide index out of range: " + index);
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class AutoSlideShow : MonoBehaviour
{
    public event Action OnTutorialMode;

    [SerializeField, Range(1, 10)]
    private float interval = 5f; // 슬라이드 전환 간격

    [SerializeField]
    public List<GameObject> slides; // 슬라이드 리스트

    [SerializeField, Range(0.1f, 2f)]
    private float fadeDuration = 0.5f; // 페이드 인/아웃 시간

    private int currentSlideIndex = 0;
    private bool isPaused = false;
    private Coroutine autoSlideCoroutine;

    private void Start()
    {
        InitializeSlides();
        ShowSlide(currentSlideIndex); 
        autoSlideCoroutine = StartCoroutine(AutoNextSlide()); 
    }

    private void InitializeSlides()
    {
        foreach (var slide in slides)
        {
            // 각 슬라이드의 SpriteRenderer를 통해 시작 시 알파 값 설정
            SpriteRenderer spriteRenderer = slide.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0;  // 시작 시 투명하게 설정
                spriteRenderer.color = color;
            }
            slide.SetActive(false); // 시작 시 비활성화
        }
    }

    private IEnumerator AutoNextSlide()
    {
        while (currentSlideIndex < slides.Count - 1 && currentSlideIndex < 13) // 최대 인덱스를 13으로 제한
        {
            if ((currentSlideIndex == 7 || currentSlideIndex == 9 || currentSlideIndex == 10 || currentSlideIndex == 11) && !isPaused)
            {
                isPaused = true;
                yield break;
            }

            yield return new WaitForSeconds(interval);
            yield return StartCoroutine(FadeToNextSlide());
        }

        if (currentSlideIndex >= 13) // 슬라이드가 13까지 완료되면 튜토리얼 모드 전환
        {
            OnTutorialMode?.Invoke();
        }
    }

    private IEnumerator FadeToNextSlide()
    {
        // 현재 슬라이드 페이드 아웃
        yield return StartCoroutine(FadeOut(slides[currentSlideIndex]));

        currentSlideIndex++;

        // 인덱스를 13 이하로 유지
        if (currentSlideIndex <= 13)
        {
            ShowSlide(currentSlideIndex);
            yield return StartCoroutine(FadeIn(slides[currentSlideIndex]));
        }
    }

    public IEnumerator FadeOut(GameObject slide)
    {
        SpriteRenderer spriteRenderer = slide.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;

        Color color = spriteRenderer.color;
        float startAlpha = color.a;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 0, time / fadeDuration);
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = 0;
        spriteRenderer.color = color;
        slide.SetActive(false); // 페이드 아웃 후 슬라이드를 비활성화
    }

    public IEnumerator FadeIn(GameObject slide)
    {
        slide.SetActive(true);
        SpriteRenderer spriteRenderer = slide.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) yield break;

        Color color = spriteRenderer.color;
        float startAlpha = color.a;
        float time = 0;

        while (time < fadeDuration)
        {
            time += Time.deltaTime;
            color.a = Mathf.Lerp(startAlpha, 1, time / fadeDuration);
            spriteRenderer.color = color;
            yield return null;
        }

        color.a = 1;
        spriteRenderer.color = color;
    }

    public void ResumeAutoSlide()
    {
        if (isPaused)
        {
            isPaused = false;
            autoSlideCoroutine = StartCoroutine(AutoNextSlide());
        }
    }

    public int GetCurrentSlideIndex()
    {
        return currentSlideIndex;
    }

    public void SetCurrentSlideIndex(int index)
    {
        currentSlideIndex = index;
        ShowSlide(currentSlideIndex);
    }

    public void ShowSlide(int index)
    {
        foreach (var slide in slides)
        {
            slide.SetActive(false);
            SpriteRenderer spriteRenderer = slide.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 0;
                spriteRenderer.color = color;
            }
        }

        if (index >= 0 && index < slides.Count)
        {
            slides[index].SetActive(true);
            SpriteRenderer spriteRenderer = slides[index].GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                Color color = spriteRenderer.color;
                color.a = 1; // 처음 나타날 때 바로 보이도록 설정
                spriteRenderer.color = color;
            }
            Debug.Log("Showing Slide: " + index);
        }
        else
        {
            Debug.LogWarning("Slide index out of range: " + index);
        }
    }
}

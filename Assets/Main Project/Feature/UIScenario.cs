using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlideData
{
    public float interval = 2f;        // 간격
    public bool isStop;                // 멈춰야 하는지\
    public bool haveToRecording;
    public GameObject slide;           // 슬라이드 오브젝트
    public AudioSource Voice;          // 음성 배열
    public int AnimatorIndex;
    public bool triggersTutorial;
}

public class UIScenario : MonoBehaviour
{
 [SerializeField]
    private List<SlideData> _slides;

    [SerializeField]
    private float fadeDuration = 1f; // 페이드 인/아웃 지속 시간

    [SerializeField]
    private Animator animator; // Animator reference
    
    [SerializeField]
    private TutorialManager tutorialManager;

    private int currentSlideIndex = 0;
    public bool isPaused = false;
    private Coroutine autoSlideCoroutine;
    
    public MicrophoneRecorder microphoneRecorder;

    private void Start()
    {
        ShowSlide(currentSlideIndex); // 첫 슬라이드 활성화
        autoSlideCoroutine = StartCoroutine(AutoNextSlide()); // 자동 슬라이드 시작
    }

    private IEnumerator AutoNextSlide()
    {
        while (currentSlideIndex < _slides.Count)
        {
            Debug.Log("AutoNextSlide - Current Slide: " + currentSlideIndex);

            // 특정 슬라이드에서 녹음 시작 및 중단 조건 처리
            if (_slides[currentSlideIndex].isStop && !isPaused &&  _slides[currentSlideIndex].haveToRecording)
            {
                isPaused = true;
                Debug.Log($"Paused at Slide {currentSlideIndex + 1}, waiting for voice recognition.");

                if (currentSlideIndex == 3) // 예: 4번째 슬라이드에서 녹음 시작
                {
                    Debug.Log("[UIScenario] Starting recording for Slide 3...");
                    microphoneRecorder.StartRecording();

                    // 일정 시간 뒤 자동으로 StopRecording 호출
                    StartCoroutine(StopRecordingAfterDelay(5f)); // 5초 뒤 녹음 중단

                    // 음성 인식 완료 후 처리
                    StartCoroutine(WaitForVoiceRecognition());
                }
                
                // 현재 슬라이드 상태를 유지하며 루프를 대기
                while (isPaused)
                {
                    yield return null; // 음성 인식 완료를 기다림
                }
            }
            
            else if (_slides[currentSlideIndex].isStop)
            {
                isPaused = true;

                while (isPaused)
                {
                    yield return null; // 음성 인식 완료를 기다림
                }
            }
            
            else if (_slides[currentSlideIndex].triggersTutorial && tutorialManager != null)
            {
                Debug.Log("Triggering Tutorial Mode from AutoNextSlide...");
                tutorialManager.ActivateTutorialCanvas();
                yield break; // 튜토리얼 모드 진입 시 AutoSlide 종료
            }

            // 현재 슬라이드의 interval 값만큼 대기
            yield return new WaitForSeconds(_slides[currentSlideIndex].interval);

            // 페이드 아웃 및 다음 슬라이드로 전환
            yield return StartCoroutine(FadeToNextSlide());
        }
    }
    
    public void TriggerFadeToNextSlide()
    {
        if (isPaused)
        {
            isPaused = false;
            _slides[currentSlideIndex].isStop = false;
            _slides[currentSlideIndex].Voice.Stop();
            // 버튼을 눌렀으므로 일시 정지를 해제
            if (autoSlideCoroutine != null)
            {
                StopCoroutine(autoSlideCoroutine); 
            }

            autoSlideCoroutine = StartCoroutine(AutoNextSlide());
            _slides[currentSlideIndex].isStop = true;
        }
        else
        {
            Debug.LogWarning("Cannot trigger FadeToNextSlide. The scenario is not paused.");
        }
    }
    
    public void ReturnToSlide(int index)
    {
        if (index >= 0 && index < _slides.Count)
        {
            // 현재 슬라이드 인덱스를 설정
            currentSlideIndex = index;

            // 슬라이드 초기화 및 활성화
            SetCurrentSlideIndex(index);

            // isStop 상태를 true로 설정
            _slides[index].isStop = true;

            // AutoNextSlide 코루틴 다시 실행
            if (autoSlideCoroutine != null)
            {
                StopCoroutine(autoSlideCoroutine); // 이전 슬라이드 진행 중단
            }
            autoSlideCoroutine = StartCoroutine(AutoNextSlide());

            Debug.Log($"Returned to Slide {index} and restarted AutoNextSlide.");
        }
        else
        {
            Debug.LogError($"Invalid slide index: {index}. Cannot return to this slide.");
        }
    }
    
    private IEnumerator StopRecordingAfterDelay(float delay)
    {
        Debug.Log($"[UIScenario] Scheduled StopRecording after {delay} seconds.");
        yield return new WaitForSeconds(delay);

        if (microphoneRecorder != null)
        {
            Debug.Log("[UIScenario] Stopping recording after delay.");
            microphoneRecorder.StopRecording();
        }
        else
        {
            Debug.LogError("[UIScenario] MicrophoneRecorder is null when attempting to stop recording.");
        }
    }
    
    private IEnumerator WaitForVoiceRecognition()
    {
        // 음성 인식 완료될 때까지 대기
        while (!microphoneRecorder.IsRecognitionComplete)
        {
            yield return null; // 매 프레임 대기
        }

        Debug.Log("Voice recognition completed. Resuming auto slide...");
        isPaused = false; // 일시 정지 해제
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
        // 특정 슬라이드에서 튜토리얼 트리거
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
            // animator.SetInteger("Scenario", _slides[index].AnimatorCount); // OLD
            animator.SetTrigger(_slides[index].AnimatorIndex.ToString());
            Debug.Log("Showing Slide: " + index);
        }
        else
        {
            Debug.LogWarning("Slide index out of range: " + index);
        }
    }
}


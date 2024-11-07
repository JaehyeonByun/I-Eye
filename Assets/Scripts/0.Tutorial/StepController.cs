using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class StepController : MonoBehaviour
{
    public Text instructionText; // 단계를 안내하는 텍스트
    public Button nextButton; // 다음 버튼
    public SoilCollector soilCollector; // SoilCollector 스크립트 참조
    private int currentStep = 0;

    private string[] steps = {
        "큰 형 돼지: 지도에 진흙이 있는 곳을 표시해뒀어.",
        "큰 형 돼지: 빨간색, 노란색, 파란색 순서대로 주워야 해!",
        "큰 형 돼지: 줍는 순서를 잘 기억하고, 그 순서대로 주우러 가줘!",
        "큰 형 돼지: 그럼 지금부터 진흙을 모으러 가자!" // 4단계 안내 메시지
    };

    void Start()
    {
        nextButton.onClick.AddListener(NextStep);
        ShowStep();
        if (soilCollector != null)
            soilCollector.enabled = false; // SoilCollector를 처음에 비활성화
    }

    void NextStep()
    {
        if (currentStep < steps.Length - 2) // 3단계 전까지는 버튼으로 진행
        {
            currentStep++;
            ShowStep();
        }
        else if (currentStep == steps.Length - 2) // 3단계 완료 후
        {
            nextButton.gameObject.SetActive(false); // 버튼 숨기기
            StartCoroutine(WaitAndProceedToNextStep()); // 10초 대기 후 4단계로 이동
        }
        else if (currentStep == steps.Length - 1) // 4단계에서 Next 버튼을 누르면
        {
            ActivateSoilCollection(); // 진흙 수집 기능 활성화
        }
    }

    IEnumerator WaitAndProceedToNextStep()
    {
        yield return new WaitForSeconds(10f); // 10초 대기
        currentStep++;
        ShowStep(); // 4단계 표시
        nextButton.gameObject.SetActive(true); // Next 버튼 다시 표시
    }

    void ActivateSoilCollection()
    {
        if (soilCollector != null)
        {
            soilCollector.enabled = true; // SoilCollector 활성화
            soilCollector.SpawnSoils(); // 진흙 프리팹 생성
            instructionText.text = ""; // 피드백 텍스트 초기화
            nextButton.gameObject.SetActive(false); // Next 버튼 숨기기
        }
    }

    void ShowStep()
    {
        instructionText.text = steps[currentStep];
    }
}
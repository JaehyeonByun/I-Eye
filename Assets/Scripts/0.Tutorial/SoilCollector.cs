using UnityEngine;
using UnityEngine.UI;

public class SoilCollector : MonoBehaviour
{
    [Header("Soil Prefabs")]
    public GameObject redSoilPrefab;
    public GameObject yellowSoilPrefab;
    public GameObject blueSoilPrefab;

    [Header("UI Elements")]
    public Text feedbackText;
    public AudioSource audioSource; // 피드백 음성 메시지
    public AudioClip incorrectClip;

    private GameObject[] soils; // 스폰된 진흙 오브젝트 배열
    private int currentStep = 0;
    private int mistakes = 0;

    private string[] colors = { "빨간색", "노란색", "파란색" }; // 진흙 색 순서
    private Vector3[] spawnPositions = { new Vector3(-0.2f, 1.6f, 1), new Vector3(0, 1.6f, 1), new Vector3(0.2f, 1.6f, 1) };

    void Start()
    {
        SpawnSoils();
        UpdateFeedback(""); // 초기 피드백 텍스트 비우기
    }

    public void SpawnSoils()
    {
        // 각 색상별 진흙을 설정된 위치에 생성하고 배열에 저장
        soils = new GameObject[3];
        soils[0] = Instantiate(redSoilPrefab, spawnPositions[0], Quaternion.identity);
        soils[1] = Instantiate(yellowSoilPrefab, spawnPositions[1], Quaternion.identity);
        soils[2] = Instantiate(blueSoilPrefab, spawnPositions[2], Quaternion.identity);
    }

    void UpdateFeedback(string message)
    {
        feedbackText.text = message;
    }

    public void CollectSoil(GameObject soil)
    {
        string chosenColor = soil.tag; // 클릭한 오브젝트의 태그로 색상 파악
        string correctColor = colors[currentStep];

        Debug.Log($"Chosen tag: {chosenColor}, Expected tag: {correctColor}"); // 태그와 순서 확인용 로그

        // 태그를 통해 순서를 검증
        if (chosenColor == correctColor)
        {
            // 올바른 순서로 진흙을 주운 경우
            currentStep++;
            UpdateFeedback($"{correctColor} 진흙을 주웠습니다.");
            Destroy(soil); // 진흙을 주운 후 제거

            if (currentStep >= colors.Length)
            {
                UpdateFeedback("모든 진흙을 올바른 순서로 주웠습니다!");
            }
        }
        else
        {
            // 잘못된 순서로 진흙을 주운 경우
            mistakes++;
            UpdateFeedback($"큰 형 돼지: 지금은 {correctColor} 진흙을 주울 차례야. 하지만 {chosenColor} 진흙을 선택했어. 다시 생각해보자!");
            audioSource.PlayOneShot(incorrectClip); // 잘못된 순서에 대한 음성 피드백
        }
    }

    void Update()
    {
        // 마우스 클릭으로 진흙을 줍는 기능 구현 (에디터에서 테스트하기 쉽게 설정)
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                Debug.Log("Object hit: " + hit.collider.gameObject.name); // 클릭한 오브젝트 이름 출력
                // 태그를 통해 클릭한 오브젝트가 진흙인지 확인
                if (hit.collider.gameObject.tag == "빨간색" || hit.collider.gameObject.tag == "노란색" || hit.collider.gameObject.tag == "파란색")
                {
                    CollectSoil(hit.collider.gameObject);
                }
            }
            else
            {
                Debug.Log("No object hit"); // Raycast가 아무 오브젝트도 감지하지 못했을 때
            }
        }
    }

}


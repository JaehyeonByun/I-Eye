using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TutorialManager : MonoBehaviour
{
    [SerializeField] private GameObject tutorialCanvas; // 튜토리얼 UI 캔버스
    [SerializeField] private GameObject dialogueCanvas; // 대화 캔버스
    [SerializeField] private ClayPickupManager clayPickupManager; // ClayPickupManager 참조
    [SerializeField] private List<RectTransform> containers; // 여러 컨테이너 RectTransform 리스트
    [SerializeField] private Image redClayImagePrefab; // 빨간 클레이 이미지 프리팹
    [SerializeField] private Image yellowClayImagePrefab; // 노란 클레이 이미지 프리팹
    [SerializeField] private Image blueClayImagePrefab; // 파란 클레이 이미지 프리팹
    
    [SerializeField] private Image wrongRedClayImagePrefab; // 빨간 클레이 이미지 프리팹
    [SerializeField] private Image wrongYellowClayImagePrefab; // 노란 클레이 이미지 프리팹
    [SerializeField] private Image wrongBlueClayImagePrefab; // 파란 클레이 이미지 프리팹

    private List<Vector2> correctRelativePositions = new List<Vector2>(); // 정답 클레이의 상대적 위치 리스트
    private List<Vector2> wrongRelativePositions = new List<Vector2>(); // 함정 클레이의 상대적 위치 리스트
    private List<string> correctClayTags = new List<string>(); // 정답 클레이 태그 리스트
    private List<string> wrongClayTags = new List<string>(); // 함정 클레이 태그 리스트

    private Vector3 minSpawnRange;
    private Vector3 maxSpawnRange;

    private void Start()
    {
        // 튜토리얼 캔버스 초기화
        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(false);

        // 클레이 픽업 매니저 초기화
        if (clayPickupManager != null)
        {
            clayPickupManager.gameObject.SetActive(true); // 활성화
            clayPickupManager.SpawnClayPrefabs(); // 클레이 생성

            // 스폰 범위 가져오기
            minSpawnRange = clayPickupManager.GetMinSpawnRange();
            maxSpawnRange = clayPickupManager.GetMaxSpawnRange();

            InitializeClayPositions(); // 클레이 위치 초기화
        }

        // 모든 컨테이너에 정답 및 함정 클레이를 배치
        foreach (var container in containers)
        {
            PlaceClayImagesInContainer(container);
        }
    }

    /// <summary>
    /// 클레이 픽업 매니저를 초기화하고 정답 및 함정 클레이의 위치와 태그를 저장합니다.
    /// </summary>
    private void InitializeClayPositions()
    {
        if (clayPickupManager != null)
        {
            var correctClayData = clayPickupManager.GetClayData(); // 정답 클레이 데이터
            var wrongClayData = clayPickupManager.GetWrongClayData(); // 함정 클레이 데이터

            Debug.Log($"Initialized {correctClayData.Count} correct clay positions.");
            Debug.Log($"Initialized {wrongClayData.Count} wrong clay positions.");

            // 정답 클레이 데이터 처리
            foreach (var data in correctClayData)
            {
                Vector3 position = data.Position;
                string tag = data.Tag;

                // X와 Y를 변환
                float relativeX = Mathf.InverseLerp(minSpawnRange.x, maxSpawnRange.x, position.x) * 1200;
                float relativeY = Mathf.InverseLerp(minSpawnRange.y, maxSpawnRange.y, position.y) * 720;

                // 상대적 위치와 태그 저장
                correctRelativePositions.Add(new Vector2(relativeX, relativeY));
                correctClayTags.Add(tag);

                Debug.Log($"Correct Clay - X: {relativeX}, Y: {relativeY}, Tag: {tag}");
            }

            // 함정 클레이 데이터 처리
            foreach (var data in wrongClayData)
            {
                Vector3 position = data.Position;
                string tag = data.Tag;

                // X와 Y를 변환
                float relativeX = Mathf.InverseLerp(minSpawnRange.x, maxSpawnRange.x, position.x) * 1200;
                float relativeY = Mathf.InverseLerp(minSpawnRange.y, maxSpawnRange.y, position.y) * 720;

                // 상대적 위치와 태그 저장
                wrongRelativePositions.Add(new Vector2(relativeX, relativeY));
                wrongClayTags.Add(tag);

                Debug.Log($"Wrong Clay - X: {relativeX}, Y: {relativeY}, Tag: {tag}");
            }
        }
        else
        {
            Debug.LogError("[TutorialManager] ClayPickupManager is not assigned.");
        }
    }

    /// <summary>
/// 주어진 컨테이너에 정답 및 함정 클레이 이미지를 배치합니다.
/// </summary>
private void PlaceClayImagesInContainer(RectTransform container)
{
    if (container == null)
    {
        Debug.LogError("Container is null!");
        return;
    }

    // 컨테이너의 중심 좌표 계산
    float containerWidth = 1300;
    float containerHeight = 800;

    // 정답 클레이 배치
    for (int i = 0; i < correctRelativePositions.Count; i++)
    {
        Image clayImagePrefab = GetClayImagePrefab(correctClayTags[i], true);
        if (clayImagePrefab == null) continue;

        Image clayImage = Instantiate(clayImagePrefab, container);

        // 앵커와 피벗 설정 (중앙 기준)
        clayImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        clayImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        clayImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // 배치 계산 (컨테이너 중심 기준)
        clayImage.rectTransform.anchoredPosition = new Vector2(
            correctRelativePositions[i].x - containerWidth / 2,
            correctRelativePositions[i].y - containerHeight / 2
        );

        Debug.Log($"Placed correct clay image at: {clayImage.rectTransform.anchoredPosition} in container {container.name}");
    }

    // 함정 클레이 배치
    for (int i = 0; i < wrongRelativePositions.Count; i++)
    {
        Image clayImagePrefab = GetClayImagePrefab(wrongClayTags[i], false);
        if (clayImagePrefab == null) continue;

        Image clayImage = Instantiate(clayImagePrefab, container);

        // 앵커와 피벗 설정 (중앙 기준)
        clayImage.rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        clayImage.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        clayImage.rectTransform.pivot = new Vector2(0.5f, 0.5f);

        // 배치 계산 (컨테이너 중심 기준)
        clayImage.rectTransform.anchoredPosition = new Vector2(
            wrongRelativePositions[i].x - containerWidth / 2,
            wrongRelativePositions[i].y - containerHeight / 2
        );

        Debug.Log($"Placed wrong clay image at: {clayImage.rectTransform.anchoredPosition} in container {container.name}");
    }
}

    /// <summary>
    /// 클레이 태그에 따라 적절한 이미지 프리팹 반환
    /// </summary>
    private Image GetClayImagePrefab(string tag, bool isCorrect)
    {
        if (isCorrect)
        {
            switch (tag)
            {
                case "red":
                    return redClayImagePrefab;
                case "yellow":
                    return yellowClayImagePrefab;
                case "blue":
                    return blueClayImagePrefab;
            }
        }
        else
        {
            switch (tag)
            {
                case "wrongRed":
                    return wrongRedClayImagePrefab;
                case "wrongYellow":
                    return wrongYellowClayImagePrefab;
                case "wrongBlue":
                    return wrongBlueClayImagePrefab;
            }
        }

        Debug.LogWarning($"Unknown clay tag: {tag}");
        return null;
    }

    /// <summary>
    /// 튜토리얼 모드를 활성화합니다.
    /// </summary>
    public void ActivateTutorialCanvas()
    {
        Debug.Log("Activating Tutorial Canvas...");

        if (tutorialCanvas != null)
            tutorialCanvas.SetActive(true); // 튜토리얼 캔버스 활성화

        if (dialogueCanvas != null)
            dialogueCanvas.SetActive(false); // 대화 캔버스 비활성화

        if (clayPickupManager != null)
        {
            clayPickupManager.ActivateAllClays(); // 클레이 오브젝트 활성화
        }
    }

    /// <summary>
    /// 저장된 클레이 위치를 반환합니다.
    /// </summary>
    /// <summary>
    /// 모든 클레이의 상대적 위치를 반환합니다.
    /// </summary>
    public List<Vector2> GetClayPositions(bool includeWrong = true)
    {
        var combinedPositions = new List<Vector2>(correctRelativePositions);
    
        if (includeWrong)
        {
            combinedPositions.AddRange(wrongRelativePositions);
        }

        return combinedPositions;
    }
}

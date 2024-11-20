using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using System.Collections;

public class ClayPickupManager : MonoBehaviour
{
    [SerializeField] private GameObject redClayPrefab;
    [SerializeField] private GameObject yellowClayPrefab;
    [SerializeField] private GameObject blueClayPrefab;

    [SerializeField] private GameObject wrongRedClayPrefab;
    [SerializeField] private GameObject wrongBlueClayPrefab;
    [SerializeField] private GameObject wrongYellowClayPrefab;

    [SerializeField] private Vector3 minSpawnRange;
    [SerializeField] private Vector3 maxSpawnRange;
    [SerializeField] private Vector3 initialRotation;

    [SerializeField] private GameObject uiCanvas; // 메시지 표시용 Canvas
    [SerializeField] private TMPro.TextMeshProUGUI messageText;

    [SerializeField] private TMPro.TextMeshProUGUI leftClayNumberText;

    [SerializeField] private AudioSource audioSource; // 클레이 매니저에 붙은 AudioSource
    [SerializeField] private AudioClip correctSound; // 정답 사운드 클립
    [SerializeField] private AudioClip wrongSound; // 오답 사운드 클립
    
    [SerializeField] private GameObject clearUI; // 클리어 UI 오브젝트
    [SerializeField] private GameObject tutorialUI;
    [SerializeField] private AudioClip clearSound; // 클리어 효과음

    private List<GameObject> clayObjects = new List<GameObject>();
    private List<ClayData> clayDataList = new List<ClayData>();
    private List<ClayData> wrongClayDataList = new List<ClayData>();

    private int currentPickupIndex = 0;
    private int correctClayCount = 0;
    private int incorrectClayCount = 0; // 잘못 주운 진흙 횟수
    private float gameStartTime;
    private float totalDistanceTraveled = 0f;
    private int hintUsageCount = 0;

    private Vector3 lastPlayerPosition;

    private readonly string[] pickupOrder = { "red", "yellow", "blue" };
    private Coroutine messageCoroutine;

    public Vector3 GetMinSpawnRange() => minSpawnRange;
    public Vector3 GetMaxSpawnRange() => maxSpawnRange;
    public List<ClayData> GetClayData() => clayDataList;
    public List<ClayData> GetWrongClayData() => wrongClayDataList;

    public delegate void OnAllCorrectClaysCollected();

    public event OnAllCorrectClaysCollected AllCorrectClaysCollected;

    public delegate void OnClayPickup(string clayTag, bool isCorrect, Vector3 position);

    public event OnClayPickup ClayPickupEvent;


    private readonly Dictionary<string, string> tagToKoreanColor = new Dictionary<string, string>
    {
        { "red", "빨간색" },
        { "yellow", "노란색" },
        { "blue", "파란색" }
    };

    void Start()
    {
        gameStartTime = Time.time;
        lastPlayerPosition = transform.position;

        if (uiCanvas != null)
            uiCanvas.SetActive(false); // 시작 시 UI 비활성화
        if (clearUI != null)
            clearUI.SetActive(false); // 시작 시 클리어 UI 비활성화
    }

    void Update()
    {
        // 이동 거리 추적
        Vector3 currentPlayerPosition = transform.position;
        totalDistanceTraveled += Vector3.Distance(lastPlayerPosition, currentPlayerPosition);
        lastPlayerPosition = currentPlayerPosition;
    }

    public void SpawnClayPrefabs()
    {
        if (clayObjects.Count > 0)
        {
            foreach (GameObject obj in clayObjects)
            {
                if (obj != null) Destroy(obj);
            }
        }

        clayObjects.Clear();
        clayDataList.Clear();
        wrongClayDataList.Clear();

        correctClayCount = 0;
        incorrectClayCount = 0;

        for (int i = 0; i < 3; i++)
        {
            AddClay(redClayPrefab, "red", true);
            AddClay(yellowClayPrefab, "yellow", true);
            AddClay(blueClayPrefab, "blue", true);
        }

        AddClay(wrongRedClayPrefab, "wrongRed", false);
        AddClay(wrongBlueClayPrefab, "wrongBlue", false);
        AddClay(wrongYellowClayPrefab, "wrongYellow", false);

        Debug.Log($"Spawned {clayObjects.Count} clays (9 correct, 3 wrong, all disabled).");
    }

    private void AddClay(GameObject prefab, string tag, bool isCorrect)
    {
        Vector3 spawnPosition = GenerateRandomPosition();
        GameObject clay = Instantiate(prefab, spawnPosition, Quaternion.Euler(initialRotation));

        clay.tag = tag;
        clay.SetActive(false);

        clayObjects.Add(clay);

        if (isCorrect)
        {
            clayDataList.Add(new ClayData(spawnPosition, tag));
        }
        else
        {
            wrongClayDataList.Add(new ClayData(spawnPosition, tag));
        }
    }

    private Vector3 GenerateRandomPosition()
    {
        return new Vector3(
            Random.Range(minSpawnRange.x, maxSpawnRange.x),
            Random.Range(minSpawnRange.y, maxSpawnRange.y),
            Random.Range(minSpawnRange.z, maxSpawnRange.z)
        );
    }

    public void ActivateAllClays()
    {
        foreach (GameObject clay in clayObjects)
        {
            clay.SetActive(true);
        }

        Debug.Log("All clays activated.");
    }

    public void AttemptPickup(GameObject clay)
    {
        string clayTag = clay.tag;

        if (clay.CompareTag(pickupOrder[currentPickupIndex]))
        {
            correctClayCount++;
            currentPickupIndex = (currentPickupIndex + 1) % pickupOrder.Length;

            // 이벤트 호출: 정답 클레이
            ClayPickupEvent?.Invoke(clayTag, true, clay.transform.position);

            Destroy(clay); // 클레이 삭제
            Debug.Log($"Correct clay picked: {clayTag}. Total: {correctClayCount}/9");

            int leftClays = 9 - correctClayCount;
            leftClayNumberText.text = $"{leftClays}개 남았어요";

            if (correctClayCount >= 9)
            {
                float elapsedTime = Time.time - gameStartTime;
                Debug.Log($"All correct clays collected! Time: {elapsedTime:F2} seconds.");
                ShowClearUI(elapsedTime); // 클리어 UI 표시
                
            }
        }
        else
        {
            Debug.Log("error!");
        }
    }
    
    private void ShowClearUI(float elapsedTime)
    {
        if (clearUI != null)
        {
            // 클리어 메시지 업데이트 (필요 시)
            if (clearUI.GetComponentInChildren<TMPro.TextMeshProUGUI>() != null)
            {
                var messageText = clearUI.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                messageText.text = $"축하합니다! 모든 진흙을 모았습니다.\n소요 시간: {elapsedTime:F2}초";
            }

            clearUI.SetActive(true); // UI 활성화
            tutorialUI.SetActive(false);
            StartCoroutine(FadeCanvasGroup(clearUI.GetComponent<CanvasGroup>(), 0f, 1f, 0.5f)); // 페이드인
            PlayClearSound(); // 클리어 효과음 재생
        }
    }

    private void PlayClearSound()
    {
        if (audioSource != null && clearSound != null)
        {
            audioSource.clip = clearSound;
            audioSource.Play();
        }
    }
    
    private IEnumerator FadeCanvasGroup(CanvasGroup canvasGroup, float startAlpha, float endAlpha, float duration, System.Action onComplete = null)
    {
        if (canvasGroup == null)
        {
            canvasGroup = clearUI.AddComponent<CanvasGroup>(); // CanvasGroup 동적 추가
        }

        float elapsedTime = 0f;
        canvasGroup.alpha = startAlpha;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / duration);
            yield return null;
        }

        canvasGroup.alpha = endAlpha;

        onComplete?.Invoke(); // 페이드 완료 시 추가 작업 실행
    }
    
    private void ShowMessage(string message)
    {
        if (messageCoroutine != null)
        {
            StopCoroutine(messageCoroutine);
        }

        messageCoroutine = StartCoroutine(DisplayMessage(message));
    }

    private IEnumerator DisplayMessage(string message)
    {
        messageText.text = message;

        // CanvasGroup 컴포넌트 가져오기
        CanvasGroup canvasGroup = uiCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = uiCanvas.AddComponent<CanvasGroup>();
        }

        uiCanvas.SetActive(true);

        // 페이드인 효과
        float fadeInDuration = 0.5f;
        for (float t = 0; t < fadeInDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(0, 1, t / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1;

        // 메시지 표시 유지
        yield return new WaitForSeconds(2f);

        // 페이드아웃 효과
        float fadeOutDuration = 0.5f;
        for (float t = 0; t < fadeOutDuration; t += Time.deltaTime)
        {
            canvasGroup.alpha = Mathf.Lerp(1, 0, t / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0;

        uiCanvas.SetActive(false);
    }

    public void UseHint()
    {
        hintUsageCount++;
        Debug.Log($"Hint used! Total hints: {hintUsageCount}");
    }

    public string GetNextClayColor()
    {
        if (currentPickupIndex < pickupOrder.Length)
        {
            // 태그에 해당하는 한국어 색상 이름 반환
            Debug.Log(tagToKoreanColor[pickupOrder[currentPickupIndex]]);
            return tagToKoreanColor[pickupOrder[currentPickupIndex]];

        }

        return "알 수 없음";
    }

    public bool CanPickup(GameObject clay)
    {
        string clayTag = clay.tag;

        // 정답 클레이 처리
        if (clayTag == pickupOrder[currentPickupIndex])
        {
            PlayCorrectPickupSound();
            return true; // Grab 가능
        }

        // 함정 클레이 처리
        if (clayTag == "wrongRed" || clayTag == "wrongBlue" || clayTag == "wrongYellow")
        {
            ShowMessage("함정이야! 다시 한번 생각해보자.");
            TriggerWrongClayEffect(clay);
            Debug.LogWarning($"Wrong clay detected: {clayTag}");
            return false; // Grab 불가능
        }

        // 순서가 잘못된 경우 처리
        ShowMessage($"지금은 {tagToKoreanColor[pickupOrder[currentPickupIndex]]} 진흙을 주울 차례야.");
        TriggerWrongClayEffect(clay);
        Debug.LogWarning($"Wrong order for clay: {clayTag}");
        return false; // Grab 불가능
    }

    private void TriggerWrongClayEffect(GameObject clay)
    {
        // 반투명 blink 효과
        StartCoroutine(FadeEffect(clay));

        // 오답 사운드 재생
        PlayWrongPickupSound();
    }

    private IEnumerator FadeEffect(GameObject clay)
    {
        Renderer renderer = clay.GetComponent<Renderer>();
        if (renderer == null)
        {
            Debug.LogWarning($"No Renderer found on {clay.name}");
            yield break;
        }

        // Renderer의 Material 배열 가져오기
        Material[] materials = renderer.materials;

        // Emission 활성화
        foreach (var mat in materials)
        {
            if (!mat.HasProperty("_EmissionColor"))
            {
                Debug.LogWarning($"Material {mat.name} does not support Emission");
                continue;
            }

            mat.EnableKeyword("_EMISSION"); // Emission 활성화
        }

        // 색상 설정
        Color targetColor;
        if (!ColorUtility.TryParseHtmlString("#A63B3D", out targetColor))
        {
            Debug.LogWarning("Failed to parse Hex color. Using default red.");
            targetColor = Color.red * 0.5f; // 기본값: 빨간색
        }

        Color originalColor = materials[0].GetColor("_EmissionColor");
        float duration = 1.0f; // 페이드 효과 지속 시간
        float elapsedTime = 0f;

        // 페이드인: EmissionColor를 빨간색으로 변경
        while (elapsedTime < duration / 2)
        {
            float t = elapsedTime / (duration / 2);
            foreach (var mat in materials)
            {
                mat.SetColor("_EmissionColor", Color.Lerp(originalColor, targetColor, t));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // 페이드아웃: EmissionColor를 원래 색상으로 복원
        elapsedTime = 0f;
        while (elapsedTime < duration / 2)
        {
            float t = elapsedTime / (duration / 2);
            foreach (var mat in materials)
            {
                mat.SetColor("_EmissionColor", Color.Lerp(targetColor, originalColor, t));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Emission 초기화
        foreach (var mat in materials)
        {
            mat.DisableKeyword("_EMISSION"); // Emission 비활성화
            mat.SetColor("_EmissionColor", originalColor); // 원래 색상 복원
        }
    }

    private void PlayCorrectPickupSound()
    {
        if (audioSource != null && correctSound != null)
        {
            audioSource.clip = correctSound;
            audioSource.Play(); // 정답 사운드 재생
        }
        else
        {
            Debug.LogWarning("AudioSource 또는 CorrectSound가 설정되지 않았습니다.");
        }
    }

    private void PlayWrongPickupSound()
    {
        if (audioSource != null && wrongSound != null)
        {
            audioSource.clip = wrongSound;
            audioSource.Play(); // 오답 사운드 재생
        }
        else
        {
            Debug.LogWarning("AudioSource 또는 WrongSound가 설정되지 않았습니다.");
        }
    }

}

public struct ClayData
{
    public Vector3 Position { get; private set; }
    public string Tag { get; private set; }

    public ClayData(Vector3 position, string tag)
    {
        Position = position;
        Tag = tag;
    }
}
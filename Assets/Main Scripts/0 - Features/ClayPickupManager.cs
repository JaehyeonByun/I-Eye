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

    [SerializeField] private InputActionReference TriggerAction;
    
    [SerializeField] private GameObject uiCanvas; // 메시지 표시용 Canvas
    [SerializeField] private TMPro.TextMeshProUGUI messageText; 
    
    [SerializeField] private TMPro.TextMeshProUGUI leftClayNumberText;
    
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

       // SpawnClayPrefabs();
        TriggerAction.action.performed += OnTriggerAction;
        
        if (uiCanvas != null)
            uiCanvas.SetActive(false); // 시작 시 UI 비활성화
    }

    void Update()
    {
        // 이동 거리 추적
        Vector3 currentPlayerPosition = transform.position;
        totalDistanceTraveled += Vector3.Distance(lastPlayerPosition, currentPlayerPosition);
        lastPlayerPosition = currentPlayerPosition;
    }

    void OnDestroy()
    {
        TriggerAction.action.performed -= OnTriggerAction;
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
        clay.transform.localScale *= 0.1f;

        Rigidbody rb = clay.AddComponent<Rigidbody>();
        rb.useGravity = false;

        SphereCollider collider = clay.AddComponent<SphereCollider>();
        collider.isTrigger = true;

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

    private void OnTriggerAction(InputAction.CallbackContext context)
    {
#if UNITY_EDITOR
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 0.5f))
        {
            AttemptPickup(hit.collider.gameObject);
        }
#else
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (var collider in hitColliders)
        {
            AttemptPickup(collider.gameObject);
        }
#endif
    }

    private void AttemptPickup(GameObject clay)
    {
        string clayTag = clay.tag;
        if (clay.CompareTag(pickupOrder[currentPickupIndex]))
        {
            correctClayCount++;
            currentPickupIndex = (currentPickupIndex + 1) % pickupOrder.Length;
            clay.SetActive(false);
            Debug.Log($"Correct clay picked: {clay.tag}. Total: {correctClayCount}/9");

            int leftClayCount = 9 - correctClayCount;
            leftClayNumberText.text = $"{leftClayCount}개 남았어요";

            if (correctClayCount >= 9)
            {
                float elapsedTime = Time.time - gameStartTime;
                Debug.Log($"All correct clays collected! Time: {elapsedTime:F2} seconds, Incorrect attempts: {incorrectClayCount}, Total distance traveled: {totalDistanceTraveled:F2}, Hints used: {hintUsageCount}");
                AllCorrectClaysCollected?.Invoke();
            }
        }
        else if (clay.CompareTag("wrongRed") || clay.CompareTag("wrongBlue") || clay.CompareTag("wrongYellow"))
        {
            incorrectClayCount++;
            clay.SetActive(true);
            ShowMessage("함정이야! 다시 한번 생각해보자.");
            Debug.LogWarning($"Collected wrong clay: {clay.tag}. Incorrect attempts: {incorrectClayCount}");
        }
        else
        {
            ShowMessage($"지금은 {tagToKoreanColor[clayTag]}을(를) 주울 차례가 아니야. \n 다시 생각해보자");
            Debug.LogWarning($"Incorrect clay picked: {clay.tag}");
        }
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
        uiCanvas.SetActive(true);

        yield return new WaitForSeconds(3f);

        uiCanvas.SetActive(false);
    }

    public void UseHint()
    {
        hintUsageCount++;
        Debug.Log($"Hint used! Total hints: {hintUsageCount}");
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
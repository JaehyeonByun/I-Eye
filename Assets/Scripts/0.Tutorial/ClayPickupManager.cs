#if UNITY_EDITOR
using UnityEditor;
#endif
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClayPickupManager : MonoBehaviour
{
    [SerializeField] private GameObject redClayPrefab;
    [SerializeField] private GameObject yellowClayPrefab;
    [SerializeField] private GameObject blueClayPrefab;
    [SerializeField] private Vector3 minSpawnRange;
    [SerializeField] private Vector3 maxSpawnRange;
    [SerializeField] private Vector3 initialRotation;
    [SerializeField] private InputActionReference TriggerAction;

    private readonly string[] pickupOrder = { "pink", "purple", "blue" };
    private int currentPickupIndex = 0;
    private int correctPickups = 0;
    private int incorrectAttempts = 0;

    void Start()
    {
        SpawnClayPrefabs();
        TriggerAction.action.performed += OnTriggerAction;
    }

    void OnDestroy()
    {
        TriggerAction.action.performed -= OnTriggerAction;
    }

    private void SpawnClayPrefabs()
    {
        List<GameObject> clayObjects = new List<GameObject>();
        for (int i = 0; i < 4; i++)
        {
            clayObjects.Add(InstantiateClay(redClayPrefab, "pink"));
            clayObjects.Add(InstantiateClay(yellowClayPrefab, "purple"));
            clayObjects.Add(InstantiateClay(blueClayPrefab, "blue"));
        }

        foreach (GameObject clay in clayObjects)
        {
            clay.SetActive(true);
        }
    }

    private GameObject InstantiateClay(GameObject prefab, string tag)
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(minSpawnRange.x, maxSpawnRange.x),
            Random.Range(minSpawnRange.y, maxSpawnRange.y),
            Random.Range(minSpawnRange.z, maxSpawnRange.z)
        );

        GameObject clay = Instantiate(prefab, spawnPosition, Quaternion.Euler(initialRotation));
        clay.tag = tag;
        clay.SetActive(false);

        Rigidbody rb = clay.AddComponent<Rigidbody>();
        rb.useGravity = false;

        SphereCollider collider = clay.AddComponent<SphereCollider>();
        collider.isTrigger = true;

        return clay;
    }

    private void OnTriggerAction(InputAction.CallbackContext context)
    {
#if UNITY_EDITOR
        // 에디터 환경에서는 마우스 클릭 위치에서 레이를 생성
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider != null && (hit.collider.CompareTag("pink") || hit.collider.CompareTag("purple") || hit.collider.CompareTag("blue")))
            {
                AttemptPickup(hit.collider.gameObject);
            }
        }
#else
        // 빌드된 환경에서는 현재 위치 주변 반경 내 오브젝트 탐지
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (var collider in hitColliders)
        {
            if (collider.CompareTag(pickupOrder[currentPickupIndex]))
            {
                AttemptPickup(collider.gameObject);
                break;
            }
        }
#endif
    }

    private void AttemptPickup(GameObject clay)
    {
        if (clay.CompareTag(pickupOrder[currentPickupIndex]))
        {
            correctPickups++;
            currentPickupIndex = (currentPickupIndex + 1) % pickupOrder.Length;
            clay.SetActive(false);

            if (correctPickups >= 12)
            {
                Debug.Log($"All clays picked correctly! Incorrect attempts: {incorrectAttempts}");
            }
        }
        else
        {
            incorrectAttempts++;
            Debug.Log($"Incorrect order! Attempts: {incorrectAttempts}");
        }
    }
}

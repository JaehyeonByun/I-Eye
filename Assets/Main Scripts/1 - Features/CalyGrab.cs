using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CalyGrab : MonoBehaviour
{
    private Vector3 lastPosition; // 이전 위치 저장
    private bool isFirstFrame = true; // 첫 프레임인지 확인
    private ClayPickupManager pickupManager; // Pickup Manager 참조
    private XRBaseInteractable interactable; // MRTK의 인터랙터

    void Start()
    {
        pickupManager = FindObjectOfType<ClayPickupManager>(); // PickupManager 검색
        interactable = GetComponent<XRBaseInteractable>(); // XRBaseInteractable 가져오기

        if (interactable != null)
        {
            // Grab 시도 이벤트 처리
            interactable.selectEntered.AddListener(HandleGrabAttempt);
        }
    }

    private void HandleGrabAttempt(SelectEnterEventArgs args)
    {
        if (pickupManager == null) return;

        // Grab 가능 여부 확인
        bool canPickup = pickupManager.CanPickup(gameObject);

        if (!canPickup)
        {
            Debug.Log($"{gameObject.name}는 주울 수 없는 클레이입니다. Grab 취소.");
            interactable.interactionManager.CancelInteractableSelection(interactable); // Grab 해제
        }
    }

    void Update()
    {
        // 첫 프레임에는 위치 초기화
        if (isFirstFrame)
        {
            lastPosition = transform.position;
            isFirstFrame = false;
            return;
        }

        // 현재 위치가 이전 위치와 다른지 확인
        if (transform.position != lastPosition)
        {
            if (pickupManager != null)
            {
                bool canPickup = pickupManager.CanPickup(gameObject);

                if (canPickup)
                {
                    Debug.Log($"{gameObject.name}가 움직여서 삭제됩니다.");
                    pickupManager.AttemptPickup(gameObject);
                }
                else
                {
                    Debug.Log($"{gameObject.name}는 주울 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("PickupManager가 설정되지 않았습니다!");
            }
        }

        // 이전 위치 업데이트
        lastPosition = transform.position;
    }

    private void OnDestroy()
    {
        // 이벤트 해제
        if (interactable != null)
        {
            interactable.selectEntered.RemoveListener(HandleGrabAttempt);
        }
    }
}

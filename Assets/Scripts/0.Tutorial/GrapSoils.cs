using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

public class GrapSoils : MonoBehaviour
{
    public GameObject DraggableCube;
    private bool isDragging = false;
    private Vector3 offset;
    public InputActionReference TriggerAction;

    public void Awake()
    {
        // Input action remains bound for editor interaction
    }

    public void Start()
    {
        TriggerAction.action.performed += OnTriggerAction;
        TriggerAction.action.canceled += OnTriggerReleased;
    }

    private void Update()
    {
#if UNITY_EDITOR
        SimulateDragInteraction();
#endif
    }

#if UNITY_EDITOR
    private void SimulateDragInteraction()
    {
        if (isDragging && DraggableCube != null)
        {
            Vector3 mousePosition = Mouse.current.position.ReadValue();
            Ray ray = Camera.main.ScreenPointToRay(mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Dragging DraggableCube to: " + hit.point);
                DraggableCube.transform.position = Vector3.Lerp(DraggableCube.transform.position, hit.point + offset, Time.deltaTime * 10f);
            }
            else
            {
                Debug.Log("Raycast did not hit any surface during drag.");
            }
        }
    }

    private void OnTriggerAction(InputAction.CallbackContext context)
    {
        Vector3 mousePosition = Mouse.current.position.ReadValue();
        Ray ray = Camera.main.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit) && hit.transform.gameObject == DraggableCube)
        {
            Debug.Log("Trigger action detected on DraggableCube");
            isDragging = true;
            offset = DraggableCube.transform.position - hit.point;
        }
        else
        {
            Debug.Log("Trigger action did not hit DraggableCube");
        }
    }

    private void OnTriggerReleased(InputAction.CallbackContext context)
    {
        Debug.Log("Trigger action released");
        isDragging = false;
    }
#endif
}
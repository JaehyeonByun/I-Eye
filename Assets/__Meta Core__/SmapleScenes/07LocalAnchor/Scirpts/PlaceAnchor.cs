using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;

public class PlaceAnchor : MonoBehaviour
{

    public GameObject HitIndicator;
    public GameObject NoHitIndicator;
    public GameObject LocalAnchor;
    Transform ARCamera;
    private ARRaycastManager _raycastManager;
    private GameObject _activeIndicator;
    private bool _isHit;
    private Vector3 _desiredPosition;
    private Quaternion _desiredRotation;
    public InputActionReference TriggerAction;
    bool isCreate = false;
    public void Awake()
    {
        _raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    public void Start()
    {
        ARCamera = Camera.main.GetComponent<Transform>();
        _activeIndicator = NoHitIndicator;
        _activeIndicator.SetActive(true);
        TriggerAction.action.performed += OnTriggerAction;
    }

    public void CastRay()
    {
        Ray ray = new Ray(ARCamera.position, ARCamera.forward);
        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
        if (_raycastManager.Raycast(ray, hitResults))
        {
            _desiredPosition = hitResults[0].pose.position;
            _desiredRotation = hitResults[0].pose.rotation;
            if(isCreate) { CreateAnchor(_desiredPosition, _desiredRotation); }
            if (!_isHit)
            {
                _activeIndicator.SetActive(false);
                _activeIndicator = HitIndicator;
                _activeIndicator.SetActive(true);
                _isHit = true;
            }
        }
        else
        {
            _desiredPosition = ARCamera.position + ARCamera.forward;
            _desiredRotation = Quaternion.identity;
            if (_isHit)
            {
                _activeIndicator.SetActive(false);
                _activeIndicator = NoHitIndicator;
                _activeIndicator.SetActive(true);
                _isHit = false;
            }
        }
    }

    private void Update()
    {


        CastRay();
        _activeIndicator.transform.position = _desiredPosition;
        _activeIndicator.transform.rotation = _desiredRotation;
    }


    private void OnTriggerAction(InputAction.CallbackContext context)
    {
        isCreate = true;
    }

    void CreateAnchor(Vector3 _pos, Quaternion _rot)
    {
        isCreate = false;
        GameObject _anchor = Instantiate(LocalAnchor);
        _anchor.transform.position = _pos;
        _anchor.transform.rotation = _rot;
    }


}

using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.ARFoundation;


public class HitTesting : MonoBehaviour
{
    public GameObject HitIndicator;
    public GameObject NoHitIndicator;
    Transform ARCamera;
    private ARRaycastManager _raycastManager;
    private GameObject _activeIndicator;
    private bool _isHit;
    private Vector3 _desiredPosition;
    private Quaternion _desiredRotation;

    public void Awake()
    {
        _raycastManager = FindObjectOfType<ARRaycastManager>();
    }

    public  void Start()
    {
        ARCamera = Camera.main.GetComponent<Transform>();
        _activeIndicator = NoHitIndicator;
        _activeIndicator.SetActive(true);
    }

    public void CastRay()
    {
        Ray ray = new Ray(ARCamera.position, ARCamera.forward);
        List<ARRaycastHit> hitResults = new List<ARRaycastHit>();
        if (_raycastManager.Raycast(ray, hitResults))
        {
            _desiredPosition = hitResults[0].pose.position;
            _desiredRotation = hitResults[0].pose.rotation;
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

}


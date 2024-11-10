using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oven : MonoBehaviour
{
    private bool _isGameClear = false; 

    [SerializeField, Range(10f, 50f)] private float lineSize = 10f;
    [SerializeField] private Transform _playerGaze;

    public int _failesCount = 0; 

    private bool _isShovelColliding = false; 
    [SerializeField] private float _gazetime = 10f;

    private bool _isCounted = false;

    void Update()
    {
        Debug.DrawRay(_playerGaze.position, _playerGaze.forward * lineSize, Color.yellow);
        RaycastHit hit;

        if (_isShovelColliding && _isGameClear == false)
        {
            if (Physics.Raycast(_playerGaze.position, _playerGaze.forward, out hit, lineSize))
            {
                if (hit.collider.CompareTag("오븐"))
                {
                    _isCounted = false;
                    Debug.Log("오븐을 응시 중");
                    _gazetime -= Time.deltaTime;
                    Debug.Log(_gazetime);
                    if (_gazetime < 0)
                    {
                        _gazetime = 0;
                        _isGameClear = true;
                        Debug.Log("GameClear!");
                    }
                }
                else
                {
                    if (!_isCounted)
                    {
                        _gazetime = 10f;
                        _failesCount += 1;
                        Debug.Log("오븐을 바라보세요");
                        _isCounted = true;
                    }
                }
            }
        }
    }

    void OnTriggerEnter(Collider _collider)
    {
        if (_collider.gameObject.CompareTag("삽"))
        {
            _isShovelColliding = true;
            Debug.Log("진흙굽기 시작");
        }
    }

    void OnTriggerExit(Collider _collider)
    {
        if (_collider.gameObject.CompareTag("삽") && _isGameClear == false)
        {
            _failesCount += 1;
            _isShovelColliding = false;
            Debug.Log("삽이 벗어남");
        }
    }
}
 
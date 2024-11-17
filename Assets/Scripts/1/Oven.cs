using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Oven : MonoBehaviour
{
    private bool _isGameClear = false; 

    [SerializeField, Range(10f, 50f)] private float lineSize = 10f;
    [SerializeField] private Transform _playerGaze;
    [SerializeField] private AudioSource _shoveSound;
    [SerializeField] private float _gazetime = 90f;
    
    private bool _isShovelColliding = false;

    private int _shovelOutCount = 0; //hyperactivityD

    private bool _isCounted = false;
    
    private float _distractedTime;
    private bool _isDistracted;
    public bool _isGameEnd;

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
                        Debug.Log("오븐을 바라보세요");
                        _isCounted = true;
                    }
                }
            }
        }
    }

    void OnTriggerEnter(Collider _collider) //삽이 오븐으로 들어오면 시간이 줄어들기 시작
    {
        if (_collider.gameObject.CompareTag("삽"))
        {
            _shoveSound.Play();
            _isShovelColliding = true;
            Debug.Log("진흙굽기 시작");
        }
    }

    void OnTriggerExit(Collider _collider) //삽이 오븐에서 나오면 정지, hyperactivityD 측정
    {
        if (_collider.gameObject.CompareTag("삽") && _isGameClear == false)
        {
            _isShovelColliding = false;
            _shovelOutCount += 1;
            Debug.Log(_shovelOutCount);
            Debug.Log("삽이 벗어남");
        }
    }
}
 
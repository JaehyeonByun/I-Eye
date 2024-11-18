using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Oven : MonoBehaviour
{

    [SerializeField, Range(10f, 50f)] private float lineSize = 10f;
    [SerializeField] private Transform _playerGaze;
    [SerializeField] private AudioSource _shoveSound;
    
    [SerializeField] private AudioSource _firstDistract;
    [SerializeField] private AudioSource _secondDistract;
    [SerializeField] private AudioSource _thirdDistract;
    
    [SerializeField] private AudioSource _gameClearSound;
    
    [SerializeField] private float _gazetime = 90f; //오븐을 바라봐야 하는 시간
    [SerializeField] private GameObject _clearUI;
    
    private bool _isShovelColliding = false;
    private bool _isGameClear = false;
    private bool _isCall = false;
    private bool _wasDistractedWhenCallCountControl = false;
    private int _distractedCount = 3;

    //InputData
    public float _distractedTime = 0f; // inattentionB
    public int _distractedWhenCall = 0; //inattentionH
    public int _shovelOutCount = 0; // hyperactivityD
    
    private bool _isDistracted = false;

    void Start()
    {
        if (_clearUI.activeSelf)
        {
            _clearUI.SetActive(false);
        }
    }
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
                    _gazetime -= Time.deltaTime;
                    Debug.Log($"클리어까지 남은 시간:{_gazetime}");
                    if (_gazetime < 70 && _distractedCount > 2)
                    {
                        DistractSoundPlay();
                        _distractedCount -= 1;
                    }
                    if (_gazetime < 50 && _distractedCount > 1)
                    {
                        DistractSoundPlay();
                        _distractedCount -= 1;
                    }
                    if (_gazetime < 30 && _distractedCount > 0)
                    {
                        DistractSoundPlay();
                        _distractedCount -= 1;
                    }
                    if (_gazetime < 0)
                    {
                        _gazetime = 0;
                        _isGameClear = true;
                        Debug.Log("GameClear!");
                    }
                    _wasDistractedWhenCallCountControl = false;
                }
            }
            else
            { 
                _distractedTime += Time.deltaTime;
                Debug.Log($"집중하지 않은 시간: {_distractedTime}");

                if (_isCall && !_wasDistractedWhenCallCountControl)
                {
                    _distractedWhenCall += 1; // 불렀을 때 산만해짐
                    _wasDistractedWhenCallCountControl = true;
                    Debug.Log($"방해했을 때 산만해진 횟수: {_distractedWhenCall}");
                }
            }
        }

        if (_isGameClear == true)
        {
            GameClear();
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

    void DistractSoundPlay()
    {
        Debug.Log("방해");
        if(_distractedCount == 3){
            _firstDistract.Play();
        }
        else if(_distractedCount == 2){
            _secondDistract.Play();
        }
        else if(_distractedCount == 1){
            _thirdDistract.Play();
        }
        _isCall = true;
        StartCoroutine(ResetCallAfterDelay(10f));
    }
    IEnumerator ResetCallAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay); // 지정된 시간 동안 대기
        _isCall = false;
    }
    void GameClear()
    {
        GameManager.Inattention_b.Append(_distractedTime);
        GameManager.Inattention_h.Append(_distractedWhenCall);
        GameManager.HyperActivity_d.Append(_shovelOutCount);
        _clearUI.SetActive(true);
    }
}
 
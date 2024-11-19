using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Brick : MonoBehaviour
{
     [SerializeField] private AudioSource _correctSound;
    [SerializeField] private AudioSource _incorrectSound;

    private Vector3 fixedPosition;
    private Quaternion fixedRotation;
    private bool isFixed = false;

    private House house;

    private void Start()
    {
        house = transform.parent.GetComponent<House>();

        if (house != null)
        {
            Debug.Log("BricksLeft: " + house.BricksLeft);
        }
        else
        {
            Debug.LogError("House 스크립트를 찾을 수 없습니다.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((gameObject.CompareTag("빨간벽돌") && other.CompareTag("빨간벽돌정답")) ||
            (gameObject.CompareTag("노란벽돌") && other.CompareTag("노란벽돌정답")) ||
            (gameObject.CompareTag("파란벽돌") && other.CompareTag("파란벽돌정답")))
        {
            if (!house.IsPositionOccupied(other.transform.position) && !isFixed)
            {
                fixedPosition = other.transform.position;
                fixedRotation = other.transform.rotation;
                LockBlockPosition();
                house.AddOccupiedPosition(fixedPosition);

                // 부딪힌 정답 위치의 콜라이더를 비활성화
                if (other.TryGetComponent<Collider>(out Collider correctCollider))
                {
                    correctCollider.enabled = false;
                }

                _correctSound.Play();
            }
        }
        else if (!isFixed && IsIncorrectPosition(other))
        {
            Vector3 incorrectPosition = other.transform.position;
            if (!house.IsIncorrectPositionRecorded(incorrectPosition))
            {
                house.RecordIncorrectPosition(incorrectPosition);
                HandleIncorrectAnswer();
            }
        }
    }
    
    private bool IsIncorrectPosition(Collider other)
    {
        return other.CompareTag("빨간벽돌정답") || 
               other.CompareTag("노란벽돌정답") || 
               other.CompareTag("파란벽돌정답");
    }

    private void HandleIncorrectAnswer()
    {
        house.Incorrect += 1; 
        _incorrectSound?.Play(); 
        Debug.Log($"오답! Incorrect count: {house.Incorrect}");
    }

    private void LockBlockPosition()
    {
        transform.position = fixedPosition;
        transform.rotation = fixedRotation;

        if (TryGetComponent<Rigidbody>(out Rigidbody rb))
        {
            rb.isKinematic = true;
        }

        if (TryGetComponent<Collider>(out Collider collider))
        {
            collider.enabled = false;
        }

        isFixed = true;
    }

    private void Update()
    {
        if (isFixed)
        {
            transform.position = fixedPosition;
            transform.rotation = fixedRotation;
        }
    }
}

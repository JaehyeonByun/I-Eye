using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 2f; // 이동 속도

    private void Update()
    {
        Vector3 moveDirection = Vector3.zero;

        if (Keyboard.current.wKey.isPressed) moveDirection += Vector3.forward;
        if (Keyboard.current.sKey.isPressed) moveDirection += Vector3.back;
        if (Keyboard.current.aKey.isPressed) moveDirection += Vector3.left;
        if (Keyboard.current.dKey.isPressed) moveDirection += Vector3.right;

        // 플레이어 이동
        transform.position += moveDirection * moveSpeed * Time.deltaTime;
    }
}
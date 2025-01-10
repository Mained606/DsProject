using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : TestCharacterManager
{
    private float moveSpeed = 5.0f;

    private Vector2 moveInput;
    private Transform cameraTransform;

    private void Start()
    {
        cameraTransform = Camera.main.transform;
    }

    private void Update()
    {
        OnMove();
    }

    private void HandleMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    private void OnMove()
    {
        if (moveInput != Vector2.zero)
        {
            Vector3 forward = cameraTransform.forward;
            Vector3 right = cameraTransform.right;

            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();

            Vector3 direction = (forward * moveInput.y + right * moveInput.x).normalized;

            transform.position += direction * moveSpeed * Time.deltaTime;

            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.2f);
            }
        }
    }
}

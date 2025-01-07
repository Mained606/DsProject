using UnityEngine;
using UnityEngine.InputSystem;

public class TestPlayer : MonoBehaviour
{
    private float moveSpeed = 5.0f;

    public PlayerInput playerInput;
    private Vector2 moveInput;
    private InputAction moveAction => playerInput.actions["Move"];

    [SerializeField] private Transform cameraTransform;

    private void Start()
    {
        playerInput = GetComponent<PlayerInput>();
        TestManager.Input.keyaction += OnMove;
    }
    void OnMove()
    {
        moveInput = moveAction.ReadValue<Vector2>();

        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;

        forward.Normalize();
        right.Normalize();

        Vector3 moveDirection = (right * moveInput.x + forward * moveInput.y).normalized;

        if(moveDirection.sqrMagnitude > 0.01f)
        {
            //Quaternion
        }

        Vector3 newPosition = new Vector3(moveInput.x, 0f, moveInput.y);
        Vector3 direction = ((newPosition + transform.position) - transform.position).normalized;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.2f);
        transform.position += newPosition * Time.deltaTime * moveSpeed;
    }

    private void OnDisable()
    {
        TestManager.Input.keyaction -= OnMove;
    }
}

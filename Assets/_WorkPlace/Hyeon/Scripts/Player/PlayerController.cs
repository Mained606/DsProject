using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private TestCharacterStats characterStats;

    public PlayerState CurrentState { get; private set; } = PlayerState.PlayerIdle;
    private float moveSpeed;

    private Vector2 moveInput;
    private Transform cameraTransform;

    private float jumpHeight = 1.0f;
    private Vector3 moveDirection;
    private Vector3 verticalVelocity;
    [SerializeField] private float gravity = -9.81f;
    [SerializeField] private bool isGrounded;

    private CharacterController characterController;


    private void Start()
    {
        characterStats = new TestCharacterStats();
        cameraTransform = Camera.main.transform;
        characterController = GetComponent<CharacterController>();

        Initialize();
    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;
        
        HandleGravity();
        ControlMovement();
        OnJump();

        Debug.Log($"Player State : {CurrentState}");
        switch (CurrentState)
        {
            case PlayerState.PlayerIdle:
                break;
            case PlayerState.PlayerWalk:
                break;
            case PlayerState.PlayerSprint:
                break;
            case PlayerState.PlayerAttack:
                break;
            case PlayerState.PlayerHit:
                break;
            case PlayerState.PlayerDeath:
                break;
        }
    }

    private void SetState(PlayerState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;
    }

    private void Initialize()
    {
        if(characterStats != null)
        {
            moveSpeed = characterStats.characterMoveSpeed;
        }
    }

    private void OnJump()
    {
        Debug.Log("OnJump");
        if (InputManager.InputActions.actions["Jump"].triggered && isGrounded)
        {
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    private void HandleGravity()
    {
        if (!isGrounded)
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
        else
        {
            if(verticalVelocity.y < 0)
            {
                verticalVelocity.y = -0.5f;
            }
        }
    }

    private void ControlMovement()
    {
        Vector2 moveInput = InputManager.InputActions.actions["Move"].ReadValue<Vector2>();
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        Vector3 direction = (forward * moveInput.y + right * moveInput.x).normalized;

        moveDirection = direction;
        moveDirection.y = verticalVelocity.y;

        characterController.Move(moveDirection * moveSpeed * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.2f);
        }
    }
}

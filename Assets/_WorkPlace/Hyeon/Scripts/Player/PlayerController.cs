using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private TestCharacterStats characterStats;
    public PlayerCombat playerCombat;

    public PlayerState CurrentState { get; private set; } = PlayerState.PlayerIdle;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float dodgeDist = 3f;
    [SerializeField] private float dodgeDuration = 0.3f;

    private Vector2 moveInput;
    public Transform cameraTransform;

    [SerializeField] private float jumpHeight = 0.5f;
    private Vector3 moveDirection;
    private Vector3 verticalVelocity;
    private float gravity = -9.81f;
    private bool isGrounded;
    private bool isSprinting;
    [SerializeField] private bool isDodging = false;
    //[SerializeField] private bool isInvincible = false;
    public bool CanMove;
    public bool CanAttack;
    public bool CanUseSkill;

    private CharacterController characterController;
    private Animator PlayerAnimator;

   

    private void Start()
    {
        characterStats = new TestCharacterStats();
        cameraTransform = Camera.main.transform;
        characterController = GetComponent<CharacterController>();
        PlayerAnimator = GetComponentInChildren<Animator>();
        CanMove = true;
        

        ValueInitialize();
    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;
        isSprinting = InputManager.InputActions.actions["Sprint"].IsPressed();
        PlayerAnimator.SetBool("Grounded", isGrounded);
        
        HandleGravity();
        if (!isDodging)
        {
            ControlMovement();
            OnJump();
            OnDodge();
        }

        //Debug.Log($"Player State : {CurrentState}");
        switch (CurrentState)
        {
            case PlayerState.PlayerIdle:
                break;
            case PlayerState.PlayerMove:
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

    // PlayerStats 초기화
    private void ValueInitialize()
    {
        if(characterStats != null)
        {
            walkSpeed = characterStats.characterWalkSpeed;
            sprintSpeed = characterStats.characterSprintSpeed;
        }
    }

    // 상시 중력 적용
    private void HandleGravity()
    {
        if (!isGrounded)
        {
            verticalVelocity.y += gravity * Time.deltaTime;
        }
        else
        {
            PlayerAnimator.SetBool("Jump", false);
            if (verticalVelocity.y < 0)
            {
                verticalVelocity.y = -0.5f;
            }
        }
    }

    private void StateCheck()
    {
        if(isGrounded && moveInput == Vector2.zero)
        {
            SetState(PlayerState.PlayerIdle);
        }
    }

    // 카메라 회전 기준으로 정면 변경
    private Vector3 GetDirection(Vector2 _moveInput)
    {
        Vector2 moveInput = _moveInput;
        Vector3 forward = cameraTransform.forward;
        Vector3 right = cameraTransform.right;

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        return (forward * moveInput.y + right * moveInput.x).normalized;
    }

    // 캐릭터 이동
    private void ControlMovement()
    {
        if (!CanMove) return;

        moveInput = InputManager.InputActions.actions["Move"].ReadValue<Vector2>();

        float currentSpeed = isSprinting ? sprintSpeed : walkSpeed;

        if (moveInput == Vector2.zero)
        {
            PlayerAnimator.SetFloat("MotionSpeed", 0);
            PlayerAnimator.SetFloat("Speed", 0);

        }
        else
        {


            PlayerAnimator.SetFloat("MotionSpeed", 1);
            PlayerAnimator.SetFloat("Speed", currentSpeed);
        }

        Vector3 direction = GetDirection(moveInput);
        
        moveDirection = direction;
        moveDirection.y = verticalVelocity.y;

        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);

        if (direction != Vector3.zero)
        {
            if(transform.rotation.eulerAngles.y != cameraTransform.rotation.eulerAngles.y)
            {

            }
            Vector3 forward = transform.forward;
            float angle = Vector3.SignedAngle(forward, direction, Vector3.up);

            if(Mathf.Abs(angle) > 0.1f)
            {

                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.2f);
            }
        }
    }

    // 캐릭터 점프
    private void OnJump()
    {
        if (!CanMove) return;

        if (InputManager.InputActions.actions["Jump"].triggered && isGrounded)
        {
            PlayerAnimator.SetBool("Jump", true);
            
            verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }

    // 캐릭터 구르기
    private void OnDodge()
    {
        if (InputManager.InputActions.actions["Dodge"].triggered && moveInput != Vector2.zero)
        {
            CanMove = false;
            StartCoroutine(Dodging());
        }
    }

    // 굴리기(무적)
    private IEnumerator Dodging()
    {
        isDodging = true;
        //isInvincible = true;

        Vector3 dodgeDirection = GetDirection(moveInput);
        float elapsedTime = 0f;

        while(elapsedTime < dodgeDuration)
        {
            characterController.Move(dodgeDirection *(dodgeDist / dodgeDuration) * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        CanMove = true;
        isDodging = false;
        //isInvincible = false;
    }

    private void avoidKeyInput()
    {
        switch (CanMove)
        {
            case true:
                InputManager.InputActions.actions["Move"].Enable();
                break;
            case false:
                InputManager.InputActions.actions["Move"].Disable();
                break;
        }
    }

    private void IdleMotion()
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    public PlayerData playerData;

    public PlayerState CurrentState { get; private set; } = PlayerState.PlayerIdle;
    [SerializeField] private float walkSpeed;
    [SerializeField] private float sprintSpeed;
    [SerializeField] private float dodgeDist = 3f;
    [SerializeField] private float dodgeDuration = 0.3f;

    private Vector2 moveInput;
    public Transform cameraTransform;

    [SerializeField] private float jumpHeight = 0.5f;
    private float lastGroundHeight;
    [SerializeField] private float fallDamageThreshold = 5f;
    [SerializeField] private float fallDamageMultiplier = 5f;
    private Vector3 moveDirection;
    private Vector3 verticalVelocity;
    private float gravity = -9.81f;
    private bool isGrounded;
    private bool isSprinting;
    [SerializeField] private bool isDodging = false;
    //[SerializeField] private bool isInvincible = false;
    public bool isFreefall;
    public bool CanMove;
    public bool CanAttack;
    public bool CanUseSkill;

    private CharacterController characterController;
    public Animator PlayerAnimator;

    
    private void OnEnable()
    {
        GameManager.playerTransform = this.transform;
    }


    private void Start()
    {
        cameraTransform = Camera.main.transform;
        characterController = GetComponent<CharacterController>();
        CanMove = true;
        CanAttack = true;
        CanUseSkill = true;
        playerData = CharacterManager.PlayerCharacterData;

        //RegistSkill();
        ValueInitialize();
    }

    private void Update()
    {
        isGrounded = characterController.isGrounded;
        isSprinting = InputManager.InputActions.actions["Sprint"].IsPressed();
        PlayerAnimator.SetBool("Grounded", isGrounded);

        if (InputManager.InputActions.actions["Interact"].triggered)
        {
            Debug.Log("Interact");
        }

        avoidKeyInput();
        
        HandleGravity();
        if (!isDodging)
        {
            ControlMovement();
            OnJump();
            OnDodge();
            OnParry();
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
        if(playerData != null)
        {
            walkSpeed = playerData.speed;
            sprintSpeed = playerData.speed * 2f;
        }
    }

    // 상시 중력 적용
    private void HandleGravity()
    {
        if (!isGrounded)
        {
            if(verticalVelocity.y <= fallDamageThreshold && !isFreefall)
            {
                isFreefall = true;
                CanAttack = false; CanUseSkill = false;
                lastGroundHeight = transform.position.y;
                PlayerAnimator.SetBool("Freefall", true);
            }
            verticalVelocity.y += gravity * Time.deltaTime;
        }
        else
        {
            if (isFreefall)
            {
                float fallDistance = lastGroundHeight - transform.position.y;

                if(fallDistance > fallDamageThreshold)
                {
                    ApplyFallDamage(fallDistance);
                }

                isFreefall = false;
                CanAttack = true; CanUseSkill = true;
                PlayerAnimator.SetBool("Freefall", false);
            }

            PlayerAnimator.SetBool("Jump", false);
            if (verticalVelocity.y < 0)
            {
                verticalVelocity.y = -0.5f;
            }
        }
    }

    private void ApplyFallDamage(float fallDistance)
    {
        float damage = (fallDistance - fallDamageThreshold) * fallDamageMultiplier;
        damage = Mathf.Max(0, damage);
        Debug.Log($"낙하 데미지: {damage}");
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

        switch (CanAttack)
        {
            case true:
                InputManager.InputActions.actions["Attack"].Enable();
                break;
            case false:
                InputManager.InputActions.actions["Attack"].Disable();
                break;
        }

        switch (CanUseSkill)
        {
            case true:
                InputManager.InputActions.actions["PlayerSkill_1"].Enable();
                InputManager.InputActions.actions["PlayerSkill_2"].Enable();
                InputManager.InputActions.actions["PlayerSkill_3"].Enable();
                break;
            case false:
                InputManager.InputActions.actions["PlayerSkill_1"].Disable();
                InputManager.InputActions.actions["PlayerSkill_2"].Disable();
                InputManager.InputActions.actions["PlayerSkill_3"].Disable();
                break;
        }
    }

    // 패링
    private void OnParry()
    {
        if (InputManager.InputActions.actions["Parry"].triggered)
        {
            PlayerAnimator.SetTrigger("Parry");
        }
        AnimatorStateInfo stateInfo = PlayerAnimator.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] clipInfo = PlayerAnimator.GetCurrentAnimatorClipInfo(0);
        if (clipInfo.Length > 0)
        {
            AnimationClip currentClip = clipInfo[0].clip;
            if (currentClip.name == "Parry")
            {
                float normalizedTime = stateInfo.normalizedTime;
                if (normalizedTime < 0.2f)
                {
                    // TODO
                    Debug.Log("can parry");
                }
            }

        }
        
    }

    // 장비 장착에 따른 스탯 변화


    private void IdleMotion()
    {

    }
}

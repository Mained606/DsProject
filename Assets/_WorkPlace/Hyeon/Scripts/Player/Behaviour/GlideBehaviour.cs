using UnityEngine;
using UnityEngine.Networking.PlayerConnection;

public class GlideBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;
    private GameObject wings;

    private bool canGlide;
    private float glidableHeight = 3.7f;        // 글라이딩 가능 높이
    private float bodyRotationSpeed = 0.05f;    // 공중에서 몸 돌리는 속도 가중치
    private LayerMask groundLayer;              // 그라운드 감지 레이어
    private float glideSpeed;                   // 글라이딩 최종 속도
    private float currentSpeed;                 // 현재 속도
    private float acceleration = 15f;           // 이동 가속 값
    Vector3 direction;                          // 인풋 이동 값

    // 블렌드 트리에 사용하는 값
    private float tiltX = 0f;
    private float tiltZ = 0f;
    private float tiltVelocityX = 0f;
    private float tiltVelocityZ = 0f;
    private float tiltSmoothTime = 0.2f;        // 부드러운 보간을 위한 시간

    // 사운드 이름 배열
    private string[] glideSound = { "Wing_On", "Glide_hmm" };


    public GlideBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
        wings = controller.wings;
        groundLayer = LayerMask.GetMask("Ground");
    }

    public void Enter()
    {
        glideSpeed = controller.glideSpeed;
        canGlide = false;
        controller.isGliding = false;
        if (wings.activeSelf)
        {
            wings.SetActive(false);
        }
    }

    public void Execute()
    {
        if (!controller.isGliding)
        {
            GlidableCheck();
            GroundedCheck();
            if (canGlide)
            {
                if (InputManager.InputActions.actions["Jump"].triggered)
                {
                    StartGilde();
                }
            }
        }
        else
        {
            OnGlide();
            GroundedCheck();
            StaminaCheck();
            if (InputManager.InputActions.actions["Jump"].triggered)
            {
                EndGlide();
            }
        }
        
    }

    public void Exit()
    {
        controller.isGliding = false;
        canGlide = false;
        if (wings.activeSelf)
        {
            wings.SetActive(false);
        }
    }

    private void GlidableCheck()
    {
        if(Physics.Raycast(controller.transform.position + Vector3.up * 1f, Vector3.down, out RaycastHit hit, glidableHeight, groundLayer))
        {
            canGlide = false;
        }
        else
        {
            canGlide = true;
        }
    }

    private void StartGilde()
    {
        controller.isGliding = true;
        SoundManager.Instance.PlayClipAtPoint(glideSound[0], controller.transform.position);
        SoundManager.Instance.PlayClipAtPoint(glideSound[1], controller.transform.position);
        animator.SetBool("Glide", true);
        currentSpeed = animator.GetFloat("Speed");
        if (controller.isJumping)
        {
            //controller.isJumping = false;
            animator.SetBool("Jump", false);
        }
        if (!wings.activeSelf)
        {
            wings.SetActive(true);
        }
        PlayerBehaviourManager.Instance.CanMove = false;
        PlayerBehaviourManager.Instance.CanJump = false;
    }
    private void EndGlide()
    {
        controller.isGliding = false;
        animator.SetBool("Glide", false);
        if (wings.activeSelf)
        {
            wings.SetActive(false);
        }
        PlayerBehaviourManager.Instance.CanMove = true;
        PlayerBehaviourManager.Instance.CanJump = true;
    }

    private void OnGlide()
    {
        Vector2 moveInput = InputManager.InputActions.actions["Move"].ReadValue<Vector2>();
        Vector3 movement;

        // 가속 적용
        if (moveInput != Vector2.zero)
        {
            direction = controller.GetDirection(moveInput);
            currentSpeed = Mathf.MoveTowards(currentSpeed, glideSpeed, acceleration * Time.deltaTime);
        }
        else
        {
            currentSpeed = Mathf.MoveTowards(currentSpeed, 0f, acceleration * Time.deltaTime);
        }

        animator.SetFloat("Speed", currentSpeed);

        // 이동
        if(direction != Vector3.zero)
        {
            movement = direction * currentSpeed * Time.deltaTime;
            movement.y = controller.verticalVelocity.y * Time.deltaTime;
            controller.characterController.Move(movement);
        }

        // 스태미너 소모
        controller.UsingStamina();

        // 몸 돌려주기
        if (direction != Vector3.zero && !TimerManager.Instance.IsGamePaused)
        {
            Vector3 forward = GameManager.playerTransform.forward;
            float angle = Vector3.SignedAngle(forward, direction, Vector3.up);

            if (Mathf.Abs(angle) > 0.1f)
            {
                GameManager.playerTransform.rotation = Quaternion.Slerp(GameManager.playerTransform.rotation, Quaternion.LookRotation(direction), bodyRotationSpeed);
            }
        }

        UpdateGlideTilt(moveInput);
    }

    private void StaminaCheck()
    {
        if(controller.playerData.staminaCurrent <= 0)
        {
            EndGlide();
        }
    }

    private void GroundedCheck()
    {
        if (controller.isGrounded)
        {
            PlayerBehaviourManager.Instance.CanMove = true;
            PlayerBehaviourManager.Instance.CanJump = true;
            PlayerBehaviourManager.Instance.CanAttack = true;
            PlayerBehaviourManager.Instance.CanBlock = true;
            PlayerBehaviourManager.Instance.CanUseSkill = true;
            PlayerBehaviourManager.Instance.CanDodge = true;

            EndGlide();
        }
        else
        {
            PlayerBehaviourManager.Instance.CanAttack = false;
            PlayerBehaviourManager.Instance.CanBlock = false;
            PlayerBehaviourManager.Instance.CanUseSkill = false;
            PlayerBehaviourManager.Instance.CanDodge = false;
            animator.ResetTrigger("NextCombo");
        }
    }

    private void UpdateGlideTilt(Vector2 moveInput)
    {
        if (direction != Vector3.zero)
        {
            Vector3 localDirection = controller.transform.InverseTransformDirection(direction);

            // 기울기 목표값 설정 (-1 ~ 1)
            float targetTiltX = Mathf.Clamp(localDirection.x, -1f, 1f);
            float targetTiltZ = Mathf.Clamp(localDirection.z, -1f, 1f);

            // SmoothDamp를 사용해 부드러운 전환
            tiltX = Mathf.SmoothDamp(tiltX, targetTiltX, ref tiltVelocityX, tiltSmoothTime);
            tiltZ = Mathf.SmoothDamp(tiltZ, targetTiltZ, ref tiltVelocityZ, tiltSmoothTime);
        }
        else
        {
            // 이동이 멈추면 원래 상태로 복귀
            tiltX = Mathf.SmoothDamp(tiltX, 0, ref tiltVelocityX, tiltSmoothTime);
            tiltZ = Mathf.SmoothDamp(tiltZ, 0, ref tiltVelocityZ, tiltSmoothTime);
        }

        // 애니메이터에 값 적용
        animator.SetFloat("DirX", tiltX);
        animator.SetFloat("DirZ", tiltZ);
    }
}

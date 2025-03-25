using UnityEngine;

public class GlideBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;
    private GameObject wings;

    private float glidableHeight = 3.7f;
    private LayerMask groundLayer;
    private float glideSpeed;
    private bool canGlide;

    private string glideSound = "Glide_hmm";


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
        SoundManager.Instance.PlayClipAtPoint(glideSound, controller.transform.position);
        animator.SetBool("Glide", true);
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

        Vector3 direction = controller.GetDirection(moveInput);
        Vector3 moveDirection = direction;

        Vector3 movement = moveDirection * glideSpeed * Time.deltaTime;
        movement.y = controller.verticalVelocity.y * Time.deltaTime;

        controller.characterController.Move(movement);
        controller.UsingStamina();

        if (direction != Vector3.zero)
        {
            Vector3 forward = GameManager.playerTransform.forward;
            float angle = Vector3.SignedAngle(forward, direction, Vector3.up);

            if (Mathf.Abs(angle) > 0.1f)
            {
                GameManager.playerTransform.rotation = Quaternion.Slerp(GameManager.playerTransform.rotation, Quaternion.LookRotation(direction), 0.05f);
            }
        }
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
}

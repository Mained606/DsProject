using UnityEngine;

public class GlideBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    private float glidableHeight = 6f;
    private float glideSpeed;
    private bool canGlide;


    public GlideBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
    }

    public void Enter()
    {
        glideSpeed = controller.glideSpeed;
        controller.isGliding = false;
        //animator.SetBool("Glide", false);
    }

    public void Execute()
    {
        if (!controller.isGliding)
        {
            GlidableCheck();
            GroundedCheck();
        }
        else
        {
            OnGlide();
            StaminaCheck();
            if (InputManager.InputActions.actions["Jump"].triggered)
            {
                SwitchGlideState();
            }
        }

        if (canGlide)
        {
            if (InputManager.InputActions.actions["Jump"].triggered)
            {
                SwitchGlideState();
            }
        }
        
    }

    public void Exit()
    {
        controller.isGliding = false;
        //animator.SetBool("Glide", false);
    }

    private void GlidableCheck()
    {
        if(Physics.Raycast(controller.transform.position, Vector3.down, out RaycastHit hit, glidableHeight))
        {
            canGlide = false;
        }
        else
        {
            canGlide = true;
        }
    }

    private void SwitchGlideState()
    {
        controller.isGliding = !controller.isGliding;
        if (controller.isJumping)
        {
            controller.isJumping = false;
            animator.SetBool("Jump", false);
        }
        //animator.SetBool("Glide", controller.isGliding);
        PlayerBehaviourManager.Instance.CanMove = !controller.isGliding;
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
            SwitchGlideState();
        }
    }

    private void GroundedCheck()
    {
        if (controller.isGrounded)
        {
            PlayerBehaviourManager.Instance.CanMove = true;
            PlayerBehaviourManager.Instance.CanAttack = true;
            PlayerBehaviourManager.Instance.CanBlock = true;
            PlayerBehaviourManager.Instance.CanUseSkill = true;
            PlayerBehaviourManager.Instance.CanDodge = true;
            controller.isGliding = false;
            //animator.SetBool("Glide", false);
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

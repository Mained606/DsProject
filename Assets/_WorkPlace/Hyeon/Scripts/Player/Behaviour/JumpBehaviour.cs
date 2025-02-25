using Unity.Android.Gradle.Manifest;
using Unity.VisualScripting;
using UnityEngine;

public class JumpBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;
    private float jumpHeight = 3.0f;    // 점프 높이
    private float gravity = -9.81f;     // 중력

    public JumpBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
    }

    public void Enter()
    {
        Debug.Log("Jump 가능 상태 진입");
        animator.SetBool("Jump", false);
        controller.isJumping = false;
        jumpHeight = controller.jumpHeight;
    }

    public void Execute()
    {
        if (controller.isJumping)
        {
            GroundedCheck();
        }

        if (InputManager.InputActions.actions["Jump"].triggered && !controller.isJumping)
        {
            OnJump();
        }
    }

    public void Exit()
    {
        animator.SetBool("Jump", false);
        controller.isJumping = false;
    }

    // 점프
    private void OnJump()
    {
        controller.isJumping = true;
        controller.verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        animator.SetBool("Jump", true);
    }

    private void GroundedCheck()
    {
        if (controller.isGrounded)
        {
            controller.isJumping = false;
            animator.SetBool("Jump", false);
        }
    }

}

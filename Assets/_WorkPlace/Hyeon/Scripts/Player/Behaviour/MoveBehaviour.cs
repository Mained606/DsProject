using UnityEngine;
using UnityEngine.EventSystems;

public class MoveBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    private float walkSpeed;
    private float sprintSpeed;
    private bool canSprint;

    private float currentStamina;

    private Vector2 moveInput;

    public MoveBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
        walkSpeed = controller.walkSpeed;
        sprintSpeed = controller.sprintSpeed;
    }

    public void Enter()
    {
        controller.isMove = false;
        PlayerBehaviourManager.Instance.CanAttack = true;
        PlayerBehaviourManager.Instance.CanUseSkill = true;
        PlayerBehaviourManager.Instance.CanBlock = true;
        PlayerBehaviourManager.Instance.CanDodge = true;
        //PlayerBehaviourManager.Instance.CanClimb = true;
    }

    public void Execute()
    {
        HandleMovement();
    }

    public void Exit()
    {
        controller.isMove = false;
    }

    private void HandleMovement()
    {
        moveInput = InputManager.InputActions.actions["Move"].ReadValue<Vector2>();

        Vector3 direction = controller.GetDirection(moveInput);
        Vector3 moveDirection = direction;
        //moveDirection.y = controller.verticalVelocity.y;

        walkSpeed = controller.walkSpeed;
        sprintSpeed = controller.sprintSpeed;
        RunableCheck();
        float currentSpeed = canSprint ? sprintSpeed : walkSpeed;

        if (moveInput == Vector2.zero)
        {
            animator.SetFloat("MotionSpeed", 1);
            animator.SetFloat("Speed", 0);
            controller.isMove = false;
            controller.isSprinting = false;
        }
        else
        {
            animator.SetFloat("MotionSpeed", 1);
            animator.SetFloat("Speed", currentSpeed);
            controller.isMove = true;
            if (canSprint)
            {
                controller.isSprinting = true;
            }
        }

        animator.SetBool("Sprint", controller.isSprinting);

        Vector3 movement = moveDirection * currentSpeed * Time.deltaTime;
        movement.y = controller.verticalVelocity.y * Time.deltaTime;

        controller.characterController.Move(movement);
        controller.UsingStamina();

        if (direction != Vector3.zero)
        {
            Vector3 forward = GameManager.playerTransform.forward;
            float angle = Vector3.SignedAngle(forward, direction, Vector3.up);

            if (Mathf.Abs(angle) > 0.1f)
            {
                GameManager.playerTransform.rotation = Quaternion.Slerp(GameManager.playerTransform.rotation, Quaternion.LookRotation(direction), 0.2f);
            }
        }
    }

    private void RunableCheck()
    {
        if (InputManager.InputActions.actions["Sprint"].IsPressed())
        {
            currentStamina = controller.playerData.staminaCurrent;
            if (controller.isRecovery && currentStamina > 10f)
            {
                canSprint = true;
                return;
            }
            else if (!controller.isRecovery)
            {
                canSprint = currentStamina >= controller.staminaUseAmount ? true : false;
                return;
            }
        }
        else
        {
            canSprint = false;
            controller.isSprinting = false;
        }
    }
}

using UnityEngine;

public class MoveBehaviour : IBehaviour
{
    private PlayerController playerController;
    private CharacterController characterController;
    private Transform cameraTransform;
    private Animator animator;
    private float walkSpeed;
    private float sprintSpeed;
    private bool isGliding;
    private bool canSprint;

    private float currentStamina;

    public MoveBehaviour(CharacterController controller, Animator anim, float walk, float sprint)
    {
        characterController = controller;
        animator = anim;
        walkSpeed = walk;
        sprintSpeed = sprint;
        //isGliding = gliding;
        playerController = GameManager.playerTransform.GetComponent<PlayerController>();

    }

    public void Enter()
    {
        
    }

    public void Execute()
    {
        Vector2 moveInput = InputManager.InputActions.actions["Move"].ReadValue<Vector2>();

        Vector3 direction = playerController.GetDirection(moveInput);
        Vector3 moveDirection = direction;
        moveDirection.y = playerController.verticalVelocity.y;

        RunableCheck();
        float currentSpeed = canSprint ? sprintSpeed : walkSpeed;
        if (isGliding) currentSpeed = walkSpeed;

        if (moveInput == Vector2.zero)
        {
            animator.SetFloat("MotionSpeed", 1);
            animator.SetFloat("Speed", 0);
            playerController.isSprinting = false;
        }
        else
        {
            animator.SetFloat("MotionSpeed", 1);
            animator.SetFloat("Speed", currentSpeed);
            if (canSprint)
            {
                playerController.isSprinting = true;
            }
        }

        animator.SetBool("Sprint", playerController.isSprinting);

        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        playerController.UsingStamina();

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

    public void Exit()
    {

    }

    private void RunableCheck()
    {
        if (InputManager.InputActions.actions["Sprint"].IsPressed())
        {
            currentStamina = playerController.playerData.staminaCurrent;
            if (playerController.isRecovery && currentStamina > 10f)
            {
                canSprint = true;
                return;
            }
            else if (!playerController.isRecovery)
            {
                canSprint = currentStamina >= playerController.staminaUseAmount ? true : false;
                return;
            }
        }
        else
        {
            canSprint = false;
            playerController.isSprinting = false;
        }
    }
}

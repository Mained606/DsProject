using UnityEngine;

public class MoveBehaviour : IBehaviour
{
    private PlayerController playerController;
    private CharacterController characterController;
    private Transform cameraTransform;
    private Animator animator;
    private float walkSpeed;
    private float sprintSpeed;
    private bool CanSprint;
    private bool isGliding;

    private float currentStamina;

    public MoveBehaviour(CharacterController controller, Animator anim, float walk, float sprint)
    {
        characterController = controller;
        animator = anim;
        walkSpeed = walk;
        sprintSpeed = sprint;
        //CanSprint = canSprint;
        //isGliding = gliding;
        playerController = GameManager.playerTransform.GetComponent<PlayerController>();

    }

    public void Enter()
    {
        
    }

    public void Execute()
    {
        Vector2 moveInput = InputManager.InputActions.actions["Move"].ReadValue<Vector2>();
        Debug.Log($"{moveInput}");

        Vector3 direction = playerController.GetDirection(moveInput);
        Vector3 moveDirection = direction;

        float currentSpeed = playerController.CanSprint ? sprintSpeed : walkSpeed;
        if (isGliding) currentSpeed = walkSpeed;

        if (moveInput == Vector2.zero)
        {
            animator.SetFloat("MotionSpeed", 1);
            animator.SetFloat("Speed", 0);
        }
        else
        {
            animator.SetFloat("MotionSpeed", 1);
            animator.SetFloat("Speed", currentSpeed);
        }

        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        Debug.Log($"{this}에서 Execute 중");
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
                CanSprint = true;
                return;
            }
            else if (!playerController.isRecovery)
            {
                CanSprint = currentStamina >= playerController.staminaUseAmount ? true : false;
                return;
            }
        }
        else
        {
            CanSprint = false;
            playerController.isSprinting = false;
        }
    }
}

using UnityEngine;

public class DodgeBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    public DodgeBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
    }

    public void Enter()
    {
        controller.isDodging = false;
        animator.SetBool("Dodge", false);
    }

    public void Execute()
    {
        HandleDodgeInput();
    }

    public void Exit()
    {
        controller.isDodging = false;
        animator.SetBool("Dodge", false);
    }

    private void HandleDodgeInput()
    {
        if (InputManager.InputActions.actions["Dodge"].triggered && !controller.isDodging)
        {
            animator.SetBool("Dodge", true);
            controller.Dodge();
        }
    }
}

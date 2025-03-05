using UnityEngine;

public class ClimbBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    public ClimbBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
    }
    public void Enter()
    {
        controller.isClimb = false;
        animator.SetBool("Climb", false);
        animator.SetBool("ClimbUp", false);
    }

    public void Execute()
    {

    }

    public void Exit()
    {
        controller.isClimb = false;
        animator.SetBool("Climb", false);
        animator.SetBool("ClimbUp", false);
    }
}

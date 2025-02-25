using UnityEngine;

public class BlockBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    public BlockBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
    }

    public void Enter()
    {

    }
    public void Execute()
    {

    }
    public void Exit()
    {

    }
}

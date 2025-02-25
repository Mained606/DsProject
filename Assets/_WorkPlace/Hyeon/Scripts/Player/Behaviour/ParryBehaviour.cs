using UnityEngine;

public class ParryBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    public ParryBehaviour()
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

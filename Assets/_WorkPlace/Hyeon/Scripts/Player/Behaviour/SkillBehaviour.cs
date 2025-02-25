using UnityEngine;

public class SkillBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    public SkillBehaviour()
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

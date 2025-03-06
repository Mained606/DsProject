using UnityEditor;
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
        controller.isParry = false;
        animator.SetBool("Parry", false);
    }

    public void Execute()
    {
        HandleParryInput();
    }

    public void Exit()
    {
        controller.isParry = false;
        animator.SetBool("Parry", false);
    }

    private void HandleParryInput()
    {
        if (InputManager.InputActions.actions["Attack"].triggered)
        {
            controller.isParry = true;
            animator.SetBool("Parry", true);
            ParryCheck();
        }
    }

    private void ParryCheck()
    {
        AnimatorStateInfo stateInfo;
        stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float normalized = stateInfo.normalizedTime;

        if (normalized >= 0.1f && normalized <= 0.9f)
        {
            //Debug.LogWarning("onParry");
            controller.playerCombat.onParry = true;
        }
        else
        {
            controller.playerCombat.onParry = false;
        }
    }
}

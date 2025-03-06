using UnityEditor;
using UnityEngine;

public class AttackBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;
    private float attackPerceptionRange;

    public AttackBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
    }

    public void Enter()
    {
        controller.playerCombat.firstAttack = true;
        attackPerceptionRange = controller.playerCombat.attackPerceptionRange;
        animator.ResetTrigger("NextCombo");
        controller.isAttack = false;
    }

    public void Execute()
    {
        HandleAttackInput();
    }

    public void Exit()
    {
        animator.ResetTrigger("NextCombo");
        controller.isAttack = false;
    }

    private void HandleAttackInput()
    {

        if (InputManager.InputActions.actions["Attack"].triggered)
        {
            if (controller.playerCombat.hasWeapon)
            {
                if (controller.playerCombat.firstAttack)
                {
                    PerformComboAttack();
                }
            }

        }
    }

    private void PerformComboAttack()
    {
        PlayerBehaviourManager.Instance.CanBlock = false;
        PlayerBehaviourManager.Instance.CanMove = false;
        PlayerBehaviourManager.Instance.CanJump = false;
        PlayerBehaviourManager.Instance.CanUseSkill = false;

        if (controller.isSprinting)
        {
            Debug.LogWarning("대쉬공격");
            controller.DashAttack();
        }
        else
        {
            controller.playerCombat.LookEnemy(attackPerceptionRange);
        }
        controller.playerCombat.firstAttack = false;
        controller.isAttack = true;
        animator.SetTrigger("NextCombo");
    }
}

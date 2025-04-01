using UnityEditor;
using UnityEngine;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class AttackBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;
    private float attackPerceptionRange;

    private float dashAttackDuration = 0.5f;
    private float dashSpeed = 15f;
    private bool isDashAttack = false;
    private float dashTimer;

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
        dashTimer = 0f;
    }

    public void Execute()
    {
        if (!isDashAttack)
        {
            HandleAttackInput();
        }
        else
        {
            PerformDashAttack();
        }
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
                if (controller.playerCombat.firstAttack && !controller.isSprinting)
                {
                    PerformComboAttack();
                }
                else if (controller.isSprinting)
                {
                    StartDashAttack();
                }
            }
        }
    }

    private void StartDashAttack()
    {
        isDashAttack = true;
        dashTimer = 0f;
        PlayerBehaviourManager.Instance.CanDodge = false;
        BoolSwitch();
        controller.isAttack = true;
        animator.SetTrigger("NextCombo");
        animator.SetFloat("Speed", 0f);
    }

    private void PerformComboAttack()
    {
        controller.playerCombat.LookEnemy(attackPerceptionRange);
        BoolSwitch();
        controller.playerCombat.firstAttack = false;
        controller.isAttack = true;
        animator.SetTrigger("NextCombo");
        animator.SetFloat("Speed", 0f);
    }

    private void PerformDashAttack()
    {
        Vector3 dashDirection = controller.transform.forward;
        dashDirection.y = controller.verticalVelocity.y;

        if (dashTimer < dashAttackDuration)
        {
            controller.characterController.Move(dashDirection * dashSpeed * Time.deltaTime);
            dashTimer += Time.deltaTime;
        }
        else
        {
            isDashAttack = false;
            animator.ResetTrigger("NextCombo");
            PlayerBehaviourManager.Instance.CanDodge = true;
            animator.SetBool("Sprint", false);
            animator.SetFloat("Speed", 0);
        }
    }

    private void BoolSwitch()
    {
        PlayerBehaviourManager.Instance.CanBlock = false;
        PlayerBehaviourManager.Instance.CanMove = false;
        PlayerBehaviourManager.Instance.CanJump = false;
        PlayerBehaviourManager.Instance.CanUseSkill = false;
    }

    
}

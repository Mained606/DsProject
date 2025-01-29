using UnityEngine;

public class ComboAttackState : StateMachineBehaviour
{
    private PlayerCombat combat;
    private bool isPressedAttackKey = false;

    private StateComboName GetStateCombo(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsName("Attack_1")) return StateComboName.Attack1;
        if (stateInfo.IsName("Attack_2")) return StateComboName.Attack2;
        if (stateInfo.IsName("Attack_3")) return StateComboName.Attack3;
        return StateComboName.Unknown;
    }

    private void SetCombatComponent(Animator animator)
    {
        if (combat == null)
        {
            combat = animator.GetComponentInParent<PlayerCombat>();
            if (combat == null)
            {
                Debug.LogError("⚠ PlayerCombat 컴포넌트를 찾을 수 없습니다.");
            }
        }
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetCombatComponent(animator);
        animator.ResetTrigger("NextCombo");
        combat?.CurrentComboStates(GetStateCombo(stateInfo));
        isPressedAttackKey = false;

        if (!combat.weaponCollider.enabled)
        {
            combat.weaponCollider.enabled = true;
            Debug.Log("🔪 무기 콜라이더 활성화!");
        }
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetCombatComponent(animator);

        if (stateInfo.normalizedTime >= 0.5f)
        {
            if (!isPressedAttackKey && InputManager.InputActions.actions["Attack"].triggered)
            {
                Debug.LogWarning("🟢 다음 콤보로 전환 요청");
                animator.SetTrigger("NextCombo");
                isPressedAttackKey = true;
            }
        }

        InputManager.InputActions.actions["Move"].Disable();
        InputManager.InputActions.actions["Jump"].Disable();
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetCombatComponent(animator);
        AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(layerIndex);
        StateComboName nextState = GetStateCombo(nextStateInfo);
        if (animator.IsInTransition(layerIndex) && 
            (nextState == StateComboName.Attack1 || nextState == StateComboName.Attack2 || nextState == StateComboName.Attack3))
        {
            Debug.Log("🔄 트랜지션 중: 콤보 유지");
            return;
        }
        if (nextState == StateComboName.Unknown)
        {
            animator.ResetTrigger("NextCombo");
            combat?.AttackFinishedCheck();
            Debug.Log("⚔ 콤보 종료: 외부로 나감");
        }
        if (combat.weaponCollider.enabled)
        {
            combat.weaponCollider.enabled = false;
            Debug.Log("🛑 무기 콜라이더 비활성화!");
        }
        InputManager.InputActions.actions["Move"].Enable();
        InputManager.InputActions.actions["Jump"].Enable();
    }

}

public enum StateComboName { Attack1, Attack2, Attack3, Unknown };

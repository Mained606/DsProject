using UnityEngine;

public class ComboAttackState : StateMachineBehaviour
{
    private PlayerCombat combat;
    private bool isPressedAttackKey = false;
    [SerializeField] private float attackPerceptionRange = 2.5f;

    private StateComboName GetStateCombo(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsName("Attack__1")) return StateComboName.Attack1;
        if (stateInfo.IsName("Attack__2")) return StateComboName.Attack2;
        if (stateInfo.IsName("Attack__3")) return StateComboName.Attack3;
        if (stateInfo.IsName("Attack__4")) return StateComboName.Attack4;
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
        //combat?.CurrentComboStates(GetStateCombo(stateInfo));
        isPressedAttackKey = false;

        if (!combat.weaponCollider.enabled)
        {
            combat.weaponCollider.enabled = true;
            //Debug.LogWarning("🔪 무기 콜라이더 활성화!");
        }
        combat?.LookEnemy(attackPerceptionRange);
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetCombatComponent(animator);
        //Debug.LogWarning($"normalizedTime: {stateInfo.normalizedTime}");
        if (stateInfo.normalizedTime >= 0.7f)
        {
            if (!isPressedAttackKey && InputManager.InputActions.actions["Attack"].triggered)
            {
                isPressedAttackKey = true;
                //Debug.LogWarning($"🟢 다음 콤보로 전환 요청");
                animator.SetTrigger("NextCombo");
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
        //Debug.LogWarning($"nextState : {nextState}");
        if (animator.IsInTransition(layerIndex) && 
            (nextState == StateComboName.Attack1 || nextState == StateComboName.Attack2 || nextState == StateComboName.Attack3 || nextState == StateComboName.Attack4))
        {
            //Debug.LogWarning("🔄 트랜지션 중: 콤보 유지");
            return;
        }
        if (nextState == StateComboName.Unknown)
        {
            animator.ResetTrigger("NextCombo");
            combat?.AttackFinishedCheck();
            //Debug.LogWarning("⚔ 콤보 종료: 외부로 나감");
            combat.firstAttack = false;
        }
        if (combat.weaponCollider.enabled)
        {
            combat.weaponCollider.enabled = false;
            //Debug.LogWarning("🛑 무기 콜라이더 비활성화!");
        }
        InputManager.InputActions.actions["Move"].Enable();
        InputManager.InputActions.actions["Jump"].Enable();
    }

}

public enum StateComboName { Attack1, Attack2, Attack3, Attack4, Unknown };

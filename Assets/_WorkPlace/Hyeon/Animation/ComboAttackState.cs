using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

public class ComboAttackState : StateMachineBehaviour
{
    private PlayerCombat combat;
    private WeaponAttack weaponAttack;
    private bool isPressedAttackKey = false;
    private bool nextComboInput = false;
    [SerializeField] private float attackPerceptionRange = 2.5f;
    [SerializeField] private float comboTime = 0.5f;

    private List<string> swordComboSounds = new List<string> { "Sword_Swing_1", "Sword_Swing_2", "Sword_Swing_3", "Sword_Swing_4" };
    private List<string> wandComboSounds = new List<string> { "Wand_Swing_1", "Wand_Swing_3" };

    private int soundIndex = 0;
    private int swordCombo = 4;
    private int wandCombo = 2;

    private StateComboName GetStateCombo(AnimatorStateInfo stateInfo)
    {
        if (stateInfo.IsName("Base Layer.ComboAttack.Attack__1")) return StateComboName.Attack1;
        if (stateInfo.IsName("Base Layer.ComboAttack.Attack__2")) return StateComboName.Attack2;
        if (stateInfo.IsName("Base Layer.ComboAttack.Attack__3")) return StateComboName.Attack3;
        if (stateInfo.IsName("Base Layer.ComboAttack.Attack__4")) return StateComboName.Attack4;
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
            weaponAttack = combat.weapon.weaponAttack;
        }
    }

    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetCombatComponent(animator);
        weaponAttack.ResetDamagedTargets();
        //animator.ResetTrigger("NextCombo");
        //combat?.CurrentComboStates(GetStateCombo(stateInfo));
        if (!combat.weaponCollider.enabled)
        {
            combat.weaponCollider.enabled = true;
            //Debug.LogWarning("OnStateEnter");
            //Debug.LogWarning("🔪 무기 콜라이더 활성화!");
        }
        combat?.LookEnemy(attackPerceptionRange);

        //03.10 HJ 추가
        ItemSkillManager.Instance.UpdateAttack();
        PlayAttackSound();
    }

    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        SetCombatComponent(animator);

        if (stateInfo.normalizedTime >= comboTime)
        {
            if (!isPressedAttackKey && InputManager.InputActions.actions["Attack"].triggered)
            {
                isPressedAttackKey = true;
                //Debug.LogWarning($"{GetStateCombo(stateInfo)} : 🟢 다음 콤보로 전환 요청");
                nextComboInput = true;
                animator.SetTrigger("NextCombo");
            }
        }
        if(stateInfo.normalizedTime > 0.8f)
        {
            combat.weaponCollider.enabled = false;
        }
    }

    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        //SetCombatComponent(animator);
        //AnimatorStateInfo nextStateInfo = animator.GetNextAnimatorStateInfo(layerIndex);
        //StateComboName nextState = GetStateCombo(nextStateInfo);
        //Debug.LogWarning($"nextState : {nextState}");
        //if (animator.IsInTransition(layerIndex) &&
        //    (nextState == StateComboName.Attack1 || nextState == StateComboName.Attack2 || nextState == StateComboName.Attack3 || nextState == StateComboName.Attack4))
        //{
        //    Debug.LogWarning("🔄 트랜지션 중: 콤보 유지");
        //    return;
        //}
        //if (nextState == StateComboName.Unknown)
        //{
        //    animator.ResetTrigger("NextCombo");
        //    combat?.AttackFinished();
        //    
        //    //combat.firstAttack = false;
        //}
        if (nextComboInput) // TODO : 입력은 들어왔지만 마지막 콤보 공격인 경우 Trigger Reset 시켜주는 기능 필요함
        {
            isPressedAttackKey = false;
            nextComboInput = false;
            return;
        }
        else
        {
            combat?.AttackFinished();
            Debug.LogWarning("⚔ 콤보 종료: 외부로 나감");
            combat.firstAttack = true;
            if (combat.weaponCollider.enabled)
            {
                combat.weaponCollider.enabled = false;
                //Debug.LogWarning("🛑 무기 콜라이더 비활성화!");
            }
            animator.ResetTrigger("NextCombo");
            soundIndex = 0;
        }

        //SetCombatComponent(animator);
        //if (animator.IsInTransition(layerIndex))
        //{
        //    Debug.LogWarning("🔄 트랜지션 중: 콤보 유지");
        //    return;
        //}
        //StateComboName nextState = GetStateCombo(stateInfo);
        //if (nextState == StateComboName.Unknown)
        //{
        //    animator.ResetTrigger("NextCombo");
        //    combat?.AttackFinishedCheck();
        //    combat.firstAttack = false;
        //}
        //if (combat.weaponCollider.enabled) combat.weaponCollider.enabled = false;
        //InputManager.InputActions.actions["Move"].Enable();
        //InputManager.InputActions.actions["Jump"].Enable();
    }

    private void PlayAttackSound()
    {
        if (combat.physicsWeapon)
        {
            if(soundIndex >= swordCombo)
            {
                soundIndex = 0;
            }
            SoundManager.Instance.PlayClipAtPoint(swordComboSounds[soundIndex], combat.transform.position, 0.2f, false);
            soundIndex++;

        }
        else if (combat.magicalWeapon)
        {
            if (soundIndex >= wandCombo)
            {
                soundIndex = 0;
            }
            SoundManager.Instance.PlayClipAtPoint(wandComboSounds[soundIndex], combat.transform.position, 0.5f, false);
            soundIndex++;
        }
    }

}

public enum StateComboName { Attack1, Attack2, Attack3, Attack4, Unknown };

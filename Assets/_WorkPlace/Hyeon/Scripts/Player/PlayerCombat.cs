using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    private PlayerController controller;
    private PlayerBehaviourManager behaviour;
    private CharacterController characterController;
    public WeaponManager weapon;
    private PlayerData playerData;
    public Collider weaponCollider;
    [SerializeField] private float attackPerceptionRange = 2.5f;
    [SerializeField] private float skillPerceptionRange = 5f;

    public Animator playerAnimator;
    private Transform closestMonster;

    public bool CanReceiveInput { get; set; } = true;
    public bool hasWeapon;
    public bool isBlocking;
    public bool firstAttack;
    [SerializeField] private bool CanParry;
    public bool onParry;

    public Quaternion targetRotation;
    private AnimatorStateInfo stateInfo;
    public bool firstCombo = false;

    private static readonly int[] SkillStateHash = {
        Animator.StringToHash("Base Layer.Skill_1"),
        Animator.StringToHash("Base Layer.Skill_2")
    };

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        characterController = GetComponent<CharacterController>();
        //weaponCollider = currentWeapon.GetComponent<Collider>();
        playerData = controller.playerData;
        behaviour = PlayerBehaviourManager.Instance;

        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

    private void Update()
    {
        ////////////////////////////////////////////////////////////
        // if (controller.uiCheck) return;
        // controller.uiCheck UI 켜져있을때 막기위한 추가 확인조건
        /// JWS 2025.01.27 13:00 수정
        if (controller.uiCheck) return;
        ////////////////////////////////////////////////////////////

        //Debug.Log($"Current CanMove : {controller.CanMove}");
        if (controller.isParry)
        {
            ParryCheck();
        }

        HandleAttackInput();
        HandleSkillInput();
        HandleBlockInput();

        SkillFinishedCheck();
        ParryFinishedCheck();
    }

    private void HandleAttackInput()
    {

        if (InputManager.InputActions.actions["Attack"].triggered)
        {
            if (isBlocking && CanParry && hasWeapon)
            {
                isBlocking = false;
                OnParry();
                return;
            }
            if (CanReceiveInput && hasWeapon)
            {
                if (!firstAttack)
                {
                    PerformComboAttack();
                }
            }
            
        }
    }

    //private void AttackFinishedCheck()
    //{
    //    AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
    //    float normalizedTime = stateInfo.normalizedTime;

    //    if (normalizedTime >= 0.95f)
    //    {
    //        Debug.LogWarning("콤보 끝");
    //        controller.CanMove = true;
    //        controller.isAttack = false;
    //        firstAttack = false;
    //    }
    //}

    //
    private void PerformComboAttack()
    {
        if (controller.isSprinting)
        {
            //Debug.LogWarning("대쉬공격");
            StartCoroutine(controller.DashAttack());
            // 대쉬공격
        }
        else
        {
            LookEnemy(attackPerceptionRange);
        }
        firstAttack = true;
        controller.isAttack = true;
        playerAnimator.SetTrigger("NextCombo");
        
    }

    // 현재 콤보진행상태 받아보는 함수 의미 없음.
    public void CurrentComboStates(StateComboName stateComboName)
    {
        Debug.LogWarning(stateComboName.ToString());
    }

    // 콤보 끝났을때 받는 함수.
    public void AttackFinished()
    {
        //Debug.LogWarning("콤보종료함");
        behaviour.CanMove = true;
        controller.isAttack = false;
    }
    ////////////////////////////////////////////////////////////

    private void HandleSkillInput()
    {
        GameObject closestMonster = GetClosestMonster(skillPerceptionRange)?.gameObject;

        int skillCounts = SkillManager.SkillDatabase.playerSkills.Count;

        for (int i = 0; i < skillCounts; i++)
        {
            string skillName = SkillManager.SkillDatabase.playerSkills[i].skillName;
            string skillTriggerName = Enum.GetName(typeof(SkillTriggerName), i);
            if (string.IsNullOrEmpty(skillTriggerName)) continue;
            if (InputManager.InputActions.actions[skillTriggerName].triggered && hasWeapon)
            {
                behaviour.CanMove = false;
                behaviour.CanAttack = false;
     
                if (SkillManager.Instance.CheckMana(EntityType.Player, skillName) &&
                    SkillManager.Instance.CanActivateSkill(EntityType.Player, skillName))
                {
                    controller.isUseSkill = true;
                    SkillManager.Instance.ActivateSkillForEntity(EntityType.Player, skillName);
                }
                else
                {
                    Debug.Log($"스킬 사용 불가: {skillName} (마나 부족 또는 쿨타임 중)");
                }
            }
        }
    }

    private enum SkillTriggerName
    {
        PlayerSkill_1,
        PlayerSkill_2,
        PlayerSkill_3,
        PlayerSkill_4,
    }

    private void HandleBlockInput()
    {
        if (InputManager.InputActions.actions["Block"].IsPressed() && behaviour.CanBlock && hasWeapon)
        {
            isBlocking = true;
            CanParry = true;
        }
        else
        {
            isBlocking = false;
            CanParry = false;
        }
        playerAnimator.SetBool("Block", isBlocking);
    }

    private void OnParry()
    {
        controller.isParry = true;
        playerAnimator.SetBool("Parry", true);
        //ParryCheck();
    }
    private void ParryCheck()
    {
        InputManager.InputActions.actions["Block"].Disable();
        isBlocking = false;
        stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        float normalized = stateInfo.normalizedTime;

        if (normalized >= 0.1f && normalized <= 0.9f)
        {
            //Debug.LogWarning("onParry");
            onParry = true;
        }
        else
        {
            onParry = false;
        }
    }

    // 스킬 애니메이션이 끝났는지 검사
    private void SkillFinishedCheck()
    {
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        if (SkillStateHash.Contains(stateInfo.fullPathHash))
        {
            float normalizedTime = stateInfo.normalizedTime;

            if (normalizedTime >= 0.95f)
            {
                controller.isUseSkill = false;
            }
        }
    }

    private void ParryFinishedCheck()
    {
        if (controller.isParry)
        {
            if (controller.AnimFinishCheck())
            {
                controller.isParry = false;
                onParry = false;
                playerAnimator.SetBool("Parry", false);
            }
        }
    }

    private Transform GetClosestMonster(float perceptionRange)
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, perceptionRange);
        Transform closestTransform = null;
        float closestDistance = Mathf.Infinity;

        foreach(Collider collider in colliders)
        {
            if (collider.CompareTag("Monster"))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTransform = collider.transform;
                }
            }
        }
        return closestTransform;
    }

    public void LookEnemy(float perceptionRange)
    {
        closestMonster = GetClosestMonster(perceptionRange);
        if (closestMonster == null) { return; }
        Vector3 dir = (closestMonster.position - transform.position).normalized;
        dir.y = 0f;
        targetRotation = Quaternion.LookRotation(dir);
        controller.transform.rotation = targetRotation;
    }

    //private void OnDrawGizmosSelected()
    //{
    //    Gizmos.color = Color.yellow;
    //    Gizmos.DrawWireSphere(transform.position, attackPerceptionRange);
    //}
}

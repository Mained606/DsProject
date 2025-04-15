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
    public WeaponManager weapon;
    public Collider weaponCollider;
    public float attackPerceptionRange = 2.5f;
    //[SerializeField] private float skillPerceptionRange = 5f;

    public Animator playerAnimator;
    private Transform closestMonster;

    public bool CanReceiveInput { get; set; } = true;
    public bool hasWeapon;
    public bool physicsWeapon;
    public bool magicalWeapon;
    public bool isBlocking;
    public bool firstAttack;
    [SerializeField] private bool CanParry;
    public bool onParry;

    public Quaternion targetRotation;

    private static readonly int[] SkillStateHash = {
        Animator.StringToHash("Base Layer.Skills.UpperAttack"),
        Animator.StringToHash("Base Layer.Skills.DownCut"),
        Animator.StringToHash("Base Layer.Skills.BlazeScatter"),
        Animator.StringToHash("Base Layer.Skills.FireStrike"),
        Animator.StringToHash("Base Layer.Skills.GlacierSpear"),
        Animator.StringToHash("Base Layer.Skills.ElectroComet"),
        Animator.StringToHash("Base Layer.Skills.TerraSurge"),
        Animator.StringToHash("Base Layer.Skills.UltimateDragon_end"),
    };

    private void Start()
    {
        controller = GetComponent<PlayerController>();
        behaviour = PlayerBehaviourManager.Instance;
        firstAttack = true;

        if (weaponCollider != null)
        {
            weaponCollider.enabled = false;
        }
    }

    private void Update()
    {
        SkillFinishedCheck();
        ParryFinishedCheck();

        if (UIManager.Instance.IsUIWindowOpen()) return;
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
        controller.isAttack = false;
        if (!controller.isDodging)
        {
            behaviour.CanMove = true;
            behaviour.CanJump = true;
            behaviour.CanUseSkill = true;
            behaviour.CanBlock = true;
        }
    }

    // 스킬 애니메이션이 끝났는지 검사
    private void SkillFinishedCheck()
    {
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        if (SkillStateHash.Contains(stateInfo.fullPathHash))
        {
            float normalizedTime = stateInfo.normalizedTime;

            if (normalizedTime >= 0.93f)
            {
                PlayerBehaviourManager.Instance.CanMove = true;
                PlayerBehaviourManager.Instance.CanJump = true;
                PlayerBehaviourManager.Instance.CanAttack = true;
                PlayerBehaviourManager.Instance.CanDodge = true;
                PlayerBehaviourManager.Instance.CanBlock = true;
                controller.isUseSkill = false;
                playerAnimator.SetBool("IsUseSkill", false);
            }
        }
    }

    private void ParryFinishedCheck()
    {
        if (controller.isParry)
        {
            AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
            if (stateInfo.IsName("Parry") && stateInfo.normalizedTime >= 0.95)
            {
                controller.isParry = false;
                onParry = false;
                playerAnimator.SetBool("Parry", false);
                PlayerBehaviourManager.Instance.CanBlock = true;
                PlayerBehaviourManager.Instance.CanAttack = true;
            }
        }
    }

    public Transform GetClosestMonster(float perceptionRange)
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
}

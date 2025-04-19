using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class SkillBehaviour : IBehaviour
{
    private PlayerController controller;
    private PlayerCombat combat;
    private Animator animator;

    private string skillName = "";
    private Skills skill;
    private int useSkillIndex = -1;

    private float skillPerceptionRange = 15f;   // 추후 SkillList 스키마 추가 가능성 있음
    private GameObject defaultSkillPosition;

    public SkillBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        combat = controller.GetComponent<PlayerCombat>();
        animator = controller.PlayerAnimator;
        defaultSkillPosition = controller.transform.GetChild(1).gameObject;
    }

    public void Enter()
    {
        controller.isUseSkill = false;
        animator.SetBool("IsUseSkill", false);
        skillName = "";
    }
    public void Execute()
    {
        if (controller.playerCombat.hasWeapon)
        {
            HandleSkillInput();
        }
    }
    public void Exit()
    {
        controller.isUseSkill = false;
        animator.SetBool("IsUseSkill", false);
        skillName = "";
    }

    private void HandleSkillInput()
    {
        //GameObject closestMonster = controller.playerCombat.GetClosestMonster(skillPerceptionRange)?.gameObject;

        int skillCounts = SkillManager.SkillDatabase.playerSkills.Count;

        if (InputManager.InputActions.actions["PlayerSkill_1"].triggered)
        {
            useSkillIndex = 0;
            ReadyToSkill();
        }
        else if (InputManager.InputActions.actions["PlayerSkill_2"].triggered)
        {
            useSkillIndex = 1;
            ReadyToSkill();
        }
        else if (InputManager.InputActions.actions["PlayerSkill_3"].triggered)
        {
            useSkillIndex = 2;
            ReadyToSkill();
        }
        else if (InputManager.InputActions.actions["PlayerSkill_4"].triggered)
        {
            useSkillIndex = 3;
            ReadyToSkill();
        }

        //for (int i = 0; i < skillCounts; i++)
        //{
        //    //string skillName = SkillManager.SkillDatabase.playerSkills[i].skillName;
        //    //string skillName = UIManager.SkillsQuickSlot.registedSkillList[i].skillName;
        //    //string skillTriggerName = Enum.GetName(typeof(SkillTriggerName), i);
        //    //if (string.IsNullOrEmpty(skillTriggerName)) continue;
        //    if (InputManager.InputActions.actions[skillTriggerName].triggered && controller.playerCombat.hasWeapon)
        //    {
        //        if (SkillManager.Instance.CheckMana(EntityType.Player, skillName) &&
        //            SkillManager.Instance.CanActivateSkill(EntityType.Player, skillName))
        //        {
        //            PlayerBehaviourManager.Instance.CanMove = false;
        //            PlayerBehaviourManager.Instance.CanAttack = false;
        //            controller.isUseSkill = true;
        //            SkillManager.Instance.ActivateSkillForEntity(EntityType.Player, skillName);
        //        }
        //        else
        //        {
        //            //Debug.Log($"스킬 사용 불가: {skillName} (마나 부족 또는 쿨타임 중)");
        //        }
        //    }
        //}
    }

    private void ReadyToSkill()
    {
        if (UIManager.SkillsQuickSlot.registedSkillList.Count > useSkillIndex)
        {
            if (UIManager.SkillsQuickSlot.registedSkillList[useSkillIndex] != null)
            {
                skillName = UIManager.SkillsQuickSlot.registedSkillList[useSkillIndex].skillName;
            }
            else
            {
                //Debug.Log($"{useSkillIndex + 1}번 퀵슬롯에 스킬이 등록되지 않았습니다.");
                return;
            }
        }
        else
        {
            return;
        }

        if (skillName != null)
        {
            skill = SkillManager.Instance.GetSkill(EntityType.Player, skillName);
            if(skill != null)
            {
                if (!SkillManager.Instance.CheckMana(EntityType.Player, skillName))
                {
                    UIManager.SystemGameMessage($"스킬 사용에 필요한 마나가 부족합니다. 필요 마나 : {skill.currentEnergyCost}, 현재 마나 : {controller.playerData.currentMp}", MessageTag.스킬_사용);
                    return;
                }
                if(!SkillManager.Instance.CanActivateSkill(EntityType.Player, skillName))
                {
                    UIManager.SystemGameMessage($"{skillName} 스킬 쿨다운 중", MessageTag.스킬_사용);
                    return;
                }
                if (!SkillManager.Instance.CheckWeaponType(skillName))
                {
                    UIManager.SystemGameMessage($"{skillName} 스킬은 {skill.skillType} 무기로만 사용 가능합니다.", MessageTag.스킬_사용);
                    return;
                }

                PlayerBehaviourManager.Instance.CanMove = false;
                PlayerBehaviourManager.Instance.CanAttack = false;
                PlayerBehaviourManager.Instance.CanDodge = false;
                PlayerBehaviourManager.Instance.CanJump = false;
                PlayerBehaviourManager.Instance.CanBlock = false;
                controller.isUseSkill = true;
                animator.SetBool("IsUseSkill", true);
                animator.SetFloat("Speed", 0f);

                if (skill.targeting)
                {
                    Transform closestMonster = combat.GetClosestMonster(skillPerceptionRange);
                    if (closestMonster != null)
                    {
                        SkillManager.Instance.ActivateSkillForEntity(EntityType.Player, skillName, closestMonster.gameObject);
                    }
                    else
                    {
                        SkillManager.Instance.ActivateSkillForEntity(EntityType.Player, skillName, defaultSkillPosition);
                    }
                }
                else
                {
                    SkillManager.Instance.ActivateSkillForEntity(EntityType.Player, skillName);
                }
                if (skill.cooldownTimer != null)
                    TimerManager.Instance.StartTimer(skill.cooldownTimer);
            }
        }
    }

    private enum SkillTriggerName
    {
        PlayerSkill_1 = 1,
        PlayerSkill_2 = 2,
        PlayerSkill_3 = 3,
        PlayerSkill_4 = 4,
    }
}

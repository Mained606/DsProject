using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class SkillBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    private string skillName = "";
    private int useSkillIndex = -1;

    //private float skillPerceptionRange = 5f;

    public SkillBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
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
        //            Debug.Log($"스킬 사용 불가: {skillName} (마나 부족 또는 쿨타임 중)");
        //        }
        //    }
        //}
    }

    private void ReadyToSkill()
    {
        if (UIManager.SkillsQuickSlot.registedSkillList.Count > useSkillIndex)
        {
            skillName = UIManager.SkillsQuickSlot.registedSkillList[useSkillIndex].skillName;
        }
        else
        {
            return;
        }

        if (skillName != null)
        {
            if (SkillManager.Instance.CheckMana(EntityType.Player, skillName) &&
                   SkillManager.Instance.CanActivateSkill(EntityType.Player, skillName) &&
                   SkillManager.Instance.CheckWeaponType(skillName))
            {
                PlayerBehaviourManager.Instance.CanMove = false;
                PlayerBehaviourManager.Instance.CanAttack = false;
                PlayerBehaviourManager.Instance.CanDodge = false;
                PlayerBehaviourManager.Instance.CanJump = false;
                PlayerBehaviourManager.Instance.CanBlock = false;
                controller.isUseSkill = true;
                animator.SetBool("IsUseSkill", true);
                SkillManager.Instance.ActivateSkillForEntity(EntityType.Player, skillName);
            }
            else
            {
                Debug.Log($"스킬 사용 불가: {skillName} (마나 부족 또는 쿨타임 중)");
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

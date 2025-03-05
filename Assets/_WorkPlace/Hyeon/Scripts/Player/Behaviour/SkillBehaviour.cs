using System;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class SkillBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    //private float skillPerceptionRange = 5f;

    public SkillBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
    }

    public void Enter()
    {
        controller.isUseSkill = false;
    }
    public void Execute()
    {
        HandleSkillInput();
    }
    public void Exit()
    {
        controller.isUseSkill = false;
    }

    private void HandleSkillInput()
    {
        //GameObject closestMonster = controller.playerCombat.GetClosestMonster(skillPerceptionRange)?.gameObject;

        int skillCounts = SkillManager.SkillDatabase.playerSkills.Count;

        for (int i = 0; i < skillCounts; i++)
        {
            string skillName = SkillManager.SkillDatabase.playerSkills[i].skillName;
            string skillTriggerName = Enum.GetName(typeof(SkillTriggerName), i);
            if (string.IsNullOrEmpty(skillTriggerName)) continue;
            if (InputManager.InputActions.actions[skillTriggerName].triggered && controller.playerCombat.hasWeapon)
            {
                if (SkillManager.Instance.CheckMana(EntityType.Player, skillName) &&
                    SkillManager.Instance.CanActivateSkill(EntityType.Player, skillName))
                {
                    PlayerBehaviourManager.Instance.CanMove = false;
                    PlayerBehaviourManager.Instance.CanAttack = false;
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
}

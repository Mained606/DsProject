using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSkill : MonoBehaviour
{
    public Animator skillAnimator;
    public PlayerController controller;

    private static readonly int[] SkillStateHash = {
        Animator.StringToHash("Base Layer.Skill_1"),
        Animator.StringToHash("Base Layer.Skill_2")
    };

    private void Update()
    {
        if (InputManager.InputActions.actions["PlayerSkill_1"].triggered)
        {
            Skill_1();
        }
        if (InputManager.InputActions.actions["PlayerSkill_2"].triggered)
        {
            Skill_2();
        }
        if (InputManager.InputActions.actions["PlayerSkill_3"].triggered)
        {
            Skill_3();
        }
        SkillFinishedCheck();
    }

    private void Skill_1()
    {
        controller.CanMove = false;
        InputManager.InputActions.actions["Attack"].Disable();  // 미리 트리거 입력돼서 스킬 끝나고 평타 나가는 거 방지
        skillAnimator.SetTrigger("Skill_1");
    }

    private void Skill_2()
    {
        controller.CanMove = false;
        InputManager.InputActions.actions["Attack"].Disable();  // 미리 트리거 입력돼서 스킬 끝나고 평타 나가는 거 방지
        skillAnimator.SetTrigger("Skill_1");
    }

    private void Skill_3()
    {
        controller.CanMove = false;
        InputManager.InputActions.actions["Attack"].Disable();  // 미리 트리거 입력돼서 스킬 끝나고 평타 나가는 거 방지
        skillAnimator.SetTrigger("Skill_1");
    }

    // 스킬 애니메이션이 끝났는지 검사
    private void SkillFinishedCheck()
    {
        AnimatorStateInfo stateInfo = skillAnimator.GetCurrentAnimatorStateInfo(0);
        if (SkillStateHash.Contains(stateInfo.fullPathHash))
        {
            float normalizedTime = stateInfo.normalizedTime;

            if (normalizedTime >= 0.95f)
            {
                controller.CanMove = true;
                InputManager.InputActions.actions["Attack"].Enable();   // 막았던 평타 키 활성화
                Debug.Log("CanMove True");
            }
        }
    }
}

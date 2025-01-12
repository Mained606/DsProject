using UnityEngine;

public class PlayerSkill : MonoBehaviour
{
    public Animator skillAnimator;


    private void Update()
    {
        if (InputManager.InputActions.actions["PlayerSkill_1"].triggered)
        {
            Skill_1();
        }
    }
    private void Skill_1()
    {
        skillAnimator.SetTrigger("Skill_1");
    }
}

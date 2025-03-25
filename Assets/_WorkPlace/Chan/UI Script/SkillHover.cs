using UnityEngine.EventSystems;
using UnityEngine;

public class SkillHover : MonoBehaviour, IPointerEnterHandler
{
    private Skills skillData;
    private SkillUI skillUI;

    public void Initialize(Skills skill, SkillUI ui)
    {
        skillData = skill;
        skillUI = ui;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (skillData == null || skillUI == null) return;
        skillUI.ShowSkillInfo(skillData);
    }
}
using UnityEngine;

public class SkillQuickSlotUI : MonoBehaviour
{
    [SerializeField] private SkillSlot[] quickSlots; // 슬롯 3개

    private void Start()
    {
        // 슬롯 초기화
        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].Initialize(i);
        }
    }

    public void AssignSkillToSlot(Skills skill, Sprite icon, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlots.Length) return;
        quickSlots[slotIndex].SetSkill(skill, icon);
    }
}

using UnityEngine;
using System.Collections.Generic;

public class SkillQuickSlotUI : MonoBehaviour
{
    public static SkillQuickSlotUI Instance { get; private set; }

    [SerializeField] private SkillSlot[] quickSlots;

    public List<Skills> registedSkillList = new List<Skills>(); // 🔥 SkillBehaviour용 공개 리스트

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].Initialize(i);
        }

        UpdateRegistedList();
    }

    public void AssignSkillToSlot(Skills skill, Sprite icon, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlots.Length) return;

        RemoveSkillIfExists(skill); // 기존 슬롯에서 제거
        quickSlots[slotIndex].SetSkill(skill, icon);

        UpdateRegistedList(); // 🧠 동기화
    }

    public void RemoveSkillIfExists(Skills skill)
    {
        foreach (var slot in quickSlots)
        {
            if (slot.GetAssignedSkill() == skill)
            {
                slot.ClearSlot();
                break;
            }
        }

        UpdateRegistedList(); // 🧠 동기화
    }

    private void UpdateRegistedList()
    {
        registedSkillList.Clear();
        foreach (var slot in quickSlots)
        {
            var skill = slot.GetAssignedSkill();
            registedSkillList.Add(skill); // null도 포함 (자리 보존용)
        }
    }
}

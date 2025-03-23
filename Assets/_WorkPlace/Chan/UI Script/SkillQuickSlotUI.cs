using UnityEngine;
using System.Collections.Generic;

public class SkillQuickSlotUI : MonoBehaviour
{
    public static SkillQuickSlotUI Instance { get; private set; }

    [SerializeField] private SkillSlot[] quickSlots;

    public List<Skills> registedSkillList = new List<Skills>(); // 진짜 등록된 스킬만 담김

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        for (int i = 0; i < quickSlots.Length; i++)
        {
            quickSlots[i].Initialize(i);
            quickSlots[i].ClearSlot(); // 시작 시 슬롯 완전 초기화
        }

        UpdateRegistedList();
    }

    public void AssignSkillToSlot(Skills skill, Sprite icon, int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= quickSlots.Length) return;

        RemoveSkillIfExists(skill); // 중복 방지
        quickSlots[slotIndex].SetSkill(skill, icon);

        UpdateRegistedList(); // 리스트 동기화
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

        UpdateRegistedList(); // 리스트 동기화
    }

    private void UpdateRegistedList()
    {
        registedSkillList.Clear();

        for (int i = 0; i < quickSlots.Length; i++)
        {
            var skill = quickSlots[i].GetAssignedSkill();
            registedSkillList.Add(skill); // 슬롯 인덱스를 그대로 리스트 인덱스로 반영
        }
    }
}

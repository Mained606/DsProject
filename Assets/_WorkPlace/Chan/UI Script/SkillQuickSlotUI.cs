using UnityEngine;
using System.Collections.Generic;

public class SkillQuickSlotUI : MonoBehaviour
{
    public static SkillQuickSlotUI Instance { get; private set; }

    [SerializeField] private SkillSlot[] quickSlots;

    public List<Skills> registedSkillList = new List<Skills>(); 

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

    // 모든 스킬 슬롯 초기화
    public void ClearAllSlots()
    {
        foreach (var slot in quickSlots)
        {
            slot.ClearSlot();
        }
        
        UpdateRegistedList(); // 리스트 동기화
    }
    
    // 퀵슬롯 초기화 후 저장된 스킬 정보로 재설정
    public void InitializeFromSaveData(List<string> savedSkillNames)
    {
        if (savedSkillNames == null || savedSkillNames.Count == 0)
        {
            Debug.LogWarning("[SkillQuickSlotUI] 저장된 스킬 퀵슬롯 데이터가 없습니다.");
            return;
        }
        
        // 모든 슬롯 초기화
        ClearAllSlots();
        
        Debug.Log($"[SkillQuickSlotUI] 스킬 퀵슬롯 초기화 (총 {savedSkillNames.Count}개 슬롯)");
        
        // 저장된 슬롯 정보에 따라 스킬 할당
        for (int i = 0; i < Mathf.Min(savedSkillNames.Count, quickSlots.Length); i++)
        {
            string skillName = savedSkillNames[i];
            
            // 빈 슬롯이 아닐 경우에만 처리
            if (!string.IsNullOrEmpty(skillName))
            {
                // SkillManager에서 해당 스킬 찾기
                Skills skill = SkillManager.Instance.GetSkill(EntityType.Player, skillName);
                
                if (skill != null)
                {
                    // 스킬 아이콘 가져오기
                    Sprite icon = ItemManager.Instance.GetSkillSprite(skillName);
                    
                    // 퀵슬롯에 스킬 할당
                    AssignSkillToSlot(skill, icon, i);
                    Debug.Log($"[SkillQuickSlotUI] 스킬 '{skillName}'을 퀵슬롯 {i}에 복원했습니다.");
                }
                else
                {
                    Debug.LogWarning($"[SkillQuickSlotUI] 저장된 스킬 '{skillName}'을 찾을 수 없습니다.");
                }
            }
        }
    }
    
    // 모든 스킬 아이콘을 새로 고치는 메서드
    public void RefreshAllSkillIcons()
    {
        Debug.Log("[SkillQuickSlotUI] 모든 스킬 아이콘 새로고침 시작");
        
        for (int i = 0; i < quickSlots.Length; i++)
        {
            var skill = quickSlots[i].GetAssignedSkill();
            if (skill != null)
            {
                // 스프라이트 새로 가져오기
                Sprite icon = ItemManager.Instance.GetSkillSprite(skill.skillName);
                if (icon != null)
                {
                    // 아이콘 갱신
                    quickSlots[i].UpdateIcon(icon);
                    Debug.Log($"[SkillQuickSlotUI] 스킬 '{skill.skillName}'의 아이콘을 새로고침했습니다.");
                }
                else
                {
                    Debug.LogWarning($"[SkillQuickSlotUI] 스킬 '{skill.skillName}'의 아이콘을 찾을 수 없습니다.");
                }
            }
        }
        
        Debug.Log("[SkillQuickSlotUI] 모든 스킬 아이콘 새로고침 완료");
    }
}

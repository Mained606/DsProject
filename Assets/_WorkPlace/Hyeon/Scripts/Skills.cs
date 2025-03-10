using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Skills : ISheetData
{
    public EntityType entityType;
    public SkillType skillType;
    public string skillName;
    public float damage;
    public float cooldown;
    public float energyCost;
    public GameObject effectPrefab;
    public string activeTriggerName;
    public float particleDelay;
    
    public float animationDuration;
    public float buffDuration;
    
    public BasicTimer cooldownTimer;
    
    // 스킬 레벨 관련 추가 필드
    public int skillLevel = 1;
    public int maxSkillLevel = 10; // 최대 레벨은 필요에 따라 조정 가능

    public Skills() { }

    public void Initialize()
    {
        cooldownTimer = new BasicTimer(cooldown);
        skillLevel = 1;
    }
    
    public float GetSkillDuration()
    {
        // 애니메이션 길이 또는 기본 지속 시간을 반환
        return animationDuration > 0 ? animationDuration : 1.0f;
    }
    
    public void LevelUp()
    {
        if (skillLevel < maxSkillLevel)
        {
            skillLevel++;
            damage *= 1.1f; // 스킬 피해량 10% 증가
        }
    }

    public void ParseData(IList<object> row)
    {
        if (row.Count < 10) throw new Exception("스킬 데이터 부족");

        entityType = Enum.TryParse(row[0].ToString(), out EntityType parsedEntity) ? parsedEntity : EntityType.Player;
        skillType = Enum.TryParse(row[1].ToString(), out SkillType parsedSkill) ? parsedSkill : SkillType.Physical;
        skillName = row[2].ToString();

        damage = float.TryParse(row[3].ToString(), out float dmg) ? dmg : 0f;
        cooldown = float.TryParse(row[4].ToString(), out float cd) ? cd : 1f;
        energyCost = float.TryParse(row[5].ToString(), out float cost) ? cost : 0f;

        activeTriggerName = row[6].ToString();
        particleDelay = float.TryParse(row[7].ToString(), out float delay) ? delay : 0f;
        animationDuration = float.TryParse(row[8].ToString(), out float animDuration) ? animDuration : 1f;
        buffDuration = float.TryParse(row[9].ToString(), out float buffDur) ? buffDur : 0f;

        // 스킬 레벨 관련
        skillLevel = int.TryParse(row[10].ToString(), out int level) ? level : 1;
        maxSkillLevel = int.TryParse(row[11].ToString(), out int maxLevel) ? maxLevel : 10;

        // 이펙트 프리팹 로드 (Resources 폴더에서 불러오기)
        string effectPrefabName = row[12].ToString();
        if (!string.IsNullOrEmpty(effectPrefabName))
        {
            effectPrefab = Resources.Load<GameObject>($"Effects/{effectPrefabName}");
            if (effectPrefab == null)
                Debug.LogWarning($"[Skills] '{effectPrefabName}' 이펙트 프리팹을 찾을 수 없습니다.");
        }

        // 쿨다운 타이머 초기화
        cooldownTimer = new BasicTimer(cooldown);

        Debug.Log($"[Skills] {skillName} 데이터 로드 완료!");
    }
}

public enum SkillType
{
    Physical,
    Magic,
    Support
}
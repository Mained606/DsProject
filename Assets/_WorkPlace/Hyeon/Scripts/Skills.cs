using System;
using UnityEngine;

[Serializable]
public class Skills
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

    public void Initialize()
    {
        cooldownTimer = new BasicTimer(cooldown);
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
            damage *= 1.1f; // 스킬 피해량 10% 증가 (예시)
        }
    }
}

public enum SkillType
{
    Physical,
    Magic,
    Support
}
using System;
using UnityEngine;

[Serializable]
public class Skills
{
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

    public void Initialize()
    {
        cooldownTimer = new BasicTimer(cooldown);
    }
    
    public float GetSkillDuration()
    {
        // 애니메이션 길이 또는 기본 지속 시간을 반환
        return animationDuration > 0 ? animationDuration : 1.0f;
    }
}

public enum SkillType
{
    Physical,
    Magic,
    Support
}
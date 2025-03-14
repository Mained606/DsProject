using System;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public class Skills : ISheetData
{
    // 스킬 기본 정보
    public EntityType entityType;
    public SkillType skillType;
    public ElementalAttribute attribute;
    public string skillName;
    public float currentDamage;
    public float damage;
    public float cooldown;
    public float energyCost;
    public GameObject effectPrefab;
    public string activeTriggerName;
    public float particleDelay;

    public float animationDuration;
    public float buffDuration;
    public BasicTimer cooldownTimer;

    // 스킬 레벨 관련 필드
    public int skillLevel = 1;
    public int maxSkillLevel = 10; // 최대 레벨 (필요에 따라 조정 가능)
    public float currentExperience; // 현재 경험치
    public float experienceToLevelUp; // 레벨업에 필요한 경험치
    private bool isLevelingUp = false;

    // 스킬 잠금 해제 확인
    public bool unLockSkill = false;
    public bool targeting = false;

    // 초기화: cooldownTimer 설정 및 초기 경험치 계산
    public void Initialize()
    {
        cooldownTimer = new BasicTimer(cooldown);
        currentDamage = damage;
        skillLevel = 1;
        currentExperience = 0;
        experienceToLevelUp = CalculateExperienceToLevelUp();
    }

    // 스킬 지속시간: 애니메이션 길이가 있다면 해당 길이 반환, 없으면 기본값 1.0f
    public float GetSkillDuration()
    {
        return animationDuration > 0 ? animationDuration : 1.0f;
    }

    // 스킬 레벨 증가 (damage 10% 증가)
    public void LevelUp()
    {
        if (!unLockSkill)
            return; // 스킬이 잠겨있으면 레벨업 불가

        if (skillLevel < maxSkillLevel)
        {
            isLevelingUp = true; // 레벨업 시작
            skillLevel++;
            currentDamage *= 1.1f; // 스킬 피해량 10% 증가
            experienceToLevelUp = CalculateExperienceToLevelUp();
            isLevelingUp = false; // 레벨업 종료
        }
    }

    // 스킬 레벨 감소 (damage 10% 감소; 최소 1레벨까지)
    public void LevelDown()
    {
        if (!unLockSkill)
            return; // 스킬이 잠겨있으면 레벨다운 불가

        if (skillLevel > 1)
        {
            skillLevel--;
            currentDamage /= 1.1f; // 스킬 피해량 원래대로 복귀
        }
    }

    // 경험치를 추가하고 레벨업 조건 만족 시 처리
    public void AddExperience(int amount)
    {
        if (amount < 0)
            return;

        currentExperience += amount;

        if (currentExperience >= experienceToLevelUp)
        {
            bool limitedSkillLevel = CharacterManager.PlayerCharacterData.level / 10 > skillLevel;
            if (!isLevelingUp)
            {
                currentExperience = limitedSkillLevel ? currentExperience - experienceToLevelUp : currentExperience = experienceToLevelUp;
                if (limitedSkillLevel) LevelUp();
            }
        }
    }

    // 레벨업에 필요한 경험치 계산 함수
    private int CalculateExperienceToLevelUp()
    {
        return Mathf.RoundToInt(skillLevel * 100f);
    }

    // 아이템이나 포인트로 올릴 수 있는 스킬 레벨 상한 제한 추가 필요
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

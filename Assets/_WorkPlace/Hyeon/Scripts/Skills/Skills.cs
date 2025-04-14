using System;
using System.Collections.Generic;

using UnityEngine;

[Serializable]
public class Skills : ISheetData
{
    [Header("기본 정보")]
    // 스킬 기본 정보
    public EntityType entityType;
    public SkillType skillType;
    public ElementalAttribute attribute;
    public string skillName;
    public float damage;
    public float cooldown;
    public float energyCost;
    public GameObject effectPrefab;
    public string activeTriggerName;
    public float particleDelay;

    public float animationDuration;
    public float buffDuration;
    public float buffValue;
    public BasicTimer cooldownTimer;
    
    // 스킬 레벨 관련 필드
    public int skillLevel = 1;
    public int maxSkillLevel = 10; // 최대 레벨 (필요에 따라 조정 가능)
    public float currentExperience; // 현재 경험치
    public float experienceToLevelUp; // 레벨업에 필요한 경험치
    private bool isLevelingUp = false;

    // 스킬 잠금 해제 확인
    public bool unLockSkill = false;
    public bool targeting = false;  // 스킬 타게팅 여부

    public float debuffDuration;        // 디버프 지속시간
    public float debuffValue;           // 디버프 효과 수치 (화상 데미지, 이동속도 감소율 등)

    public float effectDuration;    // 이펙트 지속시간 (ex 장판형)

    // ========== 250320 SH 추가 ========== 스킬 레벨에 따른 현재 스탯
    [Header("현재 스탯")]
    public float currentDamage;
    public float currentCooldown;
    public float currentEnergyCost;
    public GameObject currentEffectPrefab;
    public float currentBuffDuration;
    public float currentBuffValue;
    public float currentDebuffDuration;
    public float currentDebuffValue;

    private SkillWeights skillWeight;
    
    // 초기화: cooldownTimer 설정 및 초기 경험치 계산
    public void Initialize()
    {
        // TestFunc() 호출 대신 초기화 로직을 여기로 통합
        // 현재 값 설정
        currentDamage = damage;
        currentCooldown = cooldown;
        currentEnergyCost = energyCost;
        currentEffectPrefab = effectPrefab;
        currentBuffDuration = buffDuration;
        currentBuffValue = buffValue;
        currentDebuffDuration = debuffDuration;
        currentDebuffValue = debuffValue;
        
        // 쿨다운 타이머 초기화
        cooldownTimer = new BasicTimer(currentCooldown);
        
        // 스킬 레벨 초기화
        skillLevel = 1;
        currentExperience = 0;
        experienceToLevelUp = CalculateExperienceToLevelUp();
        
        // 디버그 로그
        string skillTypeStr = skillType == SkillType.Support ? "버프" : skillType.ToString();
        Debug.Log($"[Skills:{skillName}] 초기화 - 유형:{skillTypeStr}, 데미지:{damage}, 쿨타임:{cooldown}초, 지속시간:{buffDuration}초");
    }

    // 스킬 지속시간: 애니메이션 길이가 있다면 해당 길이 반환, 없으면 기본값 1.0f
    public float GetSkillDuration()
    {
        return animationDuration > 0 ? animationDuration : 1.0f;
    }

    // 스킬 레벨 증가 (damage 10% 증가)
    public void LevelUp(bool forceLevelUp = false)
    {
        if (!unLockSkill)
            return; // 스킬이 잠겨있으면 레벨업 불가

        if (skillLevel < maxSkillLevel || forceLevelUp)
        {
            isLevelingUp = true; // 레벨업 시작
            skillLevel++;
            SkillWeightApply();
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
            SkillWeightApply(false);
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

    // 스킬 가중치 적용 함수
    private void SkillWeightApply(bool levelUp = true)  // 디폴트 값은 레벨 상승, false는 레벨 하락
    {
        // TODO : 가중치 보정 함수
        skillWeight = SkillManager.Instance.GetSkillWeights(EntityType.Player, skillName);
        if(skillWeight == null)
        {
            Debug.Log("스킬 가중치가 null");
            return;
        }

        switch (levelUp)
        {
            case true:
                currentDamage *= skillWeight.damageIncrease;
                currentCooldown *= skillWeight.cooldownDecrease;
                currentEnergyCost *= skillWeight.energyCostIncrease;
                currentBuffDuration *= skillWeight.buffDurationIncrease;
                currentBuffValue *= skillWeight.buffValueIncrease;
                currentDebuffDuration *= skillWeight.debuffDurationIncrease;
                currentDebuffValue *= skillWeight.debuffValueIncrease;
                break;
            case false:
                currentDamage /= skillWeight.damageIncrease;
                currentCooldown /= skillWeight.cooldownDecrease;
                currentEnergyCost /= skillWeight.energyCostIncrease;
                currentBuffDuration /= skillWeight.buffDurationIncrease;
                currentBuffValue /= skillWeight.buffValueIncrease;
                currentDebuffDuration /= skillWeight.debuffDurationIncrease;
                currentDebuffValue /= skillWeight.debuffValueIncrease;
                break;
        }
    }

    // 필요시 사용 (특정 레벨 도달 시 이펙트 변화?)
    private void EffectChange()
    {
        if(skillWeight.fullLevelEffectPrefab != null)
        {
            currentEffectPrefab = skillWeight.fullLevelEffectPrefab;
        }
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

        // 2025.03.16 HYO 추가 ---------------------------------------------------------------------------------------
        // 디버프 관련 데이터 파싱 추가 (row의 크기가 충분한지 확인 필요)
        // debuffDuration = float.TryParse(row[13].ToString(), out float debuffDur) ? debuffDur : 0f;
        // debuffValue = float.TryParse(row[14].ToString(), out float debuffVal) ? debuffVal : 0f;
        
        // // 속성 파싱 추가 (아직 없다면)
        // attribute = Enum.TryParse(row[15].ToString(), out ElementalAttribute attr) ? attr : ElementalAttribute.None;
        // buffValue = float.TryParse(row[16].ToString(), out float buffVal) ? buffVal : 0f;
        // -----------------------------------------------------------------------------------------------------------

        // 쿨다운 타이머 초기화
        cooldownTimer = new BasicTimer(cooldown);

        Debug.Log($"[Skills] {skillName} 데이터 로드 완료!");
    }

    #region 03.17 C
    public string ToStringTMPro()
    {
        string color = (skillType == SkillType.Physical) ? "#1E90FF" :
                   (skillType == SkillType.Magic) ? "#FFD700" :
                   "#7CFC00"; // Support

        string info = $"<size=130%><b><color={color}>{skillName}</color></b></size>\n\n" +
                      $"유형: {skillType}\n\n" +
                      $"레벨: {skillLevel}\n\n";

        if (skillType == SkillType.Support)
        {
            info += $"버프 지속시간: {currentBuffDuration:F1}초\n" +
                    $"버프 수치: {currentBuffValue}%\n\n" +
                    $"소모 MP: {currentEnergyCost}\n\n" +
                    $"쿨타임: {currentCooldown}초\n\n";
        }
        else
        {
            info += $"데미지: {currentDamage}\n\n" +
                    $"소모 MP: {currentEnergyCost}\n\n" +
                    $"쿨타임: {currentCooldown}초\n\n";
        }

        return info;
    }
    #endregion
}

public enum SkillType
{
    Physical,
    Magic,
    Support
}

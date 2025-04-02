using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Reflection;
using static UnityEngine.GraphicsBuffer;

[Serializable]
public class StatModifier
{
    public float vitalityMultiplier = 10f;       // 체력 배율
    public float mpMultiplier = 5f;              // MP 배율
    public float levelMpBonus = 10f;             // 레벨당 추가 MP 보너스
    public float strengthMultiplier = 1.5f;      // 힘에 의한 물리 공격력 배율
    public float agilityMultiplier = 0.01f;      // 민첩성에 의한 크리티컬 확률 배율
    public float physicalResistanceMultiplier = 1f; // 물리 방어력 배율
    public float magicResistanceMultiplier = 2f;   // 마법 방어력 배율
    public float intelligenceMultiplier = 1.5f;  // 지능에 의한 마법 공격력 배율

    // 필요시, 복사 메서드를 구현할 수 있음 (이 예시에서는 간단하게 구현)
    public StatModifier Clone() => (StatModifier)this.MemberwiseClone();
}

public enum StatType
{
    Strength,
    Vitality,
    Agility,
    Intelligence
}

public enum ElementalAttribute
{
    None,     // 무속성
    Fire,     // 불
    Water,    // 물
    Electric, // 전기
    Earth     // 대지
}

// 250123 11:20AM sohyeon 모든 캐릭터데이터에 mpRecoveryRate 초기화 부분 추가함!!
// CharacterData 클래스 정의: 캐릭터의 스탯, 레벨, 경험치 등을 관리
[Serializable]
public class CharacterData : ISheetData
{
    public event Action<Transform> OnTakeDamage; // 피격 했는지 이벤트 전달
    public event Action<float> OnSpeedChanged;

    private const int maxStats = 999;
    private const int minStats = 0;
    private const float MaxDodgeChance = 0.75f;
    private const float MaxBlockChance = 0.5f;

    public string characterName; // 캐릭터 이름
    public CharacterType characterType; // 캐릭터 타입
    public GameObject characterPrefab; // 캐릭터 프리팹

    public int level; // 레벨
    [HideInInspector] public float currentExperience; // 현재 경험치
    [HideInInspector] public float experienceToLevelUp; // 레벨업에 필요한 경험치

    // 기본 스텟 (정수형)
    public int strength; // 힘 - 물리 공격력, 물리 방어력, 피해량 감소, 방어 확률 
    public int agility; // 민첩 - 회피, 치명타 확률, 공격속도, 적중률
    public int vitality; // 체력 - HP, 물리 방어력, 피해량 감소
    public int intelligence; // 지능 - 마법 데미지, 마법 방어력, MP, MP 회복 속도

    // 리소스 (체력, 마나, 스태미너 등)
    public int maxHp; // 최대 체력
    public int currentHp; // 현재 체력
    public float hpRecoveryRate; // 체력 회복 속도

    public int maxMp; // 최대 MP
    public int currentMp; // 현재 MP
    public float mpRecoveryRate; // MP 회복 속도

    public float stamina; // 최대 스태미너
    public float staminaCurrent; // 현재 스테미너
    public float staminaRecoveryRate; // 스태미너 회복 속도
    
    // 버프 전용 변수 추가
    public int hpBuffBonus = 0;
    public float physicalDamageBuffMultiplier = 1.0f;
    public float magicDamageBuffMultiplier = 1.0f;
    

    // 이동 및 공격 관련
    public float moveSpeed; // 이동 속도
    public float attackSpeed; // 공격 속도 (초당 공격 횟수)
    public float attackRange; // 공격 범위 (근거리/원거리 구분)

    // 전투 관련 스탯
    [HideInInspector] public float physicalDefense; // 물리 방어력
    [HideInInspector] public float magicDefense; // 마법 방어력
    public float physicalDamage; // 물리 공격력
    public float magicDamage; // 마법 공격력
    [HideInInspector] public float criticalChance; // 크리티컬 확률
    [HideInInspector] public float criticalDamage; // 크리티컬 데미지 배율
    [HideInInspector] public float dodgeChance; // 회피 확률
    public bool hasShield; // 방패 착용 여부
    [HideInInspector] public float blockChance = 0f; // 방어 확률
    [HideInInspector] public float physicalDamageReduction; // 피해량 감소율 (ex. 받는 피해 -%)
    [HideInInspector] public float magicDamageReduction; // 피해량 감소율 (ex. 받는 피해 -%)


    public StatModifier statModifier;
    private StatModifier baseStatModifier = new StatModifier();

    private Dictionary<StatType, int> stats;
    public Dictionary<StatType, int> tempStatChanges;
    public Dictionary<Skills, int> tempSkillChanges;

    private bool isLevelingUp = false; // 레벨업 진행 중 여부
    
    [HideInInspector] public GameObject instance;  // 생성된 몬스터 인스턴스를 저장할 필드

    public int availableSkillPoints;
    public int availableStatPoints;
    
    public ElementalAttribute attribute; // 속성

    public bool isStunned;

    // 추가: 장비 효과를 저장하는 필드
    [Header("장비 효과")]
    public float equipmentPhysicalBonus = 0f; // 장비로 인한 물리 데미지 보너스
    public float equipmentMagicBonus = 0f;    // 장비로 인한 마법 데미지 보너스
    // 기본 스탯 장비 보너스
    public int equipmentStrengthBonus = 0;    // 장비로 인한 힘 보너스
    public int equipmentAgilityBonus = 0;     // 장비로 인한 민첩 보너스 
    public int equipmentVitalityBonus = 0;    // 장비로 인한 체력 보너스
    public int equipmentIntelligenceBonus = 0;// 장비로 인한 지능 보너스
    
    // 파생 스탯 장비 보너스
    public float equipmentCriticalChanceBonus = 0f; // 장비로 인한 크리티컬 확률 보너스
    public float equipmentCriticalDamageBonus = 0f; // 장비로 인한 크리티컬 데미지 보너스
    public float equipmentDodgeChanceBonus = 0f;    // 장비로 인한 회피 확률 보너스
    public float equipmentBlockChanceBonus = 0f;    // 장비로 인한 방어 확률 보너스
    public float equipmentPhysicalDefenseBonus = 0f;// 장비로 인한 물리 방어력 보너스
    public float equipmentMagicDefenseBonus = 0f;   // 장비로 인한 마법 방어력 보너스
    public float equipmentAttackSpeedBonus = 0f;    // 장비로 인한 공격 속도 보너스
    public float equipmentMoveSpeedBonus = 0f;      // 장비로 인한 이동 속도 보너스
    public int equipmentMaxHpBonus = 0;            // 장비로 인한 최대 HP 보너스
    public int equipmentMaxMpBonus = 0;            // 장비로 인한 최대 MP 보너스

    public CharacterData() {
        // 장비 보너스 필드 초기화
        this.equipmentPhysicalBonus = 0f;
        this.equipmentMagicBonus = 0f;
        this.equipmentStrengthBonus = 0;
        this.equipmentAgilityBonus = 0;
        this.equipmentVitalityBonus = 0;
        this.equipmentIntelligenceBonus = 0;
        this.equipmentCriticalChanceBonus = 0f;
        this.equipmentCriticalDamageBonus = 0f;
        this.equipmentDodgeChanceBonus = 0f;
        this.equipmentBlockChanceBonus = 0f;
        this.equipmentPhysicalDefenseBonus = 0f;
        this.equipmentMagicDefenseBonus = 0f;
        this.equipmentAttackSpeedBonus = 0f;
        this.equipmentMoveSpeedBonus = 0f;
        this.equipmentMaxHpBonus = 0;
        this.equipmentMaxMpBonus = 0;
    }

    // 생성자: 캐릭터 초기화 및 자동 계산
    public CharacterData(
        string name,
        CharacterType type,
        GameObject prefab,
        int strength,
        int agility,
        int vitality,
        int intelligence,
        StatModifier modifier,
        float speed,
        float attackSpeed,
        float attackRange,
        float stamina,
        float staminaRecoveryRate,
        float mpRecoveryRate,
        ElementalAttribute attribute)
    {
        this.characterName = name;
        this.characterType = type;
        this.characterPrefab = prefab;

        this.strength = strength;
        this.agility = agility;
        this.vitality = vitality;
        this.intelligence = intelligence;

        this.statModifier = modifier ?? baseStatModifier;

        // 장비 보너스 필드 초기화
        this.equipmentPhysicalBonus = 0f;
        this.equipmentMagicBonus = 0f;
        this.equipmentStrengthBonus = 0;
        this.equipmentAgilityBonus = 0;
        this.equipmentVitalityBonus = 0;
        this.equipmentIntelligenceBonus = 0;
        this.equipmentCriticalChanceBonus = 0f;
        this.equipmentCriticalDamageBonus = 0f;
        this.equipmentDodgeChanceBonus = 0f;
        this.equipmentBlockChanceBonus = 0f;
        this.equipmentPhysicalDefenseBonus = 0f;
        this.equipmentMagicDefenseBonus = 0f;
        this.equipmentAttackSpeedBonus = 0f;
        this.equipmentMoveSpeedBonus = 0f;
        this.equipmentMaxHpBonus = 0;
        this.equipmentMaxMpBonus = 0;

        // 파생 스탯 초기화
        UpdateDerivedStats();

        // 리소스 초기화
        this.stamina = stamina;
        this.hpRecoveryRate = 1f; // 기본 체력 회복 속도 예시
        this.mpRecoveryRate = mpRecoveryRate;
        this.staminaRecoveryRate = staminaRecoveryRate;

        // 이동 및 공격 관련 초기화
        this.moveSpeed = speed;
        this.attackSpeed = attackSpeed;
        this.attackRange = attackRange;

        // 레벨 및 경험치 초기화
        this.level = 1;
        this.currentExperience = 0;
        this.experienceToLevelUp = CalculateExperienceToLevelUp();

        this.availableSkillPoints = 0;
        this.availableStatPoints = 0;

        this.attribute = attribute;

        InitializeStats();
    }

    public void InitializeStats()
    {
        stats = new Dictionary<StatType, int>
        {
            { StatType.Strength, strength },
            { StatType.Vitality, vitality },
            { StatType.Agility, agility },
            { StatType.Intelligence, intelligence }
        };
        
        // 임시 할당된 스탯 변경치 (초기값은 모두 0)
        tempStatChanges = new Dictionary<StatType, int>()
        {
            { StatType.Strength, 0 },
            { StatType.Vitality, 0 },
            { StatType.Agility, 0 },
            { StatType.Intelligence, 0 }
        };

        tempSkillChanges = new Dictionary<Skills, int>();
        
        // 변경 후 스탯 업데이트
        UpdateDerivedStats();
    }

    // 데이터 초기화 함수
    public void ResetDataByLevel()
    {
        // 기본적인 스탯과 상태 초기화
        currentHp = maxHp;
        currentMp = maxMp;
        staminaCurrent = stamina;
        
        // 버프 배율 초기화 (1.0이 기본값)
        physicalDamageBuffMultiplier = 1.0f;
        magicDamageBuffMultiplier = 1.0f;
        hpBuffBonus = 0;
        
        // 이전 데미지 값 저장
        float oldPhysicalDamage = physicalDamage;
        float oldMagicDamage = magicDamage;
        
        // 장비 보너스 초기화 (추가된 부분)
        // 참고: 장비 효과는 나중에 장착된 장비에서 다시 적용됨
        ResetEquipmentBonuses();
        
        // 동적 데이터 재계산
        UpdateDerivedStats();

        Debug.Log($"Character data reset. Level: {level}, HP: {currentHp}/{maxHp}, 물리 데미지: {oldPhysicalDamage}->{physicalDamage} (장비보너스: {equipmentPhysicalBonus}), 마법 데미지: {oldMagicDamage}->{magicDamage} (장비보너스: {equipmentMagicBonus})");
    }

    // 파생 스탯 계산
    public virtual void UpdateDerivedStats(bool updateCurrentValues = true)
    {
        // 장비 효과를 포함한 총 기본 스탯 계산
        int totalStrength = strength + equipmentStrengthBonus;
        int totalAgility = agility + equipmentAgilityBonus;
        int totalVitality = vitality + equipmentVitalityBonus;
        int totalIntelligence = intelligence + equipmentIntelligenceBonus;
        
        // 기본 체력 및 MP 계산에 버프 보너스와 장비 보너스를 반영
        int oldMaxHp = maxHp; // 이전 최대 체력 저장
        maxHp = Mathf.RoundToInt(totalVitality * statModifier.vitalityMultiplier) + hpBuffBonus + equipmentMaxHpBonus;
        maxMp = Mathf.RoundToInt((totalIntelligence * statModifier.mpMultiplier) +
                               (level * statModifier.levelMpBonus)) + equipmentMaxMpBonus; 

        // 물리 데미지 계산 (버프 효과와 장비 보너스 분리)
        float basePhysicalDamage = totalStrength * statModifier.strengthMultiplier;
        float buffedPhysicalDamage = basePhysicalDamage * physicalDamageBuffMultiplier;
        physicalDamage = Mathf.RoundToInt(Mathf.Max(buffedPhysicalDamage + equipmentPhysicalBonus, 1f));

        // 마법 데미지 계산 (버프 효과와 장비 보너스 분리)
        float baseMagicDamage = totalIntelligence * statModifier.intelligenceMultiplier;
        float buffedMagicDamage = baseMagicDamage * magicDamageBuffMultiplier;
        magicDamage = Mathf.RoundToInt(Mathf.Max(buffedMagicDamage + equipmentMagicBonus, 1f));

        // 방어력 계산 (장비 보너스 포함)
        physicalDefense = Mathf.RoundToInt(Mathf.Max((totalStrength + totalVitality) * statModifier.physicalResistanceMultiplier + equipmentPhysicalDefenseBonus, 1f));
        magicDefense = Mathf.RoundToInt(Mathf.Max(totalIntelligence * statModifier.magicResistanceMultiplier + equipmentMagicDefenseBonus, 1f));

        // 크리티컬 확률 계산 (장비 보너스 포함)
        criticalChance = Mathf.Min(totalAgility * statModifier.agilityMultiplier + equipmentCriticalChanceBonus, 1f);
        criticalDamage = 1.5f + equipmentCriticalDamageBonus; // 크리티컬 데미지 배율에 장비 보너스 추가

        // 회피 및 방어 확률 계산 (장비 보너스 포함)
        dodgeChance = Mathf.Min((totalAgility / 200f) + equipmentDodgeChanceBonus, MaxDodgeChance);
        
        // 방패가 있을 경우, 방어 확률 계산 (장비 보너스 포함)
        blockChance = hasShield ? Mathf.Min((totalStrength / 200f) + equipmentBlockChanceBonus, MaxBlockChance) : 0f;
        
        // 피해 감소율 계산 (최대 50% 제한)
        physicalDamageReduction = Mathf.Min(physicalDefense / (physicalDefense + 100), 0.5f);
        magicDamageReduction = Mathf.Min(magicDefense / (magicDefense + 100), 0.5f);
        
        // 속도 보너스 적용
        attackSpeed = Mathf.Max(attackSpeed + equipmentAttackSpeedBonus, 0.1f);
        moveSpeed = Mathf.Max(moveSpeed + equipmentMoveSpeedBonus, 0.1f);

        // 현재 값 업데이트 옵션이 true인 경우에만 체력 및 MP 증가 처리
        if (updateCurrentValues)
        {
            // 체력 증가 처리 (버프 적용 시)
            if (maxHp > oldMaxHp)
            {
                // 현재 체력에 버프로 인한 증가량만큼 추가
                int hpIncrease = maxHp - oldMaxHp;
                currentHp += hpIncrease;
                currentHp = Mathf.Clamp(currentHp, 0, maxHp);
            }
            // 체력 감소 처리 (버프 해제 시)
            else if (maxHp < oldMaxHp)
            {
                // 현재 체력이 새로운 최대 체력을 초과하는 경우에만 조정
                if (currentHp > maxHp)
                {
                    currentHp = maxHp;
                }
                // 현재 체력이 새로운 최대 체력보다 작은 경우에는 그대로 유지
            }

            // MP 증가 처리
            if (maxMp > currentMp)
            {
                currentMp += maxMp - currentMp;
                currentMp = Mathf.Clamp(currentMp, 0, maxMp); // 현재 MP는 최대 MP보다 크지 않게 설정
            }
        }
    }


    // 경험치를 얻었을 때 호출되는 함수
    public void AddExperience(int amount)
    {
        if (amount < 0) return;

        currentExperience += amount;

        // 경험치가 레벨업 요구치를 초과하면 레벨업 처리
        while (currentExperience >= experienceToLevelUp)
        {
            if (!isLevelingUp)
            {
                currentExperience -= experienceToLevelUp;
                LevelUp();
            }
        }
    }

    private void LevelUp()
    {
        isLevelingUp = true; // 레벨업 시작
        
        // 이펙트 추가
        UIManager.LevelUpEfeect();
        
        // 레벨 증가
        level++;

        // 새로운 레벨에 맞는 경험치 계산
        experienceToLevelUp = CalculateExperienceToLevelUp();

        // 레벨업 시 능력치 증가 및 파생 스탯 계산
        IncreaseStatsBasedOnLevel();

        // 레벨업 시 모든 리소스 회복
        currentHp = maxHp;
        currentMp = maxMp;
        staminaCurrent = stamina;

        // 디버깅 로그
        Debug.Log($"레벨업! 현재 레벨: {level}, 남은 경험치: {currentExperience}");
        
        AwardLevelUpPoints();

        isLevelingUp = false; // 레벨업 종료
    }
    
    protected virtual void AwardLevelUpPoints()
    {
        // 기본 캐릭터는 추가 포인트 지급하지 않음.
    }

    // 경험치 계산 함수
    public int CalculateExperienceToLevelUp()
    {
        // 레벨 9 -> 10, 19 -> 20, 29 -> 30 구간에서 경험치를 완화
        return (level % 10 == 9) ? Mathf.RoundToInt(level * 80f) : Mathf.RoundToInt(level * 100f);
    }

    // 능력치 증가 처리 함수 (레벨에 따른 규칙을 설정)
    private void IncreaseStatsBasedOnLevel()
    {
        strength++;
        vitality++;
        agility++;
        intelligence++;

        UpdateDerivedStats();
    }

    // 특정 스텟 증가 함수
    public void ModifyStat(StatType statType, int amount)
    {
        if (!stats.ContainsKey(statType)) return;

        stats[statType] = Mathf.Clamp(stats[statType] + amount, minStats, maxStats);

        strength = stats[StatType.Strength];
        vitality = stats[StatType.Vitality];
        agility = stats[StatType.Agility];
        intelligence = stats[StatType.Intelligence];

        UpdateDerivedStats();
    }

    // 피해를 입었을 때
    public void TakeDamage(int damage, Transform attacker = null)
    {
        currentHp = Mathf.Max(0, currentHp - damage);
        
        // CameraManager null 체크 유지
        if (CameraManager.Instance != null)
        {
            CameraManager.Instance.StartCameraShake();
        }
        
        // OnTakeDamage 이벤트 호출 (null 체크 사용)
        OnTakeDamage?.Invoke(attacker);
    }

    // 회복
    public void Heal(int amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
    }

    private void AdjustMp(float amount)
    {
        // currentMp에 적용할 때만 float를 int로 변환
        currentMp = Mathf.Clamp(currentMp + Mathf.RoundToInt(amount), 0, maxMp);
    }

    public void RegenerateMp()
    {
        AdjustMp(mpRecoveryRate); // mpRecoveryRate는 float로 유지
    }

    public void UseItemForMp(float recoveryAmount)
    {
        AdjustMp(recoveryAmount); // recoveryAmount는 float로 유지
    }

    public void UseMp(float amount)
    {
        AdjustMp(-amount); // amount는 float로 유지
    }

    private void AdjustStamina(float amount)
    {
        staminaCurrent = Mathf.Clamp(staminaCurrent + amount, 0, stamina);
    }

    public void RegenerateStamina()
    {
        AdjustStamina(staminaRecoveryRate);
    }

    public void UseItemForStamina(float recoveryAmount)
    {
        AdjustStamina(recoveryAmount);
    }

    public void UseStamina(float amount)
    {
        AdjustStamina(-amount);
    }

    // 땅 속성 데미지 증가 버프 적용
    public void ApplyEarthDamageEffect(float duration, float damageIncreasePercent)
    {
        // 매니저가 존재하는지 확인
        ElementalEffectManager.EnsureExists();
        
        var earthEffect = new EarthDamageEffect(duration, damageIncreasePercent, this);
        earthEffect.Apply();
    }
    
    // 불 속성 효과 적용
    public void ApplyFireBurnEffect(float duration, float damagePercent)
    {
        // 매니저가 존재하는지 확인
        ElementalEffectManager.EnsureExists();
        
        var fireEffect = new FireBurnEffect(duration, damagePercent, this);
        fireEffect.Apply();
    }
    
    // 물 속성 효과 적용
    public void ApplyWaterSlowEffect(float duration, float speedReduction)
    {
        // 매니저가 존재하는지 확인
        ElementalEffectManager.EnsureExists();
        
        var waterEffect = new WaterSlowEffect(duration, speedReduction, this);
        waterEffect.Apply();
    }
    
    // 전기 속성 효과 적용
    public void ApplyElectricStunEffect(float duration)
    {
        // 매니저가 존재하는지 확인
        ElementalEffectManager.EnsureExists();
        
        var electricEffect = new ElectricStunEffect(duration, this);
        electricEffect.Apply();
    }
    
    // 속성 효과 제거 편의 메서드
    public void ClearAllElementalEffects()
    {
        if (ElementalEffectManager.Instance != null)
        {
            ElementalEffectManager.Instance.ClearCharacterEffects(this);
        }
    }

    public void UpdateSpeed(float newSpeed)
    {
        moveSpeed = newSpeed;
        OnSpeedChanged?.Invoke(moveSpeed);
    }


    public CharacterData Clone()
    {
        return new CharacterData(
            this.characterName,
            this.characterType,
            this.characterPrefab,
            this.strength,
            this.agility,
            this.vitality,
            this.intelligence,
            this.statModifier != baseStatModifier ? statModifier.Clone() : baseStatModifier,
            this.moveSpeed,
            this.attackSpeed,
            this.attackRange,
            this.stamina,
            this.staminaRecoveryRate,
            this.mpRecoveryRate,
            this.attribute
        )
        {
            // 장비 보너스 복사
            equipmentPhysicalBonus = this.equipmentPhysicalBonus,
            equipmentMagicBonus = this.equipmentMagicBonus,
            equipmentStrengthBonus = this.equipmentStrengthBonus,
            equipmentAgilityBonus = this.equipmentAgilityBonus,
            equipmentVitalityBonus = this.equipmentVitalityBonus,
            equipmentIntelligenceBonus = this.equipmentIntelligenceBonus,
            equipmentCriticalChanceBonus = this.equipmentCriticalChanceBonus,
            equipmentCriticalDamageBonus = this.equipmentCriticalDamageBonus,
            equipmentDodgeChanceBonus = this.equipmentDodgeChanceBonus,
            equipmentBlockChanceBonus = this.equipmentBlockChanceBonus,
            equipmentPhysicalDefenseBonus = this.equipmentPhysicalDefenseBonus,
            equipmentMagicDefenseBonus = this.equipmentMagicDefenseBonus,
            equipmentAttackSpeedBonus = this.equipmentAttackSpeedBonus,
            equipmentMoveSpeedBonus = this.equipmentMoveSpeedBonus,
            equipmentMaxHpBonus = this.equipmentMaxHpBonus,
            equipmentMaxMpBonus = this.equipmentMaxMpBonus
        };
    }

    public virtual string ToStringForTMPro()
    {
        string baseInfo = string.Format(
            "<color=red>Name:</color> {0}\n" +
            "<color=red>Level:</color> {1}\n" +
            "<color=red>Strength:</color> {2} <color=green>(+{3})</color>\n" +
            "<color=blue>Agility:</color> {4} <color=green>(+{5})</color>\n" +
            "<color=green>Vitality:</color> {6} <color=green>(+{7})</color>\n" +
            "<color=yellow>Intelligence:</color> {8} <color=green>(+{9})</color>\n" +
            "<color=lime>Current HP:</color> {10}/{11} <color=green>(+{12})</color>\n" +
            "<color=cyan>Current MP:</color> {13}/{14} <color=green>(+{15})</color>\n" +
            "<color=lime>Stamina:</color> {16}/{17} (Recovery: {18}/s)\n" +
            "<color=purple>Physical Damage:</color> {19} <color=green>(+{20})</color>\n" +
            "<color=cyan>Magic Damage:</color> {21} <color=green>(+{22})</color>\n" +
            "<color=orange>Physical Defense:</color> {23} <color=green>(+{24})</color>\n" +
            "<color=orange>Magic Defense:</color> {25} <color=green>(+{26})</color>\n" +
            "<color=magenta>Critical Chance:</color> {27}% <color=green>(+{28}%)</color>\n" +
            "<color=teal>Dodge Chance:</color> {29}% <color=green>(+{30}%)</color>\n" +
            "<color=teal>Block Chance:</color> {31}% <color=green>(+{32}%)</color>\n" +
            "<color=yellow>Attack Speed:</color> {33} <color=green>(+{34})</color>\n" +
            "<color=teal>Speed:</color> {35} <color=green>(+{36})</color>\n" +
            "<color=orange>Current Experience:</color> {37}\n" +
            "<color=orange>Experience To Level Up:</color> {38}\n" +
            "<color=cyan>Shield Status:</color> {39}\n" +
            "<color=lime>HP Recovery Rate:</color> {40}\n" +
            "<color=lime>MP Recovery Rate:</color> {41}\n" +
            "<color=lime>Stamina Recovery Rate:</color> {42}\n" +
            "<color=teal>Physical Damage Reduction:</color> {43}%\n" +
            "<color=teal>Magic Damage Reduction:</color> {44}%",
            characterName, // 0
            level, // 1
            strength, // 2
            equipmentStrengthBonus, // 3
            agility, // 4
            equipmentAgilityBonus, // 5
            vitality, // 6
            equipmentVitalityBonus, // 7
            intelligence, // 8
            equipmentIntelligenceBonus, // 9
            currentHp, // 10
            maxHp, // 11
            equipmentMaxHpBonus, // 12
            currentMp, // 13
            maxMp, // 14
            equipmentMaxMpBonus, // 15
            staminaCurrent, // 16
            stamina, // 17
            staminaRecoveryRate, // 18
            physicalDamage, // 19
            equipmentPhysicalBonus, // 20
            magicDamage, // 21
            equipmentMagicBonus, // 22
            physicalDefense, // 23
            equipmentPhysicalDefenseBonus, // 24
            magicDefense, // 25
            equipmentMagicDefenseBonus, // 26
            criticalChance * 100, // 27
            equipmentCriticalChanceBonus * 100, // 28
            dodgeChance * 100, // 29
            equipmentDodgeChanceBonus * 100, // 30
            blockChance * 100, // 31
            equipmentBlockChanceBonus * 100, // 32
            attackSpeed, // 33
            equipmentAttackSpeedBonus, // 34
            moveSpeed, // 35
            equipmentMoveSpeedBonus, // 36
            currentExperience, // 37
            experienceToLevelUp, // 38
            hasShield ? "<color=green>Equipped</color>" : "<color=red>Not Equipped</color>", // 39
            hpRecoveryRate, // 40
            mpRecoveryRate, // 41
            staminaRecoveryRate, // 42
            physicalDamageReduction * 100, // 43
            magicDamageReduction * 100 // 44
        );
        return baseInfo;
    }

    public virtual void ParseData(IList<object> row)
    {
        if (row.Count < 10) throw new Exception("캐릭터 데이터 부족");

        characterName = row[0].ToString();
        characterType = Enum.TryParse(row[1].ToString(), out CharacterType type) ? type : CharacterType.Monster;
        strength = int.TryParse(row[2].ToString(), out int str) ? str : 0;
        agility = int.TryParse(row[3].ToString(), out int agi) ? agi : 0;
        vitality = int.TryParse(row[4].ToString(), out int vit) ? vit : 0;
        intelligence = int.TryParse(row[5].ToString(), out int intl) ? intl : 0;

        moveSpeed = float.TryParse(row[6].ToString(), out float ms) ? ms : 1.0f;
        attackSpeed = float.TryParse(row[7].ToString(), out float aspeed) ? aspeed : 1.0f;
        attackRange = float.TryParse(row[8].ToString(), out float arange) ? arange : 1.0f;

        statModifier = new StatModifier(); // 기본 스탯 보정 적용
        UpdateDerivedStats();
    }

    public List<object> ToList()
    {
        return new List<object>
        {
            characterName,
            characterType.ToString(),
            level,
            strength,
            agility,
            vitality,
            intelligence,
            currentHp,
            currentMp,
            moveSpeed,
            attackSpeed
        };
    }

    // 장비 보너스 설정 메서드 추가
    public void SetEquipmentBonuses(float physicalBonus, float magicBonus)
    {
        equipmentPhysicalBonus = physicalBonus;
        equipmentMagicBonus = magicBonus;
        
        // 장비 변경 후 스탯 재계산
        UpdateDerivedStats();
        
        Debug.Log($"[SetEquipmentBonuses] 장비 보너스 설정: 물리={physicalBonus}, 마법={magicBonus}, 적용 후 데미지: 물리={physicalDamage}, 마법={magicDamage}");
    }

    // 모든 장비 보너스를 한번에 설정하는 확장된 메서드
    public void SetAllEquipmentBonuses(
        float physicalBonus, float magicBonus,
        int strengthBonus, int agilityBonus, int vitalityBonus, int intelligenceBonus,
        float criticalChanceBonus = 0f, float criticalDamageBonus = 0f,
        float dodgeChanceBonus = 0f, float blockChanceBonus = 0f,
        float physicalDefenseBonus = 0f, float magicDefenseBonus = 0f,
        float attackSpeedBonus = 0f, float moveSpeedBonus = 0f,
        int maxHpBonus = 0, int maxMpBonus = 0)
    {
        // 기본 데미지 보너스 설정
        equipmentPhysicalBonus = physicalBonus;
        equipmentMagicBonus = magicBonus;
        
        // 기본 스탯 보너스 설정
        equipmentStrengthBonus = strengthBonus;
        equipmentAgilityBonus = agilityBonus;
        equipmentVitalityBonus = vitalityBonus;
        equipmentIntelligenceBonus = intelligenceBonus;
        
        // 파생 스탯 보너스 설정
        equipmentCriticalChanceBonus = criticalChanceBonus;
        equipmentCriticalDamageBonus = criticalDamageBonus;
        equipmentDodgeChanceBonus = dodgeChanceBonus;
        equipmentBlockChanceBonus = blockChanceBonus;
        equipmentPhysicalDefenseBonus = physicalDefenseBonus;
        equipmentMagicDefenseBonus = magicDefenseBonus;
        equipmentAttackSpeedBonus = attackSpeedBonus;
        equipmentMoveSpeedBonus = moveSpeedBonus;
        equipmentMaxHpBonus = maxHpBonus;
        equipmentMaxMpBonus = maxMpBonus;
        
        // 장비 변경 후 스탯 재계산
        UpdateDerivedStats();
        
        Debug.Log($"[SetAllEquipmentBonuses] 모든 장비 보너스 설정 완료. 4대 스탯 보너스: 힘={strengthBonus}, 민첩={agilityBonus}, 체력={vitalityBonus}, 지능={intelligenceBonus}");
    }
    
    // 장비 보너스 초기화 메서드
    public void ResetEquipmentBonuses()
    {
        equipmentPhysicalBonus = 0f;
        equipmentMagicBonus = 0f;
        equipmentStrengthBonus = 0;
        equipmentAgilityBonus = 0;
        equipmentVitalityBonus = 0;
        equipmentIntelligenceBonus = 0;
        equipmentCriticalChanceBonus = 0f;
        equipmentCriticalDamageBonus = 0f;
        equipmentDodgeChanceBonus = 0f;
        equipmentBlockChanceBonus = 0f;
        equipmentPhysicalDefenseBonus = 0f;
        equipmentMagicDefenseBonus = 0f;
        equipmentAttackSpeedBonus = 0f;
        equipmentMoveSpeedBonus = 0f;
        equipmentMaxHpBonus = 0;
        equipmentMaxMpBonus = 0;
        
        // 장비 보너스 초기화 후 스탯 재계산
        UpdateDerivedStats();
        
        Debug.Log("모든 장비 보너스가 초기화되었습니다.");
    }
}

// 플레이어 데이터
[Serializable]
public class PlayerData : CharacterData
{
    public List<string> skills = new List<string>();    // 스킬 목록
    public int gold; // 골드 소지량
    
    // 생성자
    public PlayerData(string name, GameObject prefab, int strength, int vitality, int agility, int intelligence,
        float speed, float attackSpeed, float attackRange, float stamina, float staminaRecoveryRate, float mpRecoveryRate, ElementalAttribute attribute)
        : base(name, CharacterType.Player, prefab, strength, agility, vitality, intelligence, null, speed, attackSpeed, attackRange, stamina, staminaRecoveryRate, mpRecoveryRate, attribute)
    {
        gold = 0; // 초기 골드는 0
    }

    public ElementalAttribute GetEffectiveAttackAttribute(bool isSkillAttack, Skills skill)
    {
        if (isSkillAttack)
        {
            attribute = skill.attribute;
            return this.attribute;
        }
        
        var handItem = ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.손);
        if (handItem != null && handItem.itemSkill != null)
        {
            return handItem.itemSkill.element;
        }
    
        // 장비가 없으면 플레이어 기본 속성 사용
        return this.attribute;
    }
    
    public ElementalAttribute GetEffectiveDefenseAttribute()
    {
        // 몸 슬롯에 장착된 아이템이 있다면 해당 아이템의 속성을 사용
        var bodyItem = ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.몸);
        if (bodyItem != null && bodyItem.itemSkill != null)
        {
            return bodyItem.itemSkill.element;
        }
    
        // 없으면 플레이어 기본 속성 사용
        return this.attribute;
    }
    
    // 스탯을 조정하는 메서드
    public bool AdjustTempStat(StatType statType, int delta)
    {
        // 기본 스탯 값 가져오기
        int baseValue = 0;
        switch (statType)
        {
            case StatType.Strength: baseValue = this.strength; break;
            case StatType.Vitality: baseValue = this.vitality; break;
            case StatType.Agility: baseValue = this.agility; break;
            case StatType.Intelligence: baseValue = this.intelligence; break;
        }
    
        // 임시 변경치 계산 (임시로 할당된 보너스 포인트)
        int newTemp = tempStatChanges[statType] + delta;
    
        // 임시 변경치가 음수가 되면, 즉 이미 추가한 포인트보다 더 내리려고 하는 경우 차단
        if (newTemp < 0)
        {
            Debug.LogWarning($"{statType}은(는) 이미 확정된 스탯 이하로 내려갈 수 없습니다.");
            return false;
        }
    
        // 스탯 포인트 추가인 경우 사용 가능한 포인트 체크
        if (delta > 0)
        {
            if (availableStatPoints < delta)
            {
                Debug.LogWarning("사용 가능한 스탯 포인트가 부족합니다!");
                return false;
            }
            availableStatPoints -= delta;
        }
        // 감산인 경우 환급
        else if (delta < 0)
        {
            availableStatPoints += (-delta);
        }
    
        tempStatChanges[statType] = newTemp;
        return true;
    }
    
    public bool AdjustTempSkill(Skills skill, int delta)
    {
        if (skill == null) return false;

        int currentTemp = tempSkillChanges.ContainsKey(skill) ? tempSkillChanges[skill] : 0;
        int newTemp = currentTemp + delta;
        int baseLevel = skill.skillLevel;

        // 기본 스킬 레벨 이하로 내려갈 수 없도록 제한
        if (newTemp < 0)
        {
            Debug.LogWarning($"{skill.skillName}은(는) 기본 스킬 레벨 이하로 내려갈 수 없습니다.");
            return false;
        }

        // 기본 레벨 + 임시 변경치가 최대 스킬 레벨을 초과하면 안 됨
        if (baseLevel + newTemp > skill.maxSkillLevel)
        {
            Debug.LogWarning($"{skill.skillName}은(는) 최대 스킬 레벨에 도달했습니다.");
            return false;
        }

        // delta가 양수면 사용 가능한 스킬 포인트 체크
        if (delta > 0)
        {
            if (availableSkillPoints < delta)
            {
                Debug.LogWarning("사용 가능한 스킬 포인트가 부족합니다!");
                return false;
            }
            availableSkillPoints -= delta;
        }
        // delta가 음수면 환급
        else if (delta < 0)
        {
            availableSkillPoints += (-delta);
        }

        tempSkillChanges[skill] = newTemp;
        return true;
    }
    
    // 임시 할당된 스탯 변경치를 실제 스탯에 반영하는 메서드.
    public void ConfirmTempAllocation()
    {
        foreach (var kvp in tempStatChanges)
        {
            // 기존 ModifyStat 메서드는 기본 스탯 딕셔너리와 필드 값을 함께 업데이트합니다.
            ModifyStat(kvp.Key, kvp.Value);
        }
        // 임시 할당 값 초기화
        tempStatChanges[StatType.Strength] = 0;
        tempStatChanges[StatType.Vitality] = 0;
        tempStatChanges[StatType.Agility] = 0;
        tempStatChanges[StatType.Intelligence] = 0;
        
        // (필요하면 확정 후 UI 업데이트 등 후속 처리)
    }
    
    public void ConfirmTempSkillAllocation()
    {
        foreach (var kvp in tempSkillChanges)
        {
            Skills skill = kvp.Key;
            int delta = kvp.Value;
            if (delta > 0)
            {
                for (int i = 0; i < delta; i++)
                {
                    skill.LevelUp();
                }
            }
            else if (delta < 0)
            {
                for (int i = 0; i < Math.Abs(delta); i++)
                {
                    skill.LevelDown();
                }
            }
        }
        tempSkillChanges.Clear();
    }

    
    // 임시 할당된 스탯 변경치를 실제 스탯에 반영하는 메서드.
    public void CancelTempAllocation()
    {
        // 환급: 양수로 할당된 포인트만 환급 처리
        foreach (var kvp in tempStatChanges)
        {
            if (kvp.Value > 0)
            {
                availableStatPoints += kvp.Value;
            }
        }
        // 임시 변경치 초기화
        tempStatChanges[StatType.Strength] = 0;
        tempStatChanges[StatType.Vitality] = 0;
        tempStatChanges[StatType.Agility] = 0;
        tempStatChanges[StatType.Intelligence] = 0;
    }
    
    public void CancelTempSkillAllocation()
    {
        foreach (var kvp in tempSkillChanges)
        {
            int change = kvp.Value;
            if (change > 0)
            {
                availableSkillPoints += change;
            }
        }
        tempSkillChanges.Clear();
    }
    // 해당 스탯의 임시 할당치 미리보기 값 반환
    public int GetPreviewStat(StatType statType)
    {
        return tempStatChanges[statType];
    }
    
    // 해당 스킬 레벨값 임시 할당치 값 반환
    public int GetPreviewSkillLevel(Skills skill)
    {
        if (skill == null) return 0;
        return tempSkillChanges.ContainsKey(skill) ? tempSkillChanges[skill] : 0;
    }

    public void AddSkill(string skill) => skills.Add(skill);

    public void AddGold(int amount)
    {
        if (amount < 0) return;
        gold += amount;
        UIManager.Instance.PickUpItemTextDisplay?.AddItem($"{amount}골드를 획득했습니다.", ItemManager.Instance.GetItemSprite("금화"));
        Debug.Log($"골드 추가: {amount}, 현재 골드: {gold}");
    }

    public bool UseGold(int amount)
    {
        if (amount > gold)
        {
            Debug.LogWarning("골드가 부족합니다!");
            return false;
        }
        gold -= amount;
        Debug.Log($"골드 사용: {amount}, 남은 골드: {gold}");
        return true;
    }
    
    protected override void AwardLevelUpPoints()
    {
        availableStatPoints += 5;  // 스탯 포인트 5 지급
    }
    
    public new PlayerData Clone()
    {
        var baseClone = (PlayerData)base.Clone(); // CharacterData.Clone을 호출

        baseClone.gold = this.gold;  // 골드 복사
        baseClone.skills = new List<string>(this.skills);  // 스킬 리스트 복사

        return baseClone;
    }
    public override string ToStringForTMPro()
    {
        string baseInfo = base.ToStringForTMPro();
        string skillsInfo = skills.Count > 0 ? string.Join(", ", skills) : "No skills acquired";
        string playerInfo = $"<color=gold>Gold:</color> {gold}\n" +
                            $"<color=cyan>Skills:</color> {skillsInfo}\n";
        return baseInfo + playerInfo;
    }

    public override void ParseData(IList<object> row)
    {
        base.ParseData(row);

        if (row.Count < 11) throw new Exception("플레이어 데이터 부족");

        gold = int.TryParse(row[9].ToString(), out int g) ? g : 0;

        // 스킬 목록 (쉼표로 구분된 문자열을 리스트로 변환)
        skills = row[10].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}

// 몬스터 데이터
[Serializable]
public class MonsterData : CharacterData
{
    public List<string> dropItems = new List<string>(); // 드롭 아이템
    public int experienceReward; // 경험치 보상
    public int goldReward;       // 골드 보상
    
    public MonsterData(
        string name, 
        CharacterType characterType, 
        GameObject prefab, 
        int strength, 
        int vitality, 
        int agility,
        int intelligence, 
        float speed, 
        float attackSpeed, 
        float attackRange, 
        float stamina, 
        float staminaRecoveryRate, 
        float mpRecoveryRate,
        ElementalAttribute attribute,
        List<string> dropItems,
        int experienceReward, 
        int goldReward)
        : base(name, characterType, prefab, strength, agility, vitality, intelligence, null, speed, attackSpeed, attackRange, stamina, staminaRecoveryRate, mpRecoveryRate, attribute)
    {
        this.dropItems = new List<string>(dropItems);
        this.experienceReward = experienceReward;
        this.goldReward = goldReward;
    }

    public new MonsterData Clone()
    {
        var clone = new MonsterData(
            this.characterName,
            this.characterType,
            this.characterPrefab,
            this.strength,
            this.vitality,
            this.agility,
            this.intelligence,
            this.moveSpeed,
            this.attackSpeed,
            this.attackRange,
            this.stamina,
            this.staminaRecoveryRate,
            this.mpRecoveryRate,
            this.attribute,
            new List<string>(this.dropItems), // 드롭 아이템 복사
            this.experienceReward,
            this.goldReward
        );

        return clone;
    }
    
    public override string ToStringForTMPro()
    {
        string baseInfo = base.ToStringForTMPro();
        string monsterInfo = string.Format(
            "<color=red>Experience Reward:</color> {0}\n" +
            "<color=yellow>Gold Reward:</color> {1}\n" +
            "<color=lime>Drop Items:</color> {2}\n",
            experienceReward,                // 0: 경험치 보상
            goldReward,                      // 1: 골드 보상
            string.Join(", ", dropItems)     // 2: 드롭 아이템 목록
        );

        return baseInfo + monsterInfo;
    }

    public override void ParseData(IList<object> row)
    {
        base.ParseData(row);

        if (row.Count < 13) throw new Exception("몬스터 데이터 부족");

        experienceReward = int.TryParse(row[9].ToString(), out int exp) ? exp : 0;
        goldReward = int.TryParse(row[10].ToString(), out int gold) ? gold : 0;

        // 드롭 아이템 목록 처리 (쉼표로 구분된 아이템 리스트)
        dropItems = row[11].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}

[Serializable]
public class BossData : MonsterData
{
    public List<string> SpecialSkills { get; private set; } // 보스의 특수 스킬

    public BossData(
        string name,
        CharacterType characterType,
        GameObject prefab,
        int strength,
        int vitality,
        int agility,
        int intelligence,
        float speed,
        float attackSpeed,
        float attackRange,
        float stamina,
        float staminaRecoveryRate,
        float mpRecoveryRate,
        ElementalAttribute attribute,
        List<string> dropItems,
        int experienceReward,
        int goldReward,
        List<string> specialSkills)
        : base(name, characterType, prefab, strength, vitality, agility, intelligence, speed, attackSpeed,
            attackRange, stamina, staminaRecoveryRate, mpRecoveryRate, attribute, dropItems, experienceReward, goldReward)
    {
        SpecialSkills = new List<string>(specialSkills);
    }
    
    public void AddSkill(string skill) => SpecialSkills.Add(skill);
    
    public new BossData Clone()
    {
        // BossData의 고유 속성 및 MonsterData의 공통 속성을 포함한 클론 생성
        var clone = new BossData(
            this.characterName,
            this.characterType,
            this.characterPrefab,
            this.strength,
            this.vitality,
            this.agility,
            this.intelligence,
            this.moveSpeed,
            this.attackSpeed,
            this.attackRange,
            this.stamina,
            this.staminaRecoveryRate,
            this.mpRecoveryRate,
            this.attribute,
            new List<string>(this.dropItems), // 드롭 아이템 복사
            this.experienceReward,
            this.goldReward,
            new List<string>(this.SpecialSkills) // 특수 스킬 복사
        );
        return clone;
    }
    
    public override string ToStringForTMPro()
    {
        // 부모 클래스의 ToStringForTMPro 호출
        string baseInfo = base.ToStringForTMPro();
    
        // 보스 고유의 특수 스킬 추가
        string bossInfo = string.Format(
            "<color=red>Special Skills:</color> {0}\n",
            SpecialSkills.Count > 0 ? string.Join(", ", SpecialSkills) : "None" // 특수 스킬 목록 표시
        );
    
        return baseInfo + bossInfo;
    }

    public override void ParseData(IList<object> row)
    {
        base.ParseData(row);

        if (row.Count < 14) throw new Exception("보스 데이터 부족");

        // 보스의 특수 스킬 (쉼표로 구분된 스킬 리스트)
        SpecialSkills = row[12].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();
    }
}

// 드래곤 관련
[Serializable]
public enum DragonEvolutionStage
{
    Baby,   // 아기용
    Young,  // 어린이용 
    Adult   // 어른용
}

[Serializable]
public class DragonData
{
    // 진화 이벤트 (UI 업데이트 등을 위해)
    public delegate void DragonEvolutionHandler(DragonEvolutionStage newStage);
    public static event DragonEvolutionHandler OnDragonEvolution;
    
    public string characterName;
    public CharacterType characterType;
    public GameObject prefab;
    
    // 진화 단계 관련 필드
    public DragonEvolutionStage evolutionStage = DragonEvolutionStage.Baby;
    public GameObject[] evolutionPrefabs = new GameObject[3]; // 각 진화 단계별 프리팹 (0: Baby, 1: Young, 2: Adult)
    
    // 진화 단계별 최대 레벨 제한
    public int[] maxLevelPerStage = new int[] { 15, 30, 50 }; // Baby, Young, Adult 각각의 최대 레벨
    
    public int strength;       // 힘
    public int agility;        // 민첩
    public int vitality;       // 건강
    public int intelligence;   // 지능

    public float speed;        // 이동 속도
    public float attackSpeed;  // 공격 속도
    public float attackRange;  // 기본 공격 범위

    public int bondLevel;      // 유대 레벨
    public int bondExperience; // 유대 경험치
    [SerializeField] private int _requiredExpToNextLevel; // 인스펙터에서 볼 수 있는 다음 레벨 필요 경험치
    
    // 다음 레벨업에 필요한 경험치를 속성으로 제공
    public int RequiredExpToNextLevel 
    {
        get { return _requiredExpToNextLevel; }
        private set { _requiredExpToNextLevel = value; }
    }
    
    // 모디파이어
    public DragonStatModifier statModifier;
    private DragonStatModifier baseStatModifier = new DragonStatModifier();
    
    // 물리 데미지와 마법 데미지 계산을 위한 필드 추가
    public int physicalDamage;  // 물리 공격력
    public int magicDamage;     // 마법 공격력
    public float criticalChance; // 크리티컬 확률
    public float criticalDamage; // 크리티컬 데미지 배율
    
    public ElementalAttribute dragonAttribute;
    private ElementalAttribute dragonBaseAttribute = new ElementalAttribute();


    public DragonData(
        string name, 
        CharacterType type,
        GameObject prefab, 
        int strength, 
        int agility, 
        int vitality, 
        int intelligence, 
        float speed, 
        float attackSpeed,
        float attackRange,
        float criticalChance, 
        ElementalAttribute dragonAttribute,
        DragonStatModifier modifier = null)
    {
        this.characterName = name;
        this.characterType = type;
        this.prefab = prefab;
        this.strength = strength;
        this.agility = agility;
        this.vitality = vitality;
        this.intelligence = intelligence;
        this.speed = speed;
        this.attackSpeed = attackSpeed;
        this.attackRange = attackRange;
        this.bondLevel = 1;
        this.bondExperience = 0;
        this.criticalChance = criticalChance;
        this.dragonAttribute = dragonAttribute;
        this.statModifier = modifier ?? new DragonStatModifier();
        this.evolutionStage = DragonEvolutionStage.Baby;

        // maxLevelPerStage 배열이 비어있거나 null이면 기본값으로 초기화
        if (maxLevelPerStage == null || maxLevelPerStage.Length == 0)
        {
            maxLevelPerStage = new int[] { 15, 30, 50 }; // 기본값으로 초기화
            Debug.Log("Constructor: maxLevelPerStage가 비어있어 기본값으로 초기화되었습니다: [15, 30, 50]");
        }

        // 물리 데미지와 마법 데미지 계산
        UpdateDerivedStats();
        
        // 다음 레벨업에 필요한 경험치 초기화
        RequiredExpToNextLevel = CalculateExperienceToLevelUp(bondLevel);
    }
    
    // 초기화 메서드 추가 - CharacterManager의 InitialDragon에서 호출하도록 변경
    public void Initialize()
    {
        characterName = "BabyDragon";
        characterType = CharacterType.Drogon;
        strength = 5;
        vitality = 8;
        agility = 4;
        intelligence = 6;
        speed = 5.0f;
        attackSpeed = 3f;
        attackRange = 2.5f;

        // 진화 단계 초기화
        evolutionStage = DragonEvolutionStage.Baby;
        
        // maxLevelPerStage 배열이 비어있거나 null이면 기본값으로 초기화
        if (maxLevelPerStage == null || maxLevelPerStage.Length == 0)
        {
            maxLevelPerStage = new int[] { 15, 30, 50 }; // 기본값으로 초기화
            Debug.Log("Initialize: maxLevelPerStage가 비어있어 기본값으로 초기화되었습니다: [15, 30, 50]");
        }
        
        // 유대 레벨 초기화
        bondLevel = 1;
        bondExperience = 0;
        
        // 스탯 업데이트
        UpdateDerivedStats();
        
        // 다음 레벨업에 필요한 경험치 초기화
        RequiredExpToNextLevel = CalculateExperienceToLevelUp(bondLevel);
    }

    // 물리 데미지와 마법 데미지 계산
    public void UpdateDerivedStats()
    {
        // 진화 단계에 따른 스탯 보너스 계산
        float evolutionMultiplier = GetEvolutionMultiplier();
        
        physicalDamage = Mathf.RoundToInt(Mathf.Max(strength * statModifier.strengthMultiplier * evolutionMultiplier, 1f));
        magicDamage = Mathf.RoundToInt(Mathf.Max(intelligence * statModifier.intelligenceMultiplier * evolutionMultiplier, 1f));
        criticalChance = Mathf.Min(agility * statModifier.agilityMultiplier * evolutionMultiplier, 1f);
        criticalDamage = 1.5f + ((int)evolutionStage * 0.25f); // 진화 단계에 따라 크리티컬 데미지 증가
        
        // 다음 레벨업에 필요한 경험치 업데이트
        RequiredExpToNextLevel = CalculateExperienceToLevelUp(bondLevel);
    }
    
    // 진화 단계에 따른 스탯 증가 배율 반환
    private float GetEvolutionMultiplier()
    {
        switch (evolutionStage)
        {
            case DragonEvolutionStage.Baby:
                return 1.0f;
            case DragonEvolutionStage.Young:
                return 1.5f;
            case DragonEvolutionStage.Adult:
                return 2.2f;
            default:
                return 1.0f;
        }
    }
    
    // 유대 경험치 증가
    public void AddBondExperience(int amount)
    {
        if (amount < 0) return;
        
        Debug.Log($"evolutionStage = {(int)evolutionStage} / maxLevelPerStage.Length = {maxLevelPerStage.Length}");
        
        // maxLevelPerStage 배열이 비어있거나 null이면 기본값으로 초기화
        if (maxLevelPerStage == null || maxLevelPerStage.Length == 0)
        {
            maxLevelPerStage = new int[] { 15, 30, 50 }; // 기본값으로 초기화
            Debug.Log("maxLevelPerStage가 비어있어 기본값으로 초기화되었습니다: [15, 30, 50]");
        }
        
        // 현재 진화 단계의 최대 레벨 체크 (안전하게 처리)
        int maxLevel = (evolutionStage >= 0 && (int)evolutionStage < maxLevelPerStage.Length) 
            ? maxLevelPerStage[(int)evolutionStage] 
            : 50; // 기본값으로 50 설정
        
        // 이미 최대 레벨이면 경험치 획득 중지
        if (bondLevel >= maxLevel)
        {
            Debug.Log($"{characterName}이(가) 현재 진화 단계({evolutionStage})의 최대 레벨({maxLevel})에 도달했습니다. 더 이상 레벨업할 수 없습니다.");
            return;
        }
        
        bondExperience += amount;
        Debug.Log($"{characterName}이(가) {amount}의 유대 경험치를 획득했습니다. 현재 경험치: {bondExperience}/{RequiredExpToNextLevel}");
        
        // 레벨업 체크
        while (bondExperience >= RequiredExpToNextLevel)
        {
            if (!isLevelingUp)
            {
                bondExperience -= RequiredExpToNextLevel;
                LevelUpBond();
                
                // 레벨업 후 최대 레벨 체크
                if (bondLevel >= maxLevel)
                {
                    bondExperience = 0;
                    Debug.Log($"{characterName}이(가) 현재 진화 단계의 최대 레벨({maxLevel})에 도달했습니다!");
                    break;
                }
            }
        }
        
        // 경험치 정보 업데이트
        RequiredExpToNextLevel = CalculateExperienceToLevelUp(bondLevel);
    }

    private bool isLevelingUp = false; // 레벨업 진행 중 여부
    
    private void LevelUpBond()
    {
        isLevelingUp = true; // 레벨업 시작
        
        // 이펙트 추가
        UIManager.LevelUpEfeect();
        
        // 레벨 증가
        bondLevel++;
        
        strength++;
        vitality++;
        agility++;
        intelligence++;
        
        UpdateDerivedStats();
        Debug.Log($"{characterName}의 유대 레벨이 {bondLevel}로 상승했습니다!");
        
        // 다음 레벨업에 필요한 경험치 업데이트
        RequiredExpToNextLevel = CalculateExperienceToLevelUp(bondLevel);
        
        isLevelingUp = false; // 레벨업 종료
    }
    
    // 플레이어/몬스터와 유사한 경험치 계산 방식 (배열 대신 함수 사용)
    public int CalculateExperienceToLevelUp(int currentLevel)
    {
        // 기본 경험치 요구량 (레벨에 따라 증가)
        return 120 + (currentLevel * 20);
    }
    
    // 진화 단계 업그레이드 메서드 - 메인 퀘스트 보상으로 호출됨
    public bool Evolve()
    {
        // 이미 최종 진화 단계라면 진화 불가
        if (evolutionStage == DragonEvolutionStage.Adult)
        {
            Debug.Log($"{characterName}은(는) 이미 최종 진화 단계에 도달했습니다.");
            return false;
        }
        
        // 다음 진화 단계로 업그레이드
        evolutionStage = (DragonEvolutionStage)((int)evolutionStage + 1);
        
        // 이름 변경
        switch (evolutionStage)
        {
            case DragonEvolutionStage.Young:
                characterName = "YoungDragon";
                break;
            case DragonEvolutionStage.Adult:
                characterName = "AdultDragon";
                break;
        }
        
        // 진화 시 스탯 보너스 부여
        strength += 5;
        vitality += 5;
        agility += 5;
        intelligence += 5;
        
        // 공격 범위와 속도 증가
        attackRange += 0.5f;
        attackSpeed -= 0.3f; // 공격 속도 증가 (쿨다운 감소)
        speed += 1.0f;
        
        // 현재 레벨에 맞게 스탯 업데이트
        UpdateDerivedStats();
        
        // 프리팹 업데이트 - prefab 필드 업데이트
        if (evolutionPrefabs != null && evolutionPrefabs.Length > (int)evolutionStage)
        {
            prefab = evolutionPrefabs[(int)evolutionStage];
        }
        
        // 이벤트 발생 - 컨트롤러에게 알림
        OnDragonEvolution?.Invoke(evolutionStage);
        
        Debug.Log($"{characterName}(이)가 {evolutionStage}단계로 진화했습니다!");
        return true;
    }

    // 직접 진화 단계 설정 (테스트/디버그용)
    public bool SetEvolutionStage(DragonEvolutionStage stage)
    {
        // 현재 단계와 동일하면 무시
        if (evolutionStage == stage) return false;
        
        // 이전 단계 저장
        DragonEvolutionStage oldStage = evolutionStage;
        
        // 새로운 단계 설정
        evolutionStage = stage;
        
        // 단계에 따른 이름 업데이트
        switch (stage)
        {
            case DragonEvolutionStage.Baby:
                characterName = "BabyDragon";
                break;
            case DragonEvolutionStage.Young:
                characterName = "YoungDragon";
                break;
            case DragonEvolutionStage.Adult:
                characterName = "AdultDragon";
                break;
        }
        
        // 프리팹 업데이트
        if (evolutionPrefabs != null && evolutionPrefabs.Length > (int)stage)
        {
            prefab = evolutionPrefabs[(int)stage];
        }
        
        // 스탯 업데이트
        UpdateDerivedStats();
        
        // 이벤트 발생
        OnDragonEvolution?.Invoke(stage);
        
        Debug.Log($"드래곤 진화 단계가 {oldStage}에서 {stage}로 변경되었습니다.");
        return true;
    }
    
    // ToStringForTMPro 함수에 진화 단계 정보 추가
    public string ToStringForTMPro()
    {
        // 기본 정보를 포맷하여 출력
        string baseInfo = string.Format(
            "<color=red>Name:</color> {0}\n" +
            "<color=red>Type:</color> {1}\n" +
            "<color=red>Evolution Stage:</color> {2}\n" +
            "<color=red>Bond Level:</color> {3}/{4}\n" +
            "<color=red>Strength:</color> {5}\n" +
            "<color=blue>Agility:</color> {6}\n" +
            "<color=green>Vitality:</color> {7}\n" +
            "<color=yellow>Intelligence:</color> {8}\n" +
            "<color=lime>Speed:</color> {9}\n" +
            "<color=cyan>Attack Speed:</color> {10}\n" +
            "<color=purple>Attack Range:</color> {11}\n" +
            "<color=orange>Bond Experience:</color> {12}/{13}\n" +
            "<color=magenta>Critical Chance:</color> {14}%\n" +
            "<color=teal>Critical Damage:</color> {15}\n" +
            "<color=lime>Physical Damage:</color> {16}\n" +
            "<color=cyan>Magic Damage:</color> {17}\n" +
            "<color=orange>Exp To Next Level:</color> {18}\n",
            characterName, // 0
            characterType.ToString(), // 1
            evolutionStage.ToString(), // 2
            bondLevel, // 3
            maxLevelPerStage[(int)evolutionStage], // 4 - 현재 진화 단계의 최대 레벨
            strength, // 5
            agility, // 6
            vitality, // 7
            intelligence, // 8
            speed, // 9
            attackSpeed, // 10
            attackRange, // 11
            bondExperience, // 12
            CalculateExperienceToLevelUp(bondLevel), // 13 - 경험치 계산 함수 사용
            criticalChance * 100, // 14
            criticalDamage, // 15
            physicalDamage, // 16
            magicDamage, // 17
            _requiredExpToNextLevel // 18 - 다음 레벨업에 필요한 경험치
        );
    
        return baseInfo;
    }

    public void ParseData(IList<object> row)
    {
        if (row.Count < 12) throw new Exception("드래곤 데이터 부족");

        characterName = row[0].ToString();
        characterType = Enum.TryParse(row[1].ToString(), out CharacterType type) ? type : CharacterType.Drogon;
        strength = int.TryParse(row[2].ToString(), out int str) ? str : 0;
        agility = int.TryParse(row[3].ToString(), out int agi) ? agi : 0;
        vitality = int.TryParse(row[4].ToString(), out int vit) ? vit : 0;
        intelligence = int.TryParse(row[5].ToString(), out int intl) ? intl : 0;

        speed = float.TryParse(row[6].ToString(), out float spd) ? spd : 1.0f;
        attackSpeed = float.TryParse(row[7].ToString(), out float atkSpd) ? atkSpd : 1.0f;
        attackRange = float.TryParse(row[8].ToString(), out float atkRange) ? atkRange : 1.0f;

        bondLevel = int.TryParse(row[9].ToString(), out int bondLvl) ? bondLvl : 1;
        bondExperience = int.TryParse(row[10].ToString(), out int bondExp) ? bondExp : 0;

        statModifier = new DragonStatModifier();
        evolutionStage = DragonEvolutionStage.Baby; // 기본 진화 단계는 아기용
        UpdateDerivedStats();
    }
}

[Serializable]
public class DragonStatModifier
{
    public float strengthMultiplier = 1.5f;      // 힘에 의한 물리 공격력 배율
    public float agilityMultiplier = 0.01f;      // 민첩성에 의한 크리티컬 확률 배율
    public float intelligenceMultiplier = 1.5f;  // 지능에 의한 마법 공격력 배율
    
    // 클론 메서드
    public DragonStatModifier Clone()
    {
        return new DragonStatModifier
        {
            strengthMultiplier = this.strengthMultiplier,
            agilityMultiplier = this.agilityMultiplier,
            intelligenceMultiplier = this.intelligenceMultiplier
        };
    }
}



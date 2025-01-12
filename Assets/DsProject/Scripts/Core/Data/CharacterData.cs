using System;
using UnityEngine;
using TMPro;

[Serializable]
public class StatModifier
{
    public float strengthMultiplier = 2f;     // 힘에 대한 공격력 증가 비율
    public float vitalityMultiplier = 10f;    // 체력에 대한 HP 증가 비율
    public float agilityMultiplier = 0.01f;  // 민첩성에 대한 크리티컬 확률 비율
    public float intelligenceMultiplier = 3f; // 지능에 대한 마법 공격력 비율
    
    // 방어력 계산 비율 (동적 설정)
    public float physicalDefenseMultiplier = 1f;  // 물리 방어력 계산 비율
    public float magicDefenseMultiplier = 2f;      // 마법 방어력 계산 비율
}

[Serializable]
public class CharacterData
{
    // 기본 스텟 (정수형)
    public int strength;     // 힘
    public int agility;      // 민첩
    public int vitality;     // 체력
    public int intelligence; // 지능
    
    // 이동 관련 스텟
    public float speed;      // 스피드 (이동 속도)
    public float attackSpeed; // 어택 스피드 (초당 공격 횟수)

    // 스태미나 관련
    public float stamina;     // 최대 스태미나
    public float staminaCurrent; // 현재 스태미나
    public float staminaRecoveryRate; // 스태미나 회복 속도

    // 계산된 값 (정수형)
    public int maxHp;        // 최대 HP
    public int currentHp;    // 현재 HP

    public int physicalDefense;  // 물리 방어력
    public int magicDefense;     // 마법 방어력
    public int physicalDamage;   // 물리 공격력
    public int magicDamage;      // 마법 공격력
    public float criticalChance; // 크리티컬 확률 (float)

    public int baseDamage;       // 기본 물리 공격력 (정수형)

    // StatModifier 설정 (각 캐릭터마다 별도로 설정)
    public StatModifier statModifier;
    
    private const int maxStats = 999;
    private const int minStats = 0;
    
    // CharacterData 생성 시 자동 계산
    public CharacterData(int strength, int agility, int vitality, int intelligence, StatModifier statModifier, 
        float speed, float attackSpeed, float stamina, float staminaRecoveryRate)
    {
        this.strength = strength;
        this.agility = agility;
        this.vitality = vitality;
        this.intelligence = intelligence;
        this.statModifier = statModifier;
        this.speed = speed;
        this.attackSpeed = attackSpeed;
        this.stamina = stamina;
        this.staminaCurrent = stamina; // 초기 스태미나는 최대 스태미나와 동일
        this.staminaRecoveryRate = staminaRecoveryRate;

        // 스텟 값에 따라 자동 계산
        UpdateDerivedStats();  // 모든 파생 스탯을 한 번에 계산
    }

    // 파생 스탯 계산 (중복을 줄이기 위해 한 번에 계산)
    public void UpdateDerivedStats()
    {
        maxHp = Mathf.RoundToInt(vitality * statModifier.vitalityMultiplier);  // 체력에 비례한 최대 HP
        physicalDamage = Mathf.RoundToInt(strength * statModifier.strengthMultiplier);  // 힘에 따른 물리 공격력
        physicalDefense = Mathf.RoundToInt((strength + vitality) * statModifier.physicalDefenseMultiplier);  // 힘과 체력에 비례한 물리 방어력
        magicDamage = Mathf.RoundToInt(intelligence * statModifier.intelligenceMultiplier);  // 지능에 따른 마법 공격력
        magicDefense = Mathf.RoundToInt(intelligence * statModifier.magicDefenseMultiplier);  // 지능에 따른 마법 방어력
        criticalChance = Mathf.Min(agility * statModifier.agilityMultiplier, 1f);  // 민첩성에 따른 크리티컬 확률
        baseDamage = physicalDamage + strength;  // 물리 공격력 + 힘
    }
    
    // 크리티컬 데미지 계산
    public int CalculateCriticalDamage()
    {
        return baseDamage * 2;  // 크리티컬 시 기본 데미지의 2배
    }

    // 크리티컬 여부를 판단하여 데미지 반환
    public int CalculateDamage(bool isCritical)
    {
        if (isCritical)
        {
            return CalculateCriticalDamage();
        }
        return baseDamage;  // 크리티컬이 아니면 기본 데미지
    }

    // 스텟 증가 함수들 (각각 한 번만 호출하고, 자동 계산)
    public void IncreaseStat(string statType, int amount)
    {
        // 음수 값이면 함수 종료
        if (amount < minStats) return;
        
        switch (statType)
        {
            case "strength":
                strength = Mathf.Min(strength + amount,  maxStats);
                break;
            case "vitality":
                vitality = Mathf.Min(vitality + amount,  maxStats);
                break;
            case "agility":
                agility = Mathf.Min(agility + amount,  maxStats);
                break;
            case "intelligence":
                intelligence = Mathf.Min(intelligence + amount,  maxStats);
                break;
        }

        // 증가 후 자동으로 파생 스탯 계산
        UpdateDerivedStats();
    }
    
    // 스텟 감소 함수들 (각각 한 번만 호출하고, 자동 계산)
    public void DecreaseStat(string statType, int amount)
    {
        // 음수 값이면 함수 종료
        if (amount > maxStats) return;
        
        switch (statType)
        {
            case "strength":
                strength = Mathf.Max(minStats, strength - amount);
                break;
            case "vitality":
                vitality = Mathf.Max(minStats, vitality - amount);
                break;
            case "agility":
                agility = Mathf.Max(minStats, agility - amount);
                break;
            case "intelligence":
                intelligence = Mathf.Max(minStats, intelligence - amount);
                break;
        }

        // 감소 후 자동으로 파생 스탯 계산
        UpdateDerivedStats();
    }
    
    // 피해를 입었을 때
    public void TakeDamage(int damage)
    {
        currentHp -= damage;
        currentHp = Mathf.Max(0, currentHp);  // currentHp가 0 이하로 떨어지지 않도록 처리
    }

    // 회복
    public void Heal(int amount)
    {
        currentHp += amount;
        currentHp = Mathf.Min(currentHp, maxHp);  // currentHp가 maxHp를 초과하지 않도록 처리
    }

    // 스태미나 회복 (주기적인 회복)
    public void RegenerateStamina()
    {
        staminaCurrent += Mathf.RoundToInt(staminaRecoveryRate);
        staminaCurrent = Mathf.Min(staminaCurrent, stamina);  // 스태미나는 최대치를 초과하지 않음
    }

    // 아이템을 사용했을 때 스태미나 회복
    public void UseItemForStamina(int recoveryAmount)
    {
        staminaCurrent += recoveryAmount;
        staminaCurrent = Mathf.Min(staminaCurrent, stamina); // 스태미나는 최대치를 초과하지 않도록 설정
    }
    
    // 스태미나 소모 (스킬 사용, 이동 등)
    public void UseStamina(int amount)
    {
        staminaCurrent -= amount;
        staminaCurrent = Mathf.Max(0, staminaCurrent);  // 스태미나는 0 이하로 떨어지지 않음
    }
    
    
    // ToString()을 오버라이드하여 TMP로 출력할 수 있는 형식으로 정보 제공
    public string ToStringForTMPro()
    {
        // ToString을 사용해 TextMeshPro 형식으로 출력 (색상 적용 가능)
        return $"<color=red>Strength:</color> {strength}\n" +
               $"<color=blue>Agility:</color> {agility}\n" +
               $"<color=green>Vitality:</color> {vitality}\n" +
               $"<color=yellow>Intelligence:</color> {intelligence}\n" +
               $"<color=orange>Max HP:</color> {maxHp}\n" +
               $"<color=purple>Physical Damage:</color> {physicalDamage}\n" +
               $"<color=cyan>Magic Damage:</color> {magicDamage}\n" +
               $"<color=magenta>Critical Chance:</color> {criticalChance * 100}%\n" +
               $"<color=gray>Base Damage:</color> {baseDamage}\n" +
               $"<color=lime>Stamina:</color> {staminaCurrent}/{stamina}";
    }
}

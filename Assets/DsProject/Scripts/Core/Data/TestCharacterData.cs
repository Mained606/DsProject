using System;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

// 캐릭터의 상위 타입을 정의
public enum CharacterType
{
    Player,   // 플레이어
    Monster,  // 일반 몬스터
    Boss,     // 보스
    Animal    // 동물 (특정 생태계 NPC 또는 몬스터)
}

// 동물(NPC 또는 비전투 생물)의 세부 유형을 정의
public enum AnimalType
{
    Rabbit,
    Deer,
    Boar,
    Dog
}

// 일반 몬스터의 세부 유형을 정의
public enum MonsterType
{
    Mushroom,
    MooGlet,
    Slime,
    Bear,
    ZombieGorilla,
    Devil,
    Golem,
    Bulldog,
    MophanSub1,
    MophanSub2
}

// 보스의 세부 유형을 정의
public enum BossType
{
    Mophan      // 중간 보스: 모파안
}

// StatModifier 클래스 정의: 각 캐릭터 스탯에 대한 멀티플라이어 설정
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

// CharacterData 클래스 정의: 캐릭터의 스탯, 레벨, 경험치 등을 관리
[Serializable]
public class CharacterData
{
    public string characterName;  // 캐릭터 이름
    public CharacterType characterType;  // 캐릭터 타입 추가
    
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
    
    // 레벨과 경험치 관련 변수 추가
    public int level;             // 레벨
    public int currentExperience; // 현재 경험치
    public int experienceToLevelUp; // 레벨업에 필요한 경험치
    
    // 보상 관련 변수
    public List<string> dropItems;  // 드롭 아이템 리스트
    public int experienceReward;    // 경험치 보상
    public int goldReward;        // 골드 보상
    
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
    
    // 스탯 Min & Max 관련 상수 - 추후 상수만 정리한 클래스 추가 필요
    private const int maxStats = 999;
    private const int minStats = 0;
    
    // 생성자: 캐릭터 초기화 및 자동 계산
    public CharacterData(string name, CharacterType type, int strength, int agility, int vitality, int intelligence,  
        float speed, float attackSpeed, float stamina, float staminaRecoveryRate)
    {
        this.characterName = name;
        this.characterType = type;  // 타입 저장
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

        this.level = 1; // 초기 레벨은 1
        this.currentExperience = 0; // 초기 경험치는 0
        experienceToLevelUp = CalculateExperienceToLevelUp(); // 첫 번째 레벨업에 필요한 경험치 설정
        this.dropItems = new List<string>();
        this.experienceReward = 0;
        this.goldReward = 0;

        // 스텟 값에 따라 자동 계산
        IncreaseStatsBasedOnLevel();
        //UpdateDerivedStats();  // 모든 파생 스탯을 한 번에 계산
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
    
    // 경험치를 얻었을 때 호출되는 함수
    public void GainExperience(int amount)
    {
        if (amount < 0) amount = 0; // 경험치가 음수일 경우 0으로 처리

        currentExperience += amount;

        // 경험치가 레벨업에 도달했으면 레벨업 처리
        if (currentExperience >= experienceToLevelUp)
        {
            LevelUp();
        }
    }

    // 레벨업 처리 함수
    private void LevelUp()
    {
        level++;  // 레벨 증가
        currentExperience = 0;  // 경험치 초기화
        experienceToLevelUp = CalculateExperienceToLevelUp();  // 새로운 레벨에 맞는 경험치 계산

        // 레벨업 시 능력치 증가 및 자동 계산
        IncreaseStatsBasedOnLevel();
        UpdateDerivedStats();  // 레벨업 후 파생 스탯 다시 계산

        // 레벨업 시 로그 출력 (디버깅용)
        Debug.Log($"레벨업! 현재 레벨: {level}");
    }
    
    // 경험치 계산 함수
    private int CalculateExperienceToLevelUp()
    {
        // 레벨 9 -> 10, 19 -> 20, 29 -> 30 구간에서 경험치를 완화
        if ((level) % 10 == 9)  // 레벨 9, 19, 29, 39, ...
        {
            return Mathf.RoundToInt(level * 80f);  // 완화된 구간: 경험치 요구량 80%로 설정
        }
        else
        {
            return Mathf.RoundToInt(level * 100f);  // 그 외 구간은 기본적으로 100씩 증가
        }
    }
    
    // 능력치 증가 처리 함수 (레벨에 따른 규칙을 설정)
    private void IncreaseStatsBasedOnLevel()
    {
        // 레벨에 따라 능력치를 동적으로 증가
        vitality += GetStatIncrease("vitality");
        strength += GetStatIncrease("strength");
        agility += GetStatIncrease("agility");
        intelligence += GetStatIncrease("intelligence");
    }
    
    // 각 스탯에 대해 증가량을 계산하는 함수
    private int GetStatIncrease(string statType)
    {
        // 레벨에 따른 스탯 증가 값 설정 (예시)
        switch (statType)
        {
            case "vitality":
                return level <= 10 ? 5 : 3;  // 레벨 10 이하일 때는 5 증가, 그 이상은 3 증가
            case "strength":
                return level <= 10 ? 2 : 1;  // 레벨 10 이하일 때는 2 증가, 그 이상은 1 증가
            case "agility":
                return 1;  // 민첩은 항상 1 증가
            case "intelligence":
                return 1;  // 지능도 항상 1 증가
            default:
                return 1;  // 기본값
        }
    }
    
    // 크리티컬 데미지 계산
    public int CalculateCriticalDamage()
    {
        return baseDamage * 2;  // 크리티컬 시 기본 데미지의 2배
    }

    // 크리티컬 여부를 판단하여 데미지 반환
    public int CalculateDamage(bool isCritical)
    {
        return isCritical ? CalculateCriticalDamage() : baseDamage;  // 크리티컬이면 크리티컬 데미지 반환, 아니면 기본 데미지
    }

    // 스텟 증가 함수들
    public void IncreaseStat(string statType, int amount)
    {
        if (amount < minStats) return;

        switch (statType)
        {
            case "strength":
                strength = Mathf.Min(strength + amount, maxStats);
                break;
            case "vitality":
                vitality = Mathf.Min(vitality + amount, maxStats);
                break;
            case "agility":
                agility = Mathf.Min(agility + amount, maxStats);
                break;
            case "intelligence":
                intelligence = Mathf.Min(intelligence + amount, maxStats);
                break;
        }

        // 증가 후 자동으로 파생 스탯 계산
        UpdateDerivedStats();
    }
    
    // 스텟 감소 함수들
    public void DecreaseStat(string statType, int amount)
    {
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

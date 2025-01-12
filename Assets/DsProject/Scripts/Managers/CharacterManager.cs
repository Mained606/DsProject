using System.Collections.Generic;
using UnityEngine;

public enum CharacterType
{
    Player,   // 플레이어
    Monster,  // 일반 몬스터
    Boss      // 보스
}

public enum MonsterType
{
    wildBoar,      // 야생 멧돼지
    lesserDemon,   // 약한 악마
    devil          // 강력한 악마
}

public enum BossType
{
    Mophan         // 중간 보스: 모파안
}

public class CharacterManager : BaseManager<CharacterManager>
{
    // 캐릭터 데이터 리스트 (플레이어 + 몬스터/보스)
    private List<CharacterData> characterList = new List<CharacterData>();

    // 플레이어 캐릭터 (온리 원)
    private CharacterData playerCharacter;
    
    // 플레이어 캐릭터 생성
    public void CreatePlayerCharacter(int strength, int agility, int vitality, int intelligence, 
        StatModifier statModifier, float speed, float attackSpeed, 
        float stamina, float staminaRecoveryRate)
    {
        playerCharacter = new CharacterData(name, CharacterType.Player, strength, agility, vitality, intelligence, 
            statModifier, speed, attackSpeed, stamina, staminaRecoveryRate);
        characterList.Add(playerCharacter);  // 플레이어 캐릭터를 리스트에 추가
    }
    
    // 몬스터 생성 (여러 마리의 동일한 몬스터 추가)
    public void SpawnMonsters(int numberOfMonsters, MonsterType monsterType, StatModifier statModifier, 
        float speed, float attackSpeed, float stamina, float staminaRecoveryRate)
    {
        for (int i = 0; i < numberOfMonsters; i++)
        {
            // 몬스터의 기본 스탯 설정
            var monsterStats = GetMonsterStats(monsterType);
            CharacterData newMonster = new CharacterData("Monster_" + i, CharacterType.Monster, monsterStats.strength, 
                monsterStats.agility, monsterStats.vitality, 
                monsterStats.intelligence, statModifier, speed, attackSpeed, 
                stamina, staminaRecoveryRate);
            characterList.Add(newMonster);  // 새로운 몬스터를 리스트에 추가
        }
    }
    
    // 보스 생성 (보스는 별도의 생성 방식)
    public void SpawnBoss(BossType bossType, StatModifier statModifier, float speed, float attackSpeed, 
        float stamina, float staminaRecoveryRate)
    {
        var bossStats = GetBossStats(bossType);
        CharacterData newBoss = new CharacterData(bossType.ToString(), CharacterType.Boss, bossStats.strength, 
            bossStats.agility, bossStats.vitality, bossStats.intelligence, 
            statModifier, speed, attackSpeed, stamina, staminaRecoveryRate);
        characterList.Add(newBoss);  // 새로운 보스를 리스트에 추가
    }
    
    // 몬스터 타입에 따른 기본 스탯 반환
    private (int strength, int agility, int vitality, int intelligence) GetMonsterStats(MonsterType monsterType)
    {
        switch (monsterType)
        {
            case MonsterType.wildBoar:
                return (strength: 10, agility: 5, vitality: 15, intelligence: 2);
            case MonsterType.lesserDemon:
                return (strength: 20, agility: 8, vitality: 18, intelligence: 5);
            case MonsterType.devil:
                return (strength: 30, agility: 12, vitality: 25, intelligence: 8);
            default:
                return (strength: 0, agility: 0, vitality: 0, intelligence: 0);
        }
    }
    
    // 보스 타입에 따른 기본 스탯 반환
    private (int strength, int agility, int vitality, int intelligence) GetBossStats(BossType bossType)
    {
        switch (bossType)
        {
            case BossType.Mophan:
                return (strength: 100, agility: 30, vitality: 200, intelligence: 50);
            default:
                return (strength: 0, agility: 0, vitality: 0, intelligence: 0);
        }
    }
    
    // 특정 인덱스의 캐릭터 반환 (플레이어 또는 몬스터)
    public CharacterData GetCharacter(int index)
    {
        if (index >= 0 && index < characterList.Count)
        {
            return characterList[index];
        }
        else
        {
            Debug.LogError("잘못된 캐릭터 인덱스");
            return null;
        }
    }

    // 모든 캐릭터 리스트 반환 (디버깅용)
    public List<CharacterData> GetAllCharacters()
    {
        return characterList;
    }
    
    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        // 게임 상태가 Combat 상태로 바뀔 때 몬스터를 스폰할 수 있음
        if (newState == GameSystemState.Combat)
        {
            // 예시: 보스와 몬스터를 스폰
            SpawnMonsters(5, MonsterType.wildBoar, new StatModifier(), 4f, 1f, 80f, 1.5f);
            SpawnBoss(BossType.Mophan, new StatModifier(), 2f, 0.5f, 150f, 2f);
        }
    }
}
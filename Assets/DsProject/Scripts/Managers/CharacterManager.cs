using System.Collections.Generic;
using UnityEngine;

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

public class CharacterManager : BaseManager<CharacterManager>
{
    // 캐릭터 데이터 리스트 (플레이어 + 몬스터/보스)
    private List<CharacterData> characterList = new List<CharacterData>();

    // 플레이어 캐릭터 (온리 원)
    private CharacterData playerCharacter;

    // ScriptableObject로부터 로드된 캐릭터 템플릿 리스트
    [SerializeField]
    private CharacterList characterTemplates;

    // 특정 인덱스의 캐릭터 반환 (플레이어 또는 몬스터)
    public CharacterData GetCharacterFromTemplate(int index)
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

    // 캐릭터 생성 함수: 템플릿을 복사하여 새로운 캐릭터 생성
    public CharacterData CreateCharacterFromTemplate(string templateName, string characterName = null)
    {
        // 템플릿에서 이름에 해당하는 캐릭터 데이터 검색
        CharacterData template = characterTemplates.characters.Find(c => c.characterName == templateName);

        if (template == null)
        {
            Debug.LogError($"Character template '{templateName}'을(를) 찾을 수 없습니다.");
            return null;
        }

        // 템플릿을 기반으로 새로운 캐릭터 생성 (딥 클론)
        CharacterData newCharacter = new CharacterData(
            characterName ?? template.characterName,
            template.characterType,
            template.prefab,
            template.strength,
            template.agility,
            template.vitality,
            template.intelligence,
            template.statModifier,
            template.speed,
            template.attackSpeed,
            template.stamina,
            template.staminaRecoveryRate
        );
        
        // 드롭 아이템과 경험치 보상 복사
        newCharacter.dropItems = new List<string>(template.dropItems);
        newCharacter.experienceReward = template.experienceReward;
        newCharacter.goldReward = template.goldReward;

        // 리스트에 새 캐릭터 추가
        characterList.Add(newCharacter);
        Debug.Log($"캐릭터 '{newCharacter.characterName}'클론 생성 완료.");

        return newCharacter;
    }

    // 플레이어 초기화
    public void InitializePlayer(string templateName, string playerName)
    {
        if (playerCharacter != null)
        {
            Debug.LogError("플레이어 캐릭터가 이미 초기화되었습니다.");
            return;
        }

        playerCharacter = CreateCharacterFromTemplate(templateName, playerName);
        if (playerCharacter != null)
        {
            Debug.Log($"플레이어 캐릭터 '{playerCharacter.characterName}' 초기화 완료.");
        }
    }

    // 몬스터 생성 함수 (템플릿 기반)
    public CharacterData SpawnMonster(string templateName, Vector3 spawnPosition)
    {
        CharacterData monster = CreateCharacterFromTemplate(templateName);

        if (monster != null)
        {
            Debug.Log($"몬스터 '{monster.characterName}' 생성 완료. 스폰 위치: {spawnPosition}");
        }

        return monster;
    }
    
    // 몬스터 처치 처리
    public void OnMonsterDefeated(CharacterData monster)
    {
        if (monster == null)
        {
            Debug.LogError("몬스터 데이터가 없습니다.");
            return;
        }

        // 플레이어 경험치 증가
        if (playerCharacter != null)
        {
            playerCharacter.GainExperience(monster.experienceReward);
            Debug.Log($"{monster.characterName} 처치! 경험치 +{monster.experienceReward}");
        }

        // 드롭 아이템 처리
        foreach (string item in monster.dropItems)
        {
            Debug.Log($"드롭된 아이템: {item}");
            // 실제 게임에서는 아이템 매니저를 호출해 인벤토리에 추가
        }
        
        // 골드 보상 처리
        int goldEarned = monster.goldReward;
        Debug.Log($"{monster.characterName} 처치! 골드 +{goldEarned}");

        // 몬스터 제거
        characterList.Remove(monster);
        Debug.Log($"{monster.characterName} 제거 완료.");
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        // 게임 상태 변경에 따른 처리 로직 구현 필요
    }
}
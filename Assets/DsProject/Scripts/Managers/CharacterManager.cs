using System;
using System.Collections.Generic;
using Unity.VisualScripting;
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
    Mushroom, // 초보용
    MooGlet, // 초보용
    Slime, // 초보용
    Bear, // 일반 
    ZombieGorilla, // 일반 
    Devil, // 중간급
    Golem,  // 중간급
    Bulldog,  // 중간급
    MophanS,  // 보스
    MophanSub1, // 보스부하
    MophanSub2 // 보스부하
}

// 보스의 세부 유형을 정의
public enum BossType
{
    Mophan      // 중간 보스: 모파안
}

public class CharacterManager : MonoBehaviour
{
    // 캐릭터 데이터 리스트 (플레이어 + 몬스터/보스)
    public static CharacterManager Instance;
    private List<CharacterData> characterList = new List<CharacterData>();
    public static IReadOnlyList<CharacterData> CharacterList => Instance.characterList.AsReadOnly();
    // 플레이어 캐릭터 (온리 원)
    public static  PlayerData PlayerCharacterData;

    // ScriptableObject로부터 로드된 캐릭터 템플릿 리스트
    [SerializeField]
    private CharacterList characterTemplates;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        PlayerCharacterData = characterTemplates.players[0];

        PlayerCharacterData.statModifier = new StatModifier();
        PlayerCharacterData.UpdateDerivedStats();
        Debug.Log(PlayerCharacterData.ToStringForTMPro());
    }
    //
    // private void Start()
    // {
    //     SpawnMonster("MophanS", Vector3.zero);
    // }

    public void InitialCharacter()
    {
        PlayerCharacterData = new PlayerData(
            name: "Hero",
            prefab: null, // 플레이어 프리팹 (Unity 에디터에서 할당 가능)
            strength: 10,
            vitality: 15,
            agility: 8,
            intelligence: 12,
            speed: 5.0f,
            attackSpeed: 1.2f,
            stamina: 100f,
            staminaRecoveryRate: 10f
        );

    }

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
    public MonsterData CreateCharacterFromTemplate(string Name, string characterName = null)
    {
        // 템플릿에서 이름에 해당하는 캐릭터 데이터 검색
        MonsterData template = characterTemplates.monsters.Find(c => c.characterName == Name);

        if (template == null)
        {
            Debug.LogError($"Character template '{Name}'을(를) 찾을 수 없습니다.");
            return null;
        }

        // 템플릿을 메모리에서 복제 (독립적인 인스턴스 생성)
        MonsterData cloned = template.Clone();
        cloned.IncreaseStatsBasedOnLevel();
        cloned.InitializeStats();
        cloned.UpdateDerivedStats();

        // 복제된 템플릿의 이름을 변경 (옵션)
        if (!string.IsNullOrEmpty(characterName))
        {
            cloned.characterName = characterName;
        }

        // 리스트에 복제된 캐릭터 추가 (필요 시)
        characterList.Add(cloned);

        Debug.Log($"캐릭터 템플릿 '{cloned.characterName}' 생성 완료.");
        return cloned;
    }

    // 몬스터 생성 함수 (템플릿 기반)
    public void SpawnMonster(string templateName, Vector3 spawnPosition)
    {
        MonsterData monster = CreateCharacterFromTemplate(templateName);

        if (monster != null)
        {
            Debug.Log($"몬스터 '{monster.characterName}' 생성 완료. 스폰 위치: {spawnPosition}");
            var monster1 = Instantiate(monster.prefab, spawnPosition, Quaternion.identity);
            monster1.transform.AddComponent<Test1>();
            monster1.GetComponent<Test1>().monster = monster;
        }
    }

    // 몬스터 처치 처리
    public void OnMonsterDefeated(MonsterData monster, Vector3 position)
    {
        // 플레이어 경험치 증가
        if (PlayerCharacterData != null)
        {
            PlayerCharacterData.GainExperience(monster.experienceReward);
            Debug.Log($"{monster.characterName} 처치! 경험치 +{monster.experienceReward}");
        }
        
        if (PlayerCharacterData != null )
        {
            PlayerCharacterData.AddGold(monster.goldReward); // 플레이어의 골드 추가
            Debug.Log(PlayerCharacterData.gold);
        }
        
        ItemManager.Instance.SpawnItemBox(position, monster, false);
        
        // 몬스터 제거
        characterList.Remove(monster);
        Debug.Log($"{monster.characterName} 제거 완료.");
    }
}
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    // 메타데이터
    public string saveDateTime; // 저장 날짜 및 시간
    public string playerName;   // 플레이어 이름
    public int playTime;        // 플레이 시간(초)
    
    // 플레이어 데이터
    public PlayerSaveData playerData;
    
    // 용 데이터
    public DragonSaveData dragonData;
    
    // 퀘스트 데이터
    public QuestSaveData questData;
    
    // 인벤토리 데이터
    public InventorySaveData inventoryData;
    
    // 게임 진행 상태 데이터
    public GameProgressData progressData;
    
    // 생성자
    public SaveData()
    {
        saveDateTime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        playerData = new PlayerSaveData();
        dragonData = new DragonSaveData();
        questData = new QuestSaveData();
        inventoryData = new InventorySaveData();
        progressData = new GameProgressData();
    }
}

[System.Serializable]
public class PlayerSaveData
{
    // 기본 정보
    public string characterName;
    public int level;
    public float currentExperience;
    public float experienceToLevelUp;
    
    // 기본 스탯
    public int strength;
    public int agility;
    public int vitality;
    public int intelligence;
    
    // 파생 스탯과 리소스
    public int currentHp;
    public int maxHp;
    public int currentMp;
    public int maxMp;
    public float staminaCurrent;
    public float stamina;
    
    // 전투 관련 스탯
    public float physicalDamage;
    public float magicDamage;
    public float physicalDefense;
    public float magicDefense;
    public float criticalChance;
    public float criticalDamage;
    public float dodgeChance;
    public float blockChance;
    public ElementalAttribute attribute;
    
    // 스킬 및 포인트
    public List<string> skills = new List<string>();
    public int availableSkillPoints;
    public int availableStatPoints;
    
    // 퀵슬롯 정보
    public QuickSlotSaveData quickSlotData = new QuickSlotSaveData();
    
    // 장착 중인 장비 정보
    public List<EquipmentSaveData> equippedItems = new List<EquipmentSaveData>();
    
    // 골드
    public int gold;
    
    // 이동 위치
    public Vector3SaveData position;
    public Vector3SaveData rotation;
}

[System.Serializable]
public class DragonSaveData
{
    public string characterName;
    public DragonEvolutionStage evolutionStage;
    public int bondLevel;
    public int bondExperience;
    
    // 용 스탯
    public int strength;
    public int agility;
    public int vitality;
    public int intelligence;
    
    public float criticalChance;
    public float criticalDamage;
    public int physicalDamage;
    public int magicDamage;
    
    public ElementalAttribute dragonAttribute;
}

[System.Serializable]
public class QuestSaveData
{
    public List<QuestData> activeQuests = new List<QuestData>();
    public List<string> completedQuestIds = new List<string>();
}

[System.Serializable]
public class QuestData
{
    public string questId;
    public int currentStep;
    public List<QuestObjectiveData> objectives = new List<QuestObjectiveData>();
}

[System.Serializable]
public class QuestObjectiveData
{
    public string objectiveId;
    public int currentCount;
    public int requiredCount;
    public bool isCompleted;
}

[System.Serializable]
public class InventorySaveData
{
    public List<ItemSaveData> items = new List<ItemSaveData>();
}

[System.Serializable]
public class ItemSaveData
{
    public string itemId;
    public int quantity;
    public ItemType itemType;
}

[System.Serializable]
public class EquipmentSaveData
{
    public string itemId;
    public EquipmentSlot slot;
    // 장비 특성
    public List<EquipmentStatData> stats = new List<EquipmentStatData>();
}

[System.Serializable]
public class EquipmentStatData
{
    public string statName;
    public float value;
}

[System.Serializable]
public class QuickSlotSaveData
{
    public List<QuickSlotItemData> skillSlots = new List<QuickSlotItemData>();
    public List<QuickSlotItemData> potionSlots = new List<QuickSlotItemData>();
}

[System.Serializable]
public class QuickSlotItemData
{
    public string itemId;
    public int slotIndex;
}

[System.Serializable]
public class GameProgressData
{
    public string currentScene;
    public List<string> unlockedAreas = new List<string>();
    public List<string> discoveredLocations = new List<string>();
    public List<string> defeatedBosses = new List<string>();
}

[System.Serializable]
public class Vector3SaveData
{
    public float x;
    public float y;
    public float z;
    
    public Vector3SaveData() { }
    
    public Vector3SaveData(Vector3 vector)
    {
        x = vector.x;
        y = vector.y;
        z = vector.z;
    }
    
    public Vector3 ToVector3()
    {
        return new Vector3(x, y, z);
    }
} 
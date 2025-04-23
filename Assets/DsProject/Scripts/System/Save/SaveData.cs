using System;
using System.Collections.Generic;
using UnityEngine;

namespace DsProject.Scripts.System.Save
{
    [Serializable]
    public class SaveData
    {
        public string saveDate; // 저장 일시
        public string savePointId; // 마지막 저장 지점 ID
        
        // 플레이어 데이터
        public PlayerData playerData = new PlayerData();
        
        // 인벤토리 데이터
        public InventoryData inventoryData = new InventoryData();
        
        // 스킬 데이터
        public SkillData skillData = new SkillData();
        
        // 퀘스트 데이터
        public QuestData questData = new QuestData();
        
        // 용 관련 데이터
        public DragonData dragonData = new DragonData();
        
        // 저장 가능 오브젝트 데이터 (SaveableObject 컴포넌트가 있는 오브젝트)
        public SaveableObjectsData saveableObjects = new SaveableObjectsData();
        
        [Serializable]
        public class PlayerData
        {
            // 위치 및 회전 정보
            public SerializableVector3 position;
            public SerializableQuaternion rotation;
            
            // 기본 스탯
            public float maxHealth;
            public float currentHealth;
            public float maxMana;
            public float currentMana;
            public float maxStamina;
            public float currentStamina;
            
            // 성장 스탯
            public int level;
            public float experience;
            public int statPoints;
            
            // 기본 능력치
            public int strength;
            public int intelligence;
            public int dexterity;
            public int vitality;
            
            // 파생 스탯
            public float attackPower;
            public float magicPower;
            public float defense;
            public float evasion;
            
            // 골드 추가
            public int gold;
            
            // 버프 관련 필드 추가
            public int hpBuffBonus;
            public float physicalDamageBuffMultiplier;
            public float magicDamageBuffMultiplier;
            
            // 글라이딩 기능 잠금 해제 상태
            public bool unlockGlide;
            
            // 버프 지속시간 정보를 저장할 리스트
            public List<BuffSaveData> activeBuffs = new List<BuffSaveData>();
        }
        
        // 버프 데이터를 저장하기 위한 클래스 추가
        [Serializable]
        public class BuffSaveData
        {
            public string buffId; // 버프 ID
            public float remainingDuration; // 남은 지속시간
            public float cooldownRemaining; // 남은 쿨타임
        }
        
        [Serializable]
        public class InventoryData
        {
            // 보유 아이템 목록
            public List<ItemInfo> items = new List<ItemInfo>();
            
            // 장착 중인 장비
            public ItemInfo weapon;
            public ItemInfo armor;
            public ItemInfo accessory;
            
            // 퀵슬롯에 등록된 아이템
            public List<ItemInfo> quickSlotItems = new List<ItemInfo>();
            
            [Serializable]
            public class ItemInfo
            {
                public string itemId;
                public int count;
                public int slotIndex;
                public int itemLevel;
            }
        }
        
        [Serializable]
        public class SkillData
        {
            // 습득한 스킬 목록
            public List<SkillInfo> unlockedSkills = new List<SkillInfo>();
            
            // 퀵슬롯에 등록된 스킬
            public List<string> quickSlotSkills = new List<string>();
            
            [Serializable]
            public class SkillInfo
            {
                public string skillId;
                public int level;
            }
        }
        
        [Serializable]
        public class QuestData
        {
            // 활성화된 퀘스트 목록
            public List<QuestInfo> activeQuests = new List<QuestInfo>();
            
            // 완료된 퀘스트 목록
            public List<string> completedQuests = new List<string>();
            
            [Serializable]
            public class QuestInfo
            {
                public string questId;
                public int progress; // 하위 호환성을 위해 유지 (이전 저장 데이터용)
                public bool isTracking;
                
                // 조건별 진행 상태를 저장하기 위한 컬렉션
                public List<ConditionProgressInfo> conditionProgress = new List<ConditionProgressInfo>();
            }
            
            [Serializable]
            public class ConditionProgressInfo
            {
                public string conditionId;
                public int progress;
                public bool isCompleted;
                
                public ConditionProgressInfo(string id, int prog, bool completed)
                {
                    conditionId = id;
                    progress = prog;
                    isCompleted = completed;
                }
                
                public ConditionProgressInfo() { }
            }
        }
        
        [Serializable]
        public class DragonData
        {
            public SerializableVector3 position;
            public SerializableQuaternion rotation;
            public int level;
            public float currentHealth;
            public float maxHealth;
            public List<string> unlockedAbilities = new List<string>();
            public bool isUnlocked;
            public bool isActive;
            public int evolutionStage; // 추가: 드래곤 진화 단계 (0: Baby, 1: Young, 2: Adult)
            
            // 기본 스탯들
            public int strength;       // 힘
            public int agility;        // 민첩
            public int vitality;       // 건강
            public int intelligence;   // 지능
            
            // 이동 및 공격 관련 스탯
            public float speed;        // 이동 속도
            public float attackSpeed;  // 공격 속도
            public float attackRange;  // 기본 공격 범위
            
            // 경험치 관련
            public int bondExperience;          // 유대 경험치
            public int requiredExpToNextLevel;  // 다음 레벨 필요 경험치
            
            // 계산된 스탯들
            public int physicalDamage;   // 물리 공격력
            public int magicDamage;      // 마법 공격력
            public float criticalChance;  // 크리티컬 확률
            public float criticalDamage;  // 크리티컬 데미지 배율
            
            // 원소 속성
            public int dragonAttribute;  // 드래곤 속성 (0: None, 1: Fire, 2: Water, ...)
            
            // 스탯 배율 (DragonStatModifier 직접 저장은 복잡하므로 개별 값으로 저장)
            public float strengthMultiplier;      // 힘 배율
            public float agilityMultiplier;       // 민첩 배율
            public float intelligenceMultiplier;  // 지능 배율
        }
        
        [Serializable]
        public class SaveableObjectsData
        {
            // 저장 가능한 오브젝트 목록
            public List<ObjectInfo> objects = new List<ObjectInfo>();
            
            [Serializable]
            public class ObjectInfo
            {
                public string id;
                public bool isActive;
                public SerializableVector3 position;
                public SerializableQuaternion rotation;
            }
        }
    }

    // Vector3와 Quaternion을 직렬화하기 위한 헬퍼 클래스
    [Serializable]
    public struct SerializableVector3
    {
        public float x;
        public float y;
        public float z;
        
        public SerializableVector3(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }
        
        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
        
        public static implicit operator Vector3(SerializableVector3 sVector)
        {
            return sVector.ToVector3();
        }
        
        public static implicit operator SerializableVector3(Vector3 vector)
        {
            return new SerializableVector3(vector);
        }
    }

    [Serializable]
    public struct SerializableQuaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;
        
        public SerializableQuaternion(Quaternion quaternion)
        {
            x = quaternion.x;
            y = quaternion.y;
            z = quaternion.z;
            w = quaternion.w;
        }
        
        public Quaternion ToQuaternion()
        {
            return new Quaternion(x, y, z, w);
        }
        
        public static implicit operator Quaternion(SerializableQuaternion sQuaternion)
        {
            return sQuaternion.ToQuaternion();
        }
        
        public static implicit operator SerializableQuaternion(Quaternion quaternion)
        {
            return new SerializableQuaternion(quaternion);
        }
    }
} 
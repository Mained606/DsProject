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
                public int progress;
                public bool isTracking;
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
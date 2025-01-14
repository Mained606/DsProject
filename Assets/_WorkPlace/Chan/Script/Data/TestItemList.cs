using UnityEngine;
using System.Collections.Generic;

public class TestItemList : MonoBehaviour
{
    [SerializeField]public List<Item> items = new List<Item>();
    void Awake()
    {
        InitializeItems();
    }
    void InitializeItems()
    {
        // 무기 아이템
        Item sword = new Item
        {
            itemId = 1,
            itemName = "철검",
            itemDescription = "기본적인 철제 검",
            itemType = ItemType.Weapon,
            value = 100,
            isDiscardable = true,
            weaponType = WeaponType.Melee,
            attackPower = 15,
            durability = 100,
            itemImage = null // 임시로 null 설정
        };
        sword.Initialize();
        items.Add(sword);

        // 방어구 아이템
        Item helmet = new Item
        {
            itemId = 2,
            itemName = "철제 헬멧",
            itemDescription = "기본적인 철제 헬멧",
            itemType = ItemType.Armor,
            value = 80,
            isDiscardable = true,
            equipmentSlot = EquipmentSlot.Head,
            defensePower = 10,
            durability = 100,
            itemImage = null
        };
        helmet.Initialize();
        items.Add(helmet);

        // 소모품 아이템
        Item healthPotion = new Item
        {
            itemId = 3,
            itemName = "체력 포션",
            itemDescription = "사용 시 체력을 회복합니다.",
            itemType = ItemType.Consumable,
            value = 25,
            consumableType = ConsumableType.HealthPotion,
            effectAmount = 50,
            itemImage = null
        };
        healthPotion.Initialize();
        items.Add(healthPotion);

        // 장신구 아이템
        Item ring = new Item
        {
            itemId = 4,
            itemName = "강화 반지",
            itemDescription = "능력치를 강화하는 반지",
            itemType = ItemType.Accessory,
            value = 150,
            
            itemImage = null
        };
        ring.Initialize();
        //ring.statModifiers.Add("Strength", 5);
        //ring.statModifiers.Add("Agility", 3);
        items.Add(ring);

        // 퀘스트 아이템
        Item questItem = new Item
        {
            itemId = 5,
            itemName = "마법사의 유물",
            itemDescription = "특정 퀘스트에 필요한 유물",
            itemType = ItemType.QuestItem,
            value = 0,
            questId = "QST_001",
            itemImage = null
        };
        questItem.Initialize();
        items.Add(questItem);
    }

    public List<Item> GetTestItems()
    {
        return items; // 생성한 아이템 리스트 반환
    }
}
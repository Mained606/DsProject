//using System;
//using UnityEngine;


////아이템 항목
//[Serializable]
//public class Item
//{
//    [Header("아이템 공통 속성")]
//    public int itemId;                  //아이템 고유번호
//    public string itemName;             //아이템 이름
//    public string itemDescription;      //아이템 설명
//    public ItemType itemType;           //아이템 타입
//    public int currentQuantity = 0;     //현재 수량
//    public int maxCapacity;             //아이템 최대 소지 갯수
//    public bool isCanStack;             //여러개 소지 가능한 아이템 여부
//    public int value;                   //가치
//    public bool isDiscardable;          //버릴 수 있는 아이템인지 여부
//    public float itemDropChance = 1.0f; //아이템 드랍 확률
//    public Sprite itemImage;            //아이템 이미지

//    [Header("장착용 아이템 공통 속성")]
//    public Attribute[] attribute = new Attribute[0];

//    [Header("무기 속성")]
//    public WeaponProperty weaponProperty = new WeaponProperty();

//    [Header("방어구 속성")]
//    public ArmorProperty armorProperty = new ArmorProperty();

//    [Header("소모품 속성")]
//    public ConsumableProperty consumableProperty = new ConsumableProperty();

//    [Header("퀘스트아이템 속성")]
//    public QuestItemProperty questItemProperty = new QuestItemProperty();

//    //초기화
//    public void Initialize()
//    {
//        //공통
//        maxCapacity = 1;
//        isCanStack = false;
//        isDiscardable = true;

//        switch (itemType)
//        {
//            //소모품
//            case ItemType.Consumable:
//                maxCapacity = 99;       //소모품 기본 최대 소지 갯수
//                isCanStack = true;      //여러개 소지 가능
//                break;
//            //퀘스트 아이템
//            case ItemType.QuestItem:
//                isDiscardable = false; //퀘스트 아이템은 버릴 수 없음
//                break;
//        }
//    }
//}

//#region Serializable Class
//[Serializable]
//public class Attribute
//{
//    public CharacterAttribute characterAttribute;   //능력
//    public float amount;                            //능력 값
//}

//[Serializable]
//public class WeaponProperty
//{    
//    public int attackPower;             //공격력
//    public int durability;              //내구도
//    public WeaponType weaponType;       //무기 타입

//    //생성자
//    public WeaponProperty()
//    {
//        attackPower = 10;      //기본 공격력
//        durability = 100;      //기본 내구도
//    }
//}

//[Serializable]
//public class ArmorProperty
//{
//    public int defensePower;            //방어도
//    public int durability;              //내구도
//    public EquipmentSlot equipmentSlot; //장착 위치

//    //생성자
//    public ArmorProperty()
//    {
//        defensePower = 5;      //기본 방어도
//        durability = 100;      //기본 내구도
//    }
//}

//[Serializable]
//public class ConsumableProperty
//{
//    public ConsumableType consumableType;   //소모품 타입
//    public int effectAmount;                //효과량
//}

//[Serializable]
//public class QuestItemProperty
//{
//    public int questId;                  //퀘스트 ID
//}
//#endregion

//#region ItemEnum
//public enum ItemType
//{
//    Weapon,
//    Armor,
//    Consumable,
//    Accessory,
//    QuestItem
//}

//public enum CharacterAttribute
//{
//    Strength,
//    Vitality,
//    Agility,
//    Intelligence
//}

//public enum WeaponType { Melee, Magic }
//public enum EquipmentSlot { Head, Body, Hand, Feet }
//public enum ConsumableType { None, HealthPotion, ManaPotion, Food }
//#endregion

using System.Collections.Generic;
using System;
using UnityEngine;

namespace HJ
{
    [Serializable]
    public class Item
    {
        [Header("아이템 공통 속성")]
        public int itemId;                  //아이템 고유번호
        public string itemName;             //아이템 이름
        public string itemDescription;      //아이템 설명
        public ItemType itemType;           //아이템 타입
        public int maxCapacity;             //아이템 최대 소지 갯수
        public bool isCanStack;             //여러개 소지 가능한 아이템 여부
        public int currentQuantity = 0;     //현재 수량
        public int value;                   //가치
        public bool isDiscardable;          //버릴 수 있는 아이템인지 여부
        public GameObject itemPrefab;       //아이템 프리팹 객체

        [Header("무기/방어구 속성")]
        //무기 속성
        public int attackPower;             //공격력
        public int durability;              //내구도
        public WeaponType weaponType;       //무기 타입

        //방어구 속성
        public int defensePower;            //방어도
        public EquipmentSlot equipmentSlot; //장착 위치

        [Header("소모품 속성")]
        public ConsumableType consumableType;   //소모품 타입
        public int effectAmount;                //효과량

        [Header("장신구 속성")]
        public Dictionary<string, int> statModifiers;   //능력치 변화

        [Header("퀘스트아이템 속성")]
        public string questId;                  //퀘스트 ID

        //초기화
        public void Initialize()
        {
            //공통
            maxCapacity = 1;
            isCanStack = false;
            isDiscardable = true;

            switch (itemType)
            {
                //무기
                case ItemType.Weapon:
                    attackPower = 10;      //기본 공격력
                    durability = 100;      //기본 내구도
                    break;
                //방어구
                case ItemType.Armor:
                    defensePower = 5;      //기본 방어도
                    durability = 100;      //기본 내구도
                    break;
                //소모품
                case ItemType.Consumable:
                    maxCapacity = 99;       //소모품 기본 최대 소지 갯수
                    isCanStack = true;      //여러개 소지 가능
                    break;
                //장신구
                case ItemType.Accessory:
                    statModifiers = new Dictionary<string, int>();
                    break;
                //퀘스트 아이템
                case ItemType.QuestItem:
                    isDiscardable = false; //퀘스트 아이템은 버릴 수 없음
                    break;
            }
        }
    }

    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Accessory,
        QuestItem
    }

    public enum WeaponType { Melee, Magic }
    public enum EquipmentSlot { Head, Body, Hand, Feet }
    public enum ConsumableType { None, HealthPotion, ManaPotion }

}
using System.Collections.Generic;
using System;
using UnityEngine;

namespace HJ
{
    //아이템 항목
    [Serializable]
    public class Item
    {
        public int itemId;                  //아이템 고유번호
        public string itemName;             //아이템 이름
        public string itemDescription;      //아이템 설명
        public int maxCapacity;             //아이템 최대 소지 갯수
        public bool isCanStack;             //여러개 소지 가능한 아이템 여부
        public int currentQuantity = 0;     //현재 수량
        public int value;                   //가치
        public bool isDiscardable;          //버릴 수 있는 아이템인지 여부
        public string itemPrefabName;       //아이템 프리팹 이름

        [NonSerialized]
        public GameObject itemPrefab;       //아이템 프리팹 객체

        //소지 가능 갯수 초기화
        public virtual void Initialize()
        {
            maxCapacity = 1;
            isCanStack = false;
            isDiscardable = true;
        }
    }

    //무기
    [Serializable]
    public class Weapon : Item
    {
        public int attackPower;             //공격력
        public int durability;              //내구도
        public WeaponType weaponType;       //무기 타입

        public override void Initialize()
        {
            base.Initialize();
            durability = 100;               //기본 내구도 설정
            attackPower = 10;               //기본 공격력
        }
    }

    //방어구
    public class Armor : Item
    {
        public int defensePower;            //방어도
        public int durability;              //내구도
        public EquipmentSlot equipmentSlot; //장착 위치

        public override void Initialize()
        {
            base.Initialize();
            durability = 100;               //기본 내구도 설정
            defensePower = 5;               //기본 방어도
        }
    }

    //소모품
    public class Consumable : Item
    {
        public ConsumableType consumableType; //소모품 타입
        public int effectAmount;              //효과량

        public override void Initialize()
        {
            base.Initialize();
            maxCapacity = 99;                //소모품 기본 최대 소지 갯수
            isCanStack = true;
        }
    }

    //장신구
    public class Accessory : Item
    {
        public Dictionary<string, int> statModifiers; //능력치 변화

        public override void Initialize()
        {
            base.Initialize();
        }
    }

    //퀘스트 아이템
    [Serializable]
    public class QuestItem : Item
    {
        public string questId; //연결된 퀘스트 ID

        public override void Initialize()
        {
            base.Initialize();
            isDiscardable = false;
        }
    }

    public enum WeaponType { Melee, Magic }
    public enum EquipmentSlot { Head, Body, Hand, Feet }
    public enum ConsumableType { None, HealthPotion, ManaPotion }
}
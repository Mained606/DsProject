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
        public ItemType itemType;           //아이템 타입
        public string itemPrefabName;       //아이템 프리팹 이름

        [NonSerialized]
        public GameObject itemPrefab;       //아이템 프리팹 객체

        //소지 가능 갯수 초기화
        public void Initialize()
        {
            //아이템 타입별 최대 소지갯수 설정
            switch(itemType)
            {
                case ItemType.Weapon:
                case ItemType.Armor:
                case ItemType.Quest:
                    maxCapacity = 1;
                    isCanStack = false;
                    break;
                default:
                    maxCapacity = 99;
                    isCanStack = true;
                    break;
            }
        }
    }


    //아이템 타입
    public enum ItemType
    {
        Weapon,
        Armor,
        Consumable,
        Quest,
        Etc
    }
}
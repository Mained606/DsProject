using System;
using UnityEngine;

namespace HJ
{
    //임시 아이템 항목
    [Serializable]
    public class Item
    {
        public string itemName;             //아이템 이름
        public int itemId;                  //아이템 고유번호
        public string itemDescription;      //아이템 설명
        public int maxCapacity;             //아이템 최대 소지 갯수
        public int currentQuantity;         //현재 수량
        public ItemType itemType;           //아이템 타입
        public string itemPrefabPath = "Asset/HJ/Prefab";       //아이템 프리팹
        //public float weight;              //아이템 무게
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
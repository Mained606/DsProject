using System;
using UnityEngine;

namespace HJ
{
    [Serializable]
    public class Item
    {
        public string ItemName { get; set; }            //아이템 이름
        public int ItemId { get; set; }                 //아이템 고유번호
        public string ItemDescription { get; set; }     //아이템 설명
        public int MaxCapacity { get; set; }            //아이템 최대 소지 갯수
        public float Weight { get; set; }               //아이템 무게
        public ItemType ItemType { get; set; }          //아이템 타입
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
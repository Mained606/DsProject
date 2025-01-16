using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class ShopData
{
    public string shopName;             //상점이름
    public ItemType type;               //상점이 다루는 아이템 타입
    public ItemGrade grade;             //상점이 다루는 아이템 등급
    public List<Item> availableItems;   //판매 아이템 리스트
    public bool isInteractable = true;  //활성화 여부
    public bool isSpecific = false;     //특정 아이템 판매하는 상점 여부
    public List<string> addToItemIds = null;    //추가할 아이템 리스트

    public void Initialize()
    {
        Debug.Log("상점 초기화");
        UpdateAvailableItems();
    }

    //아이템 업데이트
    public void UpdateAvailableItems()
    {
        availableItems = ItemManager.Instance.GetItemsByTypeAndGrade(type, grade);

        if (isSpecific)
        {
            var speicificItem = ItemManager.Instance.GetSpeificItem(addToItemIds);
            if (speicificItem != null)
            {
                foreach (var item in speicificItem)
                {
                    if (!availableItems.Contains(item))
                    {
                        availableItems.Add(item);
                    }
                }
            }
        }
    }
}

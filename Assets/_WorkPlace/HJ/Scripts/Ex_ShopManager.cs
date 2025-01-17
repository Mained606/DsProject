using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class ShopItem
{
    public Item item;
    public int quantity;
    public ShopItem(Item item, int quantity)
    {
        this.item = item;
        this.quantity = quantity;
    }
}
public class Ex_ShopManager
{
    #region Variables
    [Range(0, 1)]
    [SerializeField] private float sellDepreciationRate = 0.7f; //판매시 감가상각률
    private int playerMoney = 10000; //테스트용 임시변수
    public List<ShopItem> items;
    #endregion

    /// <summary>
    /// 아이템 구매
    /// </summary>
    public void PurchaseItem(string itemId, List<ShopItem> shopItems, int npcMoney, int quantitiy = 1)
    {
        var shopItem = shopItems.FirstOrDefault(i => i.item.id == itemId);

        //아이템이 상점에 없으면
        if (shopItem == null)
        {
            Debug.Log($"{itemId}가 상점에 없음");
            return;
        }

        //상점에 아이템 수량이 부족하면
        if (shopItem.quantity < quantitiy)
        {
            Debug.Log($"{itemId} 수량이 부족함");
            return;
        }

        int amount = shopItem.item.costValue * quantitiy;

        //플레이어의 소지금액이 부족하면
        if (playerMoney < amount)
        {
            Debug.Log("플레이어 소지금액 부족");
            return;
        }

        //구매
        playerMoney -= amount;  //플레이어 소지금액 감소
        npcMoney += amount;     //npc 소지금액 증가
        RemoveShopItem(itemId, shopItems, quantitiy);   //상점에서 아이템 제거
        InventoryManager.Instance.AddItem(itemId, quantitiy); //인벤토리에 아이템 추가
        Debug.Log($"{shopItem.item.name} 구매하고 남은 돈: {playerMoney}");
    }


    /// <summary>
    /// 아이템 판매
    /// </summary>
    public void SellItem(string itemId, List<ShopItem> shopItems, ref int npcMoney, int quantity = 1)
    {
        var item = InventoryManager.InventoryList.FirstOrDefault(i => i.id == itemId);

        //인벤토리에 해당 아이템이 없으면
        if (item == null)
        {
            Debug.Log($"인벤토리에 {itemId} 아이템 없음");
            return;
        }

        //인벤토리에 있는 전체 아이템 수량
        int totalQuantity = InventoryManager.InventoryList.Where(i => i.id == itemId).Sum(i => i.quantity);

        //인벤토리내 수량보다 판매 수량이 부족하면
        if (totalQuantity < quantity)
        {
            Debug.Log($"{itemId} 수량 부족");
            return;
        }

        int amount = (int)(item.costValue * quantity * sellDepreciationRate);

        //npc의 소지금액이 부족하면
        if (npcMoney < amount)
        {
            Debug.Log("npc 소지금액 부족");
            return;
        }

        //아이템 판매
        npcMoney -= amount;         //npc 소지금액 감소
        playerMoney += amount;      //플레이어 소지금액 증가
        InventoryManager.Instance.RemoveItemLogic(itemId, quantity);    //인벤토리 아이템 제거
        AddShopItem(itemId, shopItems, quantity); //상점에 아이템 추가
        Debug.Log($"{item.name} 판매하고 남은 돈: {playerMoney}");
    }

    /// <summary>
    /// 상점에 아이템 추가
    /// </summary>
    public void AddShopItem(string itemId, List<ShopItem> shopItems, int quantity = 1)
    {
        //상점에 이미 존재하는 아이템인지 찾기
        var item = ItemManager.Instance.GetItemById(itemId);
        var existingItem = shopItems.Find(i => i.item == item);

        if (existingItem != null && item.isStackable)    //존재하고 스택가능하면
        {
            int totalQuantity = existingItem.quantity + quantity;

            if (totalQuantity > item.maxStack)   //최대 수량을 초과하면 클론해서 상점아이템 리스트에 추가
            {
                int remainingQuantity = totalQuantity - item.maxStack;
                existingItem.quantity = item.maxStack;
                Debug.Log($"상점 {item.name}의 수량이 최대치 {item.maxStack}에 도달, 초과된 수량: {remainingQuantity}");

                var newItem = item.Clone();
                newItem.quantity = remainingQuantity;
                ShopItem newShopItem = new ShopItem(newItem, remainingQuantity);
                shopItems.Add(newShopItem);
            }
            else
            {
                existingItem.quantity = totalQuantity;  //최대 수량보다 적으면 수량만 추가
                Debug.Log($"상점에 {item.name}이 추가됨, 현재 수량: {totalQuantity}");
            }
        }
        else if (!item.isStackable)
        {
            ShopItem newShopItem = new ShopItem(item, quantity);
            shopItems.Add(newShopItem);
            Debug.Log($"상점에 {item.name}이 추가됨, 스택 불가능한 아이템");
        }
        else
        {
            ShopItem newShopItem = new ShopItem(item, quantity);
            shopItems.Add(newShopItem);
            Debug.Log($"상점에 {item.name}이 추가됨, 처음 추가된 아이템");
        }
    }

    /// <summary>
    /// 상점에서 아이템 제거
    /// </summary>
    public void RemoveShopItem(string itemId, List<ShopItem> shopItems, int quantity = 1)
    {
        //삭제할 아이템 리스트(99개 이상 있어서 여러개 존재할 경우 대비)
        var itemsToRemove = shopItems.Where(i => i.item.id == itemId).ToList();

        //삭제할 아이템이 존재하지 않으면
        if (itemsToRemove.Count == 0)
        {
            Debug.Log($"상점에 {itemId} 존재하지 않음");
            return;
        }

        foreach (var itemToRemove in itemsToRemove)
        {
            if (quantity <= 0) break;

            if (itemToRemove.quantity > quantity)
            {
                itemToRemove.quantity -= quantity;
                Debug.Log($"상점 {itemToRemove.item.name}이 {quantity}개 제거됨");
                quantity = 0;
            }
            else
            {
                quantity -= itemToRemove.quantity;
                shopItems.Remove(itemToRemove);
                Debug.Log($"상점 {itemToRemove.item.name}이 제거됨 (아이템 전체 제거)");
            }
        }
        if (quantity > 0)
        {
            Debug.LogWarning($"상점 {itemId} 수량 부족, 아이템 제거할 수 없음");
        }
    }
}

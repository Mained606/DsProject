using System.Collections.Generic;
using UnityEngine;

public class ShopManager : BaseManager<ShopManager>
{
    [Range(0,1)]
    [SerializeField] private float discount = 0.5f;
    private int playerMoney = 10000; //테스트용 임시변수

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }

    /// <summary>
    /// 아이템 구매
    /// </summary>
    public void PurchaseItem(Item item, List<Item> shopItems, int quantitiy = 1)
    {
        //상점에 해당 아이템이 있는지 확인
        if (shopItems.Contains(item))
        {
            //플레이어 소지 금액 확인
            if (playerMoney >= (item.value * quantitiy))
            {
                playerMoney -= (item.value * quantitiy);    //플레이어 돈 감소
                RemoveShopItem(item, shopItems, quantitiy);     //상점에서 아이템 제거
                InventoryManager.Instance.AddItem(item, quantitiy); //인벤토리에 아이템 추가
                Debug.Log($"{item.itemName} 구매하고 남은 돈: {playerMoney}");
            }
            else
            {
                Debug.Log("돈부족");
            }
        }
        else
        {
            Debug.Log("아이템이 상점에 없음");
        }
    }


    /// <summary>
    /// 아이템 판매
    /// </summary>
    public void SellItem(Item item, List<Item> shopItems , int quantity = 1)
    {
        //인벤토리에 해당 아이템이 있는지 확인
        if (InventoryManager.Instance.items.Contains(item))
        {
            playerMoney += (item.value * quantity);                 //플레이어 돈 증가
            InventoryManager.Instance.RemoveItem(item, quantity);   //인벤토리 아이템 제거
            AddShopItem(item); //상점에 아이템 추가
            Debug.Log($"{item.itemName} 판매하고 남은 돈: {playerMoney}");
        }
        else
        {
            Debug.Log("인벤토리에 아이템 없음");
        }
    }

    /// <summary>
    /// 상점애서 아이템 제거
    /// </summary>
    public void RemoveShopItem(Item item, List<Item> shopItems, int quantity = 1)
    {
        // 상점의 아이템 리스트에서 해당 아이템 찾기
        Item existingItem = shopItems.Find(c => c.itemId == item.itemId);

        //아이템이 없으면 함수 종료
        if (existingItem == null)
            return;

        //스택 가능한 아이템이고 수량이 충분할 경우, 수량만 감소
        if (existingItem.isCanStack && existingItem.currentQuantity > quantity)
        {
            //existingItem.currentQuantity = Mathf.Max(0, existingItem.currentQuantity - quantity);
            //return;

            //데이터 currentQuantity가 아닌 상점에서의 현재 수량이 조정되도록 수정해야함
        }

        //아이템의 수량이 부족하거나 스택 불가능한 경우 리스트에서 제거
        shopItems.Remove(existingItem);
    }

    /// <summary>
    /// 상점애 아이템 추가
    /// </summary>
    public void AddShopItem(Item item)
    {

    }
}

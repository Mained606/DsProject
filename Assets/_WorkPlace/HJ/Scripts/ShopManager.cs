using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopManager : BaseManager<ShopManager>
{
    #region Variables
    [SerializeField] private List<ShopData> shops = new List<ShopData>();
    public static IReadOnlyList<ShopData> shopsDataList => Instance.shops;

    [SerializeField] private int playerMoney = 1000; //테스트용 임시변수
    #endregion

    // 상점 오픈
    public void OpenShop(ShopData shopData)
    {
        if (shopData == null || !shopData.isInteractable)
        {
            Debug.Log("상점 비활성화 상태");
            return;
        }

        //UI관련 로직
    }

    //아이템 구매
    public void PurchaseItem(string itemId, ShopData data, int quantity = 1)
    {
        var purchaseItem = data.availableItems.Find(i => i.id == itemId);


        int amount = purchaseItem.costValue * quantity;

        if (playerMoney < amount)
        {
            Debug.Log("플레이어 소지 금액 부족");
            return;
        }

        //구매
        playerMoney -= amount;
        InventoryManager.Instance.AddItem(itemId, quantity);

        Debug.Log($"{purchaseItem.name} 아이템 구매하고 남은 플레이어 소지금액: {playerMoney}");
    }

    //아이템 판매
    public void SellItem(string itemId, ShopData data, float valueReductionRate, int quantity = 1)
    {
        var sellItem = InventoryManager.InventoryList.FirstOrDefault(i => i.id == itemId);

        //인벤토리에 있는 전체 아이템 수량
        int totalQuantity = InventoryManager.InventoryList.Where(i => i.id == itemId).Sum(i => i.quantity);

        if(totalQuantity < quantity)
        {
            Debug.Log($"{sellItem.name} 아이템 수량 부족");
            return;
        }

        //판매시 가치 감소를 반영한 전체 금액
        int amount = ((int)(sellItem.costValue * valueReductionRate) * quantity);

        Debug.Log($"판매금액 : {amount}");

        //판매
        playerMoney += amount;
        Debug.Log($"{sellItem.name} 아이템 판매한 후 플레이어 소지금액: {playerMoney}");

        InventoryManager.Instance.RemoveItemLogic(itemId, quantity);
    }


    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

    }
}

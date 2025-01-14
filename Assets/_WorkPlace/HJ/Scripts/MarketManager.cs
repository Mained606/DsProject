using UnityEngine;
using static UnityEditor.Timeline.Actions.MenuPriority;

public class MarketManager : BaseManager<MarketManager>
{
    [SerializeField] private float discount = 0.5f;

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }

    /// <summary>
    /// 사기
    /// </summary>
    public void BuyItem(Item item, int quantity = 1)
    {
        //플레이어 돈 감소
        //플레이어 돈 -= item.value;

        //인벤토리에 아이템 배치
        InventoryManager.Instance.AddItem(item, quantity);
    }


    /// <summary>
    /// 팔기
    /// 판매할때는 가치를 discount만큼 깎기
    /// </summary>
    public void SellItem(Item item, int quantity = 1)
    {
        //플레이어 돈 증가
        int value = (int)(1 - (item.value * discount));
        //플레이어 돈 += value;

        //인벤토리에서 아이템 삭제
        InventoryManager.Instance.RemoveItem(item, quantity);
    }
}

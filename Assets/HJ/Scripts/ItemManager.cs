using UnityEngine;
using System.Collections.Generic;

public class ItemManager : BaseManager<ItemManager>
{
    #region Variables
    public Transform player;                    //플레이어
    public ItemDatabase itemDatabase;           //아이템 데이터베이스

    public GameObject itemDropBox;              //itemdropBox 프리팹
    //[SerializeField] private float detectionDistance = 1f;      //플레이어 감지 거리
    #endregion

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        
    }

    /// <summary>
    /// id로 아이템 찾기
    /// </summary>
    public Item FindItemById(int itemId)
    {
        Item dropItem = itemDatabase.items.Find(c => c.itemId == itemId);

        return dropItem;
    }

    /// <summary>
    /// 아이템 사용
    /// </summary>
    public void UseItem(Item item, int quantity = 1)
    {
        InventoryManager.Instance.RemoveItem(item, quantity);
    }

    /// <summary>
    /// 아이템 박스 생성
    /// </summary>
    public void SpawnItemBox(Vector3 spawnPosition, List<int> dropItemIds)
    {
        GameObject itemBox = Instantiate(itemDropBox, spawnPosition, Quaternion.identity);

        DropItemBoxController dropItemBoxController = itemBox.GetComponent<DropItemBoxController>();
        if (dropItemBoxController)
        {
            dropItemBoxController.SetItems(dropItemIds);
        }
    }

    /// <summary>
    /// 아이템 드랍
    /// </summary>
    public void DropItem(Item item, int quantity = 1)
    {
        //인벤토리에서 해당 아이템 삭제
        InventoryManager.Instance.RemoveItem(item, quantity);
    }

    /// <summary>
    /// 인벤토리에 아이템 배치
    /// </summary>
    public void AddItemToInventory(Item item)
    {
        InventoryManager.Instance.AddItem(item);
    }

    #region PrivateMathod
    /// <summary>
    /// 플레이어가 근처에 있는지 감지
    /// </summary>
    //private bool IsNearPlayer()
    //{
    //    float disatance = Vector3.Distance(transform.position, player.position);

    //    return disatance <= detectionDistance;
    //}
    #endregion
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : BaseManager<ItemManager>
{
    [SerializeField] private ItemList itemList;
    [SerializeField] private GameObject dropItemPrefab;

    public static List<Item> ItemDatabase => Instance.itemList.itemList;

    protected override void Awake()
    {
        base.Awake();
        //GenerateData generateData = new GenerateData();
        //generateData.InitializeItems(itemList);
    }

    public void AddItemLogic(string itemId, int quantity = 1)
    {
        var item = GetItemById(itemId);
        item.quantity = item.quantity > 0 ? item.quantity : quantity;
        if (item != null)
        {
            InventoryManager.Instance.AddItemLogic(item);
        }
        else
        {
            Debug.LogWarning($"[ItemManager] 아이템 ID '{itemId}'를 데이터베이스에서 찾을 수 없습니다.");
        }
    }

    public void RemoveItemLogic(string itemId, int quantity = 1)
    {
        var item = GetItemById(itemId);
        if (item != null)
        {
            InventoryManager.Instance.RemoveItemLogic(item.id, quantity);
            Debug.Log($"[ItemManager] 아이템 '{item.name}' {quantity}개 제거 완료");
        }
        else
        {
            Debug.LogWarning($"[ItemManager] 아이템 ID '{itemId}'를 데이터베이스에서 찾을 수 없습니다.");
        }
    }


    public Item GetItemById(string itemId)
    {
        var item = ItemDatabase.FirstOrDefault(i => i.id == itemId);
        if (item == null)
        {
            Debug.LogWarning($"[ItemManager] ID: {itemId} 아이템을 찾을 수 없습니다.");
        }
        return item;
    }
    public void UseItem(Item item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("[InventoryManager] 사용할 아이템이 존재하지 않습니다.");
            return;
        }
        if (InventoryManager.Instance.GetItemQuantity(item.id) < quantity)
        {
            Debug.LogWarning($"[InventoryManager] '{item.name}'의 수량이 부족합니다. 요청: {quantity}, 현재: {InventoryManager.Instance.GetItemQuantity(item.id)}");
            return;
        }
        Debug.Log($"[InventoryManager] '{item.name}' 아이템 {quantity}개 사용");
        InventoryManager.Instance.RemoveItemLogic(item.id, quantity);

        // TODO
        // 아이템 사용 효과 적용 (예: 체력 회복 등) 효과 구현연결
        // ApplyItemEffect(item, quantity); 이런식의 연결함수 구현이 필요.
    }

    public void DropItem(Item item, int quantity = 1)
    {
        if (item == null)
        {
            Debug.LogWarning("[InventoryManager] 버릴 아이템이 존재하지 않습니다.");
            return;
        }
        if (InventoryManager.Instance.GetItemQuantity(item.id) < quantity)
        {
            Debug.LogWarning($"[InventoryManager] '{item.name}'의 수량이 부족합니다. 요청: {quantity}, 현재: {InventoryManager.Instance.GetItemQuantity(item.id)}");
            return;
        }
        Debug.Log($"[InventoryManager] '{item.name}' 아이템 {quantity}개 버림");
        InventoryManager.Instance.RemoveItemLogic(item.id, quantity);

        // TODO
        // 추가 드롭 연출(필요 시)
        // CreateDroppedItemInWorld(item, quantity); ; 이런식의 연결함수 구현이 필요.
    }


    public GameObject SpawnItemBox(Vector3 spawnPosition, MonsterData monsterData, bool isRandom = true)
    {
        GameObject itemBox = Instantiate(dropItemPrefab, spawnPosition, Quaternion.identity);
        if (!isRandom)
        {
            itemBox.transform.GetComponent<DropItemBoxController>().isRandomDrop = false;
            itemBox.transform.GetComponent<DropItemBoxController>().dropItemIds = monsterData.dropItems;
            Debug.Log(monsterData.dropItems.Count);
        }

        return itemBox;
    }

    protected override void HandleGameStateChange(global::GameSystemState newState, object additionalData)
    {

    }
}

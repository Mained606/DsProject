using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : BaseManager<InventoryManager>
{
    [SerializeField] private List<Item> inventory = new List<Item>();
    public static IReadOnlyList<Item> InventoryList => Instance.inventory;

    //최대 수용한도
    [SerializeField] private int maxCapacity = 99;
    //아이템 수용한도
    public int CurrentCapacity { get; set; } = 30;
    public bool IsCanUseInventory => inventory.Count < CurrentCapacity;

    public void AddItem(string itemId, int quantity = 1)
    {
        if (!IsCanUseInventory) return;
        var item = ItemManager.Instance.GetItemById(itemId).Clone();
        if (item != null)
        {
            item.quantity = quantity;
            AddItemLogic(item);
        }
        else
        {
            Debug.LogWarning($"[InventoryManager] 아이템 ID '{itemId}'를 데이터베이스에서 찾을 수 없습니다.");
        }
    }

    public void AddItemLogic(Item item)
    {
        if (!IsCanUseInventory) return;

        var existingItem = inventory.Find(i => i.id == item.id);

        // 스택 가능한 아이템 처리
        if (existingItem != null && existingItem.isStackable)
        {
            int totalQuantity = existingItem.quantity + item.quantity;

            if (totalQuantity > item.maxStack)
            {
                int remainingQuantity = totalQuantity - item.maxStack;
                existingItem.quantity = item.maxStack;
                UIManager.SystemGameMessage($"[InventoryManager] '{item.name}'의 수량이 최대치 {item.maxStack}에 도달했습니다. 초과된 수량: {remainingQuantity}", MessageTag.아이템_획득);

                var newItem = item.Clone();
                newItem.quantity = remainingQuantity;
                AddItemLogic(newItem);
            }
            else
            {
                existingItem.quantity = totalQuantity;
                UIManager.SystemGameMessage($"[InventoryManager] '{item.name}' 추가됨 (현재 수량: {existingItem.quantity})", MessageTag.아이템_획득);
            }
        }
        else if (!item.isStackable)
        {
            inventory.Add(item);
            UIManager.SystemGameMessage($"[InventoryManager] '{item.name}' 추가됨 (스택 불가 아이템)", MessageTag.아이템_획득);
        }
        else
        {
            inventory.Add(item);
            UIManager.SystemGameMessage($"[InventoryManager] '{item.name}' 추가됨 (새 아이템)", MessageTag.아이템_획득);
        }
        QuestManager.Instance.UpdateQuestProgress( QuestConditionType.Collect, item.id ,item.quantity);
        UIManager.Instance.PickUpItemTextDisplay?.AddItem(item.name, item.sprite);
    }

    public void RemoveItemLogic(string itemId, int quantity = 1)
    {
        var itemsToRemove = inventory.Where(i => i.id == itemId).ToList();

        if (itemsToRemove.Count == 0)
        {
            UIManager.SystemGameMessage($"[InventoryManager] 아이템 '{itemId}'를 소지하고 있지 않습니다.", MessageTag.아이템_획득);
            return;
        }

        foreach (var item in itemsToRemove)
        {
            if (quantity <= 0) break;

            if (item.quantity > quantity)
            {
                item.quantity -= quantity;
                UIManager.SystemGameMessage($"[InventoryManager] 아이템 '{item.name}' {quantity}개 제거됨. 남은 수량: {item.quantity}", MessageTag.아이템_획득);
                quantity = 0;
            }
            else
            {
                quantity -= item.quantity;
                inventory.Remove(item);
                UIManager.SystemGameMessage($"[InventoryManager] 아이템 '{item.name}' 제거됨 (스택 전체 제거)", MessageTag.아이템_획득);
            }
        }
        if (quantity > 0)
        {
            Debug.LogWarning($"[InventoryManager] 아이템 '{itemId}'를 {quantity}개 제거하려 했으나, 수량이 부족합니다.");
        }
        UIManager.Instance.InventoryUpdate();
    }


    public int GetItemQuantity(string itemId)
    {
        Item existingItem = inventory.FirstOrDefault(i => i.id == itemId);
        return existingItem != null ? existingItem.quantity : 0;
    }

    public bool HasQuestItem(string itemId, int requiredQuantity)
    {
        var item = inventory.FirstOrDefault(i => i.id == itemId);
        return item != null && item.quantity >= requiredQuantity;
    }

    public void ClearInventory()
    {
        if (inventory.Count > 0)
        {
            inventory.Clear();
        }
    }

    public int GetRemainingInventory()
    {
        return Mathf.Max(CurrentCapacity - inventory.Count, 0);
    }

    public void ExpandInventory(int expandSize)
    {
        int previousCapacity = CurrentCapacity;
        CurrentCapacity = Mathf.Min(CurrentCapacity + expandSize, maxCapacity);
        UIManager.SystemGameMessage($"[InventoryManager] 인벤토리가 확장되었습니다. 이전 용량: {previousCapacity}, 현재 용량: {CurrentCapacity}/{maxCapacity}", MessageTag.아이템_획득);
    }

    public void AddQuestRewardItem(string itemId, int quantity)
    {
        AddItem(itemId, quantity);
        UIManager.SystemGameMessage($"[InventoryManager] 퀘스트 보상으로 '{itemId}' 아이템이 {quantity}개 추가되었습니다.", MessageTag.아이템_획득);
    }

    public void PrintInventory()
    {
        string inventoryText = "";
        foreach (var item in inventory)
        {
            string gradeColor = item.GetGradeColor(item.grade);
            inventoryText += $"- <b><color={gradeColor}>{item.name}</color></b> (ID: <i>{item.id}</i>, <color=#00FF00>수량: {item.quantity}</color>)\n";
        }
        Debug.Log(inventoryText);
    }

    protected override void HandleGameStateChange(global::GameSystemState newState, object additionalData)
    {
    }
}

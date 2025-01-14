using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace JWS
{
    public class InventoryManager : BaseManager<InventoryManager>
    {
        [SerializeField] private List<Item> inventory = new List<Item>();
        public static IReadOnlyList<Item> InventoryList => Instance.inventory;

        public void AddItem(string itemId, int quantity = 1)
        {
            var item = ItemManager.Instance.GetItemById(itemId).CreateNew();
            if (item != null)
            {
                item.quantity = quantity;
                AddItemLogic(item);
                Debug.Log($"[InventoryManager] 아이템 '{item.name}' {quantity}개 추가됨");
            }
            else
            {
                Debug.LogWarning($"[InventoryManager] 아이템 ID '{itemId}'를 데이터베이스에서 찾을 수 없습니다.");
            }
        }

        public void AddItemLogic(Item item)
        {
            var existingItem = inventory.Find(i => i.id == item.id);

            if (existingItem != null && existingItem.isStackable)
            {
                Debug.Log($"[InventoryManager] 아이템 '{item.name}' {item.quantity}개 추가됨");
                existingItem.quantity += item.quantity;
            }
            else
            {
                inventory.Add(item);
            }
            QuestManager.Instance.UpdateQuestProgress(item);
        }


        public void RemoveItemLogic(string itemId, int quantity = 1)
        {
            var item = inventory.Find(i => i.id == itemId);

            if (item != null)
            {
                if (item.quantity > quantity)
                {
                    item.quantity -= quantity;
                    Debug.Log($"[InventoryManager] 아이템 '{item.name}' {quantity}개 제거됨. 남은 수량: {item.quantity}");
                }
                else
                {
                    inventory.Remove(item);
                    Debug.Log($"[InventoryManager] 아이템 '{item.name}' 제거됨 (모두 사용)");
                }
            }
            else
            {
                Debug.LogWarning($"[InventoryManager] 아이템 '{itemId}'를 소지하고 있지 않습니다.");
            }
        }

        public int GetItemQuantity(string itemId)
        {
            Item existingItem = inventory.FirstOrDefault(i => i.id == itemId);
            return existingItem != null ? existingItem.quantity : 0;
        }

        public bool HasItem(string itemId)
        {
            return inventory.Exists(i => i.id == itemId);
        }

        public void ClearInventory()
        {
            if (inventory.Count > 0)
            {
                inventory.Clear();
                Debug.Log("[InventoryManager] 인벤토리가 비워졌습니다.");
            }
            else
            {
                Debug.Log("[InventoryManager] 인벤토리가 비어있습니다.");
            }
        }

        public void PrintInventory()
        {
            Debug.Log("[InventoryManager] 현재 인벤토리 상태:");
            foreach (var item in inventory)
            {
                Debug.Log($"- {item.name} (ID: {item.id}, 수량: {item.quantity})");
            }
        }
        protected override void HandleGameStateChange(global::GameSystemState newState, object additionalData)
        {

        }
    }
}
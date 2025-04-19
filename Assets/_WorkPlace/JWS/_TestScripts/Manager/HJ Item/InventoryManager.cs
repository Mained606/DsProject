using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryManager : BaseManager<InventoryManager>
{
    [SerializeField] private List<Item> inventory = new List<Item>();
    [SerializeField] private QuickSlotsUI quickSlotsUI;

    public static IReadOnlyList<Item> InventoryList => Instance.inventory;

    [SerializeField] private int maxCapacity = 99;  // 인벤토리 가방 최대수용량
    public int CurrentCapacity { get; set; } = 30;   // 현재 수용량
    public bool IsCanUseInventory => inventory.Count < CurrentCapacity;

    public static QuickSlotsUI QuickSlotsUI => Instance.quickSlotsUI;




    //강화 테스트용
    //public Item selectedItem;   //03.12 HJ 추가

    ////03.12 HJ 추가
    //public void SetSelectedItem(Item item)
    //{
    //    selectedItem = item;
    //    //Debug.Log($"selectedItem: {selectedItem.name}");
    //}

    ////03.12 HJ 추가
    //public void ResetSelectedItem()
    //{
    //    if (selectedItem == null) return;
    //    selectedItem = null;
    //    //Debug.Log("selectedItem 리셋");
    //}
    //



    public void AddItemLogic(Item addItem)
    {
        if (!IsCanUseInventory)
        {
            UIManager.SystemMessage($"인벤토리를 사용할 수 없는 상태입니다.");
            return;
        }

        var existingItem = FindInventoryItem(addItem.id);
        if (existingItem != null && existingItem.isStackable)
        {
            HandleStackableItem(existingItem, addItem);
        }
        else
        {
            HandleNonStackableItem(addItem);
        }
        UIManager.Instance.InventoryUpdate();
        UpdateQuickSlotQuantity();
    }

    /// JWS  /////////////////////////////////////////
    public Item FindInventoryItem(string itemId)
    {
        return inventory.Find(i => i.id == itemId);
    }
    /// ///////////////////////////////////////////////


    /// JWS  /////////////////////////////////////////
    public bool HasItem(string itemId)
    {
        return inventory.Find(i => i.id == itemId) != null;
    }
    /// ///////////////////////////////////////////////


    private void HandleStackableItem(Item existingItem, Item addItem)
    {
        int totalQuantity = existingItem.quantity + addItem.quantity;
        int remainingQuantity = totalQuantity;

        while (remainingQuantity > addItem.maxStack)
        {
            existingItem.quantity = addItem.maxStack;
            remainingQuantity -= addItem.maxStack;
            Item newItem = addItem.Clone();
            newItem.quantity = addItem.maxStack;
            if (!AddNewItemToInventory(newItem))
            {
                UIManager.SystemMessage($"인벤토리가 가득 찼습니다. '{newItem.name}' 추가 불가.");
                return;
            }
            UIManager.SystemGameMessage($"'{newItem.name}' 추가됨 (현재 수량: {newItem.quantity})", MessageTag.아이템_획득);
        }
        if (remainingQuantity > 0)
        {
            existingItem.quantity = remainingQuantity;
            UIManager.SystemGameMessage($"'{existingItem.name}' 추가됨 (최종 수량: {existingItem.quantity})", MessageTag.아이템_획득);
        }

        UpdateQuestAndUI(existingItem);
    }

    private void HandleNonStackableItem(Item addItem)
    {
        if (!AddNewItemToInventory(addItem))
        {
            UIManager.SystemMessage($"인벤토리가 가득 찼습니다. '{addItem.name}' 추가 불가.");
            return;
        }

        string message = addItem.isStackable
            ? $"[InventoryManager] '{addItem.name}' 추가됨 (새 아이템)"
            : $"[InventoryManager] '{addItem.name}' 추가됨 (스택 불가 아이템)";
        UIManager.SystemGameMessage(message, MessageTag.아이템_획득);

        UpdateQuestAndUI(addItem);
    }

    private bool AddNewItemToInventory(Item item)
    {
        if (inventory.Count >= CurrentCapacity)
        {
            return false;
        }

        inventory.Add(item.Clone());
        if (GameStateMachine.Instance.CurrentState != GameSystemState.Shopping)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.InventoryChange);
        }
        return true;
    }

    private void UpdateQuestAndUI(Item item)
    {
        QuestManager.Instance.UpdateQuestProgress(QuestConditionType.Collect, item.id, item.quantity);
        UIManager.Instance.PickUpItemTextDisplay?.AddItem(item.name, item.sprite);
    }

    public void RemoveItemLogic(string itemId, int quantity = 1)
    {
        var itemsToRemove = inventory.Where(i => i.id == itemId).ToList();

        if (itemsToRemove.Count == 0)
        {
            UIManager.SystemGameMessage($"아이템 '{itemId}'를 소지하고 있지 않습니다.", MessageTag.아이템_획득);
            return;
        }

        foreach (var item in itemsToRemove)
        {
            if (quantity <= 0) break;

            if (item.quantity > quantity)
            {
                item.quantity -= quantity;
                UIManager.SystemGameMessage($"아이템 '{item.name}' {quantity}개 제거됨. 남은 수량: {item.quantity}", MessageTag.아이템_획득);
                quantity = 0;
            }
            else
            {
                quantity -= item.quantity;
                inventory.Remove(item);
                UIManager.SystemGameMessage($"아이템 '{item.name}' 제거됨 (스택 전체 제거)", MessageTag.아이템_획득);
            }
        }
        if (quantity > 0)
        {
            Debug.LogWarning($"[InventoryManager] 아이템 '{itemId}'를 {quantity}개 제거하려 했으나, 수량이 부족합니다.");
        }
        UIManager.Instance.InventoryUpdate();
        UpdateQuickSlotQuantity();
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
            UpdateQuickSlotQuantity();
        }
    }

    public int GetRemainingInventory()
    {
        return Mathf.Max(0, CurrentCapacity - inventory.Count);
    }

    public bool CanAddInventoryItem(string itemId, int amount)
    {
        var existingItem = FindInventoryItem(itemId);

        if (GetRemainingInventory() > 0)
        {
            return true;
        }
        else if (existingItem != null && existingItem.isStackable && (existingItem.quantity + amount <= existingItem.maxStack))
        {
            return true;
        }

        return false;
    }

    public void ExpandInventory(int expandSize)
    {
        int previousCapacity = CurrentCapacity;
        CurrentCapacity = Mathf.Min(CurrentCapacity + expandSize, maxCapacity);
        UIManager.SystemGameMessage($"인벤토리가 확장되었습니다. 이전 용량: {previousCapacity}, 현재 용량: {CurrentCapacity}/{maxCapacity}", MessageTag.아이템_획득);
        UpdateQuickSlotQuantity();
    }

    public void PrintInventory()
    {
        string inventoryText = "";
        foreach (var item in inventory)
        {
            string gradeColor = item.GetGradeColor(item.grade);
            inventoryText += $"- <b><color={gradeColor}>{item.name}</color></b> (ID: <i>{item.id}</i>, <color=#00FF00>수량: {item.quantity}</color>)\n";
        }
        //Debug.Log(inventoryText);
    }

    public static List<Item> GetSlotItem(EquipmentSlot slot)
    {
        Debug.LogWarning("위치 : " + slot);
        return Instance.inventory.FindAll(x => x.equipmentSlot == slot);
    }

    protected override void HandleGameStateChange(global::GameSystemState newState, object additionalData)
    {

    }

    /// <summary>
    /// 특정 ID와 강화 레벨을 가진 아이템의 수량을 반환합니다.
    /// </summary>
    /// <param name="itemId">확인할 아이템 ID</param>
    /// <param name="requiredLevel">필요한 강화 레벨</param>
    /// <returns>해당 ID와 강화 레벨을 가진 아이템의 수량</returns>
    public int GetItemQuantityWithLevel(string itemId, int requiredLevel)
    {
        int count = 0;
        
        // 인벤토리 내에서 해당 ID를 가진 아이템을 찾아 강화 레벨을 확인
        foreach (var item in inventory)
        {
            if (item != null && item.id == itemId)
            {
                // 아이템의 강화 레벨 확인
                if (item.itemSkill != null && item.itemSkill.Level >= requiredLevel)
                {
                    count += item.quantity;
                }
            }
        }
        
        return count;
    }

    public void UpdateQuickSlotQuantity()
    {
        if (quickSlotsUI == null) return;
        
        foreach (var slot in quickSlotsUI.GetSlots())
        {
            Item item = slot.GetItem();
            if (item != null)
            {
                // 아이템이 인벤토리에 있는지 확인
                int quantity = GetItemQuantity(item.id);
                if (quantity > 0)
                {
                    // 수량 업데이트
                    slot.UpdateQuantityText(quantity);
                }
                else
                {
                    // 인벤토리에 없으면 슬롯 리셋
                    slot.ResetSlot();
                }
            }
        }
    }
}

using System.Collections.Generic;
using UnityEngine;

public class EnhanceManager : BaseManager<EnhanceManager>
{
    [SerializeField] private List<string> EnhancementItemIds = new List<string>();
    private Dictionary<int, (int success, int downgrade, int destroy)> levelEnhanceChance;

    protected override void Start()
    {
        base.Start();

        levelEnhanceChance = new Dictionary<int, (int success, int downgrade, int destroy)>
        {
            { 0, (100, 0, 0) },
            { 1, (95, 5, 0) },
            { 2, (90, 10, 0) },
            { 3, (85, 15, 0) },
            { 4, (80, 15, 5) },
            { 5, (75, 15, 10) },
            { 6, (70, 20, 10) },
            { 7, (60, 25, 15) },
            { 8, (50, 30, 20) }
        };
    }

    public void Enhance(Item upgradeItem, Item upgradeIngredient)
    {
        if (upgradeIngredient == null || !EnhancementItemIds.Contains(upgradeIngredient.id))
        {
            Debug.Log("업그레이드 재료가 아님");
            return;
        }

        if (upgradeItem.itemSkill.Level >= 9)
        {
            Debug.Log("강화 레벨 초과");
            return;
        }

        var enhanceChances = levelEnhanceChance[upgradeItem.itemSkill.Level];
        float random = Random.value * 100;

        if(random <= enhanceChances.success)
        {
            Successed(upgradeItem);
        }
        else if(random <= enhanceChances.success + enhanceChances.downgrade)
        {
            Downgraded(upgradeItem);
        }
        else
        {
            Destroyed(upgradeItem);
        }

        ItemManager.Instance.RemoveItemLogic(upgradeIngredient.id);
        if(InventoryManager.Instance.GetItemQuantity(upgradeIngredient.id) <=0)
        {
            InventoryManager.Instance.ResetSelectedItem();
        }
    }

    private void Successed(Item item)
    {
        item.itemSkill.ApplyItemStat(item, item.itemSkill.ApplyPower(item), 1);
        item.itemSkill.Level++;

        Debug.Log($"강화 성공\n아이템 레벨: {item.itemSkill.Level}");
    }

    private void Downgraded(Item item)
    {
        item.itemSkill.Level--;
        item.itemSkill.ApplyItemStat(item, item.itemSkill.ApplyPower(item), -1);

        Debug.Log($"강화 실패, 다운그레이드\n아이템 레벨: {item.itemSkill.Level}");
    }

    private void Destroyed(Item item)
    {
        if (ItemEffectManager.Instance.GetEquippedItem(item.equipmentSlot) == item)
        {
            ItemEffectManager.Instance.UnequipmentEffect(item);
        }

        ItemManager.Instance.RemoveItemLogic(item.id);

        Debug.Log("강화 실패, 아이템 파괴");
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}

using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnhanceManager : BaseManager<EnhanceManager>
{

    [SerializeField] private List<string> EnhancementItemIds = new List<string>();
    public List<string> enhancementItemIds => EnhancementItemIds;
    private Dictionary<int, (int success, int downgrade, int destroy)> levelEnhanceChance;

    public int MaxEnhanceLevel => levelEnhanceChance.Keys.Max();

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
            //Debug.Log("업그레이드 재료가 아님");
            return;
        }

        if (upgradeItem.itemSkill.Level >= 9)
        {
            //Debug.Log("강화 레벨 초과");
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

        //강화 테스트용
        //if(InventoryManager.Instance.GetItemQuantity(upgradeIngredient.id) <=0)
        //{
        //    InventoryManager.Instance.ResetSelectedItem();
        //}

    }

    private void Successed(Item item)
    {
        item.itemSkill.ApplyItemStat(item, item.itemSkill.ApplyPower(item), 1);
        item.itemSkill.Level++;

        // 아이템 강화 성공 시 퀘스트 진행 상태 업데이트
        QuestManager.Instance.UpdateQuestProgress(QuestConditionType.Collect, item.id, 0);

        //Debug.Log($"강화 성공\n아이템 레벨: {item.itemSkill.Level}");
    }

    private void Downgraded(Item item)
    {
        item.itemSkill.Level--;
        item.itemSkill.ApplyItemStat(item, item.itemSkill.ApplyPower(item), -1);

        // 아이템 다운그레이드 시 퀘스트 진행 상태 업데이트
        QuestManager.Instance.UpdateQuestProgress(QuestConditionType.Collect, item.id, 0);

        //Debug.Log($"강화 실패, 다운그레이드\n아이템 레벨: {item.itemSkill.Level}");
    }

    private void Destroyed(Item item)
    {
        if (ItemEffectManager.Instance.GetEquippedItem(item.equipmentSlot) == item)
        {
            ItemEffectManager.Instance.UnequipmentEffect(item);
        }

        string itemId = item.id; // 아이템 ID 저장 (제거 전)

        ItemManager.Instance.RemoveItemLogic(item.id);

        // 아이템 파괴 시 퀘스트 진행 상태 업데이트
        QuestManager.Instance.UpdateQuestProgress(QuestConditionType.Collect, itemId, 0);

        //Debug.Log("강화 실패, 아이템 파괴");
    }

    public Item PreviewEnhance(Item item)
    {
        if (item == null || item.itemStat == null || item.itemSkill == null)
            return null;

        Item preview = item.Clone();


        // ✅ 실제 강화처럼 현재 레벨 기준으로 power 계산
        float previewPower = preview.itemSkill.ApplyPower(preview);

        preview.itemSkill.ApplyItemStat(preview, previewPower, 1);
        preview.itemSkill.Level++; // 시각적으로만 올라간 레벨 보여주기

        return preview;
    }
    public bool CanEnhance(Item item)
    {
        return levelEnhanceChance.ContainsKey(item.itemSkill.Level);
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}

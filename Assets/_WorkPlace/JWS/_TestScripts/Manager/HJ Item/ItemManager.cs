using System.Collections.Generic;
using System.Linq;
using UnityEngine.AddressableAssets;
using UnityEngine;


public class ItemManager : BaseManager<ItemManager>
{
    [SerializeField] private ItemList itemList;
    [SerializeField] private List<Sprite> itemSpriteList = new List<Sprite>();
    [SerializeField] private GameObject dropItemPrefab;
    [SerializeField] private float dropItemDestroyTime = 180f;      // 드랍아이템 사라지는 시간.
    [SerializeField] private GameObject[] ItemEffectPrefab;

    public static float DropItemDestroyTime => Instance.dropItemDestroyTime;


    public static List<Item> ItemDatabase => Instance.itemList.itemList;
    private Dictionary<string, Sprite> itemSpriteDictionary = new Dictionary<string, Sprite>(); // 스프라이트 딕셔너리
    private Dictionary<string, Sprite> skillSpriteDictionary = new Dictionary<string, Sprite>(); // 스킬 스프라이트 딕셔너리
    
    // 런타임 동안만 아이템 강화 정보를 유지하는 딕셔너리 추가
    private Dictionary<string, ItemEnhancementInfo> itemEnhancementData = new Dictionary<string, ItemEnhancementInfo>();
    
    // 아이템 강화 정보를 저장하는 클래스
    private class ItemEnhancementInfo
    {
        public int EnhancementLevel { get; set; } = 0;
        public ItemStat EnhancedStats { get; set; } = null;
        
        public ItemEnhancementInfo(int level, ItemStat stats)
        {
            EnhancementLevel = level;
            EnhancedStats = stats != null ? stats.Clone() : null;
        }
    }

    protected override void Awake()
    {
        base.Awake();
        ItemGenerater itemGenerater = new ItemGenerater();

        // 아이템 강화 정보 딕셔너리 초기화
        itemEnhancementData = new Dictionary<string, ItemEnhancementInfo>();
        
        // 모든 아이템의 초기 강화 정보 저장
        foreach (var item in itemList.itemList)
        {
            int initialLevel = item.itemSkill != null ? item.itemSkill.Level : 0;
            ItemStat initialStats = item.itemStat != null ? item.itemStat.Clone() : null;
            itemEnhancementData[item.id] = new ItemEnhancementInfo(initialLevel, initialStats);
        }

        Addressables.LoadAssetsAsync<Sprite>("ItemSprites", sprite =>
        {
            if (!itemSpriteDictionary.ContainsKey(sprite.name))
            {
                itemSpriteDictionary[sprite.name] = sprite; // 스프라이트 딕셔너리에 추가
                itemSpriteList.Add(sprite); // 스프라이트 리스트에도 추가
            }
        }).Completed += handle =>
        {
            //Debug.Log($"{itemSpriteList.Count}개의 아이템 스프라이트를 로드했습니다.");
        };
        Addressables.LoadAssetsAsync<Sprite>("Skills", sprite =>
        {
            if (!itemSpriteDictionary.ContainsKey(sprite.name))
            {
                skillSpriteDictionary[sprite.name] = sprite;
            }
        }).Completed += handle =>
        {
            //Debug.LogWarning($"{skillSpriteDictionary.Count}개의 아이템 스프라이트를 로드했습니다.");
        };

        if (itemList.itemList.Count <= 0)
        {
            GenerateData generateData = new GenerateData();
            generateData.InitializeItems(itemList);
        }
        foreach (var item in itemList.itemList)
        {
            if (item.quantity == 0 ) item.quantity = 1;
            if (item.type == ItemType.무기 || item.type == ItemType.방어구)
            {
                //item.itemStat.Initialize();
            }
        }
    }

    // 아이템 강화 레벨을 가져오는 메서드 추가
    public int GetItemEnhancementLevel(string itemId)
    {
        if (itemEnhancementData.TryGetValue(itemId, out ItemEnhancementInfo info))
        {
            return info.EnhancementLevel;
        }
        return 0;
    }
    
    // 아이템 강화 스탯을 가져오는 메서드 추가
    public ItemStat GetItemEnhancedStats(string itemId)
    {
        if (itemEnhancementData.TryGetValue(itemId, out ItemEnhancementInfo info) && info.EnhancedStats != null)
        {
            return info.EnhancedStats;
        }
        
        Item item = GetItemById(itemId);
        return item?.itemStat;
    }
    
    // 아이템 강화 레벨 설정 메서드 추가
    public void SetItemEnhancementLevel(string itemId, int level)
    {
        if (!itemEnhancementData.ContainsKey(itemId))
        {
            Item item = GetItemById(itemId);
            if (item == null) return;
            
            itemEnhancementData[itemId] = new ItemEnhancementInfo(level, item.itemStat?.Clone());
        }
        else
        {
            itemEnhancementData[itemId].EnhancementLevel = level;
        }
        
        // 표시용으로 런타임 아이템 객체에도 적용
        Item originalItem = GetItemById(itemId);
        if (originalItem?.itemSkill != null)
        {
            originalItem.itemSkill.Level = level;
        }
    }
    
    // 아이템 강화 스탯 설정 메서드 추가
    public void SetItemEnhancedStats(string itemId, ItemStat stats)
    {
        if (!itemEnhancementData.ContainsKey(itemId))
        {
            itemEnhancementData[itemId] = new ItemEnhancementInfo(0, stats);
        }
        else
        {
            itemEnhancementData[itemId].EnhancedStats = stats;
        }
        
        // 표시용으로 런타임 아이템 객체에도 적용
        Item originalItem = GetItemById(itemId);
        if (originalItem != null)
        {
            originalItem.itemStat = stats != null ? stats.Clone() : null;
        }
    }

    public void AddItemLogic(string itemId, int quantity = 1)
    {
        var item = GetItemById(itemId);
        item.quantity = quantity;
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
        UIManager.SystemGameMessage($"[InventoryManager] '{item.name}' 아이템 {quantity}개 사용", MessageTag.아이템_사용);
        InventoryManager.Instance.RemoveItemLogic(item.id, quantity);
        
        // 퀵슬롯 수량 업데이트
        InventoryManager.Instance.UpdateQuickSlotQuantity();

        // TODO
        // 아이템 사용 효과 적용 (예: 체력 회복 등) 효과 구현연결
        // ApplyItemEffect(item, quantity); 이런식의 연결함수 구현이 필요.

        ItemEffectManager.Instance.ApplyItemEffect(item, quantity);
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
        UIManager.SystemGameMessage($"[InventoryManager] '{item.name}' 아이템 {quantity}개 버림", MessageTag.아이템_버림);
        InventoryManager.Instance.RemoveItemLogic(item.id, quantity);
        
        // 퀵슬롯 수량 업데이트
        InventoryManager.Instance.UpdateQuickSlotQuantity();

        // TODO
        // 추가 드롭 연출(필요 시)
        // CreateDroppedItemInWorld(item, quantity); ; 이런식의 연결함수 구현이 필요.
    }

    //아이템 구매
    public void PurchaseItem(Item buyItem, int quantity = 1)
    {
        int amount = buyItem.costValue * quantity;
        Debug.Log("플레이어 : " + amount + " , " + buyItem.costValue + " , " + quantity);
        if (CharacterManager.PlayerCharacterData.gold < amount)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.InfoMessage, "보유한 금액 부족합니다.", true);
            return;
        }
        if (InventoryManager.Instance.CanAddInventoryItem(buyItem.id, quantity) && CharacterManager.PlayerCharacterData.UseGold(amount))
        {
            AddItemLogic(buyItem.id, quantity);
            InventoryManager.Instance.UpdateQuickSlotQuantity(); // 퀵슬롯 수량 업데이트
        }
    }

    public void SellItem(Item selltem, float valueReductionRate, int quantity = 1)
    {
        int totalQuantity = InventoryManager.InventoryList.Where(i => i.id == selltem.id).Sum(i => i.quantity);
        if (totalQuantity < quantity)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.InfoMessage, "아이템 수량 부족.", true);
            return;
        }
        int reducedCost = Mathf.RoundToInt(selltem.costValue * valueReductionRate);
        int amount = reducedCost * quantity;
        Debug.LogWarning($"판매 : {valueReductionRate}, {quantity}, {amount}");
        CharacterManager.PlayerCharacterData.AddGold(amount);
        InventoryManager.Instance.RemoveItemLogic(selltem.id, quantity);
        InventoryManager.Instance.UpdateQuickSlotQuantity(); // 퀵슬롯 수량 업데이트
    }

    public Sprite GetItemSprite(string spriteName)
    {
        if (itemSpriteDictionary.TryGetValue(spriteName, out Sprite sprite))
        {
            return sprite;
        }

        Debug.LogWarning($"'{spriteName}'에 해당하는 스프라이트를 찾을 수 없습니다.");
        return null;
    }

    public Sprite GetSkillSprite(string spriteName)
    {
        if (skillSpriteDictionary.Count == 0) return null;
        if (skillSpriteDictionary.TryGetValue(spriteName, out Sprite sprite))
        {
            return sprite;
        }

        //Debug.LogWarning($"'{spriteName}'에 해당하는 스프라이트를 찾을 수 없습니다.");
        return null;
    }

    public GameObject SpawnItemBox(Vector3 spawnPosition, MonsterData monsterData, bool isRandom = true)
    {
        // 드롭될 아이템이 없는 경우 아이템 박스를 생성하지 않음
        if (monsterData.isRandomDrop && monsterData.randomDropItems.Count == 0)
        {
            Debug.Log("랜덤 드롭 아이템이 없어 아이템 박스를 생성하지 않습니다.");
            return null;
        }
        else if (!monsterData.isRandomDrop && monsterData.dropItems.Count == 0)
        {
            Debug.Log("고정 드롭 아이템이 없어 아이템 박스를 생성하지 않습니다.");
            return null;
        }
        
        GameObject itemBox = Instantiate(dropItemPrefab, spawnPosition, Quaternion.identity);
        var dropController = itemBox.GetComponent<DropItemBoxController>();
        
        // 몬스터 데이터의 드롭 설정을 반영
        dropController.isRandomDrop = monsterData.isRandomDrop;
        
        if (monsterData.isRandomDrop)
        {
            // 랜덤 드롭 설정
            dropController.randomDropItems = new List<MonsterData.DropItemChance>(monsterData.randomDropItems);
            Debug.Log($"랜덤 드롭 아이템 박스 생성: {monsterData.randomDropItems.Count}개 아이템 후보");
        }
        else
        {
            // 고정 드롭 설정
            dropController.dropItemIds = new List<string>(monsterData.dropItems);
            Debug.Log($"고정 드롭 아이템 박스 생성: {monsterData.dropItems.Count}개 아이템");
        }
        
        return itemBox;
    }

    /// <summary>
    /// 01.17 희정 추가한 함수
    /// </summary>
    //타입과 등급으로 해당 아이템 가져오기
    public List<Item> GetItemsByTypeAndGrade(ItemType itemType, ItemGrade grade)
    {
        return ItemDatabase.Where(i => i.type == itemType && i.grade >= grade).ToList();
    }

    //특정 아이템 리스트
    public List<Item> GetSpeificItem(List<string> itemids)
    {
        List<Item> items = new List<Item>();
        foreach (var itemid in itemids)
        {
            items.Add(GetItemById(itemid));
        }

        return items;
    }

    public void ResetAllItemInstances()
    {
        // 모든 아이템 강화 정보 초기화
        itemEnhancementData.Clear();
        
        // 모든 아이템의 초기 상태 다시 저장
        foreach (var item in itemList.itemList)
        {
            // 강화 레벨 초기화
            if (item.itemSkill != null)
            {
                // 런타임 표시용으로 Item 객체 자체도 초기화
                item.itemSkill.Level = 0;
            }
            
            // 아이템 스탯 초기화
            if (item.itemStat != null)
            {
                item.itemStat.Initialize();
            }
            
            // 초기화된 상태를 딕셔너리에 저장
            itemEnhancementData[item.id] = new ItemEnhancementInfo(0, item.itemStat?.Clone());
            
            Debug.Log($"아이템 '{item.id}'의 강화 수치가 초기화되었습니다.");
        }
        
        Debug.Log("모든 아이템 인스턴스가 초기화되었습니다.");
    }

    protected override void HandleGameStateChange(global::GameSystemState newState, object additionalData)
    {
        // 게임 시작 시 모든 아이템 초기화
        if (newState == GameSystemState.Play || newState == GameSystemState.MainQuestPlay)
        {
            ResetAllItemInstances();
            Debug.Log("게임 시작: 모든 아이템이 초기화되었습니다.");
        }
    }
}

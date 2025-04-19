using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class ItemEffectManager : BaseManager<ItemEffectManager>
{
    #region Variables

    private Dictionary<EquipmentSlot, Item> equippedItems = new Dictionary<EquipmentSlot, Item>();
    private Dictionary<BuffType, ActiveBuff> activeBuffs = new Dictionary<BuffType, ActiveBuff>();
    private Dictionary<BuffType, Coroutine> activeCoroutines = new Dictionary<BuffType, Coroutine>();

    [SerializeField] private float effectParticleDuration = 2f; // 아이템 이펙트 파티클 재생 시간
    [SerializeField] private Vector3 particlePositionOffset = new Vector3(); // 파티클 위치 오프셋
    [SerializeField] private Item count;

    private WeaponManager weaponManager;
    public WeaponManager WeaponManager
    {
        get { return weaponManager; }
    }

    private PlayerData Player
    {
        get => CharacterManager.PlayerCharacterData;
        set => CharacterManager.PlayerCharacterData = value;
    }

    #endregion

    #region Unity Methods
    protected override void Start()
    {
        base.Start();
        weaponManager = GameManager.playerTransform.GetComponent<PlayerCombat>().weapon;
        InitializeEquipmentSlots(); // 딕셔너리 초기화
        count = Instance.equippedItems[EquipmentSlot.손];
    }

    #endregion

    #region Public Methods

    //HJ 03.06 요리 효과 추가
    // 아이템 효과 적용
    public void ApplyItemEffect(Item item, int quantity = 1)
    {
        switch (item.type)
        {
            case ItemType.소모품:
                ApplyConsumableEffect(item, quantity);
                break;

            case ItemType.무기:
            case ItemType.방어구:
            case ItemType.장신구:
                ApplyEquipmentEffect(item);
                break;

            case ItemType.요리:
                ApplyDishEffect(item);
                break;

            default:
                Debug.Log($"아이템 타입 {item.type}의 효과 없음");
                break;
        }
    }

    // 아이템기준 슬롯에서 아이템 장착 해제
    public void UnequipmentEffect(Item item)
    {
        if (equippedItems[item.equipmentSlot] != null && equippedItems[item.equipmentSlot] == item)
        {
            UnequipmentEffect(item.equipmentSlot);
        }
        else
        {
            Debug.LogWarning($"{item.name}이 {item.equipmentSlot}에 장착되어 있지 않습니다.");
        }
    }

    // 슬롯기준 슬롯에서 아이템 장착 해제
    public void UnequipmentEffect(EquipmentSlot equipmentSlot)
    {
        Item item = GetEquippedItem(equipmentSlot);
        if (item != null)
        {
            //HJ 03.07 추가
            ItemSkillManager.Instance.UnequipReset(item);
            UpdatePlayerStats(item.itemStat, -1);
            equippedItems[item.equipmentSlot] = null;
            item.isEquired = false;
            if (item.equipmentSlot == EquipmentSlot.손) weaponManager.EquipWeapon(null);
            Debug.Log($"{item.name}을 {item.equipmentSlot}에서 해제");
        }
    }

    // 특정 슬롯의 장착 아이템 가져오기
    public Item GetEquippedItem(EquipmentSlot slot)
    {
        return equippedItems[slot]; // null이면 슬롯이 비어있음을 의미
    }
    #endregion

    #region Private Methods

    private void InitializeEquipmentSlots()
    {
        foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
        {
            if (!equippedItems.ContainsKey(slot))
            {
                equippedItems[slot] = null;
            }
        }
        Debug.Log("장비 슬롯 초기화 완료");
    }

    private void ApplyConsumableEffect(Item item, int quantity = 1)
    {
        // 아이템 효과나 스탯이 null인 경우 원본 아이템 찾기
        ItemStat effectiveStat = item.itemStat;
        float effectiveAmount = item.effectAmount;
        float effectiveDuration = item.effect?.duration ?? 0;
        bool useOriginalItem = false;
        
        // 문제가 있는 경우 원본 아이템 데이터베이스에서 아이템 찾기
        if ((item.effect == null || effectiveAmount <= 0 || 
            (item.consumableType == ConsumableType.버프 && effectiveStat == null)))
        {
            Item originalItem = ItemManager.Instance.GetItemById(item.id);
            if (originalItem != null)
            {
                effectiveStat = originalItem.itemStat;
                effectiveAmount = originalItem.effectAmount;
                effectiveDuration = originalItem.effect?.duration ?? 0;
                useOriginalItem = true;
                Debug.Log($"[ApplyConsumableEffect] 원본 아이템 데이터를 사용하여 소모품 효과 적용: {item.name}");
            }
        }
        
        // 아직도 효과가 없으면 반환
        if ((item.consumableType == ConsumableType.버프 && effectiveStat == null) ||
            ((item.consumableType == ConsumableType.체력포션 || item.consumableType == ConsumableType.마나포션) && effectiveAmount <= 0))
        {
            Debug.LogWarning($"[ApplyConsumableEffect] 소모품 {item.name}의 효과를 적용할 수 없습니다.");
            return;
        }

        // 실제 효과 적용
        if (item.consumableType == ConsumableType.체력포션)
        {
            int healAmount = Mathf.RoundToInt(effectiveAmount * quantity);
            int originalHp = Player.currentHp;
            Player.currentHp = Mathf.Min(Player.maxHp, Player.currentHp + healAmount);
            int actualHealed = Player.currentHp - originalHp;
            
            Debug.Log($"{item.name}을 사용하여 체력 {healAmount}만큼 회복 시도. 실제 회복량: {actualHealed} (현재 체력: {Player.currentHp}/{Player.maxHp})");
        }
        else if (item.consumableType == ConsumableType.마나포션)
        {
            Player.currentMp = Mathf.Min(Player.maxMp, Player.currentMp + Mathf.RoundToInt(effectiveAmount * quantity));
            Debug.Log($"{item.name}을 사용하여 마나 {effectiveAmount}만큼 회복");
        }
        else if (item.consumableType == ConsumableType.버프)
        {
            if (useOriginalItem)
            {
                // 임시 아이템 생성하여 버프 적용
                Item tempItem = item.Clone();
                tempItem.itemStat = effectiveStat;
                tempItem.effect.duration = effectiveDuration;
                ApplyBuff(tempItem, effectiveDuration, quantity);
            }
            else
            {
                ApplyBuff(item, item.effect.duration, quantity);
            }
            Debug.Log($"{item.name}을 사용하여 버프 적용");
        }

        PlayParticle(item);
    }

    private void ApplyEquipmentEffect(Item item)
    {
        if (item.itemStat == null) return;        

        // 기존 아이템 해제
        if (equippedItems[item.equipmentSlot] != null)
        {
            Debug.Log($"{item.equipmentSlot}에 장착된 {equippedItems[item.equipmentSlot].name} 해제");
            UnequipmentEffect(equippedItems[item.equipmentSlot]);
        }

        // 새로운 아이템 장착
        equippedItems[item.equipmentSlot] = item;
        item.isEquired = true;
        UpdatePlayerStats(item.itemStat, 1);
        PlayParticle(item);
        if (item.equipmentSlot == EquipmentSlot.손) weaponManager.EquipWeapon(item);
        Debug.Log($"{item.name}을 {item.equipmentSlot}에 장착");

        //HJ 03.07 추가
        ItemSkillManager.Instance.EquipReset(item);
    }

    //중복 사용 가능(일반 버프 아이템)
    private void ApplyBuff(Item item, float duration, int quantity = 1)
    {
        UpdatePlayerStats(item.itemStat, item.effectAmount);
        Debug.Log($"{item.name} 버프 아이템 사용, 효과량: {item.effectAmount}");

        StartCoroutine(RemoveBuffAfterDuration(item, duration * quantity, item.effectAmount));
    }

    //HJ 03.06 추가
    //중복 사용 불가능(요리)
    private void ApplyBuff(Item item)
    {
        ItemStat itemStat = item.itemStat;
        float duration = item.effect.duration;

        // 만약 itemStat이 null이면 원본 아이템에서 가져옴
        if (itemStat == null)
        {
            Item originalItem = ItemManager.Instance.GetItemById(item.id);
            if (originalItem != null && originalItem.itemStat != null)
            {
                itemStat = originalItem.itemStat;
                duration = originalItem.effect.duration;
                Debug.Log($"[ApplyBuff] 원본 아이템 데이터를 사용하여 버프 적용: {item.name}");
            }
            else
            {
                Debug.LogWarning($"[ApplyBuff] 아이템 {item.name}의 스탯이 null이고, 원본 아이템도 찾을 수 없습니다.");
                return;
            }
        }

        ApplyStatBuff(BuffType.Basic, "Strength", itemStat.Strength, duration);
        ApplyStatBuff(BuffType.Basic, "Dexterity", itemStat.Dexterity, duration);
        ApplyStatBuff(BuffType.Basic, "Intelligence", itemStat.Intelligence, duration);
        ApplyStatBuff(BuffType.Basic, "Vitality", itemStat.Vitality, duration);

        ApplyStatBuff(BuffType.Combat, "MaxHealth", itemStat.MaxHealth, duration);
        ApplyStatBuff(BuffType.Combat, "MaxMana", itemStat.MaxMana, duration);
        ApplyStatBuff(BuffType.Combat, "PhysicalAttack", itemStat.PhysicalAttack, duration);
        ApplyStatBuff(BuffType.Combat, "MagicAttack", itemStat.MagicAttack, duration);
        ApplyStatBuff(BuffType.Combat, "PhysicalDefense", itemStat.PhysicalDefense, duration);
        ApplyStatBuff(BuffType.Combat, "MagicDefense", itemStat.MagicDefense, duration);

        ApplyStatBuff(BuffType.Support, "CriticalChance", itemStat.CriticalChance, duration);
        ApplyStatBuff(BuffType.Support, "AttackSpeed", itemStat.AttackSpeed, duration);
        ApplyStatBuff(BuffType.Support, "Evasion", itemStat.Evasion, duration);
    }

    //HJ 03.06 추가
    private void ApplyStatBuff(BuffType buffType, string statName, float value, float duration)
    {
        if (value == 0) return;

        if(activeBuffs.TryGetValue(buffType, out ActiveBuff existingBuff))
        {
            UpdatePlayerStats(existingBuff.statName, existingBuff.value, -1);
            if(activeCoroutines.ContainsKey(buffType))
            {
                StopCoroutine(activeCoroutines[buffType]);
                Debug.Log($"코루틴 종료: {buffType}");
            }
        }

        activeBuffs[buffType] = new ActiveBuff(buffType, statName, value, duration);
        UpdatePlayerStats(statName, value, 1);
        activeCoroutines[buffType] = StartCoroutine(RemoveBuffAfterDuration(buffType, statName, value, duration));
    }

    //HJ 03.06 추가
    //중복 사용 불가능(요리)
    private void ApplyDishEffect(Item item)
    {
        // 아이템 스탯이 null이거나 HealHp/HealMp가 0인 경우, 원본 아이템 데이터 참조
        ItemStat effectiveStat = item.itemStat;
        float effectiveDuration = item.effect?.duration ?? 0;
        bool useOriginalItem = false;
        
        // 문제가 있는 경우 원본 아이템 데이터베이스에서 아이템 찾기
        if (effectiveStat == null || (effectiveStat.HealHp <= 0 && effectiveStat.HealMp <= 0 && !effectiveStat.HasBuffStat()))
        {
            Item originalItem = ItemManager.Instance.GetItemById(item.id);
            if (originalItem != null && originalItem.itemStat != null)
            {
                effectiveStat = originalItem.itemStat;
                effectiveDuration = originalItem.effect?.duration ?? 0;
                useOriginalItem = true;
                Debug.Log($"[ApplyDishEffect] 원본 아이템 데이터를 사용하여 요리 효과 적용: {item.name}");
                
                // 현재 아이템의 스탯도 업데이트
                if (item.itemStat == null)
                {
                    item.itemStat = originalItem.itemStat.Clone();
                }
                else
                {
                    // HealHp와 HealMp 값만 복사
                    item.itemStat.HealHp = originalItem.itemStat.HealHp;
                    item.itemStat.HealMp = originalItem.itemStat.HealMp;
                }
                
                // 이펙트 정보도 복사
                if (item.effect == null && originalItem.effect != null)
                {
                    item.effect = originalItem.effect.Clone();
                }
            }
        }

        // 아직도 스탯이 null이면 반환
        if (effectiveStat == null)
        {
            Debug.LogWarning($"[ApplyDishEffect] 요리 아이템 {item.name}의 스탯이 null입니다. 효과를 적용할 수 없습니다.");
            return;
        }

        // 스탯 버프 효과가 있는 경우 Duration 시간만큼 지속되는 버프 적용
        if (effectiveStat.HasBuffStat())
        {
            if (effectiveDuration <= 0)
            {
                Debug.LogWarning($"[ApplyDishEffect] 요리 아이템 {item.name}의 지속시간이 유효하지 않습니다: {effectiveDuration}");
                effectiveDuration = 10f; // 기본값 설정
            }
            
            // 기본 스탯 버프 적용
            if (effectiveStat.Strength > 0)
                ApplyStatBuff(BuffType.Dish_Strength, "Strength", effectiveStat.Strength, effectiveDuration);
            
            if (effectiveStat.Dexterity > 0)
                ApplyStatBuff(BuffType.Dish_Dexterity, "Dexterity", effectiveStat.Dexterity, effectiveDuration);
            
            if (effectiveStat.Intelligence > 0)
                ApplyStatBuff(BuffType.Dish_Intelligence, "Intelligence", effectiveStat.Intelligence, effectiveDuration);
            
            if (effectiveStat.Vitality > 0)
                ApplyStatBuff(BuffType.Dish_Vitality, "Vitality", effectiveStat.Vitality, effectiveDuration);
            
            // 전투 스탯 버프 적용
            if (effectiveStat.MaxHealth > 0)
                ApplyStatBuff(BuffType.Dish_MaxHealth, "MaxHealth", effectiveStat.MaxHealth, effectiveDuration);
            
            if (effectiveStat.MaxMana > 0)
                ApplyStatBuff(BuffType.Dish_MaxMana, "MaxMana", effectiveStat.MaxMana, effectiveDuration);
            
            if (effectiveStat.PhysicalAttack > 0)
                ApplyStatBuff(BuffType.Dish_PhysicalAttack, "PhysicalAttack", effectiveStat.PhysicalAttack, effectiveDuration);
            
            if (effectiveStat.MagicAttack > 0)
                ApplyStatBuff(BuffType.Dish_MagicAttack, "MagicAttack", effectiveStat.MagicAttack, effectiveDuration);
            
            if (effectiveStat.PhysicalDefense > 0)
                ApplyStatBuff(BuffType.Dish_PhysicalDefense, "PhysicalDefense", effectiveStat.PhysicalDefense, effectiveDuration);
            
            if (effectiveStat.MagicDefense > 0)
                ApplyStatBuff(BuffType.Dish_MagicDefense, "MagicDefense", effectiveStat.MagicDefense, effectiveDuration);
                
            if (effectiveStat.CriticalChance > 0)
                ApplyStatBuff(BuffType.Dish_CriticalChance, "CriticalChance", effectiveStat.CriticalChance, effectiveDuration);
                
            if (effectiveStat.Evasion > 0)
                ApplyStatBuff(BuffType.Dish_Evasion, "Evasion", effectiveStat.Evasion, effectiveDuration);
                
            if (effectiveStat.AttackSpeed > 0)
                ApplyStatBuff(BuffType.Dish_AttackSpeed, "AttackSpeed", effectiveStat.AttackSpeed, effectiveDuration);
        }

        // 체력 및 마나 회복은 즉시 적용 (Duration과 무관)
        if (effectiveStat.HealHp > 0)
        {
            int originalHp = Player.currentHp;
            Player.currentHp = Mathf.Min(Player.maxHp, Player.currentHp + effectiveStat.HealHp);
            int healedAmount = Player.currentHp - originalHp;
            Debug.Log($"{item.name}을 사용하여 체력 {healedAmount}만큼 회복 (원래 회복량: {effectiveStat.HealHp})");
        }

        if (effectiveStat.HealMp > 0)
        {
            int originalMp = Player.currentMp;
            Player.currentMp = Mathf.Min(Player.maxMp, Player.currentMp + effectiveStat.HealMp);
            int healedAmount = Player.currentMp - originalMp;
            Debug.Log($"{item.name}을 사용하여 마나 {healedAmount}만큼 회복 (원래 회복량: {effectiveStat.HealMp})");
        }

        PlayParticle(item);
    }

    //아이템 스탯 전체 적용
    private void UpdatePlayerStats(ItemStat stat, int multiplier)
    {
        bool isEquipping = multiplier > 0;

        if (isEquipping)
        {
            // 장착 시 장비 보너스 증가 - SetAllEquipmentBonuses 메서드 사용
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus + stat.PhysicalAttack,
                Player.equipmentMagicBonus + stat.MagicAttack,
                Player.equipmentStrengthBonus + stat.Strength,
                Player.equipmentAgilityBonus + stat.Dexterity,
                Player.equipmentVitalityBonus + stat.Vitality,
                Player.equipmentIntelligenceBonus + stat.Intelligence,
                Player.equipmentCriticalChanceBonus + stat.CriticalChance,
                Player.equipmentCriticalDamageBonus, // 크리티컬 데미지 보너스는 현재 아이템에서 제공하지 않음
                Player.equipmentDodgeChanceBonus + stat.Evasion,
                Player.equipmentBlockChanceBonus, // 블록 확률 보너스는 현재 아이템에서 제공하지 않음
                Player.equipmentPhysicalDefenseBonus + stat.PhysicalDefense,
                Player.equipmentMagicDefenseBonus + stat.MagicDefense,
                Player.equipmentAttackSpeedBonus + stat.AttackSpeed,
                Player.equipmentMoveSpeedBonus, // 이동 속도 보너스는 현재 아이템에서 제공하지 않음
                Player.equipmentMaxHpBonus + stat.MaxHealth,
                Player.equipmentMaxMpBonus + stat.MaxMana
            );
        }
        else
        {
            // 해제 시 장비 보너스 감소 - SetAllEquipmentBonuses 메서드 사용
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus - stat.PhysicalAttack,
                Player.equipmentMagicBonus - stat.MagicAttack,
                Player.equipmentStrengthBonus - stat.Strength,
                Player.equipmentAgilityBonus - stat.Dexterity,
                Player.equipmentVitalityBonus - stat.Vitality,
                Player.equipmentIntelligenceBonus - stat.Intelligence,
                Player.equipmentCriticalChanceBonus - stat.CriticalChance,
                Player.equipmentCriticalDamageBonus, // 크리티컬 데미지 보너스는 현재 아이템에서 제공하지 않음
                Player.equipmentDodgeChanceBonus - stat.Evasion,
                Player.equipmentBlockChanceBonus, // 블록 확률 보너스는 현재 아이템에서 제공하지 않음
                Player.equipmentPhysicalDefenseBonus - stat.PhysicalDefense,
                Player.equipmentMagicDefenseBonus - stat.MagicDefense,
                Player.equipmentAttackSpeedBonus - stat.AttackSpeed,
                Player.equipmentMoveSpeedBonus, // 이동 속도 보너스는 현재 아이템에서 제공하지 않음
                Player.equipmentMaxHpBonus - stat.MaxHealth,
                Player.equipmentMaxMpBonus - stat.MaxMana
            );
        }

        // UpdateDerivedStats는 SetAllEquipmentBonuses 내부에서 이미 호출됨
    }

    //HJ 03.06 추가
    //한 아이템 스탯만 적용
    private void UpdatePlayerStats(string statName, float value, int multiplier)
    {
        float valueToApply = value * multiplier;
        
        // 기본 스탯
        if (statName == "Strength") 
        {
            int newValue = Player.equipmentStrengthBonus + Mathf.RoundToInt(valueToApply);
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                Player.equipmentMagicBonus,
                newValue,
                Player.equipmentAgilityBonus,
                Player.equipmentVitalityBonus,
                Player.equipmentIntelligenceBonus,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                Player.equipmentMagicDefenseBonus,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                Player.equipmentMaxMpBonus
            );
        }
        else if (statName == "Dexterity") 
        {
            int newValue = Player.equipmentAgilityBonus + Mathf.RoundToInt(valueToApply);
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                Player.equipmentMagicBonus,
                Player.equipmentStrengthBonus,
                newValue,
                Player.equipmentVitalityBonus,
                Player.equipmentIntelligenceBonus,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                Player.equipmentMagicDefenseBonus,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                Player.equipmentMaxMpBonus
            );
        }
        else if (statName == "Intelligence")
        {
            int newValue = Player.equipmentIntelligenceBonus + Mathf.RoundToInt(valueToApply);
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                Player.equipmentMagicBonus,
                Player.equipmentStrengthBonus,
                Player.equipmentAgilityBonus,
                Player.equipmentVitalityBonus,
                newValue,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                Player.equipmentMagicDefenseBonus,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                Player.equipmentMaxMpBonus
            );
        }
        else if (statName == "Vitality")
        {
            int newValue = Player.equipmentVitalityBonus + Mathf.RoundToInt(valueToApply);
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                Player.equipmentMagicBonus,
                Player.equipmentStrengthBonus,
                Player.equipmentAgilityBonus,
                newValue,
                Player.equipmentIntelligenceBonus,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                Player.equipmentMagicDefenseBonus,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                Player.equipmentMaxMpBonus
            );
        }
        
        // 파생 스탯
        else if (statName == "MaxHealth")
        {
            int newValue = Player.equipmentMaxHpBonus + Mathf.RoundToInt(valueToApply);
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                Player.equipmentMagicBonus,
                Player.equipmentStrengthBonus,
                Player.equipmentAgilityBonus,
                Player.equipmentVitalityBonus,
                Player.equipmentIntelligenceBonus,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                Player.equipmentMagicDefenseBonus,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                newValue,
                Player.equipmentMaxMpBonus
            );
        }
        else if (statName == "MaxMana")
        {
            int newValue = Player.equipmentMaxMpBonus + Mathf.RoundToInt(valueToApply);
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                Player.equipmentMagicBonus,
                Player.equipmentStrengthBonus,
                Player.equipmentAgilityBonus,
                Player.equipmentVitalityBonus,
                Player.equipmentIntelligenceBonus,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                Player.equipmentMagicDefenseBonus,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                newValue
            );
        }
        else if (statName == "PhysicalAttack")
        {
            float newValue = Player.equipmentPhysicalBonus + valueToApply;
            Player.SetAllEquipmentBonuses(
                newValue,
                Player.equipmentMagicBonus,
                Player.equipmentStrengthBonus,
                Player.equipmentAgilityBonus,
                Player.equipmentVitalityBonus,
                Player.equipmentIntelligenceBonus,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                Player.equipmentMagicDefenseBonus,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                Player.equipmentMaxMpBonus
            );
        }
        else if (statName == "MagicAttack")
        {
            float newValue = Player.equipmentMagicBonus + valueToApply;
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                newValue,
                Player.equipmentStrengthBonus,
                Player.equipmentAgilityBonus,
                Player.equipmentVitalityBonus,
                Player.equipmentIntelligenceBonus,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                Player.equipmentMagicDefenseBonus,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                Player.equipmentMaxMpBonus
            );
        }
        else if (statName == "PhysicalDefense")
        {
            float newValue = Player.equipmentPhysicalDefenseBonus + valueToApply;
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                Player.equipmentMagicBonus,
                Player.equipmentStrengthBonus,
                Player.equipmentAgilityBonus,
                Player.equipmentVitalityBonus,
                Player.equipmentIntelligenceBonus,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                newValue,
                Player.equipmentMagicDefenseBonus,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                Player.equipmentMaxMpBonus
            );
        }
        else if (statName == "MagicDefense")
        {
            float newValue = Player.equipmentMagicDefenseBonus + valueToApply;
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                Player.equipmentMagicBonus,
                Player.equipmentStrengthBonus,
                Player.equipmentAgilityBonus,
                Player.equipmentVitalityBonus,
                Player.equipmentIntelligenceBonus,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                newValue,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                Player.equipmentMaxMpBonus
            );
        }
        else if (statName == "CriticalChance")
        {
            float newValue = Player.equipmentCriticalChanceBonus + valueToApply;
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                Player.equipmentMagicBonus,
                Player.equipmentStrengthBonus,
                Player.equipmentAgilityBonus,
                Player.equipmentVitalityBonus,
                Player.equipmentIntelligenceBonus,
                newValue,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                Player.equipmentMagicDefenseBonus,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                Player.equipmentMaxMpBonus
            );
        }
        else if (statName == "AttackSpeed")
        {
            float newValue = Player.equipmentAttackSpeedBonus + valueToApply;
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                Player.equipmentMagicBonus,
                Player.equipmentStrengthBonus,
                Player.equipmentAgilityBonus,
                Player.equipmentVitalityBonus,
                Player.equipmentIntelligenceBonus,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                Player.equipmentDodgeChanceBonus,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                Player.equipmentMagicDefenseBonus,
                newValue,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                Player.equipmentMaxMpBonus
            );
        }
        else if (statName == "Evasion")
        {
            float newValue = Player.equipmentDodgeChanceBonus + valueToApply;
            Player.SetAllEquipmentBonuses(
                Player.equipmentPhysicalBonus,
                Player.equipmentMagicBonus,
                Player.equipmentStrengthBonus,
                Player.equipmentAgilityBonus,
                Player.equipmentVitalityBonus,
                Player.equipmentIntelligenceBonus,
                Player.equipmentCriticalChanceBonus,
                Player.equipmentCriticalDamageBonus,
                newValue,
                Player.equipmentBlockChanceBonus,
                Player.equipmentPhysicalDefenseBonus,
                Player.equipmentMagicDefenseBonus,
                Player.equipmentAttackSpeedBonus,
                Player.equipmentMoveSpeedBonus,
                Player.equipmentMaxHpBonus,
                Player.equipmentMaxMpBonus
            );
        }
        
        // 모든 스탯 업데이트는 SetAllEquipmentBonuses 내부에서 이미 호출됨
    }

    private IEnumerator RemoveBuffAfterDuration(Item item, float duration, int amount)
    {
        yield return new WaitForSeconds(duration);
        
        // 버프 스탯 해제
        UpdatePlayerStats(item.itemStat, -amount);
        
        Debug.Log($"{item.name} 버프 효과 해제, 효과량 {amount}");
    }

    //HJ 03.06 추가
    private IEnumerator RemoveBuffAfterDuration(BuffType buffType, string statName, float value, float duration)
    {
        yield return new WaitForSeconds(duration);
        
        // 버프 효과 제거
        UpdatePlayerStats(statName, -value, 1);
        
        Debug.Log($"{buffType} 버프 효과 해제");
        
        // 액티브 버프 딕셔너리에서 제거
        if (activeBuffs.ContainsKey(buffType))
        {
            activeBuffs.Remove(buffType);
        }
        
        // 코루틴 딕셔너리에서 제거
        if (activeCoroutines.ContainsKey(buffType))
        {
            activeCoroutines.Remove(buffType);
        }
    }

    private void PlayParticle(Item item)
    {
        if (item.effect.effectParticle != null)
        {
            var itemEffectGo = Instantiate(item.effect.effectParticle,
                GameManager.playerTransform.position + particlePositionOffset,
                Quaternion.identity);

            itemEffectGo.transform.SetParent(GameManager.playerTransform);
            Destroy(itemEffectGo, effectParticleDuration);
        }
    }

    
    #endregion

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}



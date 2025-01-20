using System.Collections.Generic;
using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// TODO 포션 같은 경우 아이템 크기에 따른 효과도 따로 부여해야될듯
/// </summary>

public class ItemEffectManager : BaseManager<ItemEffectManager>
{
    private PlayerData Player
    {
        get { return CharacterManager.PlayerCharacterData; }
        set { CharacterManager.PlayerCharacterData = value; }
    }

    private Dictionary<ItemGrade, float> gradeBonus = new Dictionary<ItemGrade, float>
    {
        { ItemGrade.일반, 0f },
        { ItemGrade.고급, 10f },
        { ItemGrade.희귀, 20f },
        { ItemGrade.에픽, 30f },
        { ItemGrade.전설, 50f },
        { ItemGrade.신화, 75f }
    };



    //아이템 효과 적용
    public void ApplyItemEffect(Item item, int quantity = 1)
    {
        switch(item.type)
        {
            case ItemType.소모품:
                ApplyConsumableEffect(item, quantity);
            break;
            case ItemType.무기:
            case ItemType.방어구:
            case ItemType.장신구:
                ApplyEquipmentEffect(item);
            break;
            default:
                Debug.Log($"아이템타입 {item.type}의 효과 없음");
            break;
        }
    }

    //아이템 장착 해제
    public void UnequipmentEffect(Item item)
    {
        ItemType itemType = item.type;
        
        if(itemType == ItemType.무기 || itemType == ItemType.방어구 || itemType == ItemType.장신구)
        {
            if(item.itemStat != null)
            {
                ItemStat enhancedStat = ApplyGradeBonus(item.itemStat, item.grade);
                UpdatePlayerStats(enhancedStat, -1);

                //TODO 인벤토리 관련 로직 추가
            }
        }
    }



    //소모품 효과
    private void ApplyConsumableEffect(Item item, int quantity = 1)
    {
        if (item.consumableType == ConsumableType.체력포션)
        {
            Player.currentHp = Mathf.Min(Player.maxHp, (Player.currentHp + item.effectAmount) * quantity);
            Debug.Log($"{item.name}을 사용하여 체력 {item.effectAmount}만큼 회복");
        }
        else if (item.consumableType == ConsumableType.마나포션)
        {
            Player.currentMp = Mathf.Min(Player.maxMp, (Player.currentMp + item.effectAmount) * quantity);
            Debug.Log($"{item.name}을 사용하여 마나 {item.effectAmount}만큼 회복");
        }
        else if(item.consumableType == ConsumableType.버프)
        {
            //TODO 버프지속시간 관리 형태
            ApplyBuff(item, 0f);
            Debug.Log($"{item.name}을 사용하여 버프 적용");
        }
    }

    //장착 아이템 효과
    private void ApplyEquipmentEffect(Item item)
    {
        if (item.itemStat != null)
        {
            ItemStat enhancedStat = ApplyGradeBonus(item.itemStat, item.grade);
            UpdatePlayerStats(enhancedStat, 1);
        }
    }

    //버프 아이템 효과
    private void ApplyBuff(Item item, float duration)
    {
        //TODO 버프아이템에도 아이템 등급별 보너스값 적용할지 고민
        //버프아이템 효과 적용
        UpdatePlayerStats(item.itemStat, 1);

        Debug.Log($"{item.name} 버프 아이템 사용, 효과량: {item.effectAmount}");

        //버프 지속시간
        StartCoroutine(RemoveBuffAfterDuration(item, duration));
    }

    //아이템 등급에 따른 보너스 값 적용 
    private ItemStat ApplyGradeBonus(ItemStat itemStat, ItemGrade grade)
    {
        if (!gradeBonus.TryGetValue(grade, out float bonusPercentage))
        {
            Debug.Log($"{grade} 등급에 대한 보너스 값이 없습니다.");
            return itemStat;
        }

        ItemStat enhancedStat = itemStat.Clone();
        enhancedStat.Strength += Mathf.RoundToInt(itemStat.Strength * (bonusPercentage / 100f));
        enhancedStat.Dexterity += Mathf.RoundToInt(itemStat.Dexterity * (bonusPercentage / 100f));
        enhancedStat.Intelligence += Mathf.RoundToInt(itemStat.Intelligence * (bonusPercentage / 100f));
        enhancedStat.Vitality += Mathf.RoundToInt(itemStat.Vitality * (bonusPercentage / 100f));
        enhancedStat.Luck += Mathf.RoundToInt(itemStat.Luck * (bonusPercentage / 100f));

        return enhancedStat;
    }

    //플레이어 스탯 업데이트
    private void UpdatePlayerStats(ItemStat stat, int multiplier)
    {
        Player.strength += stat.Strength * multiplier;
        //Player.dexterity += stat.Dexterity * multiplier;
        Player.intelligence += stat.Intelligence * multiplier;
        Player.vitality += stat.Vitality * multiplier;
        //Player.luck += stat.Luck * multiplier;

        Player.maxHp += stat.MaxHealth * multiplier;
        Player.maxMp += stat.MaxMana * multiplier;
        Player.physicalDamage += stat.PhysicalAttack * multiplier;
        Player.magicDamage += stat.MagicAttack * multiplier;
        Player.physicalDefense += stat.PhysicalDefense * multiplier;
        Player.magicDefense += stat.MagicDefense * multiplier;

        Player.criticalChance += stat.CriticalChance * multiplier;
        Player.attackSpeed += stat.AttackSpeed * multiplier;
        //Player.evasion += stat.Evasion * multiplier;
    }

    //버프 지속시간 처리
    private IEnumerator RemoveBuffAfterDuration(Item item, float duration)
    {
        yield return new WaitForSeconds(duration);

        //버프 스탯 해제
        UpdatePlayerStats(item.itemStat, -1);
        Debug.Log($"{item.name} 버프 효과 종료");
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
   
    }
}

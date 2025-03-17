using UnityEngine;
using System;

/// <summary>
/// 불은 밸런스 / 얼음은 공격력 / 전기는 공속
/// 불은 도트뎀, 물 이속감소, 전기 스턴, 땅 깡뎀
/// 속성 무기는 여러번 때리면 카운트가 올라가고 카운트 다 쓰면 일반 무기처럼... 일정 시간이 지나면 다시 충전
/// 디버프는 처음 타격했을때만 적용, 더 때린다고 해서 효과가 누적되거나 지속시간이 늘어나지 x
/// </summary>
[Serializable]
public class ItemSkill
{
    public ElementalAttribute element;     //속성
    public int level = 0;           //아이템 레벨 (강화)

    // 2025.03.16 HYO 추가 ---------
    public float debuffDuration;
    public float debuffValue;
    // ----------------------------
    
    //public GameObject attackEffect; //공격 시 나오는 효과

    [Header("방어구 속성 저항.. 쓸지 안쓸지 모르겠음")]
    public ElementalAttribute resistance;    //저항 속성
    public float resistantAmount;            //저항 수치

    //속성, 레벨, 아이템 등급 적용한 강화 수치
    public float ApplyPower(Item item)
    {
        Debug.Log(item.itemSkill.ApplyElement(item) * ApplyGradeMultiplier(item));
        return item.itemSkill.ApplyElement(item) * ApplyGradeMultiplier(item);
    }

    //아이템 수치 적용
    public void ApplyItemStat(Item item, float amount, int multiplier)
    {
        if (level >= 9)
            return;

        ItemStat itemStat = item.itemStat;
        if (itemStat == null)
        {
            Debug.Log("아이템스탯 null");
            return;
        }

        if(item.type == ItemType.무기)
        {
            if(item.weaponType == WeaponType.완드)
            {
                itemStat.MagicAttack += amount * multiplier;

                if (ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.손) == item)
                {
                    CharacterManager.PlayerCharacterData.magicDamage += amount * multiplier;
                }
            }
            else
            {
                itemStat.PhysicalAttack += amount * multiplier;

                if (ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.손) == item)
                {
                    CharacterManager.PlayerCharacterData.physicalDamage += amount * multiplier;
                }
            }
        }
        else if(item.type == ItemType.방어구)
        {
            if(itemStat.PhysicalDefense >= itemStat.MagicDefense)
            {
                itemStat.PhysicalDefense += amount * multiplier;

                if(ItemEffectManager.Instance.GetEquippedItem(item.equipmentSlot) == item)
                {
                    CharacterManager.PlayerCharacterData.physicalDefense += amount * multiplier;
                }
            }
            else
            {
                itemStat.MagicDefense += amount * multiplier;

                if (ItemEffectManager.Instance.GetEquippedItem(item.equipmentSlot) == item)
                {
                    CharacterManager.PlayerCharacterData.magicDefense += amount * multiplier;
                }
            }
        }
    }

    public ItemSkill Clone()
    {
        ItemSkill newSkill = new ItemSkill();

        newSkill.element = this.element;
        newSkill.level = this.level;
        // 2025.03.16 HYO 추가 ---------------------------
        newSkill.debuffDuration = this.debuffDuration;
        newSkill.debuffValue = this.debuffValue;
        // -----------------------------------------------

        return newSkill;
    }

    //속성, 레벨에 따른 파워
    //수치 조정 예정
    private float ApplyElement(Item item)
    {
        float basePower = 5;

        switch (item.itemSkill.element)
        {
            case ElementalAttribute.Earth:
                basePower = 10;
                break;
                //다른 속성들은 밸런스에 따라 추가할지 말지 결정
        }

        return basePower * (1f + level * 0.2f);
    }

    //등급에 따른 파워 배율
    //수치 조정 예정
    private float ApplyGradeMultiplier(Item item)
    {
        switch(item.grade)
        {
            case ItemGrade.일반: return 1.0f;
            case ItemGrade.고급: return 1.2f;
            case ItemGrade.희귀: return 1.4f;
            case ItemGrade.에픽: return 1.6f;
            case ItemGrade.전설: return 1.8f;
            case ItemGrade.신화: return 2.0f;
            default: return 1.0f;
        }
    }
}
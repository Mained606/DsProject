using UnityEngine;
using System;
using System.Collections.Generic;

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
    public GameObject attackEffect; //공격 시 나오는 효과

    [Header("무기")]
    public float power = 0;        //공격력 + (무기)

    [Header("방어구.. 쓸지 안쓸지 모르겠음")]
    public ElementalAttribute resistance;    //저항 속성
    public float resistantAmount;            //저항 수치

    //아이템 습득, 강화시에 초기화
    public void Initialize(Item item)
    {
        AdjustElementValue(item);
        ApplyItemStat(item);
    }

    /// <summary>
    /// 아이템 레벨에 따라 값을 획일적으로 수정하기
    /// </summary>
    public void AdjustElementValue(Item item)
    {
        if (level >= 9)
            return;

        if (item.type == ItemType.무기)
        {
            if (element == ElementalAttribute.Earth)
                power += 10;
            else
                power += 5;
        }
    }

    /// <summary>
    /// 다운그레이드 됐을 경우를 고려해서 수정하기
    /// </summary>
    public void ApplyItemStat(Item item)
    {
        if (level >= 9)
            return;

        if(item.type == ItemType.무기)
        {
            if(item.weaponType == WeaponType.한손무기 || item.weaponType == WeaponType.양손무기)
            {
                item.itemStat.PhysicalAttack += power;

                if (ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.손) == item)
                {
                    CharacterManager.PlayerCharacterData.physicalDamage += power;
                }
            }
        }
    }

    public ItemSkill Clone()
    {
        ItemSkill newSkill = new ItemSkill();

        newSkill.element = this.element;
        newSkill.level = this.level;
        newSkill.attackEffect = this.attackEffect;
        newSkill.power = this.power;

        return newSkill;
    }
}

//public enum ElementType
//{
//    Normal,
//    Fire,
//    Water,
//    Electric,
//    Ground
//}
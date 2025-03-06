using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 불은 밸런스 / 얼음은 공격력 / 전기는 공속
/// 불은 도트뎀, 물 이속감소, 전기 스턴, 땅
/// 속성 무기는 여러번 때리면 카운트가 올라가고 카운트 다 쓰면 일반 무기처럼... 일정 시간이 지나면 다시 충전
/// </summary>
[Serializable]
public class ItemSkill
{
    public string skillName;        //이름
    public string description;      //설명
    public ElementType element;     //속성
    public int level = 0;           //아이템 레벨 (강화)
    public GameObject weaponEffect; //무기 자체에 적용된 효과
    public GameObject attackEffect; //공격 시 나오는 효과

    public float power = 0;        //공격력 + (무기)
    //public float deffence = 1;      //방어력 + (방어구)
    public float attackSpeed = 0;   //공격 속도 +

    [HideInInspector] public bool isActive = true;

    //아이템 강화시에 초기화
    public void Initialize(Item item)
    {
        AdjustElementValue(item);
        ApplyItemStat(item);
    }

    public void AdjustElementValue(Item item)
    {
        if (level >= 9)
            return;

        if (item.type == ItemType.무기)
        {
            if (element == ElementType.Ground)
                power += 10;
            else
                power += 5;
        }
    }

    public void ApplyItemStat(Item item)
    {
        if (level >= 9)
            return;

        if(item.type == ItemType.무기)
        {
            if(item.weaponType == WeaponType.한손무기 || item.weaponType == WeaponType.양손무기)
            {
                item.itemStat.PhysicalAttack += power;

                if (ItemEffectManager.Instance.equippedItems[EquipmentSlot.손] == item)
                {
                    CharacterManager.PlayerCharacterData.physicalDamage += power;
                }
            }
        }
    }
}

public enum ElementType
{
    Normal,
    Fire,
    Water,
    Electric,
    Ground
}
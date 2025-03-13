using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class ItemSkillManager : BaseManager<ItemSkillManager>
{
    private HashSet<CharacterData> affectedTargets = new HashSet<CharacterData>();

    [SerializeField] private float maxAttackCount = 4;
    [SerializeField] private float elementRecoverTime = 10f;
    [SerializeField] private float attackEffectDuration = 0.5f;
    [SerializeField] private float debuffDuration = 10f;    //임시

    [SerializeField] private Vector3 attackParticleOffset = new Vector3();    

    private Transform WeaponTransform
    {
        get
        {
            if (ItemEffectManager.Instance.WeaponManager.CurrentWeaponObject == null)
            {
                Debug.Log("CurrentWeaponObject = null");
                return null;
            }

            return ItemEffectManager.Instance.WeaponManager.CurrentWeaponObject.transform;
        }
    }

    public int attackCount = 0;
    public Coroutine elementDisableCoroutine = null;
    [SerializeField] private bool isActive = false;
    public bool IsActive
    {
        get { return isActive; }
        set
        {
            isActive = value;
            PlayWeaponParticle(ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.손), WeaponTransform);
        }
    }

    //속성 공격
    public void ElementAttack(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (!IsActive || skill?.element is ElementalAttribute.None or ElementalAttribute.Earth)
            return;

        if (affectedTargets.Contains(target))
            return;

        affectedTargets.Add(target);
        StartCoroutine(RemoveAffectedTarget(target, debuffDuration));

        switch (skill.element)
        {
            case ElementalAttribute.Fire:
                ApplyFireEffect(weapon, target);
                break;

            case ElementalAttribute.Water:
                ApplyWaterEffect(weapon, target);
                break;

            case ElementalAttribute.Electric:
                ApplyElectircEffect(weapon, target);
                break;
        }
    }

    //속성 저항 값
    public float ElementResistantAmount(Item item, CharacterData enemy)
    {
        if (item.itemSkill == null) return 0;

        float reductionDamage = 0;

        if (item.type == ItemType.방어구 && item.itemSkill.resistance != ElementalAttribute.None)
        {
            if(enemy.attribute == item.itemSkill.resistance)
            {
                reductionDamage = item.itemSkill.resistantAmount;
            }
        }

        return reductionDamage;
    }

    //ComboAttackState에서 호출할 함수
    public void UpdateAttack()
    {
        if (!isActive) return;

        attackCount++;

        PlayAttackParticle(ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.손));

        if (attackCount >= maxAttackCount)
        {
            if(elementDisableCoroutine != null)
            {
                StopCoroutine(elementDisableCoroutine);
                elementDisableCoroutine = null;
            }
            
            elementDisableCoroutine = StartCoroutine(DisableWeaponEffect());
        }
    }

    //장착시 카운트 리셋
    public void EquipReset(Item item)
    {
        ResetCount(item);
        IsActive = true;
    }

    //장비 해제시 카운트 리셋
    public void UnequipReset(Item item)
    {
        ResetCount(item);
        IsActive = false;

        ActiveEffectGo(item, false);
    }

    #region PrivateMethod
    private void ResetCount(Item weapon)
    {
        if (weapon.type != ItemType.무기) return;

        if (weapon.itemSkill == null) return;

        if (weapon.itemSkill.element == ElementalAttribute.None || weapon.itemSkill.element == ElementalAttribute.Earth) return;

        if (elementDisableCoroutine != null)
        {
            StopCoroutine(elementDisableCoroutine);
            elementDisableCoroutine = null;
        }

        attackCount = 0;
    }

    //지속뎀
    private void ApplyFireEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !IsActive) return;

        //지속뎀 로직 추가
        Debug.Log("지속뎀");
    }

    //이속 감소
    private void ApplyWaterEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !IsActive) return;

        //이속 감소
        Debug.Log("이속감소");
    }

    //감전
    private void ApplyElectircEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !IsActive) return;

        //감전
        Debug.Log("감전");
    }

    //무기 속성 이펙트 비활성화
    private IEnumerator DisableWeaponEffect()
    {
        IsActive = false;

        yield return new WaitForSeconds(elementRecoverTime);

        attackCount = 0;
        IsActive = true;

        elementDisableCoroutine = null;
    }

    //속성 이펙트 실행
    private void PlayWeaponParticle(Item weapon, Transform weaponTransform)
    {
        if (weapon == null || weapon.itemSkill == null) return;

        if (weapon.itemSkill.element == ElementalAttribute.None) return;

        Transform effectTransform = ActiveEffectGo(weapon, true);

        if (effectTransform == null) return;

        VisualEffect effect = effectTransform.GetComponent<VisualEffect>();

        if (effect != null)
        {
            if (IsActive)
            {
                effect.Play();
            }
            else
            {
                effect.Stop();
            }
        }
    }

    //공격 이펙트 실행
    private void PlayAttackParticle(Item weapon)
    {
        GameObject effect = weapon.itemSkill.attackEffect;

        if(effect != null)
        {
            //Debug.Log("공격 파티클 실행");

            GameObject attackEffect = Instantiate(effect,
                WeaponTransform.position + attackParticleOffset,
                WeaponTransform.rotation);

            attackEffect.transform.SetParent(WeaponTransform);
            Destroy(attackEffect, attackEffectDuration);
        }
    }

    private IEnumerator RemoveAffectedTarget(CharacterData target, float duration)
    {
        yield return new WaitForSeconds(duration);

        affectedTargets.Remove(target);
    }

    private Transform ActiveEffectGo(Item item, bool isActive)
    {
        int index = (int)item.itemSkill.element - 1;
        if (WeaponTransform.childCount <= index || index < 0) return null;

        Transform effectTransform = WeaponTransform.GetChild(index);
        effectTransform.gameObject.SetActive(isActive);

        return effectTransform;
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemSkillManager : BaseManager<ItemSkillManager>
{
    public Dictionary<Item, Coroutine> elementCoroutine = new Dictionary<Item, Coroutine>();

    [SerializeField] private float elementRecoverTime = 10f;
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private Vector3 weaponParticleOffset = new Vector3();
    [SerializeField] private Vector3 particleOffset = new Vector3();

    private Transform weaponTransform;

    public int attackCount = 0;
    public Coroutine elementDisableCoroutine = null;
    private bool isActive = false;
    public bool IsActive
    {
        get { return isActive; }
        set
        {
            isActive = value;
            PlayWeaponParticle(ItemEffectManager.Instance.equippedItems[EquipmentSlot.손], weaponTransform);
        }
    }

    protected override void Start()
    {
        base.Start();

        weaponTransform = ItemEffectManager.Instance.weaponManager.CurrentWeaponObject.transform;
    }

    //CombatManager ProcessAttack에서 활용
    public void ApplyElementEffect(Item weapon, CharacterData target, Transform targetTransform)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || skill.element == ElementType.Normal || skill.element == ElementType.Ground)
            return;

        if (!IsActive)
            return;

        switch (skill.element)
        {
            case ElementType.Fire:
                ApplyFireEffect(weapon, target);
                break;

            case ElementType.Water:
                ApplyWaterEffect(weapon, target);
                break;

            case ElementType.Electric:
                ApplyElectircEffect(weapon, target);
                break;
        }

        PlayAttackParticle(weapon);
    }

    //ComboAttackState에서 호출할 함수
    public void UpdateAttackCount()
    {
        attackCount++;

        if(attackCount >= 4)
        {
            if(elementDisableCoroutine != null)
            {
                StopCoroutine(elementDisableCoroutine);
            }
            
            elementDisableCoroutine = StartCoroutine(DisableWeaponEffect());
        }
    }

    //무기 바꿨을때 카운트 리셋
    public void ResetAttackCount(Item weapon)
    {
        if (weapon.type != ItemType.무기) return;

        if (!ItemEffectManager.Instance.equippedItems.ContainsKey(EquipmentSlot.손)) return;

        if (weapon.itemSkill.element == ElementType.Normal || weapon.itemSkill.element == ElementType.Ground) return;

        if (ItemEffectManager.Instance.equippedItems[EquipmentSlot.손] != weapon)
        {
            if(elementDisableCoroutine != null)
            {
                StopCoroutine(elementDisableCoroutine);
            }

            attackCount = 0;
            IsActive = true;
        }
    }

    //장비 해제시 카운트 리셋
    public void UnequipResetCount(Item item)
    {
        if (item.type != ItemType.무기) return;

        if (item.itemSkill.element == ElementType.Normal || item.itemSkill.element == ElementType.Ground) return;

        if (elementDisableCoroutine != null)
        {
            StopCoroutine(elementDisableCoroutine);
        }

        attackCount = 0;
        IsActive = true;
    }

    //CombatManager ProcessAttack에서 데미지 계산시 사용
    public float ElementDamageMultiplier(Item weapon, CharacterData target)
    {
        ElementType element = weapon.itemSkill.element;

        if (element == ElementType.Normal) return 0;

        float damageMultiplier = 0;

        //if (element == ElementType.Fire)
        //{
        //    if(target.elementType == ElementType.Ground)
        //        damageMultiplier = 2f;
        //    else if (target.elementType == ElementType.Water)
        //        damageMultiplier = 0.5f;
        //}
        //else if (element == ElementType.Water)
        //{
        //    if(target.elementType == ElementType.Fire)
        //        damageMultiplier = 2f;
        //    else if (target.elementType == ElementType.Electric)
        //        damageMultiplier = 0.5f;
        //}
        //else if (element == ElementType.Electric)
        //{
        //    if (target.elementType == ElementType.Water)
        //        damageMultiplier = 2f;
        //    else if (target.elementType == ElementType.Fire)
        //        damageMultiplier = 0.5f;
        //}
        //else if (element == ElementType.Ground)
        //{
        //    if (target.elementType == ElementType.Electric)
        //        damageMultiplier = 2f;
        //    else if (target.elementType == ElementType.Water)
        //        damageMultiplier = 0.5f;
        //}

        return damageMultiplier;
    }

    #region PrivateMethod
    //지속뎀
    private void ApplyFireEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !IsActive) return;

        //지속뎀 로직 추가
    }

    //이속 감소
    private void ApplyWaterEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !IsActive) return;

        //이속 감소
    }

    //감전
    private void ApplyElectircEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !IsActive) return;

        //감전
    }

    //무기 속성 이펙트 비활성화
    private IEnumerator DisableWeaponEffect()
    {
        IsActive = false;
        //이펙트 비활성화 로직 추가

        yield return new WaitForSeconds(elementRecoverTime);

        attackCount = 0;
        IsActive = true;
        //이펙트 활성화 로직 추가

        elementDisableCoroutine = null;
    }

    //속성 이펙트 실행
    private void PlayWeaponParticle(Item weapon, Transform weaponTransform)
    {
        if (weapon == null || weapon.itemSkill == null) return;

        if (weapon.itemSkill.element == ElementType.Normal) return;

        GameObject effect = weapon.itemSkill.attackEffect;

        if(effect != null)
        {
            if (weaponTransform == null)
                return;

            ParticleSystem particle = weaponTransform.GetComponentInChildren<ParticleSystem>();

            if(particle != null)
            {
                if(IsActive)
                {
                    particle.Play();
                }
                else
                {
                    particle.Stop();
                }
            }
        }
    }

    //공격 이펙트 실행
    private void PlayAttackParticle(Item weapon)
    {
        GameObject effect = weapon.itemSkill.attackEffect;

        if(effect != null)
        {
            GameObject attackEffect = Instantiate(effect,
                transform.position + particleOffset,
                Quaternion.identity);

            attackEffect.transform.SetParent(this.transform);
            Destroy(attackEffect, effectDuration);
        }
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
    #endregion
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkillManager : BaseManager<ItemSkillManager>
{
    public Dictionary<Item, Coroutine> elementCoroutine = new Dictionary<Item, Coroutine>();
    [SerializeField] private float elementRecoverTime = 10f;
    [SerializeField] private float effectDuration = 0.5f;
    [SerializeField] private Vector3 particleOffset = new Vector3();

    public int attackCount = 0;
    public Coroutine elementDisableCoroutine = null;
    public bool isActive = true;

    public void ApplyElementEffect(Item weapon, CharacterData target, Transform targetTransform)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !isActive || attackCount >= 4)
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

    public void UpdateAttackCount()
    {
        attackCount++;

        if(attackCount >= 4)
        {
            elementDisableCoroutine = StartCoroutine(DisableWeaponEffect());
        }
    }

    public float ElementDamageMultiplier(Item weapon, CharacterData target)
    {
        ElementType element = weapon.itemSkill.element;

        if (element == ElementType.Normal)
            return 0;

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

        if (skill == null || !isActive)
            return;

        //지속뎀 로직 추가
    }

    //이속 감소
    private void ApplyWaterEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !isActive)
            return;

        //이속 감소
    }

    //감전
    private void ApplyElectircEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !isActive)
            return;

        //감전
    }

    //무기 속성 비활성화
    private IEnumerator DisableWeaponEffect()
    {
        isActive = false;
        //이펙트 비활성화 로직 추가

        yield return new WaitForSeconds(elementRecoverTime);

        isActive = true;
        //이펙트 활성화 로직 추가
    }

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

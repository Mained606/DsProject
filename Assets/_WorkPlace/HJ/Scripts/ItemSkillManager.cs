using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class ItemSkillManager : BaseManager<ItemSkillManager>
{
    public Dictionary<Item, Coroutine> elementCoroutine = new Dictionary<Item, Coroutine>();
    [SerializeField] private float elementRecoverTime = 10f;
    [SerializeField] private float effectDuration = 2f;
    [SerializeField] private Vector3 particleOffset = new Vector3();

    public int attackCount = 0;

    public void ApplyElementEffect(Item weapon, CharacterData target, Transform targetTransform)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !skill.isActive)
            return;

        if(attackCount >= 5)
        {
            elementCoroutine[weapon] = StartCoroutine(DisalbeWeaponEffect(weapon));
            return;
        }

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

        PlayParticle(weapon, targetTransform);
    }

    public float ElementDamageMultiplier(Item weapon, CharacterData target)
    {
        ElementType element = weapon.itemSkill.element;

        if (element == ElementType.Normal)
            return 0;

        float damageMultiplier = 0;

        //if (element == ElementType.Fire && target.elementType == ElementType.Ice)
        //{
        //    damageMultiplier = 1.2f;
        //}
        //else if (element == ElementType.Ice && target.elementType == ElementType.Fire)
        //{
        //    damageMultiplier = 0.8f;
        //}
        //else if(element == ElementType.Electric && target.elementType == ElementType.Fire)
        //{
        //    damageMultiplier = 1.1f;
        //}

        return damageMultiplier;
    }

    //지속뎀
    private void ApplyFireEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !skill.isActive)
            return;

        //지속뎀 로직 추가
    }

    //동결
    private void ApplyWaterEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !skill.isActive)
            return;

        //동결 로직 추가
    }

    //감전
    private void ApplyElectircEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !skill.isActive)
            return;

        //감전 로직 추가
    }

    //무기 속성 비활성화
    private IEnumerator DisalbeWeaponEffect(Item weapon)
    {
        weapon.itemSkill.isActive = false;
        //이펙트 비활성화 로직 추가

        yield return new WaitForSeconds(elementRecoverTime);

        weapon.itemSkill.isActive = true;
        //이펙트 활성화 로직 추가

        //딕셔너리 키값 제거
        if (elementCoroutine.ContainsKey(weapon))
            elementCoroutine.Remove(weapon);
    }

    private void PlayAttackParticle(Item weapon)
    {
        GameObject effect = weapon.itemSkill.attackEffect;

        if(effect != null)
        {

        }
    }

    private void PlayParticle(Item weapon, Transform targetTransform)
    {
        GameObject effect = weapon.itemSkill.attackEffect;

        if (effect != null)
        {
            var itemEffectGo = Instantiate(effect,
                targetTransform.position + particleOffset,
                Quaternion.identity);

            itemEffectGo.transform.SetParent(targetTransform);
            Destroy(itemEffectGo, effectDuration);
        }
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}

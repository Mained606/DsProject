using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.VFX;

public class ItemSkillManager : BaseManager<ItemSkillManager>
{
    public GameObject[] elementEffects;

    private HashSet<CharacterData> affectedTargets = new HashSet<CharacterData>();

    [SerializeField] private float maxAttackCount = 4;
    [SerializeField] private float elementRecoverTime = 10f;
    [SerializeField] private float attackEffectDuration = 0.5f;
    // 2025.03.16 HYO 주석 처리 ---------------------------------------
    // [SerializeField] private float debuffDuration = 10f;    //임시  
    // ---------------------------------------------------------------
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
            PlayWeaponParticle(ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.손));
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
        
        // 2025.03.16 HYO 주석 처리 ------------------------------------------------------------
        // StartCoroutine(RemoveAffectedTarget(target, debuffDuration));
        // ------------------------------------------------------------------------------------
        
        // 2025.03.16 HYO 추가 ----------------------------------------------------------------
        StartCoroutine(RemoveAffectedTarget(target, weapon.itemSkill.debuffDuration));
        // ------------------------------------------------------------------------------------
        
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

        if(item.itemSkill != null)
        {
            item.itemSkill.OnLevelChanged += level => ApplyLevelEffect(item, WeaponTransform, level);
        }

        if (WeaponTransform != null)
        {
            WeaponAttack weapon = WeaponTransform.GetComponent<WeaponAttack>();

            if (weapon == null || weapon.elementMaterials.Length <= 0)
                return; 

            int index = (int)item.itemSkill.element - 1;
            if (weapon.elementMaterials.Length >= index && index >= 0)
            {
                WeaponTransform.GetComponent<MeshRenderer>().material = weapon.elementMaterials[index];
            }
        }
    }

    //장비 해제시 카운트 리셋
    public void UnequipReset(Item item)
    {
        ResetCount(item);
        IsActive = false;

        ActiveEffect(item, false);

        if(item.itemSkill != null)
        {
            item.itemSkill.OnLevelChanged -= level => ApplyLevelEffect(item, WeaponTransform, level);
        }
    }

    #region Private Method
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
        // 2025.03.16 HYO 추가 ------------------------------------------------------------
        target.ApplyBurn(weapon.itemSkill.debuffDuration, weapon.itemSkill.debuffValue);
        // --------------------------------------------------------------------------------
    }

    //이속 감소
    private void ApplyWaterEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !IsActive) return;

        //이속 감소
        Debug.Log("이속감소");

        // 2025.03.16 HYO 추가 ------------------------------------------------------------
        target.ApplyFreeze(weapon.itemSkill.debuffDuration, weapon.itemSkill.debuffValue);
        // --------------------------------------------------------------------------------
    }

    //감전
    private void ApplyElectircEffect(Item weapon, CharacterData target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !IsActive) return;

        //감전
        Debug.Log("감전");

        // 2025.03.16 HYO 추가 ------------------------------------------------------------
        target.ApplyElectrify(weapon.itemSkill.debuffDuration);
        // --------------------------------------------------------------------------------
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
    private void PlayWeaponParticle(Item weapon)
    {
        if (weapon == null || weapon.itemSkill == null) return;

        if (weapon.itemSkill.element == ElementalAttribute.None) return;

        Transform effectTransform = ActiveEffect(weapon, true);

        if (effectTransform == null) return;

        VisualEffect effect = effectTransform.GetComponent<VisualEffect>();        

        if (effect != null)
        {
            if (IsActive)
            {
                effect.Play();
                ApplyLevelEffect(weapon, effectTransform, weapon.itemSkill.Level);
            }
            else
            {
                effect.Stop();
            }
        }
    }

    private void ApplyLevelEffect(Item item, Transform currentItem, int level)
    {
        if (item.type != ItemType.무기) return;

        ItemSkill skill = item.itemSkill;
        if (skill == null || skill.element == ElementalAttribute.None) return;

        VisualEffect effect = currentItem?.GetComponentInChildren<VisualEffect>();
        if (effect == null) return;

        if(skill.element == ElementalAttribute.Fire)
        {
            effect.SetFloat("SmokeSize", CalculateValue(1.2f, level));
            effect.SetFloat("PoisonRate", CalculateValue(125, level));
            effect.SetFloat("ParticlesRate", CalculateValue(30, level));
        }
        else if(skill.element == ElementalAttribute.Water)
        {
            effect.SetFloat("GooRate", CalculateValue(30 - 10, level) + 10);
            effect.SetFloat("SmokeRate", CalculateValue(15, level));
            effect.SetFloat("ParticlesRate", CalculateValue(30, level));
            effect.SetFloat("ParticlesSpiralRate", CalculateValue(10, level));
        }
        else if(skill.element == ElementalAttribute.Electric)
        {
            effect.SetFloat("SparksSize", CalculateValue(0.8f, level));
            effect.SetFloat("SmokeRate", CalculateValue(15, level));
            effect.SetFloat("ElectricityRate", CalculateValue(25, level));
        }
        else if(skill.element == ElementalAttribute.Earth)
        {
            effect.SetFloat("PoisonRate", CalculateValue(50, level));
            effect.SetFloat("SmokeSize", CalculateValue(0.8f, level));
            effect.SetFloat("ParticlesRate", CalculateValue(30, level));
        }
    }

    private float CalculateValue(float maxValue, int level)
    {
        return (maxValue / 10) * (level + 1);
    }

    //공격 이펙트 실행
    private void PlayAttackParticle(Item weapon)
    {
        int index = (int)weapon.itemSkill.element - 1;

        if (index < 0) return;

        GameObject effect = elementEffects[index];

        if(effect != null)
        {
            GameObject attackEffect = Instantiate(effect, WeaponTransform.position, WeaponTransform.rotation);

            if(WeaponTransform != null)
            {
                WeaponAttack weaponAttack = WeaponTransform.GetComponent<WeaponAttack>();
                attackEffect.transform.localScale = weaponAttack.effectScale;
            }

            attackEffect.transform.SetParent(WeaponTransform);
            Destroy(attackEffect, attackEffectDuration);
        }
    }

    private IEnumerator RemoveAffectedTarget(CharacterData target, float duration)
    {
        yield return new WaitForSeconds(duration);

        affectedTargets.Remove(target);
    }

    private Transform ActiveEffect(Item item, bool isActive)
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

//[System.Serializable]
//public class ElementEffectValue
//{
//    int minFireValue;
//    int maxFireValue;
//    int minWaterValue;
//    int maxWaterValue;
//    int minElectricValue;
//    int maxElectricValue;
//    int minEarthValue;
//    int maxEarthValue;
//}

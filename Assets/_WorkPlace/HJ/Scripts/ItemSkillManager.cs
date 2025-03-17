using System.Collections;
using System.Collections.Generic;
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

        // 2025.03.17 HYO 수정 - 땅 속성 추가로 인해 조건 주석 --------------------------------------
        // if (!IsActive || skill?.element is ElementalAttribute.None or ElementalAttribute.Earth)
        // ---------------------------------------------------------------------------------------

        // 2025.03.17 HYO 수정 - 속성 공격 조건 수정 -----------------------------------
        if (!IsActive || skill?.element == ElementalAttribute.None)
            return;
        // --------------------------------------------------------------------------

        if (affectedTargets.Contains(target))
            return;

        affectedTargets.Add(target);

        // 2025.03.16 HYO 주석 처리 ------------------------------------------------------------
        // StartCoroutine(RemoveAffectedTarget(target, debuffDuration));
        // ------------------------------------------------------------------------------------
        
        // 2025.03.17 HYO 수정 - 기존 고정 지속시간에서 무기 스킬의 지속시간 사용 ---------
        StartCoroutine(RemoveAffectedTarget(target, weapon.itemSkill.debuffDuration));
        // --------------------------------------------------------------------------
        
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
            // 2025.03.17 HYO 수정 - 땅 속성 추가 ------------------------------------------------------------
            case ElementalAttribute.Earth:
                ApplyEarthEffect(weapon, target);
                break;
            // ------------------------------------------------------------------------------------
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
        // 2025.03.17 HYO 수정 - 무기 속성 체크 주석 처리 ----------------------------------------------------------------------------
        // if (weapon.type != ItemType.무기) return;
        // if (weapon.itemSkill == null) return;
        // if (weapon.itemSkill.element == ElementalAttribute.None || weapon.itemSkill.element == ElementalAttribute.Earth) return;
        // -------------------------------------------------------------------------------------------------------------------------

        //무기 속성 체크
        ItemSkill skill = weapon.itemSkill;
        if (skill.element == ElementalAttribute.None) return;
        
        // 2025.03.16 HYO 수정 - Earth 속성 추가 및 관련 처리 ------------------------
        // Earth 속성도 처리 가능하도록 조건 수정
        IsActive = true;
        attackCount = 0;
        // --------------------------------------------------------------------------

        // 이미 돌아가고 있던 코루틴이 있다면 제거
        if (elementDisableCoroutine != null)
        {
            StopCoroutine(elementDisableCoroutine);
            elementDisableCoroutine = null;
        }
    }

    //지속뎀
    private void ApplyFireEffect(Item weapon, CharacterData target)
    {
        // 2025.03.17 HYO 수정 - 속성 공격 조건 수정 -----------------------------------
        // ItemSkill skill = weapon.itemSkill;
        // if (skill == null || !IsActive) return;
        // --------------------------------------------------------------------------

        PlayAttackParticle(weapon);
        
        // 2025.03.16 HYO 수정 - 새로운 효과 적용 시스템 사용 ------------------------
        // 화상 효과 적용 - 새 시스템 사용
        target.ApplyFireBurnEffect(weapon.itemSkill.debuffDuration, weapon.itemSkill.debuffValue);
        // --------------------------------------------------------------------------
        
        IncrementAttackCountAndCheckLimit();
    }

    //이속 감소
    private void ApplyWaterEffect(Item weapon, CharacterData target)
    {
        // 2025.03.17 HYO 수정 - 속성 공격 조건 수정 -----------------------------------
        // ItemSkill skill = weapon.itemSkill;
        // if (skill == null || !IsActive) return;
        // --------------------------------------------------------------------------

        PlayAttackParticle(weapon);
        
        // 2025.03.16 HYO 수정 - 새로운 효과 적용 시스템 사용 ------------------------
        // 이동속도 감소 효과 적용 - 새 시스템 사용
        target.ApplyWaterSlowEffect(weapon.itemSkill.debuffDuration, weapon.itemSkill.debuffValue);
        // --------------------------------------------------------------------------
        
        IncrementAttackCountAndCheckLimit();
    }

    //감전
    private void ApplyElectircEffect(Item weapon, CharacterData target)
    {
        // 2025.03.17 HYO 수정 - 속성 공격 조건 수정 -----------------------------------
        // ItemSkill skill = weapon.itemSkill;
        // if (skill == null || !IsActive) return;
        // --------------------------------------------------------------------------

        PlayAttackParticle(weapon);
        
        // 2025.03.16 HYO 수정 - 새로운 효과 적용 시스템 사용 ------------------------
        // 스턴 효과 적용 - 새 시스템 사용
        target.ApplyElectricStunEffect(weapon.itemSkill.debuffDuration);
        // --------------------------------------------------------------------------
        
        IncrementAttackCountAndCheckLimit();
    }

    // 2025.03.16 HYO 추가 - 중복 코드 제거를 위한 함수 -----------------------------
    private void IncrementAttackCountAndCheckLimit()
    {
        attackCount++;
        
        if (attackCount >= maxAttackCount)
        {
            IsActive = false;
            
            if (elementDisableCoroutine != null)
            {
                StopCoroutine(elementDisableCoroutine);
                elementDisableCoroutine = null;
            }
            
            elementDisableCoroutine = StartCoroutine(DisableWeaponEffect());
        }
    }
    // --------------------------------------------------------------------------

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

        VisualEffect effect = currentItem.GetComponentInChildren<VisualEffect>();
        if (effect == null) return;

        float amount = 0;

        switch (skill.element)
        {
            case ElementalAttribute.Fire:
                float smoke = (1.2f / 10) * (level + 1);
                float poision = (125 / 10) * (level + 1);
                effect.SetFloat("SmokeSize", smoke);
                effect.SetFloat("PoisonRate", poision);
                break;
            case ElementalAttribute.Water:
                amount = ((30 - 10) / 10) * (level + 1) + 10;
                effect.SetFloat("GooRate", amount);
                break;
            case ElementalAttribute.Electric:
                amount = (25 / 10) * (level + 1);
                effect.SetFloat("ElectricityRate", amount);
                break;
            case ElementalAttribute.Earth:
                amount = (50 / 10) * (level + 1);
                effect.SetFloat("PoisonRate", amount);
                break;
        }
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

    // 2025.03.16 HYO 추가 - 땅 속성 효과 구현 -----------------------------------
    // 땅 속성 효과 구현
    private void ApplyEarthEffect(Item weapon, CharacterData target)
    {
        PlayAttackParticle(weapon);
        
        // 땅 속성은 디버프가 아닌 공격자에게 데미지 증가 효과 적용
        CharacterData attacker = CharacterManager.PlayerCharacterData;
        
        // 데미지 증가 효과를 공격자에게 적용
        attacker.ApplyEarthDamageEffect(weapon.itemSkill.debuffDuration, weapon.itemSkill.debuffValue);
        
        IncrementAttackCountAndCheckLimit();
    }
    // --------------------------------------------------------------------------

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

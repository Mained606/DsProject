using System;
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
                //Debug.Log("CurrentWeaponObject = null");
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

    private event Action<int> onLevelChangedHandler;



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

    //속성 저항 계산(비율)
    public float ApplyElementalResistance(Item item, CharacterData enemy)
    {
        if (item.type != ItemType.방어구) return 0f;
        if (item.itemSkill.resistance == ElementalAttribute.None) return 0f;

        float reductionRate = 0f;
        
        if(enemy.attribute == item.itemSkill.resistance)
        {
            //기본 아이템 저항 비율
            reductionRate = item.itemSkill.resistantRate;

            //등급 적용
            reductionRate *= item.itemSkill.ApplyGradeMultiplier(item);

            //레벨 적용
            float levelBonus = Mathf.Clamp(1 + item.itemSkill.Level * 0.02f, 1f, 1.5f);
            reductionRate *= levelBonus;

            //최대 저항율 제한
            reductionRate = Mathf.Clamp(reductionRate, 0f, 0.8f);
        }

        return reductionRate;
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
        if (item.type != ItemType.무기) return;

        ResetCount(item);
        IsActive = true;

        if(item.itemSkill != null)
        {
            onLevelChangedHandler = level => ApplyLevelEffect(item, WeaponTransform, level);
            item.itemSkill.OnLevelChanged += onLevelChangedHandler;
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
        if (item.type != ItemType.무기) return;

        ResetCount(item);
        IsActive = false;

        ActiveEffect(item, false);

        if(item.itemSkill != null && onLevelChangedHandler != null)
        {
            item.itemSkill.OnLevelChanged -= onLevelChangedHandler;
            onLevelChangedHandler = null;
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
        
        // 2025.03.16 HYO 수정 - 새로운 효과 적용 시스템 사용 ------------------------
        // 화상 효과 적용 - 새 시스템 사용
        target.ApplyFireBurnEffect(weapon.itemSkill.debuffDuration, weapon.itemSkill.debuffValue);
        // --------------------------------------------------------------------------
    }

    //이속 감소
    private void ApplyWaterEffect(Item weapon, CharacterData target)
    {
        // 2025.03.17 HYO 수정 - 속성 공격 조건 수정 -----------------------------------
        // ItemSkill skill = weapon.itemSkill;
        // if (skill == null || !IsActive) return;
        // --------------------------------------------------------------------------
        
        // 2025.03.16 HYO 수정 - 새로운 효과 적용 시스템 사용 ------------------------
        // 이동속도 감소 효과 적용 - 새 시스템 사용
        target.ApplyWaterSlowEffect(weapon.itemSkill.debuffDuration, weapon.itemSkill.debuffValue);
        // --------------------------------------------------------------------------
    }

    //감전
    private void ApplyElectircEffect(Item weapon, CharacterData target)
    {
        // 2025.03.17 HYO 수정 - 속성 공격 조건 수정 -----------------------------------
        // ItemSkill skill = weapon.itemSkill;
        // if (skill == null || !IsActive) return;
        // --------------------------------------------------------------------------
        
        // 2025.03.16 HYO 수정 - 새로운 효과 적용 시스템 사용 ------------------------
        // 스턴 효과 적용 - 새 시스템 사용
        target.ApplyElectricStunEffect(weapon.itemSkill.debuffDuration);
        // --------------------------------------------------------------------------
    }

    // 2025.03.16 HYO 추가 - 중복 코드 제거를 위한 함수 -----------------------------
    //private void IncrementAttackCountAndCheckLimit()
    //{
    //    attackCount++;
        
    //    if (attackCount >= maxAttackCount)
    //    {
    //        IsActive = false;
            
    //        if (elementDisableCoroutine != null)
    //        {
    //            StopCoroutine(elementDisableCoroutine);
    //            elementDisableCoroutine = null;
    //        }
            
    //        elementDisableCoroutine = StartCoroutine(DisableWeaponEffect());
    //    }
    //}
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
        if (weapon.type != ItemType.무기) return;

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
        if (skill.element == ElementalAttribute.None) return;

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

        VisualEffect visualEffect = effect.GetComponentInChildren<VisualEffect>();
        if (visualEffect != null)
        {
            int level = weapon.itemSkill.Level;

            if(level < 3)
            {
                visualEffect.SetFloat("DarkAlpha", 0.5f);
                visualEffect.SetFloat("ParticlesSize", 0.1f);
                visualEffect.SetFloat("FloatingParticlesRate", 0.5f);
            }
            else if(level < 7)
            {
                visualEffect.SetFloat("DarkAlpha", 5);
                visualEffect.SetFloat("ParticlesSize", 0.5f);
                visualEffect.SetFloat("FloatingParticlesRate", 10);
            }
            else
            {
                visualEffect.SetFloat("DarkAlpha", 20);
                visualEffect.SetFloat("ParticlesSize", 1.2f);
                visualEffect.SetFloat("FloatingParticlesRate", 25);
            }
        }

        if (effect != null)
        {
            GameObject attackEffect = Instantiate(effect, WeaponTransform.position, WeaponTransform.rotation);

            if (WeaponTransform != null)
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
        // 땅 속성은 디버프가 아닌 공격자에게 데미지 증가 효과 적용
        CharacterData attacker = CharacterManager.PlayerCharacterData;
        
        // 데미지 증가 효과를 공격자에게 적용
        attacker.ApplyEarthDamageEffect(weapon.itemSkill.debuffDuration, weapon.itemSkill.debuffValue);
        
    }
    // --------------------------------------------------------------------------

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
    #endregion
}
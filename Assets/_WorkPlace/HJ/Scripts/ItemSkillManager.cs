using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSkillManager : BaseManager<ItemSkillManager>
{
    public Dictionary<Item, Coroutine> elementCoroutine = new Dictionary<Item, Coroutine>();
    public Dictionary<CharacterData, Coroutine> targetCoroutine = new Dictionary<CharacterData, Coroutine>();

    [SerializeField] private float maxAttackCount = 4;
    [SerializeField] private float elementRecoverTime = 10f;
    [SerializeField] private float attackEffectDuration = 0.5f;
    [SerializeField] private float targetEffectDuration = 5f;

    [SerializeField] private Vector3 attackParticleOffset = new Vector3();    

    private Transform WeaponTransform
    {
        get
        {
            if (ItemEffectManager.Instance.weaponManager.CurrentWeaponObject == null)
            {
                Debug.Log("CurrentWeaponObject = null");
                return null;
            }

            Debug.Log("WeaponTransform");
            
            return ItemEffectManager.Instance.weaponManager.CurrentWeaponObject.transform;
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

    //CombatManager ProcessAttack에서 활용
    public void ApplyElementEffect(Item weapon, CharacterData target, Transform targetTransform)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || skill.element == ElementalAttribute.None || skill.element == ElementalAttribute.Earth)
            return;

        if (!IsActive)
            return;


        if (!targetCoroutine.ContainsKey(target))
        {
            switch (skill.element)
            {
                case ElementalAttribute.Fire:
                    ApplyFireEffect(weapon, target, targetTransform);
                    break;

                case ElementalAttribute.Water:
                    ApplyWaterEffect(weapon, target, targetTransform);
                    break;

                case ElementalAttribute.Electric:
                    ApplyElectircEffect(weapon, target, targetTransform);
                    break;
            }

            PlayTargetParticle(weapon, targetTransform);
        }
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
            }
            
            elementDisableCoroutine = StartCoroutine(DisableWeaponEffect());
        }
    }

    //무기 장착시 카운트 리셋
    public void ResetAttackCount(Item weapon)
    {
        if (weapon.type != ItemType.무기) return;

        if(weapon.itemSkill == null)
        {
            Debug.Log($"{weapon.name}의 itemSkill이 null");
            return;
        }

        if (weapon.itemSkill.element == ElementalAttribute.None || weapon.itemSkill.element == ElementalAttribute.Earth) return;

        if (elementDisableCoroutine != null)
        {
            StopCoroutine(elementDisableCoroutine);
        }

        attackCount = 0;
        IsActive = true;
    }

    //장비 해제시 카운트 리셋
    public void UnequipResetCount(Item item)
    {
        if (item.type != ItemType.무기) return;

        if (item.itemSkill == null)
        {
            Debug.Log($"{item.name}의 itemSkill이 null");
            return;
        }

        if (item.itemSkill.element == ElementalAttribute.None || item.itemSkill.element == ElementalAttribute.Earth) return;

        if (elementDisableCoroutine != null)
        {
            StopCoroutine(elementDisableCoroutine);
        }

        attackCount = 0;
        IsActive = false;
    }

    #region PrivateMethod
    //지속뎀
    private void ApplyFireEffect(Item weapon, CharacterData target, Transform targetTransform)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !IsActive) return;

        //지속뎀 로직 추가
    }

    //이속 감소
    private void ApplyWaterEffect(Item weapon, CharacterData target, Transform targetTransform)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !IsActive) return;

        //이속 감소
    }

    //감전
    private void ApplyElectircEffect(Item weapon, CharacterData target, Transform targetTransform)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !IsActive) return;

        //감전
    }

    //무기 속성 이펙트 비활성화
    private IEnumerator DisableWeaponEffect()
    {
        Debug.Log("무기 속성 비활성화");
        IsActive = false;

        yield return new WaitForSeconds(elementRecoverTime);

        Debug.Log("무기 속성 활성화");

        attackCount = 0;
        IsActive = true;

        elementDisableCoroutine = null;
    }

    //속성 이펙트 실행
    private void PlayWeaponParticle(Item weapon, Transform weaponTransform)
    {
        if (weapon == null || weapon.itemSkill == null)
        {
            Debug.Log("itemSkill 없음");
            return;
        }

        if (weapon.itemSkill.element == ElementalAttribute.None)
        {
            Debug.Log("노말 타입 무기");
            return;
        }
        ParticleSystem particle = weaponTransform.GetComponentInChildren<ParticleSystem>();

        if(particle != null)
        {
            Debug.Log("속성 이펙트 실행");

            if (IsActive)
            {
                particle.Play();
            }
            else
            {
                particle.Stop();
            }
        }
    }

    //공격 이펙트 실행
    private void PlayAttackParticle(Item weapon)
    {
        GameObject effect = weapon.itemSkill.attackEffect;

        if(effect != null)
        {
            Debug.Log("공격 파티클 실행");

            GameObject attackEffect = Instantiate(effect,
                WeaponTransform.position + attackParticleOffset,
                Quaternion.identity);

            attackEffect.transform.SetParent(WeaponTransform);
            Destroy(attackEffect, attackEffectDuration);
        }
        else
        {
            Debug.Log("공격파티클 없음");
        }
    }

    //타겟 이펙트 실행
    private void PlayTargetParticle(Item weapon, Transform targetTransform)
    {
        GameObject effect = weapon.itemSkill.targetEffect;

        if(effect != null)
        {
            GameObject targetEffect = Instantiate(effect, targetTransform.position, Quaternion.identity);

            targetEffect.transform.SetParent(targetTransform);
            Destroy(targetEffect, targetEffectDuration);
        }
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
    #endregion
}

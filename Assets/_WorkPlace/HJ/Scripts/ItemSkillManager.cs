using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 타겟을 트랜스폼으로 받아올지 몬스터데이터로 받아올지 고민할것..
/// 몬스터 데이터에 각 속성별 카운트 추가
/// 일정 시간이 지나면 카운트가 0이 되도록 설정
/// </summary>
public class ItemSkillManager : BaseManager<ItemSkillManager>
{
    [SerializeField] private float effectDuration = 1f;
    [SerializeField] private Vector3 particleOffset = new Vector3();

    

    public void ApplyElementEffect(Item weapon, Transform target)
    {
        switch (weapon.itemSkill.element)
        {
            case ElementType.Fire:
                ApplyFireEffect(weapon, target);
                break;

            case ElementType.Ice:
                ApplyIceEffect(weapon, target);
                break;

            case ElementType.Electric:
                ApplyElectircEffect(weapon, target);
                break;
        }

        PlayParticle(weapon, target);
    }

    //일정수치가 지나면 지속뎀
    private void ApplyFireEffect(Item weapon, Transform target)
    {
        ItemSkill skill = weapon.itemSkill;

        if (skill == null || !skill.isActive)
            return;

        
    }

    private void ApplyIceEffect(Item weapon, Transform target)
    {
        //점점 이속저하되다가 일정 카운트가 지나면 동결
    }

    private void ApplyElectircEffect(Item weapon, Transform target)
    {
        //감전
    }

    //화상
    private IEnumerator ApplyBurnt(Transform target)
    {
        bool isBurnt = true;

        while(isBurnt)
        {
            //지속 뎀지

            yield return new WaitForSeconds(10f);

            isBurnt = false;
        }
    }

    private void PlayParticle(Item weapon, Transform target)
    {
        GameObject effect = weapon.itemSkill.attackEffect;

        if (effect != null)
        {
            var itemEffectGo = Instantiate(effect,
                target.position + particleOffset,
                Quaternion.identity);

            itemEffectGo.transform.SetParent(target);
            Destroy(itemEffectGo, effectDuration);
        }
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 속성 효과의 기본 추상 클래스
/// 모든 속성별 효과(버프, 디버프)의 기본 클래스
/// </summary>
[System.Serializable]
public abstract class ElementalEffect
{
    public ElementalAttribute ElementType { get; protected set; }
    public float Duration { get; protected set; }
    public float CurrentDuration { get; protected set; }
    public CharacterData Target { get; protected set; }
    
    public ElementalEffect(ElementalAttribute elementType, float duration, CharacterData target)
    {
        ElementType = elementType;
        Duration = duration;
        CurrentDuration = duration;
        Target = target;
    }
    
    public virtual void Apply()
    {
        // 하위 클래스에서 구현
    }
    
    public virtual void Remove()
    {
        // 하위 클래스에서 구현
    }
    
    public virtual IEnumerator ProcessEffect()
    {
        // 기본 구현: 지속 시간 대기
        yield return new WaitForSeconds(Duration);
        Remove();
    }
}

/// <summary>
/// 땅 속성 데미지 증가 효과
/// </summary>
[System.Serializable]
public class EarthDamageEffect : ElementalEffect
{
    private float damageIncreasePercent; // 데미지 증가율 (%)
    
    public EarthDamageEffect(float duration, float damageIncreasePercent, CharacterData target) 
        : base(ElementalAttribute.Earth, duration, target)
    {
        this.damageIncreasePercent = damageIncreasePercent;
    }
    
    public override void Apply()
    {
        Debug.Log($"{Target.characterName}에 땅 속성 데미지 증가 효과 적용: {damageIncreasePercent}% 증가, {Duration}초 지속");
        if (ElementalEffectManager.Instance != null)
        {
            ElementalEffectManager.Instance.AddEffect(this);
        }
        else
        {
            Debug.LogWarning("ElementalEffectManager 인스턴스가 없습니다. 효과가 적용되지 않습니다.");
        }
    }
    
    public override void Remove()
    {
        Debug.Log($"{Target.characterName}의 땅 속성 데미지 증가 효과 종료");
        if (ElementalEffectManager.Instance != null)
        {
            ElementalEffectManager.Instance.RemoveEffect(this);
        }
    }
    
    public float GetDamageMultiplier()
    {
        return 1f + (damageIncreasePercent / 100f);
    }
}

/// <summary>
/// 불 속성 화상 효과 (기존 BurnDebuff 대체)
/// </summary>
[System.Serializable]
public class FireBurnEffect : ElementalEffect
{
    private float damagePercent; // 최대 체력 대비 데미지 비율 (%)
    
    public FireBurnEffect(float duration, float damagePercent, CharacterData target) 
        : base(ElementalAttribute.Fire, duration, target)
    {
        this.damagePercent = damagePercent;
    }
    
    public override void Apply()
    {
        Debug.Log($"{Target.characterName}에 화상 효과 적용: 매초 최대 체력의 {damagePercent}% 피해, {Duration}초 지속");
        if (ElementalEffectManager.Instance != null)
        {
            ElementalEffectManager.Instance.AddEffect(this);
        }
        else
        {
            Debug.LogWarning("ElementalEffectManager 인스턴스가 없습니다. 효과가 적용되지 않습니다.");
        }
    }
    
    public override void Remove()
    {
        Debug.Log($"{Target.characterName}의 화상 효과 종료");
        if (ElementalEffectManager.Instance != null)
        {
            ElementalEffectManager.Instance.RemoveEffect(this);
        }
    }
    
    public override IEnumerator ProcessEffect()
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            if (damagePercent > 0)
            {
                // 최대 체력 기준으로 데미지 계산
                int damageAmount = Mathf.RoundToInt(Target.maxHp * (damagePercent / 100f));
                
                // 최소 데미지 설정
                damageAmount = Mathf.Max(1, damageAmount);
                
                Target.TakeDamage(damageAmount);
                
                if (Target.instance != null && Target.instance.gameObject != null)
                {
                    Vector3 targetPosition = Target.instance.transform.position;
                    MessageTag messageTag = Target.characterType == CharacterType.Player ? 
                        MessageTag.플레이어_피해 : MessageTag.적_피해;
                    UIManager.DisplayPopupText(damageAmount.ToString(), targetPosition, messageTag);
                }
                
                // 사망 여부 확인
                if (Target.currentHp <= 0)
                {
                    Transform targetTransform = (Target.instance != null && Target.instance.gameObject != null) 
                        ? Target.instance.transform 
                        : null;
                    CombatManager.Instance.CheckAndHandleDeath(Target, targetTransform, Target.characterType != CharacterType.Player);
                    Remove();
                    yield break;
                }
            }
            
            elapsed += 1f;
            yield return new WaitForSeconds(1f);
        }
        
        Remove();
    }
}

/// <summary>
/// 물 속성 이동속도 감소 효과 (기존 FreezeDebuff 대체)
/// </summary>
[System.Serializable]
public class WaterSlowEffect : ElementalEffect
{
    private float speedReduction; // 이동속도 감소율 (%)
    private float originalSpeed;
    
    public WaterSlowEffect(float duration, float speedReduction, CharacterData target) 
        : base(ElementalAttribute.Water, duration, target)
    {
        this.speedReduction = speedReduction;
    }
    
    public override void Apply()
    {
        originalSpeed = Target.moveSpeed;
        float reductionAmount = originalSpeed * (speedReduction / 100f);
        Target.moveSpeed = Mathf.Max(0, originalSpeed - reductionAmount);
        Target.UpdateSpeed(Target.moveSpeed);
        
        Debug.Log($"{Target.characterName}에 이동속도 감소 효과 적용: {speedReduction}% 감소, {Duration}초 지속");
        if (ElementalEffectManager.Instance != null)
        {
            ElementalEffectManager.Instance.AddEffect(this);
        }
        else
        {
            Debug.LogWarning("ElementalEffectManager 인스턴스가 없습니다. 효과가 적용되지 않습니다.");
        }
    }
    
    public override void Remove()
    {
        Target.moveSpeed = originalSpeed;
        Target.UpdateSpeed(Target.moveSpeed);
        
        Debug.Log($"{Target.characterName}의 이동속도 감소 효과 종료");
        if (ElementalEffectManager.Instance != null)
        {
            ElementalEffectManager.Instance.RemoveEffect(this);
        }
    }
}

/// <summary>
/// 전기 속성 스턴 효과 (기존 ElectrifyDebuff 대체)
/// </summary>
[System.Serializable]
public class ElectricStunEffect : ElementalEffect
{
    public ElectricStunEffect(float duration, CharacterData target) 
        : base(ElementalAttribute.Electric, duration, target)
    {
    }
    
    public override void Apply()
    {
        Target.isStunned = true;
        
        // 만약 타겟이 몬스터이고 인스턴스가 있다면 AI 상태를 스턴 상태로 변경
        if (Target.characterType != CharacterType.Player && Target.instance != null)
        {
            // 일반 몬스터인 경우
            BaseMonsterAI monsterAI = Target.instance.GetComponent<BaseMonsterAI>();
            if (monsterAI != null)
            {
                // 새로운 ApplyStun 메서드 사용
                monsterAI.ApplyStun(Duration);
            }
            
            // 보스인 경우
            BaseBossAI bossAI = Target.instance.GetComponent<BaseBossAI>();
            if (bossAI != null)
            {
                bossAI.ApplyStun(Duration);
            }
        }
        
        Debug.Log($"{Target.characterName}에 스턴 효과 적용: {Duration}초 지속");
        if (ElementalEffectManager.Instance != null)
        {
            ElementalEffectManager.Instance.AddEffect(this);
        }
        else
        {
            Debug.LogWarning("ElementalEffectManager 인스턴스가 없습니다. 효과가 적용되지 않습니다.");
        }
    }
    
    public override void Remove()
    {
        Target.isStunned = false;
        
        // 만약 타겟이 몬스터이고 인스턴스가 있다면 스턴 상태 해제
        if (Target.characterType != CharacterType.Player && Target.instance != null)
        {
            // 일반 몬스터인 경우
            BaseMonsterAI monsterAI = Target.instance.GetComponent<BaseMonsterAI>();
            if (monsterAI != null)
            {
                // 이미 스턴 상태이면 현재 상태 재설정
                if (monsterAI.GetAIState() == BaseMonsterAI.AIState.Stun)
                {
                    // 스턴에서 회복 후 적절한 상태로 전환
                    if (monsterAI.GetPlayerTarget() != null)
                    {
                        monsterAI.ChangeState(BaseMonsterAI.AIState.Chasing);
                    }
                    else
                    {
                        monsterAI.ChangeState(BaseMonsterAI.AIState.Patrolling);
                    }
                }
            }
            
            // 보스인 경우
            BaseBossAI bossAI = Target.instance.GetComponent<BaseBossAI>();
            if (bossAI != null)
            {
                // 보스의 상태 전환은 내부 로직에 맡김
                // 하지만 명시적으로 isStunned 플래그를 false로 설정
                bossAI.ResetStun();
            }
        }
        
        Debug.Log($"{Target.characterName}의 스턴 효과 종료");
        if (ElementalEffectManager.Instance != null)
        {
            ElementalEffectManager.Instance.RemoveEffect(this);
        }
    }
} 
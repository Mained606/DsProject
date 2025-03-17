using System.Collections;
using UnityEngine;

[System.Serializable]
public abstract class Debuff
{
    public float Duration { get; private set; }
    public CharacterData Character { get; private set; }

    public Debuff(float duration, CharacterData character)
    {
        Duration = duration;
        Character = character;
    }

    public abstract IEnumerator ApplyEffect();
}

[System.Serializable]
public class BurnDebuff : Debuff
{
    private float damagePercent; // 최대 체력 대비 데미지 비율 (%)

    public BurnDebuff(float duration, float damagePercent, CharacterData character) 
        : base(duration, character)
    {
        this.damagePercent = damagePercent; // 도트 데미지 수치 (%)
    }

    public override IEnumerator ApplyEffect()
    {
        float elapsed = 0f;
        while (elapsed < Duration)
        {
            if (damagePercent > 0)
            {
                // 최대 체력 기준으로 데미지 계산
                int damageAmount = Mathf.RoundToInt(Character.maxHp * (damagePercent / 100f));
                
                // 최소 데미지 설정 (너무 작은 데미지는 최소 1로 보장)
                damageAmount = Mathf.Max(1, damageAmount);
                
                Character.TakeDamage(damageAmount);
                
                // 기본 null 체크만 유지
                if (Character.instance != null && Character.instance.gameObject != null)
                {
                    Vector3 targetPosition = Character.instance.transform.position;
                    // 플레이어가 화상 데미지를 받을 때는 플레이어_피해 태그 사용, 몬스터가 화상 데미지를 받을 때는 적_피해 태그 사용
                    MessageTag messageTag = Character.characterType == CharacterType.Player ? 
                        MessageTag.플레이어_피해 : MessageTag.적_피해;
                    UIManager.DisplayPopupText(damageAmount.ToString(), targetPosition, messageTag);
                }
                
                // 사망 여부 확인 - 간결하게 유지
                if (Character.currentHp <= 0)
                {
                    Transform targetTransform = (Character.instance != null && Character.instance.gameObject != null) 
                        ? Character.instance.transform 
                        : null;
                    CombatManager.Instance.CheckAndHandleDeath(Character, targetTransform, Character.characterType != CharacterType.Player);
                    DebuffManager.Instance.RemoveDebuff(this);
                    yield break; // 사망 시 코루틴 종료
                }
            }
            else
            {
                Debug.LogWarning("damagePercent 값이 0입니다.");
            }
            elapsed += 1f;
            yield return new WaitForSeconds(1f);
        }
        DebuffManager.Instance.RemoveDebuff(this);
    }
}

[System.Serializable]
public class FreezeDebuff : Debuff
{
    private float speedReduction; // 감소율 %적용 ItemList ItemSkill 인스펙터에서 Value값 70 넣을 시 이동속도 70% 감소  

    public FreezeDebuff(float duration, float speedReduction, CharacterData character) 
        : base(duration, character)
    {
        this.speedReduction = speedReduction;
    }

    public override IEnumerator ApplyEffect()
    {
        float originalSpeed = Character.moveSpeed;
        float reductionAmount = originalSpeed * (speedReduction / 100f); // 퍼센트 적용
        Character.moveSpeed = Mathf.Max(0, originalSpeed - reductionAmount); // 이동 속도가 0 이하로 감소하지 않도록 제한
        Character.UpdateSpeed(Character.moveSpeed); // 이벤트 트리거

        yield return new WaitForSeconds(Duration);

        Character.moveSpeed = originalSpeed;

        Character.UpdateSpeed(Character.moveSpeed); // 이벤트 트리거
        DebuffManager.Instance.RemoveDebuff(this);
    }
}

[System.Serializable]
public class ElectrifyDebuff : Debuff
{
    public ElectrifyDebuff(float duration, CharacterData character) 
        : base(duration, character)
    {
    }

    public override IEnumerator ApplyEffect()
    {
        bool isStunned = true;
        // 스턴 효과 적용
        Character.isStunned = true;

        yield return new WaitForSeconds(Duration);

        // 스턴 해제
        Character.isStunned = false;
        DebuffManager.Instance.RemoveDebuff(this);
    }
}
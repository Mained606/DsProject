// using TreeEditor;
using UnityEngine;
using UnityEngine.VFX;
using System.Collections.Generic;

public class ElectroComet : MonoBehaviour
{
    private Skills skills;
    private VisualEffect vfx;
    [SerializeField] private LayerMask layer;

    private float attackRange;
    [SerializeField] private float attackInterval = 1f;
    private float timer = 0f;
    private Vector3 spawnPosition;

    private bool elementalAttack = false;
    // 이미 스턴이 적용된 적을 추적하기 위한 리스트
    private HashSet<CharacterData> alreadyStunnedTargets = new HashSet<CharacterData>();
    // 연속 히트 상태를 조절하기 위한 히트 간격 (이 값이 작을수록 히트 애니메이션이 더 자주 재생됨)
    [SerializeField] private float hitAnimCooldown = 0.5f;
    private Dictionary<CharacterData, float> hitAnimTimers = new Dictionary<CharacterData, float>();

    private void Start()
    {
        skills = SkillManager.Instance.GetSkill(EntityType.Player, "ElectroComet");
        vfx = GetComponentInChildren<VisualEffect>();
        attackRange = vfx.GetFloat("Radius");
        spawnPosition = transform.position;
        timer = attackInterval/2;
    }

    private void Update()
    {
        // 히트 애니메이션 타이머 업데이트
        UpdateHitAnimTimers();
        Attack();
    }

    // 히트 애니메이션 타이머 업데이트
    private void UpdateHitAnimTimers()
    {
        List<CharacterData> expiredTimers = new List<CharacterData>();
        Dictionary<CharacterData, float> updatedTimers = new Dictionary<CharacterData, float>();
        
        // 모든 타이머를 확인하고 만료된 것과 업데이트가 필요한 것을 분류
        foreach (var entry in hitAnimTimers)
        {
            // 타이머 감소
            float newTime = entry.Value - Time.deltaTime;
            
            if (newTime <= 0)
            {
                expiredTimers.Add(entry.Key);
            }
            else
            {
                // 바로 수정하지 않고 새 값을 별도 딕셔너리에 저장
                updatedTimers[entry.Key] = newTime;
            }
        }
        
        // 만료된 타이머 제거
        foreach (var character in expiredTimers)
        {
            hitAnimTimers.Remove(character);
        }
        
        // 새 타이머 값으로 업데이트
        foreach (var entry in updatedTimers)
        {
            hitAnimTimers[entry.Key] = entry.Value;
        }
    }

    private void Attack()
    {
        if(timer < attackInterval)
        {
            timer += Time.deltaTime;
            return;
        }
        Collider[] hitColliders = Physics.OverlapSphere(spawnPosition, attackRange, layer);
        foreach(Collider hit in hitColliders)
        {
            BaseMonsterData baseMonsterData = hit.transform.GetComponent<BaseMonsterData>();
            if (baseMonsterData != null)
            {
                // monsterOrBossData가 MonsterData일 경우 처리
                MonsterData enemyMonsterData = baseMonsterData.monsterOrBossData as MonsterData;
                if (enemyMonsterData != null)
                {
                    // 캐릭터 데이터와 해당 타겟의 트랜스폼 전달
                    ProcessMonsterHit(enemyMonsterData, hit.transform);
                }

                // monsterOrBossData가 BossData일 경우 처리
                BossData enemyBossData = baseMonsterData.monsterOrBossData as BossData;
                if (enemyBossData != null)
                {
                    // 캐릭터 데이터와 해당 타겟의 트랜스폼 전달
                    ProcessMonsterHit(enemyBossData, hit.transform);
                }
            }
        }
        
        // 첫 타격 이후 모든 대상에 대해 전기 속성 효과 비활성화
        if (!elementalAttack)
        {
            elementalAttack = true;
        }
        
        timer = 0f;
    }

    // 몬스터/보스 타격 처리 통합 메서드
    private void ProcessMonsterHit(CharacterData targetData, Transform targetTransform)
    {
        // 이미 스턴이 적용되었는지 확인
        bool alreadyStunned = alreadyStunnedTargets.Contains(targetData);
        
        // 히트 애니메이션 쿨다운 확인
        bool canPlayHitAnim = !hitAnimTimers.ContainsKey(targetData);
        
        // 첫 타격이고 이미 스턴된 적이 아닌 경우에만 스턴 효과 적용
        if (!elementalAttack && !alreadyStunned)
        {
            // 스턴 효과와 함께 공격 처리
            CombatManager.Instance.ProcessAttack(
                CharacterManager.PlayerCharacterData, 
                targetData, 
                targetTransform, 
                true, 
                true, // 데미지 적용
                skills, 
                false, 
                skills.attribute, 
                skills.debuffDuration, 
                skills.debuffValue
            );
            
            // 스턴 적용된 대상 추적
            alreadyStunnedTargets.Add(targetData);
            
            // 히트 애니메이션 타이머 설정
            hitAnimTimers[targetData] = hitAnimCooldown;
        }
        else
        {
            // 이미 스턴된 적이거나 첫 타격이 아닌 경우
            // 히트 애니메이션 쿨다운에 따라 애니메이션 재생 여부 결정
            bool playHitAnim = canPlayHitAnim;
            
            // 스턴 없이 데미지만 적용
            CombatManager.Instance.ProcessAttack(
                CharacterManager.PlayerCharacterData, 
                targetData, 
                targetTransform, 
                true, // 항상 플레이어의 공격임을 표시
                true, // 데미지 적용
                skills, 
                false, 
                ElementalAttribute.None, 
                0f
            );
            
            // 히트 애니메이션 제어 - BaseMonsterAI 컴포넌트를 찾아서 직접 제어
            if (playHitAnim)
            {
                // 타겟의 Animator 컴포넌트를 찾아 Hit 트리거를 직접 설정
                Animator animator = targetTransform.GetComponent<Animator>();
                if (animator != null)
                {
                    // Hit 트리거를 초기화하고 다시 설정하여 애니메이션 재생
                    animator.ResetTrigger("Hit");
                    animator.SetTrigger("Hit");
                }
                
                hitAnimTimers[targetData] = hitAnimCooldown;
            }
        }
    }

    private void OnDestroy()
    {
        // 객체가 파괴될 때 추적 리스트와 타이머 정리
        alreadyStunnedTargets.Clear();
        hitAnimTimers.Clear();
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class MophanAI : BaseBossAI
{
    [Header("대쉬 설정")]
    public float dashSpeed = 30f;     // 대쉬 시 이동 속도
    public float dashDistance = 30f;  // 대쉬 시 이동할 거리
    private bool hasAppliedDashDamage = false;

    [Header("점프 설정")]
    public float jumpSpeed = 20f;     // 점프 시 수평 이동 속도
    public float jumpHeight = 12f;    // 점프 최고 높이
    public float jumpDistance = 20f;  // 점프 시 이동할 거리

    [Header("텔레포트 설정")]
    public float teleportInterval = 80f;      // 텔레포트 간격 (미사용)
    public float teleportRange = 10f;         // 텔레포트 범위

    [SerializeField] private GameObject firePoint1;  // AoE 스킬 시 사용할 위치 (예시)

    // 기본 보스 AI의 ExecuteBossAttack을 오버라이드하여 특화된 공격 패턴을 구현
    protected override IEnumerator ExecuteBossAttack()
    {
        if (isAttacking) yield break;
        isAttacking = true;

        List<string> bossSkillNames = SkillManager.Instance.GetAvailableSkills(EntityType.Boss);
        if (bossSkillNames.Count == 0)
        {
            Debug.LogWarning("보스의 사용 가능한 스킬이 없습니다.");
            SetState(BossState.Idle);
            isAttacking = false;
            yield break;
        }

        Skills selectedSkill = null;
        int attempt = 0;
        int maxAttempts = 10;
        while (selectedSkill == null && attempt < maxAttempts)
        {
            string randomSkillName = bossSkillNames[Random.Range(0, bossSkillNames.Count)];
            Skills skill = SkillManager.Instance.GetSkill(EntityType.Boss, randomSkillName);

            if (skill != null && !skill.cooldownTimer.IsRunning)
            {
                selectedSkill = skill;
            }
            attempt++;
        }

        if (selectedSkill == null)
        {
            Debug.LogWarning("사용 가능한 스킬을 찾지 못했습니다.");
            SetState(BossState.Idle);
            isAttacking = false;
            yield break;
        }

        switch (selectedSkill.skillName)
        {
            case "OrbExplosion":
                animator.SetTrigger(IsRoaring);
                yield return new WaitForSeconds(roarDuration);
                isRotating = false; // 회전 방지
                SkillManager.Instance.ActivateSkillForEntity(EntityType.Boss, selectedSkill.skillName, playerTarget.gameObject, this.transform);
                yield return new WaitForSeconds(4f);
                isRotating = true;  // 회전 가능
                break;
            case "RapidFireball":
                animator.SetTrigger(IsRoaring);
                yield return new WaitForSeconds(roarDuration);
                isRotating = false; // 회전 방지
                SkillManager.Instance.ActivateSkillForEntity(EntityType.Boss, selectedSkill.skillName, firePoint1, this.transform);
                yield return new WaitForSeconds(4f);
                isRotating = true;  // 회전 가능
                break;
            case "Dash":
                animator.SetTrigger(IsRoaring);
                yield return new WaitForSeconds(roarDuration);
                // ROARING 후 플레이어를 향해 회전
                yield return StartCoroutine(RotateTowardsPlayerSmoothly());
                // 대쉬 실행
                SkillManager.Instance.ActivateSkillForEntity(EntityType.Boss, selectedSkill.skillName, gameObject);
                isPerformingSpecialMove = true;
                yield return StartCoroutine(PerformDash());
                isPerformingSpecialMove = false;
                break;
            case "Jump":
                animator.SetTrigger(IsRoaring);
                yield return new WaitForSeconds(roarDuration);
                // ROARING 후 플레이어를 향해 회전
                yield return StartCoroutine(RotateTowardsPlayerSmoothly());
                isPerformingSpecialMove = true;
                yield return StartCoroutine(PerformJump(selectedSkill));
                isPerformingSpecialMove = false;
                break;
            default:
                Debug.LogWarning("처리되지 않은 스킬: " + selectedSkill.skillName);
                break;
        }

        float skillDuration = selectedSkill.GetSkillDuration();
        yield return new WaitForSeconds(skillDuration + attackCooldown);

        if (playerTarget != null)
        {
            if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange)
            {
                SetState(BossState.Attacking);
            }
            else
            {
                SetState(BossState.Chasing);
            }
        }
        else
        {
            SetState(BossState.Idle);
        }

        isAttacking = false;
    }

    // 대쉬 구현 로직
    private IEnumerator PerformDash()
    {
        Vector3 startPosition = transform.position;
        Vector3 dashDirection = transform.forward; // 기존 대시 방향
        float distanceTravelled = 0f;
        animator.SetBool(IsDashing, true);
        isRotating = false; // 회전 방지
        hasAppliedDashDamage = false;
        Skills skills = SkillManager.SkillDatabase.bossSkills[2];

        while (distanceTravelled < dashDistance)
        {
            float step = dashSpeed * Time.deltaTime;
            characterController.Move(dashDirection * step);
            distanceTravelled = Vector3.Distance(startPosition, transform.position);

            // 플레이어와 충돌했을 경우 밀어내기 (수평 방향으로 밀어내기)
            if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) < 3f) // 플레이어와의 거리 확인
            {
                // 플레이어와의 상대적인 수평 방향 계산 (y축을 무시)
                Vector3 directionToPlayer = (playerTarget.position - transform.position).normalized;
                directionToPlayer.y = 0; // y축을 0으로 설정해 수평 방향으로만 밀어내기

                // 플레이어를 밀어낼 벡터는 대시 방향과 직각인 벡터로 설정 (벡터의 외적 사용)
                Vector3 pushDirection = Vector3.Cross(directionToPlayer, Vector3.up).normalized;

                float pushForce = 50f; // 밀어내는 힘 설정
                playerTarget.GetComponent<CharacterController>().Move(pushDirection * pushForce * Time.deltaTime);
                
                // 한 번만 데미지 들어가도록 플래그 사용
                if (!hasAppliedDashDamage)
                {
                    CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, this.bossData, playerTarget, false, false, skills, true, skills.attribute, skills.debuffDuration, skills.debuffValue);
                    hasAppliedDashDamage = true;
                }
            }

            yield return null;
        }
        animator.SetBool(IsDashing, false);
        isRotating = true; // 회전 재개
    }

    // 점프 구현 로직
    private IEnumerator PerformJump(Skills selectedSkill)
    {
        Vector3 startPosition = transform.position;
        Vector3 targetPosition = playerTarget.position;

        // 플레이어와의 거리가 너무 가까운 경우 오프셋을 적용하지 않도록 함
        Vector3 offset;
        do
        {
            offset = new Vector3(Random.Range(-3f, 3f), 0f, Random.Range(-3f, 3f)); // x, z축으로 랜덤 오프셋
            targetPosition = playerTarget.position + offset;
        }
        while (Vector3.Distance(targetPosition, playerTarget.position) < 1.5f); // 플레이어와의 거리가 1.5 이상일 때만 오프셋 적용

        float jumpDuration = jumpDistance / jumpSpeed;
        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / jumpDuration;

            Vector3 horizontalPosition = Vector3.Lerp(startPosition, targetPosition, t);
            float verticalOffset = 4 * jumpHeight * t * (1 - t);
            Vector3 newPosition = horizontalPosition;
            newPosition.y = startPosition.y + verticalOffset;

            // 보스 점프 이동
            characterController.Move(newPosition - transform.position);
            yield return null;
        }
        
        SkillManager.Instance.ActivateSkillForEntity(EntityType.Boss, selectedSkill.skillName, gameObject, this.transform);
    }
    
    // Mophan 전용 랜덤 위치 생성 메서드
    public Vector3 GetRandomSkillPosition()
    {
        return GetRandomPosition(teleportRange);
    }
}

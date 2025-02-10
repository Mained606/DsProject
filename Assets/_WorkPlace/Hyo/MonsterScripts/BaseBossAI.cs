using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BaseBossAI : MonoBehaviour
{
    public static readonly int IsRoaring = Animator.StringToHash("IsRoaring");
    public static readonly int IsDashing = Animator.StringToHash("IsDashing");
    public static readonly int IsJumping = Animator.StringToHash("IsJumping");
    public static readonly int Hit = Animator.StringToHash("Hit");
    public static readonly int IsDead = Animator.StringToHash("IsDead");
    public static readonly int IsChasing = Animator.StringToHash("IsChasing");


    public enum BossState { Idle, Roaring, Chasing, Attacking, Returning, Hit, Stun, Dead }

    [Header("보스 AI 설정")]
    public float searchRange = 30f;           // 보스가 플레이어를 감지하는 범위
    public float maxDistance = 50f;           // 보스 스폰 위치로부터 플레이어가 벗어나면 복귀 처리할 거리
    public float roarDuration = 5f;           // 로어링(울부짖기) 애니메이션 지속 시간
    public float teleportInterval = 80f;      // 텔레포트 간격 (미사용)
    public float teleportRange = 10f;         // 텔레포트 범위
    public float hitDuration = 1f;            // 피격 상태 지속 시간
    public float movementSpeed;               // 이동 속도 (BossData에서 할당)
    public float attackRange;                 // 공격 가능 거리 (또는 체이싱→어택 전환 기준)
    public float turnSpeed = 10f;
    
    [Header("스킬 설정")]
    public float attackCooldown = 10f; // 공격 간격
    private float attackCooldownTimer;
        
    [Header("대쉬 설정")]
    public float dashSpeed = 30f;     // 대쉬 시 이동 속도
    public float dashDistance = 30f;  // 대쉬 시 이동할 거리

    [Header("점프 설정")]
    public float jumpSpeed = 20f;     // 점프 시 수평 이동 속도
    public float jumpHeight = 12f;     // 점프 최고 높이
    public float jumpDistance = 20f;  // 점프 시 이동할 거리

    [SerializeField] private BossState currentState = BossState.Idle;
    [SerializeField] private Transform playerTarget;
    private Vector3 spawnPosition;

    private Animator animator;
    private CharacterController characterController;
    [SerializeField] private BossData bossData;
    
    private Vector3 velocity;
    private float gravity = -9.81f;
    
    private bool isAttacking = false;
    private bool isStunned = false;
    private bool isPerformingSpecialMove = false;
    private bool isRotating = true; // 회전 방지 플래그 추가
    
    [SerializeField] private GameObject firePoint1;  // AoE 스킬 시 사용할 위치 (예시)
    [SerializeField] private bool respawn = false;
    private bool hasAppliedDashDamage = false;
    private float arrivedDistance = 1f;

    protected virtual void OnDestroy()
    {
        if (bossData != null) bossData.OnTakeDamage -= HandleTakeDamage;
    }
    
    protected virtual void OnEnable()
    {
        // 풀링 적용 후 리스폰 상태일 때 초기화
        if (respawn)
        {
            // 상태 초기화
            animator.SetBool(IsDashing, false);
            animator.SetBool(IsChasing, false);
            animator.ResetTrigger(IsDead);
            animator.ResetTrigger(IsJumping);
            animator.ResetTrigger(IsRoaring);
            animator.ResetTrigger(Hit);
            playerTarget = null;
            isAttacking = false;
            isPerformingSpecialMove = false;
            isRotating = true;
            currentState = BossState.Idle;
            
            // 풀링된 몬스터를 다시 캐릭터 리스트에 추가
            CharacterManager.Instance.AddCharacter(this.bossData);
        }
    }

    private void Start()
    {
        spawnPosition = transform.position;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        bossData = GetComponent<BaseMonsterData>().GetBossData();

        if (bossData != null)
        {
            bossData.OnTakeDamage += HandleTakeDamage;
            movementSpeed = bossData.moveSpeed;
            attackCooldown = bossData.attackSpeed;
            attackRange = bossData.attackRange;
        }
    }

    private void Update()
    {
        if (currentState == BossState.Dead/* || currentState == BossState.Returning*/) return;
        
        HandleGravity();

        attackCooldownTimer -= Time.deltaTime;
        
        // 만약 플레이어가 보스 스폰 위치로부터 maxDistance 이상 멀어졌다면
        if (playerTarget != null && Vector3.Distance(spawnPosition, playerTarget.position) > maxDistance)
        {
            // 게임 상태를 탐험(Exploration)으로 전환하고, 보스는 복귀(Returning) 상태로 변경
            GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
            SetState(BossState.Returning);
        }

        // 플레이어가 있을 경우 회전 처리 (체이싱 및 어택 시)
        if (currentState == BossState.Chasing || currentState == BossState.Attacking)
        {
            RotateTowardsPlayer();
        }

        // 상태별 로직 처리
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleLogic();
                break;
            case BossState.Roaring:
                // 로어링은 코루틴에서 처리됨.
                break;
            case BossState.Chasing:
                HandleChasingLogic();
                break;
            case BossState.Attacking:
                HandleCombatLogic();
                break;
            case BossState.Returning:
                HandleReturningLogic();
                break;
            case BossState.Hit:
                // Hit 상태는 코루틴으로 처리
                break;
            case BossState.Stun:
                // 스턴 상태 (필요 시 추가 구현)
                break;
        }

        /////////////////////////////////////////////////////////////////////////////////////////////
        /// 2025.02.08 JWS 수정
        /////////////////////////////////////////////////////////////////////////////////////////////
        if (currentState == BossState.Returning || currentState == BossState.Dead || currentState == BossState.Idle)
            UIManager.Instance.BossHudDisplay(false);
        else UIManager.Instance.BossHudDisplay(true, bossData);
        /////////////////////////////////////////////////////////////////////////////////////////////

        // 서치: 플레이어가 아직 탐지되지 않았다면 검색
        if (currentState != BossState.Returning)
        {
            SearchForPlayer();
        }
    }
    
    private void HandleIdleLogic()
    {
        // 플레이어가 이미 감지되었다면 배틀 시작 (StartBossBattle 내부에서 상태 전환 처리)
        if (playerTarget != null)
        {
            StartBossBattle();
        }
    }
    
    private void HandleChasingLogic()
    {
        if (!playerTarget || Vector3.Distance(transform.position, spawnPosition) > maxDistance)
        {
            playerTarget = null; // 플레이어를 잃거나 최대 이동 범위를 벗어난 경우
            SetState(BossState.Returning); // 복귀 상태로 전환
            return;
        }

        // 플레이어 방향 계산
        Vector3 direction = (playerTarget.position - transform.position);
        direction.y = 0;
        if (direction.magnitude < 0.3f)
            return;
        direction.Normalize();
        
        animator.SetBool(IsChasing, true);
        // 부드러운 회전 후 이동
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        characterController.Move(direction * movementSpeed * Time.deltaTime);

        // 플레이어와의 거리가 attackRange 이내면 어택 상태로 전환
        if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange)
        {
            animator.SetBool(IsChasing, false);
            SetState(BossState.Attacking);
        }
    }
    
    private void HandleCombatLogic()
    {
        if (playerTarget == null)
        {
            SetState(BossState.Idle);
            return;
        }

        // 만약 플레이어와의 거리가 attackRange를 벗어나면 체이싱 상태로 전환
        if (Vector3.Distance(transform.position, playerTarget.position) > attackRange)
        {
            SetState(BossState.Chasing);
            return;
        }

        if (!isAttacking)
        {
            StartCoroutine(ExecuteBossAttack());
        }
    }
    
    private void HandleReturningLogic()
    {
        animator.SetBool(IsDashing, false);
        animator.SetBool(IsChasing, false);
        //animator.ResetTrigger(IsDead);
        animator.ResetTrigger(IsJumping);
        animator.ResetTrigger(IsRoaring);
        animator.ResetTrigger(Hit);
        playerTarget = null;
        isAttacking = false;
        isPerformingSpecialMove = false;
        isRotating = true;
        SearchForPlayer();
        MoveTowards();
    }
    
    // 목표 지점으로 이동
    protected virtual void MoveTowards()
    {
        if (isStunned || currentState == BossState.Roaring || currentState == BossState.Hit || currentState == BossState.Stun ||
            currentState == BossState.Dead || Vector3.Distance(transform.position, spawnPosition) <= arrivedDistance)
        {
            animator.SetBool(IsChasing, false);
            SetState(BossState.Idle); // 스폰 위치에 도달하면 패트롤 상태로 전환
            return;
        }

        // 이동 방향 계산
        Vector3 direction = (spawnPosition - transform.position).normalized;
        direction.y = 0; // 수직 방향을 0으로 설정하여 수평 이동만 처리
        
        isRotating = true;

        // 이동 처리
        Vector3 movement = direction * movementSpeed * Time.deltaTime;

        // CharacterController의 Move 메서드를 사용
        Debug.Log("보스 이동중");

        characterController.Move(movement);

        // 부드러운 회전 처리
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

        // 애니메이션 처리
        if (!animator.GetBool(IsChasing)) animator.SetBool(IsChasing, true);
    }

    private void SearchForPlayer()
    {
        if (playerTarget != null)
            return;

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRange);
        foreach (var hit in hits)
        {
            // 자기 자신의 Collider(또는 자식 포함)를 무시
            if(hit.transform == transform || hit.transform.IsChildOf(transform))
                continue;

            if (hit.CompareTag("Player"))
            {
                playerTarget = hit.transform;
                StartBossBattle();
                return;
            }
        }
    }
    
    private void RotateTowardsPlayer()
    {
        if (playerTarget == null || !isRotating) // 회전 중지
            return;

        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5f);
        }
    }

    private void StartBossBattle()
    {
        if (GameStateMachine.Instance.CurrentState != GameSystemState.BossBattle)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.BossBattle);
        }
        Debug.Log("스타트 배틀");
        SetState(BossState.Roaring);
    }

    private void SetState(BossState newState)
    {
        // 죽거나 스턴 상태이면 상태 전환 불가
        if (currentState == BossState.Dead || isStunned)
            return;

        // 공격 중일 때 일반 상태 전환은 막음 (단, 스턴/피격/죽음은 예외)
        if (isAttacking && newState != BossState.Stun && newState != BossState.Hit && newState != BossState.Dead)
            return;

        if (currentState == newState)
            return;

        currentState = newState;

        switch (newState)
        {
            case BossState.Roaring:
                StartCoroutine(RoaringSequence());
                break;
            case BossState.Hit:
                StartCoroutine(HandleHitState());
                break;
            case BossState.Dead:
                HandleDeath();
                break;
        }
    }

    private IEnumerator RoaringSequence()
    {
        Debug.Log("로어링 시작");
        animator.SetTrigger(IsRoaring);
        yield return new WaitForSeconds(roarDuration);
        SetState(BossState.Chasing);
    }
    
    private IEnumerator HandleHitState()
    {
        animator.SetTrigger(Hit);
        yield return new WaitForSeconds(hitDuration);
        SetState(BossState.Attacking);
    }
    private IEnumerator ExecuteBossAttack()
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
    private IEnumerator RotateTowardsPlayerSmoothly()
    {
        Vector3 targetDirection = (playerTarget.position - transform.position).normalized;
        float timeToRotate = 1f; // 회전 시간 설정
        float elapsedTime = 0f;

        while (elapsedTime < timeToRotate)
        {
            elapsedTime += Time.deltaTime;
            float smoothStep = Mathf.SmoothStep(0f, 1f, elapsedTime / timeToRotate);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(targetDirection), smoothStep);
            yield return null;
        }
    }

    private IEnumerator PerformDash()
    {
        Vector3 startPosition = transform.position;
        Vector3 dashDirection = transform.forward; // 기존 대시 방향
        float distanceTravelled = 0f;
        animator.SetBool(IsDashing, true);
        isRotating = false; // 회전 방지
        hasAppliedDashDamage = false;


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
                    CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, this.bossData, playerTarget, false, false, 3f, true);
                    hasAppliedDashDamage = true;
                }
            }

            yield return null;
        }
        animator.SetBool(IsDashing, false);
        isRotating = true; // 회전 방지
    }

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
        while (Vector3.Distance(targetPosition, playerTarget.position) < 1.5f); // 플레이어와의 거리가 1 이상일 때만 오프셋 적용

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
    
    protected virtual void HandleGravity()
    {
        if (isPerformingSpecialMove) return;
    
        if (characterController.isGrounded)
        {
            velocity.y = 0;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;  // 중력 적용
        }
    
        characterController.Move(velocity * Time.deltaTime);
    }
    
    protected virtual void HandleTakeDamage(Transform attacker)
    {
        // 플레이어가 공격하면 플레이어 타겟 지정
        if (attacker.CompareTag("Player"))
        {
            playerTarget = attacker;
        }

        // 이부분은 불필요. 플레이어를 강제로 재설정하면 데미지가 이상해짐.
        //else
        //{
        //    // 만약 attacker가 플레이어가 아니라면, 올바른 플레이어 참조(예: GameManager.playerTransform)로 설정
        //    playerTarget = GameManager.playerTransform;
        //}

        // 보스 배틀 상태가 아니라면 전환
        if (GameStateMachine.Instance.CurrentState != GameSystemState.BossBattle)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.BossBattle);
        }
        SetState(BossState.Roaring);
    }
    
    private void HandleDeath()
    {
        animator.SetTrigger(IsDead);
        Debug.Log("보스가 사망했습니다.");
    }
    
    public void SetDeadState(bool pooling)
    {
        SetState(BossState.Dead);
        StartCoroutine(OnDeathAnimationEnd(pooling));
    }

    private IEnumerator OnDeathAnimationEnd(bool pooling)
    {
        characterController.Move(Vector3.zero); // 이동 정지

        yield return new WaitForSeconds(1.5f);
        if (pooling)
        {
            respawn = true;
            gameObject.SetActive(false);
            bossData.ResetDataByLevel();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    
    protected void RestartHitAnimation()
    {
        animator.ResetTrigger(Hit);
        animator.SetTrigger(Hit);
    }
    
    // 랜덤 위치를 보스 주변에서 생성하는 함수
    private Vector3 GetRandomSkillPosition()
    {
        float randomX = Random.Range(-teleportRange, teleportRange);
        float randomZ = Random.Range(-teleportRange, teleportRange);
        Vector3 randomPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        return randomPosition;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnPosition, 3f);
        
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(this.transform.position, searchRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.spawnPosition, maxDistance);
    }
}

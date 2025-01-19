using System.Collections;
using UnityEngine;

public class MushroomAi : MonoBehaviour
{
    public enum AIState { Idle, Patrolling, Chasing, Attacking, Returning }

    [Header("움직임 세팅")]
    public float patrolRange = 4f; // 패트롤 범위
    public float idleTime = 2f; // 대기 시간
    public float movementSpeed = 2f; // 이동 속도
    public float maxPatrolDistance = 10f; // 최대 패트롤 범위 (스폰 위치로부터 최대 거리)

    [Header("서치 & 공격 세팅")]
    public float searchRange = 7f; // 플레이어 검색 범위
    public float attackRange = 1.5f; // 공격 범위
    public float attackCooldown = 2f; // 공격 쿨타임

    [SerializeField] private AIState currentState = AIState.Idle; // 현재 상태
    [SerializeField] private Vector3 spawnPosition; // 초기 위치
    private Vector3 targetPosition; // 목표 위치 (패트롤)
    private Transform playerTarget; // 타겟 플레이어
    private float attackCooldownTimer = 0f; // 공격 쿨타임 타이머

    private Animator animator; // 애니메이터
    private Collider col; // 콜라이더
    private Rigidbody rb; // Rigidbody (물리 설정)
    private MonsterData monsterData; // 몬스터 데이터

    private void Start()
    {
        // 초기화
        spawnPosition = transform.position; // 초기 위치 저장
        animator = GetComponent<Animator>(); // 애니메이터 캐시
        col = GetComponent<Collider>(); // 콜라이더 캐시
        rb = GetComponent<Rigidbody>(); // Rigidbody 캐시
        monsterData = GetComponent<Test1>().monster; // 몬스터 데이터 캐시

        SetNewPatrolTarget(); // 새로운 패트롤 목표 설정
        SetState(AIState.Patrolling); // 처음에는 패트롤 상태로 시작
    }

    private void Update()
    {
        attackCooldownTimer -= Time.deltaTime; // 공격 쿨타임 타이머 감소
        
        SearchForPlayer();
        
        // 상태에 따라 처리
        switch (currentState)
        {
            case AIState.Idle:
                HandleIdleState();
                break;
            case AIState.Patrolling:
                HandlePatrollingState();
                break;
            case AIState.Chasing:
                HandleChasingState();
                break;
            case AIState.Attacking:
                HandleAttackingState();
                break;
            case AIState.Returning:
                HandleReturningState();
                break;
        }
    }

    private void HandleIdleState()
    {
        // 플레이어가 발견되면 추적 상태로 전환
        if (playerTarget != null)
        {
            SetState(AIState.Chasing);
            return;
        }

        // 대기 상태에서는 걷지 않음
        animator.SetBool("isWalking", false);
        StartCoroutine(SwitchStateAfterDelay(AIState.Patrolling, idleTime)); // 일정 시간 후 패트롤 상태로 전환
    }

    private void HandlePatrollingState()
    {
        // 플레이어를 발견하면 추적 상태로 전환
        if (playerTarget != null)
        {
            SetState(AIState.Chasing);
            return;
        }

        MoveTowards(targetPosition); // 목표 위치로 이동

        // 목표 위치에 도달하면 대기 상태로 전환
        if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            SetState(AIState.Idle);
        }
    }

    private void HandleChasingState()
    {
        // 플레이어를 최초로 발견한 경우, maxPatrolDistance 내에서는 계속 추적
        if (playerTarget == null || Vector3.Distance(transform.position, spawnPosition) > maxPatrolDistance)
        {
            // 최대 패트롤 범위를 벗어나면, 타겟을 잃고 제자리로 돌아가는 상태로 전환
            playerTarget = null;
            SetState(AIState.Returning); // 제자리로 돌아가는 상태
            return;
        }

        // 플레이어가 공격 범위에 들어오면 공격 상태로 전환
        if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange)
        {
            SetState(AIState.Attacking);
        }
        else
        {
            MoveTowards(playerTarget.position); // 플레이어 쪽으로 이동
        }
    }

    private void HandleAttackingState()
    {
        // 플레이어가 없거나 공격 범위를 벗어나면 돌아가기
        if (playerTarget == null || Vector3.Distance(transform.position, playerTarget.position) > searchRange)
        {
            playerTarget = null;
            SetState(AIState.Patrolling);
            return;
        }

        // 공격 범위를 벗어나면 추적 상태로 전환
        if (Vector3.Distance(transform.position, playerTarget.position) > attackRange)
        {
            SetState(AIState.Chasing);
            return;
        }

        // 공격 쿨타임이 지나면 공격 실행
        if (attackCooldownTimer <= 0f)
        {
            PerformAttack();
            attackCooldownTimer = attackCooldown; // 쿨타임 리셋
        }
    }

    private void HandleReturningState()
    {
        // 스폰 위치로 돌아가며 이동
        MoveTowards(spawnPosition);
        Debug.Log(spawnPosition + "리턴중");

        // 스폰 위치에 도달하면 패트롤 상태로 전환
        if (Vector3.Distance(transform.position, spawnPosition) <= 0.1f)
        {
            SetState(AIState.Patrolling); // 다시 패트롤로 돌아감
        }
    }

    private void SearchForPlayer()
    {
        // 이미 플레이어를 추적 중이면 다시 찾지 않음
        if (playerTarget != null) return;

        // 주변에 있는 플레이어를 찾음
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerTarget = hit.transform; // 플레이어 타겟 설정
                return;
            }
        }
    }

    private void MoveTowards(Vector3 destination)
    {
        // 목표 방향으로 이동
        Vector3 direction = (destination - transform.position).normalized;
        
        transform.position += direction * movementSpeed * Time.deltaTime;
        transform.LookAt(new Vector3(destination.x, transform.position.y, destination.z)); // Y축을 기준으로 회전

        animator.SetBool("isWalking", true); // 걷는 애니메이션 실행
    }

    private void PerformAttack()
    {
        // 공격 상태로 전환하고 애니메이션 실행
        SetAttackingState();
        animator.SetTrigger("Attack");
        CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, monsterData, transform, false);
    }

    private void SetNewPatrolTarget()
    {
        // 패트롤을 위한 새로운 목표 위치 설정
        targetPosition = spawnPosition + new Vector3(
            Random.Range(-patrolRange, patrolRange),
            0,
            Random.Range(-patrolRange, patrolRange)
        );
    }

    private void SetState(AIState newState)
    {
        // 상태가 이미 동일하면 변경하지 않음
        if (currentState == newState) return;

        currentState = newState;

        // 상태별 초기화
        switch (currentState)
        {
            case AIState.Patrolling:
                SetNewPatrolTarget();
                break;
            case AIState.Idle:
                break;
            case AIState.Returning:
                break;
            case AIState.Chasing:
                break;
            case AIState.Attacking:
                break;
        }
    }

    private IEnumerator SwitchStateAfterDelay(AIState newState, float delay)
    {
        // 일정 시간 후 상태 전환
        yield return new WaitForSeconds(delay);
        SetState(newState);
    }
    
    private void SetAttackingState()
    {
        // 공격 상태에서는 물리 엔진 비활성화
        SetCollisionAndPhysics(false);
        StartCoroutine(ReEnablePhysics()); // 공격 후 물리 엔진 재활성화
    }

    private void SetCollisionAndPhysics(bool enablePhysics)
    {
        // 물리 충돌 및 물리 설정 변경
        if (col != null)
        {
            col.isTrigger = !enablePhysics; // 충돌 여부 설정
        }

        if (rb != null)
        {
            rb.isKinematic = !enablePhysics; // Kinematic 설정
            rb.collisionDetectionMode = enablePhysics ? CollisionDetectionMode.ContinuousDynamic : CollisionDetectionMode.Discrete;
        }
    }

    private IEnumerator ReEnablePhysics()
    {
        // 일정 시간 후 물리 계산 재활성화
        yield return new WaitForSeconds(attackCooldown);
        SetCollisionAndPhysics(true);
    }

    private void OnDrawGizmosSelected()
    {
        // Gizmo를 통해 범위 시각화 (디버깅용)
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRange);
        
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(spawnPosition, maxPatrolDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

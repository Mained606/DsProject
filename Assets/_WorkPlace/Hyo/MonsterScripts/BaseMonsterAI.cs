using System.Collections;
using UnityEngine;

public class BaseMonsterAI : MonoBehaviour
{
    public enum AIState { Idle, Patrolling, Chasing, Attacking, Returning, Dead }

    [Header("움직임 세팅")]
    public float patrolRange = 4f; // 패트롤 범위 (몬스터가 움직일 수 있는 반경)
    public float idleTime = 2f; // 대기 상태에서 머무는 시간
    public float movementSpeed = 2f; // 이동 속도
    public float turnSpeed = 10f;
    public float maxPatrolDistance = 10f; // 스폰 위치에서 최대 이동 가능 거리

    [Header("서치 & 공격 세팅")]
    public float searchRange = 7f; // 플레이어 탐지 범위
    public float attackRange = 1.5f; // 공격 범위
    public float attackCooldown = 2f; // 공격 쿨타임

    [SerializeField] protected AIState currentState = AIState.Idle; // 현재 AI 상태
    [SerializeField] protected Vector3 spawnPosition; // 스폰 위치
    protected Vector3 targetPosition; // 패트롤 목적지
    protected Transform playerTarget; // 탐지된 플레이어
    protected float attackCooldownTimer = 0f; // 공격 쿨타임 타이머

    protected Animator animator; // 애니메이터 캐시
    protected Collider col; // 몬스터의 충돌체 캐시
    protected Rigidbody rb; // 몬스터의 물리 캐시
    protected MonsterData monsterData; // 몬스터 데이터

    protected virtual void Start()
    {
        spawnPosition = transform.position; // 스폰 위치 저장
        animator = GetComponent<Animator>();
        col = GetComponent<Collider>();
        rb = GetComponent<Rigidbody>();
        monsterData = GetComponent<Test1>().monster; // MonsterData 참조 (Test1 컴포넌트에서 캐싱)
        if (monsterData != null) monsterData.OnTakeDamage += HandleTakeDamage;

        SetNewPatrolTarget(); // 새로운 패트롤 목표 설정
        SetState(AIState.Patrolling); // 초기 상태를 패트롤로 설정
    }
    
    protected virtual void OnDestroy()
    {
        if (monsterData != null) monsterData.OnTakeDamage -= HandleTakeDamage;
    }
    
    protected virtual void HandleTakeDamage()
    {
        // 피격 애니메이션 실행
        animator.SetTrigger("Hit");
    }

    protected virtual void Update()
    {
        if (currentState == AIState.Dead) return; // 사망 상태에서는 동작하지 않음

        attackCooldownTimer -= Time.deltaTime; // 공격 쿨타임 감소
        SearchForPlayer(); // 플레이어 탐색

        // 현재 상태에 따라 동작 수행
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

    // 대기 상태 처리
    protected virtual void HandleIdleState()
    {
        if (playerTarget != null)
        {
            SetState(AIState.Chasing); // 플레이어가 발견되면 추적 상태로 전환
            return;
        }

        animator.SetBool("isWalking", false); // 대기 상태에서 걷는 애니메이션 비활성화
        StartCoroutine(SwitchStateAfterDelay(AIState.Patrolling, idleTime)); // 일정 시간 후 패트롤 상태로 전환
    }

    // 패트롤 상태 처리
    protected virtual void HandlePatrollingState()
    {
        if (playerTarget != null)
        {
            SetState(AIState.Chasing); // 플레이어가 발견되면 추적 상태로 전환
            return;
        }

        MoveTowards(targetPosition); // 목표 위치로 이동

        if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            SetState(AIState.Idle); // 목표 지점 도착 시 대기 상태로 전환
        }
    }

    // 추적 상태 처리
    protected virtual void HandleChasingState()
    {
        if (playerTarget == null || Vector3.Distance(transform.position, spawnPosition) > maxPatrolDistance)
        {
            playerTarget = null; // 플레이어를 잃거나 최대 이동 범위를 벗어난 경우
            SetState(AIState.Returning); // 복귀 상태로 전환
            return;
        }

        if (Vector3.Distance(transform.position, playerTarget.position) <= attackRange)
        {
            SetState(AIState.Attacking); // 공격 범위에 들어오면 공격 상태로 전환
        }
        else
        {
            MoveTowards(playerTarget.position); // 플레이어 쪽으로 이동
        }
    }

    // 공격 상태 처리
    protected virtual void HandleAttackingState()
    {
        if (playerTarget == null || Vector3.Distance(transform.position, playerTarget.position) > searchRange)
        {
            playerTarget = null;
            SetState(AIState.Patrolling); // 플레이어가 범위를 벗어나면 패트롤 상태로 전환
            return;
        }

        if (Vector3.Distance(transform.position, playerTarget.position) > attackRange)
        {
            SetState(AIState.Chasing); // 공격 범위를 벗어나면 추적 상태로 전환
            return;
        }

        if (attackCooldownTimer <= 0f)
        {
            PerformAttack(); // 공격 실행
            attackCooldownTimer = attackCooldown; // 쿨타임 리셋
        }
    }

    // 복귀 상태 처리
    protected virtual void HandleReturningState()
    {
        MoveTowards(spawnPosition); // 스폰 위치로 돌아감

        if (Vector3.Distance(transform.position, spawnPosition) <= 0.1f)
        {
            SetState(AIState.Patrolling); // 스폰 위치에 도달하면 패트롤 상태로 전환
        }
    }

    // 플레이어 탐색
    protected virtual void SearchForPlayer()
    {
        if (playerTarget != null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerTarget = hit.transform; // 플레이어 발견 시 타겟 설정
                return;
            }
        }
    }

    // 목표 지점으로 이동
    protected virtual void MoveTowards(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;

        // 이동 처리
        transform.position += direction * movementSpeed * Time.deltaTime;

        // 스무스 회전 처리
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        animator.SetBool("isWalking", true);
    }

    // 공격 실행
    protected virtual void PerformAttack()
    {
        animator.SetTrigger("Attack"); // 공격 애니메이션 실행
        
        CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, monsterData, transform, false);
    }
    
    protected void SetNewPatrolTarget()
    {
        // 패트롤을 위한 새로운 목표 위치 설정
        targetPosition = spawnPosition + new Vector3(
            Random.Range(-patrolRange, patrolRange),
            0,
            Random.Range(-patrolRange, patrolRange)
        );
    }

    // 상태 전환
    protected virtual void SetState(AIState newState)
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
                GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
                break;
            case AIState.Chasing:
                GameStateMachine.Instance.ChangeState(GameSystemState.Combat);
                break;
            case AIState.Attacking:
                break;
            case AIState.Dead:
                animator.SetTrigger("isDead");
                break;
        }
    }

    protected virtual IEnumerator SwitchStateAfterDelay(AIState newState, float delay)
    {
        // 일정 시간 후 상태 전환
        yield return new WaitForSeconds(delay); // 기본 지연 시간 적용
        SetState(newState);
    }

    public void SetDeadState()
    {
        SetState(AIState.Dead);

        OnDeathAnimationEnd();
    }
    
    public void OnDeathAnimationEnd()
    {
        // 몬스터 제거 처리
        col.enabled = false;
        rb.isKinematic = true;
        Destroy(this.gameObject, 1f);
    }
    
    private void OnDrawGizmosSelected()
    {
        // 디버깅을 위한 Gizmo 시각화
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRange);

        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(spawnPosition, maxPatrolDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BaseMonsterAI : MonoBehaviour
{
    public static readonly int IsWalking = Animator.StringToHash("isWalking");
    public static readonly int Attack = Animator.StringToHash("Attack");
    public static readonly int IsDead = Animator.StringToHash("isDead");
    public static readonly int Hit = Animator.StringToHash("Hit");

    public enum AIState { Idle, Patrolling, Chasing, Attacking, Returning, Stun, Hit, Dead }

    [Header("움직임 세팅")]
    public float patrolRange = 10f; // 패트롤 범위 (몬스터가 움직일 수 있는 반경)
    public float idleTime = 2f; // 대기 상태에서 머무는 시간
    protected float movementSpeed; // 이동 속도
    public float turnSpeed = 10f;
    public float maxPatrolDistance = 40f; // 스폰 위치에서 최대 이동 가능 거리
    public float arrivedDistance = 1f; // 도착 판정 거리 

    [Header("서치 & 공격 세팅")]
    public float searchRange = 7f; // 플레이어 탐지 범위

    protected float attackRange; // 공격 범위
    protected float attackCooldown; // 공격 쿨타임
    protected float attackCooldownTimer = 0f; // 공격 쿨타임 타이머
    public float hitStateDuration = 1f;

    [SerializeField] protected AIState currentState = AIState.Idle; // 현재 AI 상태
    [SerializeField] protected Vector3 spawnPosition; // 스폰 위치
    protected Vector3 targetPosition; // 패트롤 목적지
    protected Transform playerTarget; // 탐지된 플레이어

    protected Animator animator; // 애니메이터 캐시
   
    protected MonsterData monsterData; // 몬스터 데이터
    
    [SerializeField] private bool respawn = false;
    protected bool isAttacking = false; // 공격 중인지 여부 플래그
    
    protected bool isStunned = false;
    [SerializeField] protected float stunDuration = 5f; // 스턴 지속 시간
    
    protected CharacterController characterController;
    private Vector3 velocity;
    private float gravity = -9.81f;
    
    private Coroutine hitCoroutine; // 현재 실행 중인 코루틴을 저장
    
    protected WeaponCollider baseweaponCollider; // **공통 무기 콜라이더 변수 추가**
    
    protected virtual void OnEnable()
    {
        // 풀링 적용 후 리스폰 상태일 때 초기화
        if (respawn)
        {
            // 상태 초기화
            animator.SetBool(IsWalking, false);
            animator.ResetTrigger(IsDead);
            animator.ResetTrigger(Attack);
            playerTarget = null;
            currentState = AIState.Idle;
            
            // 풀링된 몬스터를 다시 캐릭터 리스트에 추가
            CharacterManager.Instance.AddCharacter(this.monsterData);
            
            CheckLandingAndSetPatrolTarget();
        }
    }
    
    protected void CheckLandingAndSetPatrolTarget()
    {
        if (characterController.isGrounded)
        {
            SetNewPatrolTarget(); // 착지 후 타겟 설정
            SetState(AIState.Patrolling);
        }
        else
        {
            StartCoroutine(CheckForLanding()); // 착지 여부를 주기적으로 체크
        }
    }
    
    // 착지 여부를 주기적으로 체크
    protected IEnumerator CheckForLanding()
    {
        WaitForFixedUpdate wait = new WaitForFixedUpdate(); // FixedUpdate 주기마다 실행
        
        while (!characterController.isGrounded)
        {
            yield return wait; // FixedUpdate 이후에만 체크하여 불필요한 연산 감소
        }
    
        // 착지 후 타겟 설정
        SetNewPatrolTarget();
        SetState(AIState.Patrolling); // 착지 후 패트롤 상태로 전환
    }

    protected virtual void OnDestroy()
    {
        if (monsterData != null) monsterData.OnTakeDamage -= HandleTakeDamage;
    }
    
    protected virtual void Start()
    {
        spawnPosition = transform.position; // 스폰 위치 저장
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        monsterData = GetComponent<BaseMonsterData>().GetMonsterData(); // MonsterData 참조 (Test1 컴포넌트에서 캐싱)
        
        if (monsterData != null)
        {
            monsterData.OnTakeDamage += HandleTakeDamage;
            movementSpeed = monsterData.moveSpeed;
            attackCooldown = monsterData.attackSpeed;
            attackRange = monsterData.attackRange;
        }
        
        // 무기 콜라이더가 있을 경우 자동으로 찾기 (자식 오브젝트에서 검색)
        baseweaponCollider = GetComponentInChildren<WeaponCollider>();
        
        // 착지 여부를 체크하는 메서드 호출
        CheckLandingAndSetPatrolTarget();
    }
    
    protected virtual void HandleTakeDamage(Transform attacker)
    {
        // if (attacker.CompareTag("Player")) playerTarget = attacker;
        playerTarget = GameManager.playerTransform;
        
        // 현재 상태가 Hit이더라도 다시 피격 상태로 전환 가능하도록 변경
        if (currentState == AIState.Hit)
        {
            RestartHitAnimation(); 
        }
        else
        {
            SetState(AIState.Hit);
        }
    }
    
    protected void RestartHitAnimation()
    {
        animator.ResetTrigger(Hit); // 기존 트리거를 초기화
        animator.SetTrigger(Hit); // 다시 트리거를 활성화하여 애니메이션을 재생
    }
    
    // MonsterData를 반환하는 메서드 추가
    public MonsterData GetMonsterData()
    {
        return monsterData;
    }
    
    protected virtual void FixedUpdate()
    {
        if (currentState == AIState.Dead) return; // 사망 상태에서는 동작하지 않음
        
        // 상시 중력 적용
        HandleGravity();

        attackCooldownTimer -= Time.deltaTime; // 공격 쿨타임 감소
        
        
        SearchForPlayer(); // 플레이어 탐색
        
        // 상태 처리
        HandleCurrentState();
    }
    
    protected virtual void HandleCurrentState()
    {
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
            case AIState.Stun:
                HandleStunState();
                break;
        }
    }
    
    // 상태 전환
    protected virtual void SetState(AIState newState)
    {
        // 죽음 상태나 스턴이면 상태전환 막기
        if (currentState == AIState.Dead || isStunned) return;
        
        // 공격중일 때 상태 전환을 막지만 예외적으로 스턴, 히트, 데드 상태로는 전환 가능
        if (isAttacking && newState != AIState.Stun && newState != AIState.Hit && newState != AIState.Dead) return;
        
        // **스턴, 피격, 사망 상태로 전환 시 무기 콜라이더 자동 비활성화**
        if (newState == AIState.Stun || newState == AIState.Hit || newState == AIState.Dead)
        {
            DisableWeaponCollider();
        }
        
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
                GameStateMachine.Instance.ChangeState(GameSystemState.Combat);
                break;
            case AIState.Attacking:
                break;
            case AIState.Hit:
                HandleHitState();
                break;
            case AIState.Dead:
                StopAllActions();
                animator.SetTrigger(IsDead);
                break;
        }
    }
    
    protected void DisableWeaponCollider()
    {
        if (baseweaponCollider)
        {
            baseweaponCollider.EnableWeaponCollider(false);
        }
    }
    
    protected virtual void HandleHitState()
    {
        if (isAttacking) StopAllActions();
    
        RestartHitAnimation(); // 피격 애니메이션 강제 재시작
        
        // 실행 중인 코루틴이 있다면 먼저 중지
        if (hitCoroutine != null)
        {
            StopCoroutine(hitCoroutine);
        }

        // 새로운 코루틴 실행 및 저장
        hitCoroutine = StartCoroutine(RecoverFromHit());
    }
    
    protected IEnumerator RecoverFromHit()
    {
        yield return new WaitForSeconds(hitStateDuration); // 피격 상태 유지 시간
        
        hitCoroutine = null; // 완료 후 참조 해제
        
        if (!isStunned)
        {
            SetState(playerTarget ? AIState.Chasing : AIState.Patrolling);
        }
    }
    
    // 외부 호출용 함수
    public void ChangeState(AIState newState)
    {
        SetState(newState);
    }
    
    protected virtual void SetNewPatrolTarget()
    {
        // 패트롤을 위한 새로운 목표 위치 설정
        bool validPositionFound = false;
        int maxAttempts = 10;
        int attempts = 0;

        while (!validPositionFound && attempts < maxAttempts)
        {
            Vector3 randomPosition = spawnPosition + new Vector3(
                Random.Range(-patrolRange, patrolRange),
                0f,  // Y 값은 나중에 계산
                Random.Range(-patrolRange, patrolRange)
            );

            // 레이캐스트를 사용하여 랜덤 위치의 실제 y 값 얻기
            RaycastHit hit;
            if (Physics.Raycast(randomPosition + Vector3.up * 10f, Vector3.down, out hit, Mathf.Infinity))
            {
                float groundY = hit.point.y;

                // 높이 제한을 더 유연하게 조정 (너무 높거나 낮은 곳 피함)
                if (groundY < spawnPosition.y + 2f && groundY > spawnPosition.y - 3f) 
                {
                    if (IsOnTerrain(randomPosition))
                    {
                        targetPosition = new Vector3(randomPosition.x, groundY, randomPosition.z);
                        validPositionFound = true;
                    }
                }
            }
            
            // --- 2025-02-01 04:17 Hyo 수정된 코드 충분히 테스트 후 문제 없을 시 삭제 처리 -----------------------------
            // if (Physics.Raycast(randomPosition + Vector3.up * 10f, Vector3.down, out hit, Mathf.Infinity))
            // {
            //     randomPosition.y = hit.point.y;
            //
            //     // Y값이 지나치게 높은 값이 되지 않도록 보정
            //     if (randomPosition.y < spawnPosition.y + 1f) // 지면과 너무 멀리 떨어지지 않도록 제한
            //     {
            //         if (IsOnTerrain(randomPosition)) // 목표 위치가 터레인 위인지 확인
            //         {
            //             targetPosition = randomPosition;
            //             validPositionFound = true;
            //         }
            //     }
            // }
            // ----------------------------------------------------------------------------------------------------

            attempts++;
        }

        if (!validPositionFound)
        {
            Debug.LogWarning("위치를 지정할수 없어 스폰위치로 지정");
            targetPosition = spawnPosition;
        }
    }

    private bool IsOnTerrain(Vector3 position)
    {
        Terrain terrain = Terrain.activeTerrain;
        if (!terrain) return false;

        Vector3 terrainPosition = terrain.transform.position;
        TerrainData terrainData = terrain.terrainData;

        return position.x >= terrainPosition.x &&
               position.x <= terrainPosition.x + terrainData.size.x &&
               position.z >= terrainPosition.z &&
               position.z <= terrainPosition.z + terrainData.size.z;
    }
    
    // 대기 상태 처리
    protected virtual void HandleIdleState()
    {
        if (playerTarget)
        {
            SetState(AIState.Chasing); // 플레이어가 발견되면 추적 상태로 전환
            return;
        }

        animator.SetBool(IsWalking, false); // 대기 상태에서 걷는 애니메이션 비활성화
        StartCoroutine(SwitchStateAfterDelay(AIState.Patrolling, idleTime)); // 일정 시간 후 패트롤 상태로 전환
    }

    // 패트롤 상태 처리
    protected virtual void HandlePatrollingState()
    {
        if (playerTarget)
        {
            SetState(AIState.Chasing); // 플레이어가 발견되면 추적 상태로 전환
            return;
        }

        MoveTowards(targetPosition); // 목표 위치로 이동

        if (Vector3.Distance(transform.position, targetPosition) <= arrivedDistance)
        {
            SetState(AIState.Idle); // 목표 지점 도착 시 대기 상태로 전환
        }
    }

    // 추적 상태 처리
    protected virtual void HandleChasingState()
    {
        if (!playerTarget || Vector3.Distance(transform.position, spawnPosition) > maxPatrolDistance)
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
        if (!playerTarget || Vector3.Distance(transform.position, spawnPosition) > maxPatrolDistance)
        {
            playerTarget = null; // 플레이어를 잃거나 최대 이동 범위를 벗어난 경우
            SetState(AIState.Returning); // 복귀 상태로 전환
            return;
        }
        
        if (CharacterManager.PlayerCharacterData.currentHp <= 0 || !playerTarget)
        {
            playerTarget = null;
            SetState(AIState.Patrolling); // 플레이어가 범위를 벗어나면 패트롤 상태로 전환
            return;
        }

        if (Vector3.Distance(transform.position, playerTarget.position) > attackRange && !isAttacking)
        {
            SetState(AIState.Chasing); // 공격 범위를 벗어나면 추적 상태로 전환
            return;
        }
        
        //
        if (attackCooldownTimer <= 0f && !isAttacking)
        {
            PerformAttack(); // 공격 실행
            attackCooldownTimer = attackCooldown; // 쿨타임 리셋
        }
    }

    // 복귀 상태 처리
    protected virtual void HandleReturningState()
    {
        isAttacking = false;
        
        SearchForPlayer();
        
        if (playerTarget)
        {
            SetState(AIState.Chasing); // 플레이어가 발견되면 추적 상태로 전환
            return;
        }
        
        MoveTowards(spawnPosition); // 스폰 위치로 돌아감

        if (Vector3.Distance(transform.position, spawnPosition) <= arrivedDistance * 1.1f)
        {
            SetState(AIState.Patrolling); // 스폰 위치에 도달하면 패트롤 상태로 전환
        }
    }
    
    protected virtual void HandleStunState()
    {
        // 패링 당한 후 효과 적용
        if (isStunned) return;

        // 디버그용 코드 -------------------------------------------------
        //UIManager.Instance.TogglinfoMessageWindow("스턴됨");
        Debug.LogWarning("몬스터 스턴됨");
        // -------------------------------------------------------------
        
        isStunned = true;
        StopAllActions();
    
        StartCoroutine(RecoverFromStun());
    }


    // 플레이어 탐색
    protected virtual void SearchForPlayer()
    {
        if (playerTarget) return;

        Collider[] hits = new Collider[30]; // 배열 크기 설정
        int hitCount = Physics.OverlapSphereNonAlloc(transform.position, searchRange, hits);

        if (hitCount == hits.Length)
        {
            Debug.LogWarning("탐지 배열 크기가 부족할 수 있습니다. 크기를 늘려주세요.");
        }

        if (CharacterManager.PlayerCharacterData.currentHp <= 0)
        {
            playerTarget = null;
            return;
        }

        for (int i = 0; i < hitCount; i++)
        {
            Collider hit = hits[i];
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
        if (isStunned) return;
        
        Vector3 direction = (destination - transform.position).normalized;

        // 이동 처리
        Vector3 movement = direction * (movementSpeed * Time.deltaTime);

        // CharacterController의 Move 메서드를 사용
        characterController.Move(movement);

        // 부드러운 회전 처리
        Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);

        // 애니메이션 처리
        animator.SetBool(IsWalking, true);
    }

    // 공격 실행
    protected virtual void PerformAttack()
    {
        if (isAttacking || isStunned) return; // 이미 공격 중이거나 스턴 상태면 공격 방지
        
        StopAllActions();
        animator.SetTrigger(Attack); // 공격 애니메이션 실행

        // 애니메이션의 길이 가져오기
        // AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        // float animationLength = stateInfo.length;
        // // 70% 지점에 맞춰 메서드 호출
        // Invoke(nameof(ExecuteAttack), animationLength * 0.7f);
        
        // 공격 후 이동을 방지하는 상태로 설정
        isAttacking = true;
        StartCoroutine(ResetAttackState(animator.GetCurrentAnimatorClipInfo(0)[0].clip.length));
    }
    
    protected virtual IEnumerator ResetAttackState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isAttacking = false;
    }
    
    protected virtual void ExecuteAttack()
    {
        CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, monsterData, transform, false, false);
    }
    
    protected void StopAllActions()
    {
        // 이동 및 애니메이션 동작 멈춤
        animator.SetBool(IsWalking, false);
        animator.ResetTrigger(Attack);
        
        // 공격 중이라면 즉시 취소
        isAttacking = false;
        StopAllCoroutines(); // 공격 코루틴 정지
        CancelInvoke(nameof(ExecuteAttack)); // 공격 실행 취소
        
        DisableWeaponCollider();
        
        // 중력 및 이동을 완전히 멈추도록 velocity 초기화
        velocity = Vector3.zero;
        characterController.Move(Vector3.zero);
        
        // 현재 방향을 고정 (불필요한 회전 방지)
        if (playerTarget)
        {
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
        }
    }
    
    protected IEnumerator RecoverFromStun()
    {
        yield return new WaitForSeconds(stunDuration);
        isStunned = false;
        
        // 몬스터가 이미 죽었으면 Dead 상태로 변경
        if (monsterData.currentHp <= 0)
        {
            SetState(AIState.Dead);
            yield break; // 상태 변경 후 즉시 코루틴 종료
        }
    
        if (playerTarget && Vector3.Distance(transform.position, playerTarget.position) <= searchRange)
        {
            SetState(AIState.Chasing);
        }
        else
        {
            SetState(AIState.Returning);
        }
    }

    protected virtual IEnumerator SwitchStateAfterDelay(AIState newState, float delay)
    {
        // 일정 시간 후 상태 전환
        yield return new WaitForSeconds(delay); // 기본 지연 시간 적용
        SetState(newState);
    }

    public void SetDeadState(bool pooling)
    {
        SetState(AIState.Dead);
        StartCoroutine(OnDeathAnimationEnd(pooling));
    }
    
    IEnumerator OnDeathAnimationEnd(bool pooling)
    {
        characterController.Move(Vector3.zero); // 이동 정지

        yield return new WaitForSeconds(1.5f);
        if (pooling)
        {
            respawn = true;
            gameObject.SetActive(false);

            // 데이터 초기화 부분
            monsterData.ResetDataByLevel();
        }
        else
        {
            Destroy(this.gameObject);
        }
    }
    
    // 상시 중력 적용
    protected virtual void HandleGravity()
    {
        if (!characterController.isGrounded)
        {
            velocity.y += gravity * Time.deltaTime;
        }
        else
        {
            velocity.y = 0;
        }

        // 이동 처리에 velocity 값 전달
        characterController.Move(velocity * Time.deltaTime);
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
        
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(targetPosition, arrivedDistance);
        
        // // 착지 여부 시각화
        // if (characterController.isGrounded)
        // {
        //     Gizmos.color = Color.green; // 착지 시 초록색
        // }
        // else
        // {
        //     Gizmos.color = Color.red; // 공중에 있을 때 빨간색
        // }
        // Gizmos.DrawWireSphere(transform.position, 1f); // 착지 상태 표시
    }
}

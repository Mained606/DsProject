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
    public float hitDuration = 1f;            // 피격 상태 지속 시간
    public float movementSpeed;               // 이동 속도 (BossData에서 할당)
    public float attackRange;                 // 공격 가능 거리 (또는 체이싱→어택 전환 기준)
    public float turnSpeed = 10f;
    
    [Header("스킬 설정")]
    public float attackCooldown = 10f; // 공격 간격
    protected float attackCooldownTimer;
        
    [SerializeField] protected BossState currentState = BossState.Idle;
    [SerializeField] protected Transform playerTarget;
    protected Vector3 spawnPosition;

    protected Animator animator;
    protected CharacterController characterController;
    [SerializeField] protected BossData bossData;
    
    protected Vector3 velocity;
    protected float gravity = -9.81f;
    
    protected bool isAttacking = false;
    protected bool isStunned = false;
    protected bool isPerformingSpecialMove = false;
    protected bool isRotating = true; // 회전 방지 플래그 추가
    
    [SerializeField] protected bool respawn = false;
    protected float arrivedDistance = 1f;

    [SerializeField] protected float stunDuration = 5f; // 스턴 지속 시간 (패링 시 적용되는 기본값)

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
            animator.SetBool(IsDead, false);
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

    protected virtual void Start()
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
        
        bossData.OnSpeedChanged += UpdateMovementSpeed;
    }

    protected virtual void Update()
    {
        if (currentState == BossState.Dead) return;
        
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
        if (currentState == BossState.Returning || currentState == BossState.Dead || currentState == BossState.Idle || GameStateMachine.Instance.CurrentState == GameSystemState.Exploration)
            UIManager.Instance.BossHudDisplay(false);
        else UIManager.Instance.BossHudDisplay(true, bossData);
        /////////////////////////////////////////////////////////////////////////////////////////////

        // 서치: 플레이어가 아직 탐지되지 않았다면 검색
        if (currentState != BossState.Returning)
        {
            SearchForPlayer();
        }
    }
    
    protected virtual void HandleIdleLogic()
    {
        // 플레이어가 이미 감지되었다면 배틀 시작 (StartBossBattle 내부에서 상태 전환 처리)
        if (playerTarget != null)
        {
            StartBossBattle();
        }
    }
    
    protected virtual void HandleChasingLogic()
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
    
    protected virtual void HandleCombatLogic()
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
    
    protected virtual void HandleReturningLogic()
    {
        animator.SetBool(IsDashing, false);
        animator.SetBool(IsChasing, false);
        animator.SetBool(IsDead, false);
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

    protected virtual void SearchForPlayer()
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
    
    protected virtual void RotateTowardsPlayer()
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

    protected virtual void StartBossBattle()
    {
        if (GameStateMachine.Instance.CurrentState != GameSystemState.BossBattle)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.BossBattle);
        }
        SetState(BossState.Roaring);
    }

    protected virtual void SetState(BossState newState)
    {
        // 죽음 상태면 상태 전환 불가
        if (currentState == BossState.Dead)
            return;

        // 스턴 상태인 경우, 스턴 해제 후에만 다른 상태로 전환 가능
        // (단, Stun, Hit, Dead 상태로 전환하는 것은 항상 허용)
        if (isStunned && newState != BossState.Stun && newState != BossState.Hit && newState != BossState.Dead)
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
            case BossState.Stun:
                HandleStunState();
                break;
            case BossState.Dead:
                HandleDeath();
                break;
        }
    }

    protected virtual IEnumerator RoaringSequence()
    {
        Debug.Log("로어링 시작");
        animator.SetTrigger(IsRoaring);
        yield return new WaitForSeconds(roarDuration);
        SetState(BossState.Chasing);
    }
    
    protected virtual IEnumerator HandleHitState()
    {
        animator.SetTrigger(Hit);
        
        // 피격 상태에서도 플레이어 추적 가능하도록 즉시 처리
        StartCoroutine(AllowMovementWhileHit());
        
        yield return new WaitForSeconds(hitDuration);
        SetState(BossState.Attacking);
    }
    
    protected virtual IEnumerator AllowMovementWhileHit()
    {
        // 약간의 딜레이를 주어 피격 애니메이션이 시작될 시간을 줌
        yield return new WaitForSeconds(0.1f);
        
        // 플레이어가 있고 스턴 상태가 아니면 추적 로직 활성화
        if (playerTarget != null && !isStunned && currentState == BossState.Hit)
        {
            // 이동 가능하도록 설정 (실제 이동 로직은 MoveTowardsWhileHit에서 처리)
            StartCoroutine(MoveTowardsWhileHit());
        }
    }
    
    protected virtual IEnumerator MoveTowardsWhileHit()
    {
        float hitMovementSpeed = movementSpeed * 0.5f; // 피격 중 이동 속도는 일반 이동의 50%로 감소
        
        // 피격 상태가 유지되는 동안 계속 실행
        while (currentState == BossState.Hit && !isStunned && playerTarget != null)
        {
            // 플레이어 방향으로 회전
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            direction.y = 0; // y축 회전 제한
            
            if (direction != Vector3.zero && isRotating)
            {
                Quaternion lookRotation = Quaternion.LookRotation(direction);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, turnSpeed * Time.deltaTime);
                
                // 앞으로 이동 (회전 방향으로)
                Vector3 moveDirection = transform.forward * hitMovementSpeed * Time.deltaTime;
                
                // 중력 적용 유지
                moveDirection.y = velocity.y;
                
                // 캐릭터 컨트롤러를 통한 이동
                characterController.Move(moveDirection);
            }
            
            yield return null;
        }
    }

    // 보스의 공격 실행 로직을 가상 메서드로 정의하여 자식 클래스에서 오버라이드 가능하게 함
    protected virtual IEnumerator ExecuteBossAttack()
    {
        if (isAttacking) yield break;
        isAttacking = true;
        
        // 기본 대기 시간
        yield return new WaitForSeconds(attackCooldown);
        
        isAttacking = false;
        
        // 공격 후 상태 결정
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
    }

    protected virtual IEnumerator RotateTowardsPlayerSmoothly()
    {
        if (playerTarget == null) yield break;
        
        Vector3 targetDirection = (playerTarget.position - transform.position).normalized;
        targetDirection.y = 0;
        
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
        if (attacker != null && attacker.CompareTag("Player"))
        {
            playerTarget = attacker;
        }
        // attacker가 null이면 기본적으로 GameManager의 playerTransform 사용
        else if (attacker == null && GameManager.playerTransform != null)
        {
            playerTarget = GameManager.playerTransform;
        }

        // 보스 배틀 상태가 아니라면 전환
        if (GameStateMachine.Instance.CurrentState != GameSystemState.BossBattle)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.BossBattle);
        }
        SetState(BossState.Roaring);
    }
    
    protected virtual void HandleDeath()
    {
        animator.SetBool(IsDead, true);
        playerTarget = null;
        isAttacking = false;
        isPerformingSpecialMove = false;
        isRotating = true;
        UIManager.Instance.BossHudDisplay(false);
        Debug.Log("보스가 사망했습니다.");
    }
    
    public virtual void SetDeadState(bool pooling)
    {
        SetState(BossState.Dead);
        StartCoroutine(OnDeathAnimationEnd(pooling));
    }

    protected virtual IEnumerator OnDeathAnimationEnd(bool pooling)
    {
        characterController.Move(Vector3.zero); // 이동 정지

        yield return new WaitForSeconds(3f);
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
    
    protected virtual void RestartHitAnimation()
    {
        animator.ResetTrigger(Hit);
        animator.SetTrigger(Hit);
    }
    
    // 랜덤 위치를 보스 주변에서 생성하는 함수
    protected virtual Vector3 GetRandomPosition(float range)
    {
        float randomX = Random.Range(-range, range);
        float randomZ = Random.Range(-range, range);
        Vector3 randomPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        return randomPosition;
    }

    protected virtual void UpdateMovementSpeed(float newSpeed)
    {
        movementSpeed = newSpeed;
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(spawnPosition, 3f);
        
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(this.transform.position, searchRange);
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(this.spawnPosition, maxDistance);
    }

    // 보스 스턴 상태 처리 메서드
    protected virtual void HandleStunState()
    {
        // 이미 스턴 상태면 중복 적용하지 않음
        if (isStunned) return;

        Debug.LogWarning("보스 스턴됨: " + gameObject.name);
        
        isStunned = true;
        isAttacking = false;
        isPerformingSpecialMove = false;
        
        // 이동 멈춤
        if (characterController)
        {
            characterController.Move(Vector3.zero);
        }
        
        // 애니메이션 설정
        if (animator)
        {
            // 모든 애니메이션 초기화
            animator.SetBool(IsDashing, false);
            animator.SetBool(IsChasing, false);
            
            // Stun 파라미터가 있는지 확인
            AnimatorControllerParameter[] parameters = animator.parameters;
            foreach (AnimatorControllerParameter param in parameters)
            {
                if (param.name == "Stunned" || param.name == "Stun")
                {
                    if (param.type == AnimatorControllerParameterType.Trigger)
                    {
                        animator.SetTrigger(param.name);
                    }
                    else if (param.type == AnimatorControllerParameterType.Bool)
                    {
                        animator.SetBool(param.name, true);
                    }
                    break;
                }
            }
        }
    }
    
    // 지정된 시간 후 스턴에서 회복
    protected virtual IEnumerator RecoverFromStun(float duration)
    {
        Debug.Log($"{gameObject.name} 보스 스턴 지속시간: {duration}초");
        
        // 스턴 상태 적용
        isStunned = true;
        
        // 지정된 시간만큼 대기
        yield return new WaitForSeconds(duration);
        
        // 스턴 상태 해제
        isStunned = false;
        
        // 스턴 애니메이션 파라미터 초기화
        if (animator)
        {
            AnimatorControllerParameter[] parameters = animator.parameters;
            foreach (AnimatorControllerParameter param in parameters)
            {
                if ((param.name == "Stunned" || param.name == "Stun") && 
                    param.type == AnimatorControllerParameterType.Bool)
                {
                    animator.SetBool(param.name, false);
                    break;
                }
            }
        }
        
        // 보스가 이미 죽었는지 확인
        if (bossData.currentHp <= 0)
        {
            currentState = BossState.Dead;
            animator.SetBool(IsDead, true);
            yield break;
        }
        
        // 현재 상태를 명시적으로 변경 (스턴에서 회복)
        currentState = BossState.Idle; // 먼저 상태를 초기화
        
        // 회복 후 플레이어가 있으면 공격 상태로 전환
        if (playerTarget)
        {
            SetState(BossState.Attacking);
        }
        else
        {
            SetState(BossState.Idle);
        }
        
        Debug.Log($"{gameObject.name} 보스 스턴에서 회복됨");
    }
    
    // 외부에서 스턴 적용을 위한 공개 메서드
    public virtual void ApplyStun(float duration = -1)
    {
        // 이미 스턴 상태인 경우 무시
        if (isStunned)
        {
            Debug.Log($"[BaseBossAI] {gameObject.name} 보스는 이미 스턴 상태입니다. 중복 스턴을 무시합니다.");
            return;
        }
        
        // 지정된 지속시간이 없으면 기본값 사용
        float actualDuration = duration > 0 ? duration : stunDuration;
        
        // 모든 코루틴 중지 대신 스턴 관련 코루틴만 중지
        StopCoroutine("RecoverFromStun");
        
        // 상태 변경 및 새 코루틴 시작
        SetState(BossState.Stun);
        StartCoroutine(RecoverFromStun(actualDuration));
        
        Debug.Log($"{gameObject.name} 보스에게 {actualDuration}초 스턴 적용");
    }

    // 스턴 상태를 초기화하는 메서드
    public virtual void ResetStun()
    {
        isStunned = false;
        
        // 현재 보스가 스턴 상태인 경우 적절한 다른 상태로 전환
        if (currentState == BossState.Stun)
        {
            if (playerTarget != null)
            {
                SetState(BossState.Attacking);
            }
            else
            {
                SetState(BossState.Idle);
            }
            
            Debug.Log($"{gameObject.name} 보스 스턴 초기화 완료");
        }
    }
}

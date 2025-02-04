using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BaseBossAI : MonoBehaviour
{
    public static readonly int IsRoaring = Animator.StringToHash("IsRoaring");
    public static readonly int IsTeleporting = Animator.StringToHash("IsTeleporting");
    public static readonly int Hit = Animator.StringToHash("Hit");
    public static readonly int IsDead = Animator.StringToHash("IsDead");

    public enum BossState { Idle, Roaring, Attacking, Returning, Hit, Stun, Dead }

    [Header("보스 AI 설정")]
    public float searchRange = 30f; // 보스 탐색 범위
    public float maxDistance = 50f; // 플레이어가 벗어날 최대 거리
    public float roarDuration = 3f; // 함성 지속 시간
    public float teleportInterval = 80f; // 텔레포트 간격
    public float teleportRange = 10f; // 텔레포트 범위
    public float hitDuration = 1f; // 피격 상태 유지 시간
    public float movementSpeed;
    public float attackRange;

    [Header("스킬 설정")]
    public float attackCooldown = 3f; // 공격 간격
    private float attackCooldownTimer;

    [SerializeField] private BossState currentState = BossState.Idle;
    private Transform playerTarget;
    private Vector3 spawnPosition;

    private Animator animator;
    private CharacterController characterController;
    private BossData bossData;
    
    private Vector3 velocity;
    private float gravity = -9.81f;
    
    private bool isAttacking = false;
    private bool isStunned = false;

    protected virtual void OnDestroy()
    {
        if (bossData != null) bossData.OnTakeDamage -= HandleTakeDamage;
    }
    private void Start()
    {
        spawnPosition = transform.position;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        bossData = GetComponent<Test1>().bossData;

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
        if (currentState == BossState.Dead || currentState == BossState.Returning) return;
        
        HandleGravity();

        attackCooldownTimer -= Time.deltaTime;
        
        SearchForPlayer();

        HandleCurrentState();
    }
    
    protected virtual void HandleCurrentState()
    {
        switch (currentState)
        {
            case BossState.Idle:
                break;
            case BossState.Roaring:
                break;
            case BossState.Attacking:
                HandleCombatLogic();
                break;
        }
    }

    private void SearchForPlayer()
    {
        if (playerTarget != null) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, searchRange);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerTarget = hit.transform;
                StartBossBattle();
                return;
            }
        }
    }

    private void StartBossBattle()
    {
        GameStateMachine.Instance.ChangeState(GameSystemState.BossBattle);
        // UIManager.Instance.BossHudUP(bossData);
        SetState(BossState.Roaring);
    }

    private void SetState(BossState newState)
    {
        // 죽음 상태나 스턴이면 상태전환 막기
        if (currentState == BossState.Dead || isStunned) return;
        
        // 공격중일 때 상태 전환을 막지만 예외적으로 스턴, 히트, 데드 상태로는 전환 가능
        if (isAttacking && newState != BossState.Stun && newState != BossState.Hit && newState != BossState.Dead) return;
        
        // 상태가 이미 동일하면 변경하지 않음
        if (currentState == newState) return;
        
        currentState = newState;

        switch (newState)
        {
            case BossState.Roaring:
                StartCoroutine(RoaringSequence());
                break;
            case BossState.Attacking:
                break;
            case BossState.Returning:
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
        Debug.Log("롤링 시작");
        animator.SetTrigger(IsRoaring);
        yield return new WaitForSeconds(roarDuration);
        SetState(BossState.Attacking);
    }

    private void HandleCombatLogic()
    {
        // 전투 로직 실행
        
        // 랜덤 스킬 사용
        
        // 이후 기능 추가
        
    }
    
    private void HandleDeath()
    {
        animator.SetTrigger(IsDead);
        Debug.Log("보스가 사망했습니다.");
    }

    private IEnumerator HandleHitState()
    {
        animator.SetTrigger(Hit);
        yield return new WaitForSeconds(hitDuration);
        SetState(BossState.Attacking);
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
    
    protected virtual void HandleTakeDamage(Transform attacker)
    {
        // if (attacker.CompareTag("Player")) playerTarget = attacker;
        playerTarget = GameManager.playerTransform;
        
        // 현재 상태가 Hit이더라도 다시 피격 상태로 전환 가능하도록 변경
        if (currentState == BossState.Hit)
        {
            RestartHitAnimation(); 
        }
        else
        {
            SetState(BossState.Hit);
        }
    }
    
    protected void RestartHitAnimation()
    {
        animator.ResetTrigger(Hit); // 기존 트리거를 초기화
        animator.SetTrigger(Hit); // 다시 트리거를 활성화하여 애니메이션을 재생
    }
}

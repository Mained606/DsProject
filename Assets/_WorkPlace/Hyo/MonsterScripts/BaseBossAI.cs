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
    public float roarDuration = 4f; // 함성 지속 시간
    public float teleportInterval = 80f; // 텔레포트 간격
    public float teleportRange = 10f; // 텔레포트 범위
    public float hitDuration = 1f; // 피격 상태 유지 시간
    public float movementSpeed;
    public float attackRange;

    [Header("스킬 설정")]
    public float attackCooldown = 10f; // 공격 간격
    private float attackCooldownTimer;

    [SerializeField] private BossState currentState = BossState.Idle;
    private Transform playerTarget;
    private Vector3 spawnPosition;

    private Animator animator;
    private CharacterController characterController;
    [SerializeField] private BossData bossData;
    
    private Vector3 velocity;
    private float gravity = -9.81f;
    
    private bool isAttacking = false;
    private bool isStunned = false;

    [SerializeField] private GameObject firePoint1;

    protected virtual void OnDestroy()
    {
        if (bossData != null) bossData.OnTakeDamage -= HandleTakeDamage;
    }
    private void Start()
    {
        spawnPosition = transform.position;
        animator = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();
        bossData = GetComponent<BaseMonsterData>().bossData;

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
        RotateTowardsPlayer();
        HandleCurrentState();
    }
    
    protected virtual void HandleCurrentState()
    {
        switch (currentState)
        {
            case BossState.Idle:
                HandleIdleLogic();
                break;
            case BossState.Roaring:
                break;
            case BossState.Attacking:
                HandleCombatLogic();
                break;
        }
    }

    private void HandleIdleLogic()
    {
        if (playerTarget == null) return;
        
        if (Vector3.Distance(transform.position, playerTarget.position) > maxDistance)
        {
            playerTarget = null;
            SetState(BossState.Idle);
        }
        else
        {
            SetState(BossState.Attacking);
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
    
    private void RotateTowardsPlayer()
    {
        if (playerTarget == null) return;

        Vector3 direction = (playerTarget.position - transform.position).normalized;
        direction.y = 0;

        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 2f);
        }
    }

    private void StartBossBattle()
    {
        GameStateMachine.Instance.ChangeState(GameSystemState.BossBattle, bossData);
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
        Debug.Log("Roaring 시작");
        animator.SetTrigger(IsRoaring);
        yield return new WaitForSeconds(roarDuration);
        SetState(BossState.Attacking);
    }

    private void HandleCombatLogic()
    {
        if (currentState != BossState.Attacking || isAttacking) return;

        StartCoroutine(ExecuteBossAttack());
    }

    private IEnumerator ExecuteBossAttack()
    {
        if (isAttacking) yield break; // 이미 공격 중이라면 실행하지 않음
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
        int maxAttempts = 10; // 최대 10번만 시도
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
            Debug.LogWarning("사용 가능한 스킬이 없습니다.");
            SetState(BossState.Idle);
            isAttacking = false;
            yield break;
        }
        
        Vector3 randomSkillPosition = GetRandomSkillPosition();
        
        switch (selectedSkill.skillName)
        {
            case "OrbExplosion":
                animator.SetTrigger(IsRoaring);
                yield return new WaitForSeconds(roarDuration);
                SkillManager.Instance.ActivateSkillForEntity(EntityType.Boss, selectedSkill.skillName, GameManager.playerTransform.gameObject);
                break;
            case "Test2":
                animator.SetTrigger(IsRoaring);
                yield return new WaitForSeconds(5f);
                SkillManager.Instance.ActivateSkillForEntity(EntityType.Boss, selectedSkill.skillName, firePoint1);
                break;
            case "Test3":
                Debug.Log("테스트 스킬1 발동");
                SkillManager.Instance.ActivateSkillForEntity(EntityType.Boss, selectedSkill.skillName, gameObject);
                break;
        }
        
        // 스킬 지속 시간 가져오기
        float skillDuration = selectedSkill.GetSkillDuration();
        yield return new WaitForSeconds(skillDuration + attackCooldown); // 스킬 후 대기시간 포함

        // 공격 후 플레이어가 여전히 유효한가?
        if (playerTarget != null && Vector3.Distance(transform.position, playerTarget.position) <= maxDistance)
        {
            SetState(BossState.Attacking); // 다시 공격 시도
        }
        else
        {
            SetState(BossState.Idle); // 타겟이 없으면 Idle 상태로 전환
        }

        isAttacking = false; // 공격 완료 후 플래그 해제
    }
    
    private void HandleDeath()
    {
        animator.SetTrigger(IsDead);
        
        // GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
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
    
    // 랜덤 위치를 보스 주변에서 생성하는 함수
    private Vector3 GetRandomSkillPosition()
    {
        float randomX = Random.Range(-teleportRange, teleportRange);
        float randomZ = Random.Range(-teleportRange, teleportRange);
        Vector3 randomPosition = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
        return randomPosition;
    }
}

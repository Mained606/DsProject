using System.Collections;
using UnityEngine;

public class MushroomAi : MonoBehaviour
{
    public enum AIState { Idle, Patrolling, Chasing, Attacking, Returning }

    [Header("움직임 세팅")]
    public float patrolRange = 4f; // 랜덤 이동 범위
    public float idleTime = 2f; // 대기 시간
    public float movementSpeed = 2f; // 이동 속도

    [Header("서치 & 공격 세팅")]
    public float searchRange = 7f; // 서치 범위
    public float attackRange = 1.5f; // 공격 범위
    public float attackCooldown = 2f; // 공격 쿨타임

    private AIState currentState = AIState.Idle;
    private Vector3 spawnPosition;
    private Vector3 targetPosition;
    private Transform playerTarget;
    private float attackCooldownTimer = 0f;
    private Animator animator;
    private MonsterData monsterData;

    private void Start()
    {
        spawnPosition = transform.position;
        animator = GetComponent<Animator>();
        monsterData = GetComponent<Test1>().monster;
        SetNewPatrolTarget();
    }

    private void Update()
    {
        attackCooldownTimer -= Time.deltaTime;

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

        SearchForPlayer();
    }

    private void HandleIdleState()
    {
        if (playerTarget != null)
        {
            ChangeState(AIState.Chasing);
            return;
        }

        animator?.SetBool("isWalking", false);
        StartCoroutine(SwitchStateAfterDelay(AIState.Patrolling, idleTime));
    }

    private void HandlePatrollingState()
    {
        if (playerTarget != null)
        {
            ChangeState(AIState.Chasing);
            return;
        }

        MoveTowards(targetPosition);

        if (Vector3.Distance(transform.position, targetPosition) <= 0.1f)
        {
            ChangeState(AIState.Idle);
        }
    }

    private void HandleChasingState()
    {
        if (playerTarget == null || Vector3.Distance(transform.position, playerTarget.position) > searchRange)
        {
            playerTarget = null;
            ChangeState(AIState.Returning);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer <= attackRange)
        {
            ChangeState(AIState.Attacking);
        }
        else
        {
            MoveTowards(playerTarget.position);
        }
    }

    private void HandleAttackingState()
    {
        if (playerTarget == null || Vector3.Distance(transform.position, playerTarget.position) > searchRange)
        {
            playerTarget = null;
            ChangeState(AIState.Returning);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > attackRange)
        {
            ChangeState(AIState.Chasing);
            return;
        }

        if (attackCooldownTimer <= 0f)
        {
            PerformAttack();
            attackCooldownTimer = attackCooldown;
        }
    }

    private void HandleReturningState()
    {
        MoveTowards(spawnPosition);

        if (Vector3.Distance(transform.position, spawnPosition) <= 0.1f)
        {
            ChangeState(AIState.Patrolling);
        }
    }

    private void SearchForPlayer()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, searchRange);

        foreach (var hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                playerTarget = hit.transform;
                return;
            }
        }
    }

    private void MoveTowards(Vector3 destination)
    {
        Vector3 direction = (destination - transform.position).normalized;
        transform.position += direction * movementSpeed * Time.deltaTime;
        transform.LookAt(new Vector3(destination.x, transform.position.y, destination.z));

        animator?.SetBool("isWalking", true);
    }

    private void PerformAttack()
    {
        animator?.SetTrigger("Attack");
        CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, monsterData, transform, false);
    }

    private void SetNewPatrolTarget()
    {
        targetPosition = spawnPosition + new Vector3(
            Random.Range(-patrolRange, patrolRange),
            0,
            Random.Range(-patrolRange, patrolRange)
        );
    }

    private void ChangeState(AIState newState)
    {
        if (currentState == newState) return;

        currentState = newState;

        if (currentState == AIState.Patrolling)
        {
            SetNewPatrolTarget();
        }

        if (currentState == AIState.Idle || currentState == AIState.Returning)
        {
            animator?.SetBool("isWalking", false);
        }
    }

    private IEnumerator SwitchStateAfterDelay(AIState newState, float delay)
    {
        yield return new WaitForSeconds(delay);
        ChangeState(newState);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

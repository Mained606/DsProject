using System.Collections;
using UnityEngine;

public class MushroomAi : MonoBehaviour
{
    [Header("움직임 세팅")]
    public float patrolRange = 4f; // 랜덤 이동 범위

    [Header("서치 & 공격 세팅")]
    public float searchRange = 7f; // 서치 범위
    public float attackRange = 1.5f; // 공격 범위
    public float attackCooldown = 2f; // 공격 쿨타임

    private Vector3 spawnPosition; // 스폰 위치
    private Vector3 targetPosition; // 랜덤 이동 목표 위치
    private Transform playerTarget; // 플레이어 타겟
    private bool isAttacking = false;
    private bool canAttack = true;

    private Animator animator;
    private MonsterData monsterData;

    private void Start()
    {
        spawnPosition = transform.position;
        animator = GetComponent<Animator>();
        monsterData = GetComponent<Test1>().monster;
        StartCoroutine(PatrolRoutine());
    }

    private void Update()
    {
        // 플레이어 탐색
        SearchForPlayer();

        if (playerTarget != null)
        {
            // 플레이어가 서치 범위 내에 있을 경우 공격 실행
            ApproachAndAttackPlayer();
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
                break;
            }
        }
    }

    private void ApproachAndAttackPlayer()
    {
        if (playerTarget == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, playerTarget.position);

        if (distanceToPlayer > attackRange)
        {
            // 플레이어에게 접근
            Vector3 direction = (playerTarget.position - transform.position).normalized;
            transform.position += direction * monsterData.speed * Time.deltaTime;
            transform.LookAt(playerTarget.position);

            if (animator != null)
            {
                animator.SetBool("isWalking", true);
            }
        }
        else if (canAttack && distanceToPlayer <= attackRange)
        {
            // 공격 실행
            StartCoroutine(AttackPlayer());
        }
    }
    
    // 패트롤 패턴
    private IEnumerator PatrolRoutine()
    {
        while (true)
        {
            if (playerTarget == null)
            {
                // 랜덤 위치 설정
                targetPosition = spawnPosition + new Vector3(
                    Random.Range(-patrolRange, patrolRange),
                    0,
                    Random.Range(-patrolRange, patrolRange)
                );

                // 목표 위치로 이동
                while (Vector3.Distance(transform.position, targetPosition) > 0.1f)
                {
                    Vector3 direction = (targetPosition - transform.position).normalized;
                    transform.position += direction * monsterData.speed * Time.deltaTime;
                    transform.LookAt(targetPosition);

                    if (animator != null)
                    {
                        animator.SetBool("isWalking", true);
                    }

                    yield return null;
                }

                if (animator != null)
                {
                    animator.SetBool("isWalking", false);
                }

                // 잠시 대기
                yield return new WaitForSeconds(Random.Range(2f, 5f));
            }
            else
            {
                yield return null;
            }
        }
    }

    private IEnumerator AttackPlayer()
    {
        if (isAttacking) yield break;

        isAttacking = true;
        canAttack = false;

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        // CombatManager를 통한 공격 처리
        CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, this.monsterData, this.transform, false);

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
        isAttacking = false;
    }

    private void OnDrawGizmosSelected()
    {
        // 서치 범위와 공격 범위를 시각적으로 표시
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, searchRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}

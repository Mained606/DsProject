using System.Collections.Generic;
using UnityEngine;

public class DragonController : MonoBehaviour
{
    private static readonly int TurnDirection = Animator.StringToHash("turnDirection");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");

    // ===== 기존 이동/따라다니기 관련 필드 =====
    [SerializeField] private DragonData dragonData;         // 드래곤 데이터
    private Transform player;              // 따라다닐 플레이어
    private Animator animator;             // 용의 애니메이터
    [SerializeField] private Transform firePoint;           // 파이어 포인트
    private Vector3 lastPlayerPosition;    // 플레이어의 마지막 위치
    private bool isMoving;                 // 현재 이동 중인지 상태

    [Header("Follow Settings")]
    public float followDistance = 3f;      // 플레이어와 유지할 거리
    public float teleportDistance = 10f;   // 순간이동할 거리
    public float followSpeed = 5f;         // 따라다니는 속도
    public float hoverHeight = 2f;         // 플레이어 위로 떠 있을 높이
    public float rotationSpeed = 5f;       // 회전 속도

    private Vector3 offset;                // 플레이어와의 상대 위치

    // ===== 전투 관련 필드 =====
    [Header("Combat Settings")]
    public float detectRange = 10f;        // 전투 시, 주변 몬스터 탐색 범위
    public float meleeRange = 1f;          // 근접 공격 범위
    public float maxDistanceFromPlayer = 15f; // 플레이어로부터 이만큼 멀어지면 타겟 해제 & 순간이동
    
    private float meleeCooldown;  // 근접 공격 쿨타임
    [SerializeField] private GameObject fireballPrefab; // 파이어볼 프리팹

    // 간단한 쿨다운 예시
    public float skillCooldown = 8f;       // 스킬 쿨다운
    public float rangedCooldown = 4f;      // 원거리 공격(파이어볼) 쿨다운

    private float skillTimer = 0f;
    private float rangedTimer = 0f;

    // 현재 타겟(몬스터)
    [SerializeField] private MonsterData currentTarget = null;
    [SerializeField] private Transform currentTargetTransform = null;

    // 우선순위: 1. 스킬 > 2. 파이어볼 > 3. 근접 공격
    // 스킬 / 원거리 공격 / 근접 공격 메서드는 아직 구현 전이라면 스텁(빈 함수)으로 둬도 됩니다.

    private void OnEnable()
    {
        GameManager.DragonTransform = transform;
    }

    private void Start()
    {
        // 기존 설정
        dragonData = CharacterManager.DragonData;

        player = GameManager.playerTransform; // 플레이어 Transform 가져오기
        animator = GetComponent<Animator>();  // 애니메이터 가져오기
        if (dragonData == null)
        {
            Debug.LogError("DragonData가 CharacterManager에서 설정되지 않았습니다.");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player Transform이 GameManager에서 설정되지 않았습니다.");
            return;
        }

        if (animator == null)
        {
            Debug.LogError("Animator가 이 오브젝트에 연결되어 있지 않습니다.");
            return;
        }
        
        // 어택 스피드를 기반으로 근접 공격 쿨타임 계산
        if (dragonData != null)
        {
            meleeCooldown = dragonData.attackSpeed;  // 어택 스피드를 기준으로 쿨타임 계산 (초단위)
        }
        else
        {
            meleeCooldown = 3f;  // 기본 쿨타임 설정 (어택 스피드가 없을 경우)
        }

        // 플레이어와 초기 상대 위치 설정
        offset = new Vector3(0, hoverHeight, -followDistance);
        lastPlayerPosition = player.position; // 초기 위치 저장
    }

    private void Update()
    {
        // 쿨다운 갱신
        UpdateCooldowns();

        // 전투 상태일 때만 몬스터를 추적
        if (GameStateMachine.Instance.CurrentState == GameSystemState.Combat)
        {
            HandleCombatLogic();
        }
        else
        {
            // 전투 상태가 아닐 때는 플레이어를 따라감
            FollowPlayerLogic();
        }
    }

    #region 전투 로직

    // 전투 상황에서 실행되는 메인 로직
    private void HandleCombatLogic()
    {
        // 플레이어와 너무 멀어졌을 때, 타겟을 해제하고 플레이어 근처로 순간이동
        float distanceFromPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceFromPlayer > maxDistanceFromPlayer)
        {
            currentTarget = null;
            currentTargetTransform = null;
            // 플레이어 옆으로 순간이동
            transform.position = player.position + offset;
            return;
        }

        // 현재 타겟이 없거나 이미 죽었으면 새로운 타겟을 찾는다
        if (currentTarget == null || currentTarget.currentHp <= 0)
        {
            FindNearestTarget();
        }

        // 타겟이 존재하지 않으면 공격 불가 -> 그대로 종료
        if (currentTarget == null)
        {
            FollowPlayerLogic();
            return;
        }

        // 공격 우선순위 1) 스킬, 2) 원거리 공격(파이어볼), 3) 근접 공격
        if (!IsSkillOnCooldown())
        {
            UseSkillAttack();
        }
        else if (!IsRangedOnCooldown())
        {
            UseRangedAttack();
        }
        else
        {
            // 타겟 위치로 이동(근접 공격 등을 위해)
            MoveTowardTarget(currentTargetTransform);
            UseMeleeAttack();
            
        }
    }


    // 주변에서 가장 가까운 몬스터를 찾아서 타겟 설정
    private void FindNearestTarget()
    {
        float closestDistance = float.MaxValue;
        MonsterData closestMonster = null;
        Transform closestTransform = null;

        Vector3 dragonPos = transform.position;

        // 모든 캐릭터를 순회하면서 몬스터만 필터링하여 처리
        foreach (var character in CharacterManager.Instance.CharacterList)
        {
            // 몬스터만 필터링
            if (character is MonsterData monster)
            {
                // 드래곤과 몬스터 간의 거리 계산
                float dist = Vector3.Distance(dragonPos, monster.instance.transform.position);

                // 탐지 범위 내에서 가장 가까운 몬스터를 찾음
                if (dist < closestDistance && dist <= detectRange)
                {
                    closestDistance = dist;
                    closestMonster = monster;
                    closestTransform = monster.instance.transform;
                }
            }
        }

        // 가장 가까운 몬스터를 타겟으로 설정
        if (closestMonster != null)
        {
            currentTarget = closestMonster;
            currentTargetTransform = closestTransform;
            Debug.Log($"타겟 몬스터: {closestMonster.characterName}");
        }
        else
        {
            currentTarget = null;
            currentTargetTransform = null;
            Debug.Log("주변에 타겟이 없습니다.");
        }
    }

    private void MoveTowardTarget(Transform targetTransform)
    {
        if (targetTransform == null) return;

        // 근접 범위에 들어가지 않았다면 이동 (단, “용은 공격받지 않는다” 가정이므로 충돌 처리X)
        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
        if (distanceToTarget > meleeRange)
        {
            // target 쪽으로 이동
            Vector3 dir = (targetTransform.position - transform.position).normalized;
            Vector3 movePos = transform.position + dir * (followSpeed * Time.deltaTime);
            // 살짝 높이를 유지(hoverHeight)하려면 아래처럼 Y값만 변경
            movePos.y = player.position.y + hoverHeight; 
            transform.position = movePos;

            // 회전
            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, lookRot, rotationSpeed * Time.deltaTime);

            // 애니메이터 상태도 갱신 가능
            animator.SetBool(IsMoving, true);
        }
        else
        {
            // 이미 충분히 가까우면 이동 중지
            animator.SetBool(IsMoving, false);
        }
    }

    #region 공격 메서드(스킬, 원거리, 근접)

    // 1) 스킬 공격 (우선순위 1위)
    private void UseSkillAttack()
    {
        Debug.Log("[드래곤] 스킬 공격 발사!");
        // 여기에 SkillManager를 통해 스킬 발사하는 로직 추가
        // 예) SkillManager.Instance.CastDragonSkill(someSkillId, currentTargetTransform);

        // 스킬 쿨다운 시작
        skillTimer = skillCooldown;
    }

    // 2) 원거리 공격(파이어볼) (우선순위 2위)
    private void UseRangedAttack()
    {
        if (fireballPrefab != null && currentTarget != null)
        {
            
            Transform targetPos = currentTargetTransform; // 타겟의 위치로 향하게 설정
            
            animator.SetTrigger("isRangedAttack");
            
            // 파이어볼 인스턴스화
            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);

            // 파이어볼에 대한 초기화
            FireballController fireballController = fireball.GetComponent<FireballController>();
            fireballController.Initialize(targetPos, dragonData, currentTarget);
            
            // 원거리 공격 쿨다운 시작
            rangedTimer = rangedCooldown;
        }
    } 
    
    // 3) 근접 공격 (우선순위 3위)
    private void UseMeleeAttack()
    {
        // 어택 스피드 기반 쿨타임 체크
        if (meleeCooldown <= 0f)
        {
            animator.SetTrigger("isMeleeAttack");
            
            // CombatManager 호출 등을 통해 실제 근접 공격 처리 (필요에 따라)
            CombatManager.Instance.ProcessDragonAttack(dragonData, currentTarget, currentTargetTransform, false);
            // 공격 후 쿨타임 갱신
            meleeCooldown = dragonData.attackSpeed;  // 다시 어택 스피드에 맞춰 쿨타임 설정
        }
    }
    #endregion

    // 쿨다운 시간 갱신
    private void UpdateCooldowns()
    {
        // 어택 스피드를 반영한 근접 공격 쿨타임 갱신
        if (meleeCooldown > 0f)
        {
            meleeCooldown -= Time.deltaTime;
            if (meleeCooldown < 0f) meleeCooldown = 0f;
        }

        // 기존의 스킬 및 원거리 공격 쿨타임도 갱신
        if (skillTimer > 0f)
        {
            skillTimer -= Time.deltaTime;
            if (skillTimer < 0f) skillTimer = 0f;
        }

        if (rangedTimer > 0f)
        {
            rangedTimer -= Time.deltaTime;
            if (rangedTimer < 0f) rangedTimer = 0f;
        }
    }

    // 스킬 쿨다운 여부
    private bool IsSkillOnCooldown() => (skillTimer > 0f);
    private bool IsRangedOnCooldown() => (rangedTimer > 0f);

    #endregion

    #region 플레이어 따라다니기 로직 (기존 로직 유지/보완)

    private void FollowPlayerLogic()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 순간이동 조건
        if (distanceToPlayer > teleportDistance)
        {
            // 플레이어 옆으로 순간이동
            transform.position = player.position + offset;
        }
        else
        {
            // 플레이어를 따라다니는 동작
            Vector3 targetPosition = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * followSpeed);

            // 회전
            RotateTowardsPlayer();
        }

        // 플레이어의 이동 상태 업데이트
        UpdateMovementState();
        // 블렌드 트리 파라미터 업데이트
        UpdateTurnDirection();
    }

    private void UpdateMovementState()
    {
        // 플레이어가 이동 중인지 확인
        bool playerIsMoving = Vector3.Distance(player.position, lastPlayerPosition) > 0.01f;

        // 상태가 변경되었을 경우 애니메이터 업데이트
        if (playerIsMoving != isMoving)
        {
            isMoving = playerIsMoving;
            animator.SetBool(IsMoving, isMoving);
        }

        // 플레이어의 현재 위치 저장
        lastPlayerPosition = player.position;
    }

    private void RotateTowardsPlayer()
    {
        // 플레이어가 바라보는 방향으로 용을 회전
        Quaternion targetRotation = player.rotation;
        // y축 회전만 반영
        targetRotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        // 부드럽게 회전
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void UpdateTurnDirection()
    {
        // 플레이어의 바라보는 방향
        Vector3 playerForward = player.forward;
        Vector3 dragonForward = transform.forward;

        // 좌/우 방향 차이 계산
        float turnDirectionValue = Vector3.SignedAngle(dragonForward, playerForward, Vector3.up) / 90f;
        turnDirectionValue = Mathf.Clamp(turnDirectionValue, -1f, 1f);

        // 애니메이터 파라미터 업데이트
        animator.SetFloat(TurnDirection, turnDirectionValue);
    }

    #endregion

    private void OnDrawGizmos()
    {
        // 이동 관련 기즈모 표시
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
            Gizmos.DrawWireSphere(player.position, teleportDistance);
        }

        // 전투 탐색 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }
}

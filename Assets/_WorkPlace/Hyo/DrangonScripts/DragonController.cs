using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DragonState
{
    Idle,
    Moving,
    MeleeAttack,
    RangedAttack,
    SkillAttack,
    BuffSkill
}

public class DragonController : MonoBehaviour
{
    private static readonly int TurnDirection = Animator.StringToHash("turnDirection");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int IsRangedAttack = Animator.StringToHash("isRangedAttack");
    private static readonly int IsMeleeAttack = Animator.StringToHash("isMeleeAttack");
    private static readonly int IsUseSkill = Animator.StringToHash("isUseSkill");

    // ===== 기존 이동/따라다니기 관련 필드 =====
    [SerializeField] private DragonData dragonData;         // 드래곤 데이터
    private Transform player;              // 따라다닐 플레이어
    private Animator animator;             // 용의 애니메이터
    [SerializeField] private Transform firePoint;           // 파이어 포인트
    private Vector3 lastPlayerPosition;    // 플레이어의 마지막 위치
    [SerializeField] private bool isMoving;                 // 현재 이동 중인지 상태

    [Header("Follow Settings")]
    public float followDistance = 3f;      // 플레이어와 유지할 거리
    public float teleportDistance = 10f;   // 순간이동할 거리
    public float followSpeed;         // 따라다니는 속도
    public float hoverHeight = 2f;         // 플레이어 위로 떠 있을 높이
    public float rotationSpeed = 5f;       // 회전 속도

    private Vector3 offset;                // 플레이어와의 상대 위치

    // ===== 전투 관련 필드 =====
    [Header("Combat Settings")]
    public float detectRange = 40f;        // 전투 시, 주변 몬스터 탐색 범위
    public float meleeRange;          // 근접 공격 범위
    public float maxDistanceFromPlayer = 15f; // 플레이어로부터 이만큼 멀어지면 타겟 해제 & 순간이동
    
    // 전투 종료 체크 중복 방지를 위한 플래그
    private bool isCheckingEndCombat = false;
    
    [SerializeField] private GameObject fireballPrefab; // 파이어볼 프리팹

    //쿨다운
    private BasicTimer meleeCooldown;  // 근접 공격 쿨타임
    [SerializeField] private BasicTimer skillCooldown = new BasicTimer(1f);                              // 스킬 쿨다운
    [SerializeField] private readonly BasicTimer rangedCooldown = new BasicTimer(10f);      // 원거리 공격(파이어볼) 쿨다운
    [SerializeField] private float rangedDelayTime = 1.5f;
    
    [SerializeField] private bool isAttacking = false;

    // 현재 타겟(몬스터)
    [SerializeField] private MonsterData currentTarget = null;
    [SerializeField] private Transform currentTargetTransform = null;

    // 드래곤의 상태를 나타내는 변수 추가
    [SerializeField] private DragonState currentState = DragonState.Idle;
    
    // ===== 진화 관련 추가 필드 =====
    [Header("Evolution Settings")]
    [SerializeField] private GameObject[] dragonModelPrefabs = new GameObject[3]; // 각 진화 단계별 모델 프리팹 
    [SerializeField] private Transform[] modelParents = new Transform[3]; // 각 진화 단계별 모델 위치 부모 (없다면 null)
    [SerializeField] private GameObject evolutionEffectPrefab; // 진화 시 재생할 이펙트 (옵션)
    
    // 현재 활성화된 모델 레퍼런스
    private GameObject currentModelInstance;
    
    // 진화 효과를 위한 머티리얼
    [SerializeField] private Material evolutionMaterial;
    private Material[] originalMaterials;

    private void OnEnable()
    {
        // 진화 이벤트 구독
        DragonData.OnDragonEvolution += HandleEvolution;
        
        // CharacterManager와 PlayerCharacterData가 초기화되었는지 확인
        if (CharacterManager.Instance == null || CharacterManager.PlayerCharacterData == null)
        {
            Debug.LogWarning("[DragonController] CharacterManager 또는 PlayerCharacterData가 초기화되지 않았습니다.");
            return;
        }

        // 플레이어의 원래 데미지 값 백업
        float originalPlayerPhysicalDamage = CharacterManager.PlayerCharacterData.physicalDamage;
        float originalPlayerMagicDamage = CharacterManager.PlayerCharacterData.magicDamage;
        float originalPlayerPhysicalMultiplier = CharacterManager.PlayerCharacterData.physicalDamageBuffMultiplier;
        float originalPlayerMagicMultiplier = CharacterManager.PlayerCharacterData.magicDamageBuffMultiplier;
        
        // 드래곤 Transform 설정
        GameManager.DragonTransform = transform;
        
        // 활성화 후 플레이어 스탯이 변경되었는지 확인
        StartCoroutine(CheckAndRestorePlayerStats(
            originalPlayerPhysicalDamage, 
            originalPlayerMagicDamage, 
            originalPlayerPhysicalMultiplier, 
            originalPlayerMagicMultiplier
        ));
    }
    
    private void OnDisable()
    {
        // 진화 이벤트 구독 해제
        DragonData.OnDragonEvolution -= HandleEvolution;
    }
    
    // 플레이어 스탯 확인 및 복원 코루틴
    private IEnumerator CheckAndRestorePlayerStats(
        float originalPhysicalDamage, 
        float originalMagicDamage, 
        float originalPhysicalMultiplier, 
        float originalMagicMultiplier)
    {
        // 1프레임 대기
        yield return null;
        
        // 플레이어 스탯이 변경되었는지 확인
        if (CharacterManager.PlayerCharacterData.physicalDamage != originalPhysicalDamage || 
            CharacterManager.PlayerCharacterData.magicDamage != originalMagicDamage ||
            CharacterManager.PlayerCharacterData.physicalDamageBuffMultiplier != originalPhysicalMultiplier ||
            CharacterManager.PlayerCharacterData.magicDamageBuffMultiplier != originalMagicMultiplier)
        {
            // 원래 값으로 복원
            CharacterManager.PlayerCharacterData.physicalDamage = originalPhysicalDamage;
            CharacterManager.PlayerCharacterData.magicDamage = originalMagicDamage;
            CharacterManager.PlayerCharacterData.physicalDamageBuffMultiplier = originalPhysicalMultiplier;
            CharacterManager.PlayerCharacterData.magicDamageBuffMultiplier = originalMagicMultiplier;
            
            // 플레이어 스탯 재계산
            CharacterManager.PlayerCharacterData.UpdateDerivedStats();
            
            Debug.LogWarning("드래곤 활성화 중 플레이어 스탯이 변경되어 원래 값으로 복원되었습니다.");
        }
    }

    private void Start()
    {
        // CharacterManager 확인
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("[DragonController] CharacterManager.Instance가 null입니다.");
            return;
        }

        // 기존 설정
        dragonData = CharacterManager.DragonData;

        // PlayerCharacterData 확인
        if (CharacterManager.PlayerCharacterData == null)
        {
            Debug.LogError("[DragonController] CharacterManager.PlayerCharacterData가 null입니다.");
            return;
        }

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
            meleeCooldown = new BasicTimer(dragonData.attackSpeed);  // 어택 스피드를 기준으로 쿨타임 계산 (초단위)
        }
        else
        {
            meleeCooldown = new BasicTimer(3f);  // 기본 쿨타임 설정 (어택 스피드가 없을 경우)
        }

        // 플레이어와 초기 상대 위치 설정
        offset = new Vector3(0, hoverHeight, -followDistance);
        lastPlayerPosition = player.position; // 초기 위치 저장
        meleeRange = dragonData.attackRange;
        followSpeed = dragonData.speed;
        
        // 진화 단계에 맞는 모델 적용
        UpdateDragonModel();
        
        // 드래곤 데이터의 프리팹 배열에 모델 프리팹들 설정
        if (dragonData != null && dragonModelPrefabs != null && dragonModelPrefabs.Length > 0)
        {
            // 중요: 게임 시작 시 DragonData.evolutionPrefabs가 설정되어 있지 않다면 초기화
            if (dragonData.evolutionPrefabs == null || dragonData.evolutionPrefabs.Length != dragonModelPrefabs.Length)
            {
                dragonData.evolutionPrefabs = new GameObject[dragonModelPrefabs.Length];
                for (int i = 0; i < dragonModelPrefabs.Length; i++)
                {
                    dragonData.evolutionPrefabs[i] = dragonModelPrefabs[i];
                }
            }
        }
    }

    private void Update()
    {
        // 전투 상태일 때만 몬스터를 추적
        if (GameStateMachine.Instance.CurrentState == GameSystemState.Combat || GameStateMachine.Instance.CurrentState == GameSystemState.BossBattle )
        {
            HandleCombatLogic();
            
            // [추가] 전투 상태에서 주변에 적이 하나도 없다면, 전투 종료 후보
            if (!IsEnemyInRange(detectRange) && !isCheckingEndCombat)
            {
                // 1초(또는 2초) 후에 다시 확인 → 진짜 없으면 Exploration 전환
                StartCoroutine(CheckEndCombatCoroutine(1f));
            }
        }
        else
        {
            // 전투 상태가 아닐 때는 플레이어를 따라감
            currentTarget = null;
            currentTargetTransform = null;
            FollowPlayerLogic();
        }
    }
    
    // [1] 주변 몬스터가 있는지(= 전투가 계속될 가능성이 있는지) 확인하는 함수
    private bool IsEnemyInRange(float range)
    {
        foreach (var character in CharacterManager.Instance.CharacterList)
        {
            if (character is MonsterData monster && monster.currentHp > 0)
            {
                float dist = Vector3.Distance(transform.position, monster.instance.transform.position);
                if (dist <= range)
                {
                    return true; // 탐지 범위 내 살아있는 몬스터 발견
                }
            }
        }
        return false; // 범위 내 적 없음
    }
    
    // [2] 일정 시간 후 다시 확인해서, 여전히 적이 없다면 전투 종료
    private IEnumerator CheckEndCombatCoroutine(float delay)
    {
        isCheckingEndCombat = true;

        // 지연시간 (1~2초 권장)
        yield return new WaitForSeconds(delay);

        // 아직도 탐지 범위 내 적이 없고, 게임 상태가 Combat이면 Exploration으로 전환
        if (!IsEnemyInRange(detectRange) && (GameStateMachine.Instance.CurrentState == GameSystemState.Combat || GameStateMachine.Instance.CurrentState == GameSystemState.BossBattle))
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
        }

        // 체크 완료
        isCheckingEndCombat = false;
    }

    #region 전투 로직

    // 전투 상황에서 실행되는 메인 로직
    private void HandleCombatLogic()
    {
        if (isAttacking) return; 
        
        // 플레이어와 너무 멀어졌으면 타겟 해제 후 Idle 로
        float distanceFromPlayer = Vector3.Distance(transform.position, player.position);
        float distanceFromTarget = currentTargetTransform != null ? Vector3.Distance(transform.position, currentTargetTransform.position) : 0f ;
        if (distanceFromPlayer > maxDistanceFromPlayer)
        {
            currentTarget = null;
            currentTargetTransform = null;
            currentState = DragonState.Idle;
            FollowPlayerLogic();
            return;
        }

        // 현재 타겟이 없거나 타겟이 사망했다면 새로운 타겟을 찾는다
        if (currentTarget == null || currentTarget.currentHp <= 0)
        {
            currentTarget = null;
            currentTargetTransform = null;
        
            // 새로운 타겟 탐색
            FindNearestTarget();

            // 탐색 후에도 없으면 플레이어 따라감 (Idle 상태)
            if (currentTarget == null)
            {
                currentState = DragonState.Idle;
                FollowPlayerLogic();
                return;
            }
        }

        // 공격 우선순위 1) 스킬, 2) 원거리 공격(파이어볼), 3) 근접 공격
        if (!skillCooldown.IsRunning)
        {
            if (currentState == DragonState.Moving || currentState == DragonState.Idle)
            {
                currentState = DragonState.SkillAttack;
                UseSkillAttack();
            }
        }
        else if (!rangedCooldown.IsRunning && distanceFromTarget < teleportDistance)
        {
            if (currentState == DragonState.Moving || currentState == DragonState.Idle)
            {
                currentState = DragonState.RangedAttack;
                StartCoroutine(UseRangedAttack());
            }
        }
        /// //////////////////////////////////////////////////////////
        /// 2025.01.10 JWS  시연을 위해 잠시 막아둠.
        //else if (!meleeCooldown.IsRunning)
        //{
        //    if (currentState == DragonState.Moving || currentState == DragonState.Idle)
        //    {
        //        currentState = DragonState.MeleeAttack;

        //        // 타겟 위치로 이동(근접 공격 등을 위해)
        //        MoveTowardTarget(currentTargetTransform);
        //    }
        //}
        /// //////////////////////////////////////////////////////////
    }

    // 주변에서 가장 가까운 몬스터를 찾아서 타겟 설정
    private void FindNearestTarget()
    {
        float closestDistance = float.MaxValue;
        MonsterData closestMonster = null;
        Transform closestTransform = null;

        Vector3 dragonPos = transform.position;

        // 모든 캐릭터를 순회하면서 몬스터와 보스를 필터링하여 처리
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
            else if (character is BossData boss)
            {
                // 보스일 경우 BossData를 MonsterData로 캐스팅
                MonsterData bossMonster = boss as MonsterData;
                if (bossMonster != null)
                {
                    // 보스와의 거리 계산
                    float dist = Vector3.Distance(dragonPos, boss.instance.transform.position);

                    // 탐지 범위 내에서 가장 가까운 보스를 찾음
                    if (dist < closestDistance && dist <= detectRange)
                    {
                        closestDistance = dist;
                        closestMonster = bossMonster; // 캐스팅된 몬스터 데이터 사용
                        closestTransform = boss.instance.transform;
                    }
                }
            }
        }

        // 가장 가까운 몬스터 또는 보스를 타겟으로 설정
        if (closestMonster != null)
        {
            currentTarget = closestMonster;
            currentTargetTransform = closestTransform;
        }
        else
        {
            currentTarget = null;
            currentTargetTransform = null;
        }
    }

    
    private void MoveTowardTarget(Transform targetTransform)
    {
        if (!targetTransform) return;

        // 근접 범위에 들어가지 않았다면 이동 (단, "용은 공격받지 않는다" 가정이므로 충돌 처리X)
        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
        if (distanceToTarget > meleeRange)
        {
            currentState = DragonState.Moving;
        
            // target 쪽으로 이동
            Vector3 dir = (targetTransform.position - transform.position).normalized;
            Vector3 movePos = transform.position + dir * (followSpeed * Time.deltaTime);

            // Y축을 고정하고, 플레이어의 Y축 + hoverHeight를 유지하도록 설정
            movePos.y = transform.position.y; // 기존 높이를 유지

            transform.position = movePos;

            // 회전 (Y축만 회전하도록 수정)
            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
    
            // Y축 회전만 적용하고, 나머지 X, Z 회전은 그대로 유지
            transform.rotation = Quaternion.Euler(0f, lookRot.eulerAngles.y, 0f);

            // 애니메이터 상태도 갱신 가능
            animator.SetBool(IsMoving, true);
        }
        else
        {
            // 이미 충분히 가까우면 공격
            animator.SetBool(IsMoving, false);
        
            // 밀리 어택 상태로 진입하고, 공격을 수행
            if (!meleeCooldown.IsRunning)
            {
                currentState = DragonState.MeleeAttack;
                UseMeleeAttack();
            }
            else
            {
                currentState = DragonState.Idle;
            }
        }
    }
    
    #region 공격 메서드(스킬, 원거리, 근접)

    // 1) 스킬 공격 (우선순위 1위)
    private void UseSkillAttack()
    {
        if (!skillCooldown.IsRunning)
        {
            isAttacking = true;
            
            List<string> availableBuffs = SkillManager.Instance.GetAvailableBuffs(EntityType.Dragon);
            if (availableBuffs.Count == 0)
            {
                Debug.Log($"[드래곤] 사용 가능한 버프 스킬 목록: {string.Join(", ", availableBuffs)}");
                return;
            }
            
            string chosenBuff = availableBuffs[Random.Range(0, availableBuffs.Count)];
            
            Debug.Log("[드래곤] 스킬 공격 발사! 선택된 버프 스킬: " + chosenBuff);
            
            SkillManager.Instance.ApplyBuff(EntityType.Dragon, chosenBuff);
            
            float chosenBuffCooldown = SkillManager.Instance.GetCooldownForSkill(EntityType.Dragon, chosenBuff);

            // 스킬 쿨다운 시작
            skillCooldown = new BasicTimer(chosenBuffCooldown);
            TimerManager.Instance.StartTimer(skillCooldown);    // 스킬 쿨다운에 맞도록 사용

            isAttacking = false;
            currentState = DragonState.Idle;
        }
    }

    // 2) 원거리 공격(파이어볼) (우선순위 2위)
    private IEnumerator UseRangedAttack()
    {
        if (fireballPrefab != null && currentTarget != null && !rangedCooldown.IsRunning)
        {
            isAttacking = true; // 공격 진행 중 플래그 설정

            // 타겟 위치를 콜라이더의 정중앙으로 설정
            Vector3 targetCenter = GetTargetCenter(currentTargetTransform);

            animator.SetTrigger(IsRangedAttack);

            // 파이어볼 인스턴스화
            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);

            // 파이어볼 초기화 (중앙 좌표 + 타겟 Transform 전달)
            FireballController fireballController = fireball.GetComponent<FireballController>();
            fireballController.Initialize(targetCenter, dragonData, this.transform.position, currentTarget, currentTargetTransform);

            // 원거리 공격 쿨다운 시작
            TimerManager.Instance.StartTimer(rangedCooldown);

            yield return new WaitForSeconds(rangedDelayTime);

            // 딜레이 후 상태를 Idle로 전환 및 공격 진행 플래그 해제
            isAttacking = false;
            currentState = DragonState.Idle;
        }
    }

    
    // 3) 근접 공격 (우선순위 3위)
    private void UseMeleeAttack()
    {
        if (!meleeCooldown.IsRunning)
        {
            isAttacking = true; // 공격 진행 중 플래그 설정

            // Debug.Log("Melee Attack Executed!");
            animator.SetTrigger(IsMeleeAttack);

            // CombatManager 호출 등을 통해 실제 근접 공격 처리 (필요에 따라)
            CombatManager.Instance.ProcessDragonAttack(dragonData, currentTarget, currentTargetTransform, false);

            // 공격 후 쿨타임 갱신
            TimerManager.Instance.StartTimer(meleeCooldown);

            isAttacking = false;
            currentState = DragonState.Idle;
        }
    }
    
    // 타겟의 콜라이더 정중앙 위치를 반환하는 함수
    private Vector3 GetTargetCenter(Transform targetTransform)
    {
        CharacterController targetController = targetTransform.GetComponent<CharacterController>();
        if (targetController != null)
        {
            return targetController.bounds.center; // 정중앙 위치 반환
        }
        return targetTransform.position; // 기본 위치 반환 (콜라이더가 없을 경우)
    }
    
    #endregion
    
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
            isMoving = false;
        }
        else
        {
            // 플레이어를 따라다니는 동작
            Vector3 targetPosition = player.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * (distanceToPlayer > teleportDistance ? followSpeed * 10f : followSpeed));

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

            // 상태 변경
            currentState = isMoving ? DragonState.Moving : DragonState.Idle;
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
        }

        // 전투 탐색 범위 표시
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
    }

    // 진화 단계에 따른 모델 업데이트
    private void UpdateDragonModel()
    {
        if (dragonData == null) return;
        
        // 현재 활성화된 모델이 있다면 비활성화
        if (currentModelInstance != null)
        {
            Destroy(currentModelInstance);
            currentModelInstance = null;
        }
        
        // 현재 진화 단계에 맞는 모델 인스턴스화
        int stageIndex = (int)dragonData.evolutionStage;
        if (dragonModelPrefabs != null && dragonModelPrefabs.Length > stageIndex && dragonModelPrefabs[stageIndex] != null)
        {
            // 프리팹 로그 출력
            Debug.Log($"현재 진화 단계({dragonData.evolutionStage})의 프리팹: {dragonModelPrefabs[stageIndex].name}");
            
            // 부모 트랜스폼 결정
            Transform parentTransform = transform;
            if (modelParents != null && modelParents.Length > stageIndex && modelParents[stageIndex] != null)
            {
                parentTransform = modelParents[stageIndex];
            }
            
            // 모델 인스턴스화
            currentModelInstance = Instantiate(dragonModelPrefabs[stageIndex], parentTransform.position, parentTransform.rotation, parentTransform);
            
            // 새 모델에서 애니메이터 찾거나 연결
            Animator modelAnimator = currentModelInstance.GetComponent<Animator>();
            if (modelAnimator != null)
            {
                // 새 애니메이터로 업데이트
                animator = modelAnimator;
            }
            
            // FirePoint를 찾기 위한 개선된 로직
            FindAndSetupFirePoint();
            
            // 진화 단계에 따른 추가 조정 (크기 등)
            switch (dragonData.evolutionStage)
            {
                case DragonEvolutionStage.Baby:
                    currentModelInstance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    break;
                    
                case DragonEvolutionStage.Young:
                    currentModelInstance.transform.localScale = new Vector3(2f, 2f, 2f);
                    break;
                    
                case DragonEvolutionStage.Adult:
                    currentModelInstance.transform.localScale = new Vector3(4f, 4f, 4f);
                    break;
            }
        }
        else
        {
            Debug.LogError($"드래곤 모델 프리팹이 설정되지 않았습니다. 단계: {dragonData.evolutionStage}");
        }
    }
    
    // FirePoint를 찾거나 생성하는 기능 추가
    private void FindAndSetupFirePoint()
    {
        if (currentModelInstance == null) return;
        
        // 1. 먼저 직접적인 자식에서 찾기
        Transform foundFirePoint = currentModelInstance.transform.Find("FirePoint");
        
        // 2. 찾지 못했다면 재귀적으로 모든 자식에서 찾기
        if (foundFirePoint == null)
        {
            foundFirePoint = FindFirePointRecursively(currentModelInstance.transform);
        }
        
        // 3. 찾지 못했다면 Head나 머리 관련 트랜스폼에서 찾기
        if (foundFirePoint == null)
        {
            foundFirePoint = FindHeadTransform(currentModelInstance.transform);
        }
        
        // 4. 그래도 찾지 못했다면 새로 생성
        if (foundFirePoint == null)
        {
            foundFirePoint = CreateNewFirePoint();
        }
        
        // 찾았거나 새로 만든 FirePoint 적용
        if (foundFirePoint != null)
        {
            firePoint = foundFirePoint;
            Debug.Log($"FirePoint 설정 완료: {firePoint.name}");
        }
        else
        {
            Debug.LogWarning("FirePoint를 찾거나 생성하지 못했습니다.");
        }
    }

    // 재귀적으로 특정 키워드가 포함된 트랜스폼 찾는 함수
    private Transform FindTransformByKeywords(Transform parent, string[] keywords)
    {
        foreach (Transform child in parent)
        {
            // 이름에 키워드가 포함된 경우
            string childNameLower = child.name.ToLower();
            foreach (string keyword in keywords)
            {
                if (childNameLower.Contains(keyword))
                {
                    return child;
                }
            }
            
            // 자식의 자식에서 재귀적으로 찾기
            Transform found = FindTransformByKeywords(child, keywords);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    // 재귀적으로 FirePoint를 찾는 함수
    private Transform FindFirePointRecursively(Transform parent)
    {
        string[] fireKeywords = { "fire", "mouth", "point" };
        return FindTransformByKeywords(parent, fireKeywords);
    }

    // Head나 머리 관련 트랜스폼 찾기
    private Transform FindHeadTransform(Transform parent)
    {
        string[] headKeywords = { "head", "neck", "mouth", "jaw" };
        return FindTransformByKeywords(parent, headKeywords);
    }

    // 새로운 FirePoint 생성
    private Transform CreateNewFirePoint()
    {
        // 모델의 앞쪽에 FirePoint 생성
        GameObject newFirePoint = new GameObject("FirePoint");
        newFirePoint.transform.SetParent(currentModelInstance.transform);
        newFirePoint.transform.localPosition = new Vector3(0, 0.5f, 1f); // 대략적인 머리 위치
        Debug.Log("새로운 FirePoint 생성됨");
        return newFirePoint.transform;
    }

    // 진화 이벤트 핸들러
    private void HandleEvolution(DragonEvolutionStage newStage)
    {
        if (dragonData == null) return;
        
        // 모델 변경 전에 진화 이펙트 재생
        StartCoroutine(PlayEvolutionEffect());
    }
    
    // 진화 이펙트 재생 코루틴
    private IEnumerator PlayEvolutionEffect()
    {
        if (currentModelInstance == null) yield break;
        
        // 원본 머티리얼 저장
        Renderer[] renderers = currentModelInstance.GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        
        // 진화 머티리얼로 교체 (있다면)
        if (evolutionMaterial != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].material;
                renderers[i].material = evolutionMaterial;
            }
        }
        
        // 진화 이펙트 재생 (있다면)
        if (evolutionEffectPrefab != null)
        {
            GameObject effect = Instantiate(evolutionEffectPrefab, currentModelInstance.transform.position, Quaternion.identity);
            Destroy(effect, 3f); // 3초 후 이펙트 제거
        }
        
        yield return new WaitForSeconds(1.5f);
        
        // 모델 업데이트
        UpdateDragonModel();
        
        // 레벨업 사운드 재생 (예시)
        // AudioManager.Instance.PlaySFX("DragonEvolve");
        
        // 화면 효과 (필요하다면)
        // UIManager.Instance.ShowEvolutionEffect();
        
        Debug.Log($"드래곤이 {dragonData.evolutionStage} 단계로 진화했습니다!");
    }
}

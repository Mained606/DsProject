using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum DragonState
{
    // 이 enum은 레거시 호환성을 위해 유지됩니다. 
    // 실제 상태 관리는 DragonStateMachine을 통해 이루어집니다.
    Idle,
    Moving,
    MeleeAttack,
    RangedAttack,
    SkillAttack,
    UltimateAttack
}

public class DragonController : MonoBehaviour
{
    // 싱글톤 인스턴스
    public static DragonController Instance { get; private set; }
    
    #region 애니메이터 해시
    public static readonly int TurnDirection = Animator.StringToHash("turnDirection");
    public static readonly int IsMoving = Animator.StringToHash("isMoving");
    public static readonly int IsRangedAttack = Animator.StringToHash("isRangedAttack");
    public static readonly int IsMeleeAttack = Animator.StringToHash("isMeleeAttack");
    public static readonly int IsUseSkill = Animator.StringToHash("isSkillAttack");
    public static readonly int IsUltimate = Animator.StringToHash("isUltimate");
    #endregion

    #region 기본 필드
    [SerializeField] private DragonData dragonData;
    private Transform player;
    private Animator animator;
    [SerializeField] private Transform firePoint;
    private Vector3 lastPlayerPosition;
    [SerializeField] private bool isMoving;
    
    // FSM 관련 필드
    private DragonStateMachine stateMachine;
    
    // 이 변수는 FSM으로 전환 후 사용하지 않지만, 인스펙터 표시를 위해 남겨둡니다
    [HideInInspector] [SerializeField] private DragonState legacyState = DragonState.Idle;
    #endregion

    #region 이동 설정
    [Header("이동 설정")]
    public float followDistance = 3f;
    public float teleportDistance = 20f;
    public float followSpeed;
    public float hoverHeight = 2f;
    public float rotationSpeed = 5f;
    public float groundCheckDistance = 1f;
    private Vector3 offset;
    private float lastGroundHeight; // 마지막 프레임의 지면 높이
    private float heightSmoothVelocity; // 높이 보간용 속도 변수
    private bool isTeleporting = false; // 텔레포트 중인지 추적하는 변수
    private float teleportCooldown = 0f; // 텔레포트 쿨다운 시간
    #endregion

    #region 전투 설정
    [Header("전투 설정")]
    public float detectRange = 40f;
    public float meleeRange;
    [Tooltip("몬스터 크기에 따른 추가 공격 범위 (음수 값은 더 가까이 접근)")]
    [SerializeField] private float additionalRangeForSize = -0.8f;
    public float maxDistanceFromPlayer = 15f;
    private bool isCheckingEndCombat = false;
    [SerializeField] private GameObject fireballPrefab;
    
    // 성능 최적화를 위한 캐시 변수 추가
    private Dictionary<Transform, float> targetRadiusCache = new Dictionary<Transform, float>();
    private float dragonBodyRadius = 0f;
    private float lastDragonBodyRadiusUpdate = 0f;
    private float dragonBodyRadiusUpdateInterval = 5f; // 5초마다 업데이트
    #endregion

    #region 쿨다운 및 타겟
    private BasicTimer meleeCooldown;
    [SerializeField] private BasicTimer skillCooldown = new BasicTimer(10f);
    [SerializeField] private BasicTimer rangedCooldown = new BasicTimer(8f);
    [SerializeField] private BasicTimer ultimateCooldown = new BasicTimer(60f);
    [SerializeField] private float rangedDelayTime = 1.5f;
    [Header("드래곤 스킬 설정")]
    [Tooltip("드래곤이 몇 초마다 한 번씩 버프 스킬을 사용할지 설정합니다")]
    [SerializeField] private float dragonSkillCooldown = 20f; // 드래곤 스킬 전체 공통 쿨다운
    [SerializeField] private bool isAttacking = false;
    [SerializeField] private MonsterData currentTarget = null;
    [SerializeField] private Transform currentTargetTransform = null;
    #endregion

    #region 스킬 설정
    [Header("스킬 설정")]
    [Tooltip("드래곤이 사용할 버프 스킬 이름 목록")]
    [SerializeField] private string[] buffSkills = new string[] { "PlayerBuffHP", "PlayerBuffPhysical", "PlayerBuffMagic" };
    #endregion

    #region 진화 설정
    [Header("진화 설정")]
    [SerializeField] private GameObject[] dragonModelPrefabs = new GameObject[3];
    [SerializeField] private Transform[] modelParents = new Transform[3];
    [SerializeField] private GameObject evolutionEffectPrefab;
    private GameObject currentModelInstance;
    [SerializeField] private Material evolutionMaterial;
    private Material[] originalMaterials;
    #endregion

    #region 공개 프로퍼티 (FSM용)
    // FSM에 필요한 정보를 접근할 수 있는 프로퍼티들
    public float DetectRange => detectRange;
    public bool IsAttacking => isAttacking;
    public bool HasTarget => currentTarget != null && currentTargetTransform != null;
    public bool IsMeleeCooldown => meleeCooldown.IsRunning;
    public bool IsRangedCooldown => rangedCooldown.IsRunning;
    public bool IsSkillCooldown => skillCooldown.IsRunning;
    public bool IsUltimateCooldown => ultimateCooldown.IsRunning;
    
    // 추가 퍼블릭 프로퍼티
    public DragonData DragonData => dragonData;
    public MonsterData CurrentTarget => currentTarget;
    public Transform CurrentTargetTransform => currentTargetTransform;
    #endregion

    #region Unity 라이프사이클
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    private void OnEnable()
    {
        // 진화 이벤트 구독
        DragonData.OnDragonEvolution += HandleEvolution;
        
        // 플레이어 스탯 백업 및 드래곤 Transform 설정
        if (CharacterManager.Instance != null && CharacterManager.PlayerCharacterData != null)
        {
            float originalPlayerPhysicalDamage = CharacterManager.PlayerCharacterData.physicalDamage;
            float originalPlayerMagicDamage = CharacterManager.PlayerCharacterData.magicDamage;
            float originalPlayerPhysicalMultiplier = CharacterManager.PlayerCharacterData.physicalDamageBuffMultiplier;
            float originalPlayerMagicMultiplier = CharacterManager.PlayerCharacterData.magicDamageBuffMultiplier;
            
            GameManager.DragonTransform = transform;
            
            StartCoroutine(CheckAndRestorePlayerStats(
                originalPlayerPhysicalDamage, 
                originalPlayerMagicDamage, 
                originalPlayerPhysicalMultiplier, 
                originalPlayerMagicMultiplier
            ));
        }
        else
        {
            Debug.LogWarning("[DragonController] CharacterManager 또는 PlayerCharacterData가 초기화되지 않았습니다.");
        }
    }
    
    private void OnDisable()
    {
        DragonData.OnDragonEvolution -= HandleEvolution;
    }

    private void Start()
    {
        // 초기화 검증
        if (CharacterManager.Instance == null)
        {
            Debug.LogError("[DragonController] CharacterManager.Instance가 null입니다.");
            return;
        }

        dragonData = CharacterManager.DragonData;
        player = GameManager.playerTransform;
        animator = GetComponent<Animator>();

        if (dragonData == null || player == null || animator == null)
        {
            Debug.LogError("[DragonController] 필수 컴포넌트가 없습니다.");
            return;
        }
        
        // Awake에서 초기화하지 못했다면 여기서 다시 시도
        if (SkillManager.Instance != null)
        {
            InitializeBuffCooldowns();
        }
        
        // 쿨다운 초기화
        meleeCooldown = new BasicTimer(dragonData.attackSpeed);
        
        // 진화 단계에 따른 followDistance 설정
        UpdateDragonSettingsByEvolutionStage();
        
        // 오프셋 및 데이터 설정
        UpdateOffset();
        lastPlayerPosition = player.position;
        lastGroundHeight = transform.position.y; // 초기 높이 설정
        followSpeed = dragonData.speed;
        
        // 진화 단계에 맞는 모델 적용
        UpdateDragonModel();
        
        // 드래곤 데이터 프리팹 설정
        SetupDragonDataPrefabs();
        
        // FSM 초기화 - 마지막에 실행하여 모든 필요한 컴포넌트와 변수가 초기화된 후 상태 설정
        stateMachine = new DragonStateMachine(this, animator);
        stateMachine.SetState<DragonIdleState>();
    }

    private void Update()
    {
        // 텔레포트 쿨다운 감소
        if (teleportCooldown > 0)
        {
            teleportCooldown -= Time.deltaTime;
        }
        
        // FSM 상태 업데이트
        stateMachine.UpdateState();
        
        // 현재 FSM 상태에 따라 legacyState 업데이트 (디버깅용)
        UpdateLegacyStateForDebug();
        
        // 궁극기 입력 감지 (예: Z키)
        if (Input.GetKeyDown(KeyCode.Z) && !ultimateCooldown.IsRunning)
        {
            stateMachine.SetState<DragonUltimateAttackState>();
        }
    }

    // 디버깅을 위해 현재 FSM 상태에 따라 legacyState 업데이트
    private void UpdateLegacyStateForDebug()
    {
        if (stateMachine.CurrentState is DragonIdleState) legacyState = DragonState.Idle;
        else if (stateMachine.CurrentState is DragonMovingState) legacyState = DragonState.Moving;
        else if (stateMachine.CurrentState is DragonMeleeAttackState) legacyState = DragonState.MeleeAttack;
        else if (stateMachine.CurrentState is DragonRangedAttackState) legacyState = DragonState.RangedAttack;
        else if (stateMachine.CurrentState is DragonSkillAttackState) legacyState = DragonState.SkillAttack;
        else if (stateMachine.CurrentState is DragonUltimateAttackState) legacyState = DragonState.UltimateAttack;
    }
    #endregion

    #region 초기화 메서드
    private void InitializeBuffCooldowns()
    {
        // SkillManager가 초기화되지 않았다면 안전하게 리턴
        if (SkillManager.Instance == null)
        {
            Debug.LogWarning("[DragonController] SkillManager.Instance가 null입니다. 버프 초기화는 Start에서 다시 시도합니다.");
            return;
        }
        
        // SkillManager에 드래곤 버프 스킬 등록
        foreach (string buffName in buffSkills)
        {
            SkillManager.Instance.RegisterDragonBuff(buffName);
        }
    }
    
    private void SetupDragonDataPrefabs()
    {
        if (dragonData != null && dragonModelPrefabs != null && dragonModelPrefabs.Length > 0)
        {
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
    #endregion

    #region 플레이어 스탯 관리
    private IEnumerator CheckAndRestorePlayerStats(
        float originalPhysicalDamage, 
        float originalMagicDamage, 
        float originalPhysicalMultiplier, 
        float originalMagicMultiplier)
    {
        yield return null;
        
        if (CharacterManager.PlayerCharacterData.physicalDamage != originalPhysicalDamage || 
            CharacterManager.PlayerCharacterData.magicDamage != originalMagicDamage ||
            CharacterManager.PlayerCharacterData.physicalDamageBuffMultiplier != originalPhysicalMultiplier ||
            CharacterManager.PlayerCharacterData.magicDamageBuffMultiplier != originalMagicMultiplier)
        {
            CharacterManager.PlayerCharacterData.physicalDamage = originalPhysicalDamage;
            CharacterManager.PlayerCharacterData.magicDamage = originalMagicDamage;
            CharacterManager.PlayerCharacterData.physicalDamageBuffMultiplier = originalPhysicalMultiplier;
            CharacterManager.PlayerCharacterData.magicDamageBuffMultiplier = originalMagicMultiplier;
            
            CharacterManager.PlayerCharacterData.UpdateDerivedStats();
            
            Debug.LogWarning("드래곤 활성화 중 플레이어 스탯이 변경되어 원래 값으로 복원되었습니다.");
        }
    }
    #endregion

    #region 이동 로직
    private void FollowPlayerLogic()
    {
        FollowPlayer();
    }

    private float GetGroundHeight(Vector3 position)
    {
        RaycastHit hit;
        // 더 높은 위치에서 레이캐스트 시작
        Vector3 rayOrigin = new Vector3(position.x, position.y + 50f, position.z);
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 150f, LayerMask.GetMask("Terrain", "Ground")))
        {
            // 지형이 경사면인 경우를 고려하여 드래곤 단계에 따른 추가 여유 높이 적용
            float terrainInclinationBuffer = Vector3.Angle(hit.normal, Vector3.up) * 0.05f;
            float rawHeight = hit.point.y + hoverHeight + terrainInclinationBuffer;
            
            // 갑작스러운 높이 변화를 완화하기 위해 이전 높이값과 보간
            float targetHeight = Mathf.SmoothDamp(
                lastGroundHeight, 
                rawHeight, 
                ref heightSmoothVelocity, 
                0.3f  // 값이 작을수록 더 빠르게 목표 높이에 도달
            );
            
            // 높이 변화가 너무 큰 경우 제한 (갑작스러운 언덕이나 절벽 방지)
            float maxHeightChange = 2.0f; // 한 번에 변경될 수 있는 최대 높이 차이
            targetHeight = Mathf.Clamp(
                targetHeight, 
                lastGroundHeight - maxHeightChange, 
                lastGroundHeight + maxHeightChange
            );
            
            lastGroundHeight = targetHeight; // 다음 프레임을 위해 현재 높이 저장
            return targetHeight;
        }
        
        return lastGroundHeight; // 레이캐스트 실패 시 이전 높이 유지
    }

    private void UpdateMovementState()
    {
        bool playerIsMoving = Vector3.Distance(player.position, lastPlayerPosition) > 0.01f;

        if (playerIsMoving != isMoving)
        {
            isMoving = playerIsMoving;
            animator.SetBool(IsMoving, isMoving);
        }

        lastPlayerPosition = player.position;
    }

    private void RotateTowardsPlayer()
    {
        Quaternion targetRotation = Quaternion.Euler(0, player.rotation.eulerAngles.y, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private void UpdateTurnDirection()
    {
        Vector3 playerForward = player.forward;
        Vector3 dragonForward = transform.forward;
        float turnDirectionValue = Vector3.SignedAngle(dragonForward, playerForward, Vector3.up) / 90f;
        turnDirectionValue = Mathf.Clamp(turnDirectionValue, -1f, 1f);
        animator.SetFloat(TurnDirection, turnDirectionValue);
    }

    // 텔레포트 상태 초기화를 위한 코루틴
    private IEnumerator ResetTeleportState(float delay)
    {
        yield return new WaitForSeconds(delay);
        isTeleporting = false;
    }
    #endregion

    #region 전투 로직
    private bool IsEnemyInRange(float range)
    {
        foreach (var character in CharacterManager.Instance.CharacterList)
        {
            if (character is MonsterData monster && monster.currentHp > 0)
            {
                float dist = Vector3.Distance(transform.position, monster.instance.transform.position);
                if (dist <= range)
                {
                    return true;
                }
            }
        }
        return false;
    }

    // 전투 종료 확인 코루틴
    public IEnumerator CheckEndCombatCoroutine(float delay)
    {
        isCheckingEndCombat = true;
        yield return new WaitForSeconds(delay);

        if (!IsEnemyInRange(detectRange) && 
           (GameStateMachine.Instance.CurrentState == GameSystemState.Combat || 
            GameStateMachine.Instance.CurrentState == GameSystemState.BossBattle))
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
            
            // 전투 종료 시 상태를 Idle로 설정
            stateMachine.SetState<DragonIdleState>();
        }

        isCheckingEndCombat = false;
    }

    // 전투 모드 시작 시 호출될 수 있는 메서드
    public void StartCombatMode()
    {
        // 이미 전투 체크 중이면 무시
        if (isCheckingEndCombat) return;
        
        // 타겟 찾기 시도
        FindNearestTarget();
        
        // 상태에 따라 적절한 FSM 상태로 전환
        if (HasTarget)
        {
            // 거리에 따라 상태 결정
            if (IsTargetInMeleeRange() && !IsMeleeCooldown)
            {
                stateMachine.SetState<DragonMeleeAttackState>();
            }
            else
            {
                stateMachine.SetState<DragonMovingState>();
            }
        }
    }

    /// <summary>
    /// 가장 가까운 타겟 찾기
    /// </summary>
    public void FindNearestTarget()
    {
        float closestDistance = float.MaxValue;
        MonsterData closestMonster = null;
        Transform closestTransform = null;
        Vector3 dragonPos = transform.position;

        foreach (var character in CharacterManager.Instance.CharacterList)
        {
            if (character is MonsterData monster && monster.currentHp > 0)
            {
                float dist = Vector3.Distance(dragonPos, monster.instance.transform.position);
                if (dist < closestDistance && dist <= detectRange)
                {
                    closestDistance = dist;
                    closestMonster = monster;
                    closestTransform = monster.instance.transform;
                }
            }
            else if (character is BossData boss && boss.currentHp > 0)
            {
                MonsterData bossMonster = boss as MonsterData;
                if (bossMonster != null)
                {
                    float dist = Vector3.Distance(dragonPos, boss.instance.transform.position);
                    if (dist < closestDistance && dist <= detectRange)
                    {
                        closestDistance = dist;
                        closestMonster = bossMonster;
                        closestTransform = boss.instance.transform;
                    }
                }
            }
        }

        currentTarget = closestMonster;
        currentTargetTransform = closestTransform;
    }

    private float GetEffectiveDistanceToTarget(Transform targetTransform)
    {
        if (targetTransform == null) return float.MaxValue;
        
        // 타겟의 중심점 가져오기
        Vector3 targetCenter = GetTargetCenter(targetTransform);
        
        // 드래곤 모델 크기를 주기적으로 업데이트 (매 프레임 계산하지 않음)
        if (Time.time - lastDragonBodyRadiusUpdate > dragonBodyRadiusUpdateInterval)
        {
            UpdateDragonBodyRadius();
            lastDragonBodyRadiusUpdate = Time.time;
        }
        
        // 타겟 반경 캐싱 로직
        float targetRadius = 0f;
        if (!targetRadiusCache.TryGetValue(targetTransform, out targetRadius))
        {
            // 캐시에 없으면 계산하여 저장
            targetRadius = CalculateTargetRadius(targetTransform);
            targetRadiusCache[targetTransform] = targetRadius;
            
            // 캐시 크기 제한 (메모리 관리)
            if (targetRadiusCache.Count > 50)
            {
                CleanupTargetRadiusCache();
            }
        }
        
        // 실질적인 거리 = 기본 거리 - 드래곤 콜라이더 크기 - 타겟 크기 - 추가 여유 범위
        return Vector3.Distance(transform.position, targetCenter) - dragonBodyRadius - targetRadius - additionalRangeForSize;
    }
    
    // 드래곤 본체 반경 계산 및 업데이트
    private void UpdateDragonBodyRadius()
    {
        if (currentModelInstance == null) return;
        
        // 먼저 캡슐 콜라이더 검색 (더 정확한 결과)
        CapsuleCollider capsuleCollider = currentModelInstance.GetComponentInChildren<CapsuleCollider>();
        if (capsuleCollider != null)
        {
            dragonBodyRadius = capsuleCollider.radius * Mathf.Max(
                currentModelInstance.transform.localScale.x,
                currentModelInstance.transform.localScale.z
            );
            
            // 모델이 회전되어 있을 수 있으므로 local to world 변환
            dragonBodyRadius *= transform.lossyScale.x;
        }
        else
        {
            // 캡슐 콜라이더가 없으면 일반 콜라이더 사용
            Collider[] colliders = currentModelInstance.GetComponentsInChildren<Collider>();
            if (colliders.Length > 0)
            {
                // 모든 콜라이더 중 가장 큰 것을 선택
                float maxRadius = 0f;
                foreach (Collider col in colliders)
                {
                    float radius = Mathf.Max(col.bounds.extents.x, col.bounds.extents.z);
                    if (radius > maxRadius)
                    {
                        maxRadius = radius;
                    }
                }
                dragonBodyRadius = maxRadius;
            }
            else
            {
                // 콜라이더가 없으면 드래곤 모델의 크기에 기반하여 추정
                Renderer[] renderers = currentModelInstance.GetComponentsInChildren<Renderer>();
                if (renderers.Length > 0)
                {
                    // 모든 렌더러의 바운드를 확인
                    Bounds combinedBounds = renderers[0].bounds;
                    foreach (Renderer renderer in renderers)
                    {
                        combinedBounds.Encapsulate(renderer.bounds);
                    }
                    dragonBodyRadius = Mathf.Max(combinedBounds.extents.x, combinedBounds.extents.z) * 0.8f; // 80%만 사용
                }
            }
        }
    }
    
    // 타겟 반경 계산
    private float CalculateTargetRadius(Transform targetTransform)
    {
        if (targetTransform == null) return 0f;
        
        // CharacterController로부터 크기 정보 추출
        CharacterController targetController = targetTransform.GetComponent<CharacterController>();
        if (targetController != null)
        {
            // 타겟의 반지름 활용 (x,z 평면 기준)
            return targetController.radius;
        }
        else
        {
            // CharacterController가 없는 경우, 콜라이더 사용 시도
            Collider targetCollider = targetTransform.GetComponent<Collider>();
            if (targetCollider != null)
            {
                // 대략적인 크기 추정 (바운딩 박스의 절반 크기)
                return Mathf.Max(targetCollider.bounds.extents.x, targetCollider.bounds.extents.z);
            }
        }
        
        return 0f; // 기본값
    }
    
    // 타겟 반경 캐시 정리 (오래된 항목 제거)
    private void CleanupTargetRadiusCache()
    {
        // 현재 타겟은 항상 보존
        List<Transform> keysToRemove = new List<Transform>();
        
        // 현재 타겟이 아닌 항목 중에서 절반을 제거
        int removeCount = targetRadiusCache.Count / 2;
        int count = 0;
        
        foreach (var key in targetRadiusCache.Keys)
        {
            if (key != currentTargetTransform)
            {
                keysToRemove.Add(key);
                count++;
                
                if (count >= removeCount)
                    break;
            }
        }
        
        // 캐시에서 선택된 항목 제거
        foreach (var key in keysToRemove)
        {
            targetRadiusCache.Remove(key);
        }
    }

    /// <summary>
    /// 타겟을 향해 이동
    /// </summary>
    public void MoveTowardTarget()
    {
        if (!currentTargetTransform) return;

        // 타겟과의 실질적인 거리 계산 (타겟 크기를 고려함)
        float effectiveDistanceToTarget = GetEffectiveDistanceToTarget(currentTargetTransform);
        
        if (effectiveDistanceToTarget > meleeRange)
        {
            // 타겟의 중심으로 방향 설정
            Vector3 targetCenter = GetTargetCenter(currentTargetTransform);
            Vector3 dir = (targetCenter - transform.position).normalized;
            
            // 진화 단계에 따른 이동 속도 사용 (이미 UpdateDragonSettingsByEvolutionStage에서 설정됨)
            Vector3 movePos = transform.position + dir * (followSpeed * Time.deltaTime);
            
            // 지형 높이 확인 및 적용 (GetGroundHeight 내부에서 부드러운 보간 처리)
            movePos.y = GetGroundHeight(movePos);
            
            transform.position = movePos;
            
            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Euler(0f, lookRot.eulerAngles.y, 0f);
            
            // 애니메이션 업데이트만 담당
            animator.SetBool(IsMoving, true);
        }
        else
        {
            animator.SetBool(IsMoving, false);
            
            // 전투 시 적을 정면으로 바라보도록 설정
            Vector3 targetDir = (currentTargetTransform.position - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(targetDir, Vector3.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, 
                                              Quaternion.Euler(0f, lookRot.eulerAngles.y, 0f), 
                                              Time.deltaTime * rotationSpeed * 2f); // 빠른 회전
        }
    }
    #endregion

    #region 공격 메서드
    private IEnumerator UseRangedAttack()
    {
        if (fireballPrefab != null && currentTarget != null && !rangedCooldown.IsRunning)
        {
            isAttacking = true;
            
            Vector3 targetCenter = GetTargetCenter(currentTargetTransform);
            
            // 적을 향해 회전하는 코드 추가
            Vector3 targetDir = (targetCenter - transform.position).normalized;
            Quaternion lookRot = Quaternion.LookRotation(targetDir, Vector3.up);
            float rotationTime = 0f;
            float maxRotationTime = 0.5f; // 최대 회전 시간
            
            // 회전이 완료될 때까지 부드럽게 회전
            while (rotationTime < maxRotationTime)
            {
                transform.rotation = Quaternion.Lerp(transform.rotation, 
                                                   Quaternion.Euler(0f, lookRot.eulerAngles.y, 0f), 
                                                   Time.deltaTime * rotationSpeed * 3f); // 더 빠른 회전
                rotationTime += Time.deltaTime;
                yield return null;
            }
            
            // 회전 완료 후 애니메이션 재생
            animator.SetTrigger(IsRangedAttack);
            
            yield return new WaitForSeconds(0.5f); // 발사 전 딜레이
            
            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
            FireballController fireballController = fireball.GetComponent<FireballController>();
            fireballController.Initialize(targetCenter, dragonData, transform.position, currentTarget, currentTargetTransform);
            
            TimerManager.Instance.StartTimer(rangedCooldown);
            
            yield return new WaitForSeconds(rangedDelayTime);
            
            isAttacking = false;
        }
    }

    private void TriggerUltimateAttack()
    {
        if (!ultimateCooldown.IsRunning)
        {
            // 현재 구현되지 않음 - 이후 구현 예정
            //Debug.Log("[드래곤] 궁극기 발동!");
            
            // 궁극기 로직 (예: 주변 모든 적에게 광역 공격)
            // 다음 업데이트에서 구현 예정
            
            animator.SetTrigger(IsUltimate);
            
            // 쿨다운 시작
            TimerManager.Instance.StartTimer(ultimateCooldown);
        }
    }
    
    private Vector3 GetTargetCenter(Transform targetTransform)
    {
        CharacterController targetController = targetTransform.GetComponent<CharacterController>();
        if (targetController != null)
        {
            return targetController.bounds.center;
        }
        return targetTransform.position;
    }
    
    private string GetBuffKoreanName(string buffType)
    {
        switch (buffType)
        {
            case "PlayerBuffHP": 
                return "체력 강화";
            case "PlayerBuffPhysical": 
                return "물리 공격력 강화";
            case "PlayerBuffMagic": 
                return "마법 공격력 강화";
            default: 
                return buffType;
        }
    }
    #endregion

    #region 진화 시스템
    private void UpdateDragonModel()
    {
        if (dragonData == null) return;
        
        if (currentModelInstance != null)
        {
            Destroy(currentModelInstance);
            currentModelInstance = null;
        }
        
        int stageIndex = (int)dragonData.evolutionStage;
        if (dragonModelPrefabs != null && dragonModelPrefabs.Length > stageIndex && dragonModelPrefabs[stageIndex] != null)
        {
            Transform parentTransform = transform;
            if (modelParents != null && modelParents.Length > stageIndex && modelParents[stageIndex] != null)
            {
                parentTransform = modelParents[stageIndex];
            }
            
            currentModelInstance = Instantiate(dragonModelPrefabs[stageIndex], parentTransform.position, parentTransform.rotation, parentTransform);
            
            Animator modelAnimator = currentModelInstance.GetComponent<Animator>();
            if (modelAnimator != null)
            {
                animator = modelAnimator;
                
                // StateMachine의 애니메이터 참조도 업데이트
                if (stateMachine != null)
                {
                    stateMachine.UpdateAnimator(animator);
                    //Debug.Log("[DragonController] 애니메이터 참조 업데이트 완료");
                }
            }
            
            FindAndSetupFirePoint();
            
            // 진화 단계별 크기 조정
            switch (dragonData.evolutionStage)
            {
                case DragonEvolutionStage.Baby:
                    currentModelInstance.transform.localScale = new Vector3(1.0f, 1.0f, 1.0f);
                    followDistance = 3f; // 기본 거리
                    hoverHeight = 2f; // 기본 높이
                    break;
                case DragonEvolutionStage.Young:
                    currentModelInstance.transform.localScale = new Vector3(2f, 2f, 2f);
                    followDistance = 5f; // 중간 크기에 맞게 거리 증가
                    hoverHeight = 3f; // 중간 단계 높이 증가
                    break;
                case DragonEvolutionStage.Adult:
                    currentModelInstance.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    followDistance = 7f; // 큰 크기에 맞게 거리 더 증가
                    hoverHeight = 5f; // 성체 드래곤의 높이 크게 증가
                    break;
            }
            
            // 진화 단계별 설정 업데이트
            UpdateDragonSettingsByEvolutionStage();
            
            // 오프셋 업데이트
            UpdateOffset();
        }
        else
        {
            Debug.LogError($"드래곤 모델 프리팹이 설정되지 않았습니다. 단계: {dragonData.evolutionStage}");
        }
    }
    
    private void HandleEvolution(DragonEvolutionStage newStage)
    {
        if (dragonData == null) return;
        StartCoroutine(PlayEvolutionEffect());
    }
    
    private IEnumerator PlayEvolutionEffect()
    {
        if (currentModelInstance == null) yield break;
        
        // 머티리얼 효과
        Renderer[] renderers = currentModelInstance.GetComponentsInChildren<Renderer>();
        originalMaterials = new Material[renderers.Length];
        
        if (evolutionMaterial != null)
        {
            for (int i = 0; i < renderers.Length; i++)
            {
                originalMaterials[i] = renderers[i].material;
                renderers[i].material = evolutionMaterial;
            }
        }

        yield return new WaitForSeconds(0.5f);
        
        // 진화 이펙트 재생
        if (evolutionEffectPrefab != null)
        {
            GameObject effect = Instantiate(evolutionEffectPrefab, currentModelInstance.transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        yield return new WaitForSeconds(0.5f);
        
        UpdateDragonModel();
        
        //Debug.Log($"드래곤이 {dragonData.evolutionStage} 단계로 진화했습니다!");
    }
    
    // 오프셋 업데이트 메서드 추가
    private void UpdateOffset()
    {
        offset = new Vector3(0, hoverHeight, -followDistance);
    }

    // 진화 단계별 설정 업데이트 메서드 추가
    private void UpdateDragonSettingsByEvolutionStage()
    {
        switch (dragonData.evolutionStage)
        {
            case DragonEvolutionStage.Baby:
                followDistance = 3f;
                hoverHeight = 2f;
                meleeRange = dragonData.attackRange * 0.5f;
                followSpeed = dragonData.speed;
                break;
            case DragonEvolutionStage.Young:
                followDistance = 5f;
                hoverHeight = 2f;
                meleeRange = dragonData.attackRange * 0.7f;
                followSpeed = dragonData.speed * 1.3f;
                break;
            case DragonEvolutionStage.Adult:
                followDistance = 7f;
                hoverHeight = 4f;
                meleeRange = dragonData.attackRange * 0.9f;
                followSpeed = dragonData.speed * 1.6f;
                break;
        }
    }
    #endregion

    #region 파이어 포인트 관리
    private void FindAndSetupFirePoint()
    {
        if (currentModelInstance == null) return;
        
        Transform foundFirePoint = currentModelInstance.transform.Find("FirePoint");
        
        if (foundFirePoint == null)
        {
            foundFirePoint = FindFirePointRecursively(currentModelInstance.transform);
        }
        
        if (foundFirePoint == null)
        {
            foundFirePoint = FindHeadTransform(currentModelInstance.transform);
        }
        
        if (foundFirePoint == null)
        {
            foundFirePoint = CreateNewFirePoint();
        }
        
        if (foundFirePoint != null)
        {
            firePoint = foundFirePoint;
            //Debug.Log($"FirePoint 설정 완료: {firePoint.name}");
        }
        else
        {
            Debug.LogWarning("FirePoint를 찾거나 생성하지 못했습니다.");
        }
    }
    
    private Transform FindTransformByKeywords(Transform parent, string[] keywords)
    {
        foreach (Transform child in parent)
        {
            string childNameLower = child.name.ToLower();
            foreach (string keyword in keywords)
            {
                if (childNameLower.Contains(keyword))
                {
                    return child;
                }
            }
            
            Transform found = FindTransformByKeywords(child, keywords);
            if (found != null)
            {
                return found;
            }
        }
        
        return null;
    }
    
    private Transform FindFirePointRecursively(Transform parent)
    {
        string[] fireKeywords = { "fire", "mouth", "point" };
        return FindTransformByKeywords(parent, fireKeywords);
    }
    
    private Transform FindHeadTransform(Transform parent)
    {
        string[] headKeywords = { "head", "neck", "mouth", "jaw" };
        return FindTransformByKeywords(parent, headKeywords);
    }
    
    private Transform CreateNewFirePoint()
    {
        GameObject newFirePoint = new GameObject("FirePoint");
        newFirePoint.transform.SetParent(currentModelInstance.transform);
        newFirePoint.transform.localPosition = new Vector3(0, 0.5f, 1f);
        //Debug.Log("새로운 FirePoint 생성됨");
        return newFirePoint.transform;
    }
    #endregion

    #region 기즈모
    private void OnDrawGizmos()
    {
        // 이동 관련 기즈모
        if (player != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawLine(transform.position, player.position);
        }

        // 전투 탐색 범위
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectRange);
        
        // 근접 공격 범위
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, meleeRange);
    }
    #endregion

    #region FSM 관련 메서드
    /// <summary>
    /// 플레이어가 이동 중인지 확인
    /// </summary>
    public bool IsPlayerMoving()
    {
        // 플레이어에 PlayerController 컴포넌트가 있는지 확인하고, 있으면 직접 이동 상태 사용
        PlayerController playerController = player.GetComponent<PlayerController>();
        if (playerController != null)
        {
            return playerController.isMove;
        }
        
        // PlayerController를 사용할 수 없는 경우 기존 로직 사용
        float distanceThreshold = 0.05f; // 더 큰 임계값 사용
        Vector3 playerMovementDirection = player.forward;
        float dotProduct = Vector3.Dot((player.position - lastPlayerPosition).normalized, playerMovementDirection);
        
        // 플레이어의 이동 방향과 일치하는 움직임이 있고 거리가 임계값을 넘으면 이동 중으로 판단
        return Vector3.Distance(player.position, lastPlayerPosition) > distanceThreshold || 
            (dotProduct > 0.7f && Vector3.Distance(player.position, lastPlayerPosition) > 0.01f);
    }
    
    /// <summary>
    /// 플레이어와의 거리가 너무 먼지 확인
    /// </summary>
    public bool IsPlayerTooFar()
    {
        return Vector3.Distance(transform.position, player.position) > maxDistanceFromPlayer;
    }
    
    /// <summary>
    /// 타겟이 근접 공격 범위 내에 있는지 확인
    /// </summary>
    public bool IsTargetInMeleeRange()
    {
        if (!HasTarget) return false;
        return GetEffectiveDistanceToTarget(currentTargetTransform) <= meleeRange;
    }
    
    /// <summary>
    /// 타겟이 지정된 범위 내에 있는지 확인
    /// </summary>
    public bool IsTargetInRange(float range)
    {
        if (!HasTarget) return false;
        return GetEffectiveDistanceToTarget(currentTargetTransform) <= range;
    }
    
    /// <summary>
    /// 타겟을 탐색할 필요가 있는지 확인
    /// </summary>
    public bool ShouldFindTarget()
    {
        return currentTarget == null || currentTarget.currentHp <= 0;
    }
    
    /// <summary>
    /// 타겟을 초기화
    /// </summary>
    public void ResetTarget()
    {
        currentTarget = null;
        currentTargetTransform = null;
    }
    
    /// <summary>
    /// 전투 종료 체크 시작
    /// </summary>
    public void StartEndCombatCheck()
    {
        if (!isCheckingEndCombat)
        {
            StartCoroutine(CheckEndCombatCoroutine(1f));
        }
    }
    
    /// <summary>
    /// 전투 상태인지 확인
    /// </summary>
    public bool IsInCombat()
    {
        return GameStateMachine.Instance.CurrentState == GameSystemState.Combat || 
               GameStateMachine.Instance.CurrentState == GameSystemState.BossBattle;
    }
    
    /// <summary>
    /// 플레이어를 따라다니는 로직
    /// </summary>
    public void FollowPlayer()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 순간이동 조건 - 일정 거리 이상이고 쿨다운이 끝났을 때
        if (distanceToPlayer > teleportDistance && teleportCooldown <= 0)
        {
            Vector3 teleportPosition = player.position + offset;
            teleportPosition.y = GetGroundHeight(teleportPosition);
            transform.position = teleportPosition;
            
            // 순간이동 직후 애니메이션 상태 명시적 설정
            isMoving = false;
            animator.SetBool(IsMoving, false);
            isTeleporting = true;
            
            // 텔레포트 쿨다운 설정 (3초)
            teleportCooldown = 3.0f;
            
            // 회전 업데이트
            UpdateTurnDirection();
            
            // 마지막 플레이어 위치 저장 (텔레포트 후 즉시 업데이트)
            lastPlayerPosition = player.position;
            
            // 약간의 지연 후 이동 가능하도록 Coroutine 시작
            StartCoroutine(ResetTeleportState(0.5f));
            
            return;
        }
        else
        {
            // 텔레포트 중이 아닐 때만 플레이어를 따라다니는 동작 수행
            if (!isTeleporting)
            {
                // 플레이어를 따라다니는 동작
                Vector3 targetPosition = player.position + offset;
                targetPosition.y = GetGroundHeight(targetPosition);
                
                float currentSpeed = distanceToPlayer > teleportDistance/2 ? followSpeed * 2f : followSpeed;
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * currentSpeed);

                // 회전
                RotateTowardsPlayer();
                
                // 이동 상태 업데이트
                isMoving = IsPlayerMoving();
                animator.SetBool(IsMoving, isMoving);
                UpdateTurnDirection();
                
                // 플레이어와의 거리가 멀거나, 0.5초마다 마지막 위치 갱신 (과도한 상태 전환 방지)
                if (distanceToPlayer > followDistance * 0.8f || Time.frameCount % 30 == 0)
                {
                    lastPlayerPosition = player.position;
                }
            }
        }
    }
    
    /// <summary>
    /// 타겟을 향해 회전
    /// </summary>
    public void RotateToTarget()
    {
        if (currentTargetTransform == null || player == null) return;
        
        Vector3 directionToTarget = currentTargetTransform.position - transform.position;
        directionToTarget.y = 0; // Y축 회전만 고려
        
        if (directionToTarget != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
        }
    }
    
    /// <summary>
    /// 근접 공격 수행
    /// </summary>
    public void PerformMeleeAttack()
    {
        if (HasTarget)
        {
            CombatManager.Instance.ProcessDragonAttack(dragonData, currentTarget, currentTargetTransform, false);
        }
    }
    
    /// <summary>
    /// 원거리 공격용 발사체 발사
    /// </summary>
    public void LaunchProjectile()
    {
        if (HasTarget && fireballPrefab != null && firePoint != null)
        {
            Vector3 targetCenter = GetTargetCenter(currentTargetTransform);
            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
            FireballController fireballController = fireball.GetComponent<FireballController>();
            fireballController.Initialize(targetCenter, dragonData, transform.position, currentTarget, currentTargetTransform);
        }
    }
    
    /// <summary>
    /// 버프 스킬 사용
    /// </summary>
    public void UseBuffSkill()
    {
        // 쿨다운이 끝난 버프 스킬 목록 가져오기
        List<string> availableBuffs = new List<string>();
        
        // 버프 선택 우선순위
        foreach (string buffName in buffSkills)
        {
            bool isOnCooldown = SkillManager.Instance.IsSkillOnCooldown(EntityType.Dragon, buffName);
            bool isActive = SkillManager.Instance.IsBuffActive(buffName);
            
            if (!isOnCooldown)
            {
                // 비활성 버프를 우선 추가
                if (!isActive)
                {
                    availableBuffs.Add(buffName);
                }
                // 모든 비활성 버프를 찾은 후에도 없다면, 활성 버프도 추가
                else if (availableBuffs.Count == 0)
                {
                    availableBuffs.Add(buffName);
                }
            }
        }
        
        if (availableBuffs.Count == 0)
        {
            return;
        }

        // 랜덤으로 버프 선택
        string chosenBuff = availableBuffs[UnityEngine.Random.Range(0, availableBuffs.Count)];
        
        // Skills 객체 가져오기
        Skills buffSkill = SkillManager.Instance.GetSkill(EntityType.Dragon, chosenBuff);
        if (buffSkill == null)
        {
            Debug.LogError($"[드래곤] {chosenBuff} 스킬을 찾을 수 없습니다.");
            return;
        }

        // 버프 정보 가져오기
        float buffDuration = buffSkill.buffDuration;
        float buffValue = buffSkill.buffValue;

        // 애니메이션 재생
        animator.SetTrigger(IsUseSkill);
        
        // 버프 적용 - 쿨다운은 SkillManager 내에서 관리
        SkillManager.Instance.ApplyBuff(EntityType.Dragon, chosenBuff);
        
        // 버프 효과 텍스트 생성
        string buffEffectText = chosenBuff.Contains("HP") ? $"+{buffValue} HP" : $"+{buffValue}%";
        
        // 시스템 메시지로 버프 적용 알림
        UIManager.SystemGameMessage($"드래곤이 {GetBuffKoreanName(chosenBuff)}(을)를 적용했습니다 ({buffEffectText}, {buffDuration}초 지속)", MessageTag.플레이어_버프);
        
        // 드래곤 스킬 전체 쿨다운 시작
        skillCooldown = new BasicTimer(dragonSkillCooldown);
        TimerManager.Instance.StartTimer(skillCooldown);
    }
    
    /// <summary>
    /// 궁극기 공격 수행
    /// </summary>
    public void PerformUltimateAttack()
    {
        // 주변 모든 적에게 광역 공격
        foreach (var character in CharacterManager.Instance.CharacterList)
        {
            if (character is MonsterData monster && monster.currentHp > 0)
            {
                float dist = Vector3.Distance(transform.position, monster.instance.transform.position);
                if (dist <= detectRange * 0.7f) // 광역 공격 범위는 탐지 범위의 70%
                {
                    // 궁극기 데미지는 일반 공격의 2배
                    CombatManager.Instance.ProcessDragonAttack(dragonData, monster, monster.instance.transform, true, 2.0f);
                }
            }
        }
        
        // 궁극기 이펙트 표시 (이후 구현)
        //Debug.Log("[드래곤] 궁극기 발동!");
    }
    
    /// <summary>
    /// 근접 공격 쿨다운 시작
    /// </summary>
    public void StartMeleeCooldown()
    {
        TimerManager.Instance.StartTimer(meleeCooldown);
    }
    
    /// <summary>
    /// 원거리 공격 쿨다운 시작
    /// </summary>
    public void StartRangedCooldown()
    {
        TimerManager.Instance.StartTimer(rangedCooldown);
    }
    
    /// <summary>
    /// 스킬 쿨다운 시작
    /// </summary>
    public void StartSkillCooldown()
    {
        TimerManager.Instance.StartTimer(skillCooldown);
    }
    
    /// <summary>
    /// 궁극기 쿨다운 시작
    /// </summary>
    public void StartUltimateCooldown()
    {
        TimerManager.Instance.StartTimer(ultimateCooldown);
    }
    #endregion

    #region 공개 메서드
    // 세이브/로드 시스템에서 호출하기 위한 공개 메서드
    public void UpdateDragonModelPublic()
    {
        UpdateDragonModel();
    }
    
    // 공격 상태 설정
    public void SetAttacking(bool attacking)
    {
        isAttacking = attacking;
    }
    #endregion
}

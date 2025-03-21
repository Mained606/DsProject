using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public enum DragonState
{
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
    private static readonly int TurnDirection = Animator.StringToHash("turnDirection");
    private static readonly int IsMoving = Animator.StringToHash("isMoving");
    private static readonly int IsRangedAttack = Animator.StringToHash("isRangedAttack");
    private static readonly int IsMeleeAttack = Animator.StringToHash("isMeleeAttack");
    private static readonly int IsUseSkill = Animator.StringToHash("isUseSkill");
    private static readonly int IsUltimate = Animator.StringToHash("isUltimate");
    #endregion

    #region 기본 필드
    [SerializeField] private DragonData dragonData;
    private Transform player;
    private Animator animator;
    [SerializeField] private Transform firePoint;
    private Vector3 lastPlayerPosition;
    [SerializeField] private bool isMoving;
    [SerializeField] private DragonState currentState = DragonState.Idle;
    #endregion

    #region 이동 설정
    [Header("이동 설정")]
    public float followDistance = 3f;
    public float teleportDistance = 10f;
    public float followSpeed;
    public float hoverHeight = 2f;
    public float rotationSpeed = 5f;
    public float groundCheckDistance = 1f;
    private Vector3 offset;
    #endregion

    #region 전투 설정
    [Header("전투 설정")]
    public float detectRange = 40f;
    public float meleeRange;
    public float maxDistanceFromPlayer = 15f;
    private bool isCheckingEndCombat = false;
    [SerializeField] private GameObject fireballPrefab;
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
    // 버프별 쿨다운은 Skills 객체에서 직접 가져오므로 buffCooldowns 배열은 제거
    // 각 버프의 쿨다운 남은 시간을 추적하는 딕셔너리
    private Dictionary<string, float> buffCooldownRemaining = new Dictionary<string, float>();
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

        // 버프 쿨다운 초기화
        InitializeBuffCooldowns();
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
        
        // 쿨다운 초기화
        meleeCooldown = new BasicTimer(dragonData.attackSpeed);
        
        // 오프셋 및 데이터 설정
        offset = new Vector3(0, hoverHeight, -followDistance);
        lastPlayerPosition = player.position;
        meleeRange = dragonData.attackRange;
        followSpeed = dragonData.speed;
        
        // 진화 단계에 맞는 모델 적용
        UpdateDragonModel();
        
        // 드래곤 데이터 프리팹 설정
        SetupDragonDataPrefabs();
    }

    private void Update()
    {
        if (GameStateMachine.Instance.CurrentState == GameSystemState.Combat || 
            GameStateMachine.Instance.CurrentState == GameSystemState.BossBattle)
        {
            // 버프 쿨다운 감소
            UpdateBuffCooldowns();
            
            HandleCombatLogic();
            
            // 전투 종료 확인
            if (!IsEnemyInRange(detectRange) && !isCheckingEndCombat)
            {
                StartCoroutine(CheckEndCombatCoroutine(1f));
            }
        }
        else
        {
            // 탐색 모드에서는 플레이어 따라다니기
            currentTarget = null;
            currentTargetTransform = null;
            FollowPlayerLogic();
        }
        
        // 궁극기 입력 감지 (예: Z키)
        if (Input.GetKeyDown(KeyCode.Z) && !ultimateCooldown.IsRunning)
        {
            TriggerUltimateAttack();
        }
    }
    #endregion

    #region 초기화 메서드
    private void InitializeBuffCooldowns()
    {
        // 모든 버프 스킬의 쿨다운 초기화
        foreach (string buffName in buffSkills)
        {
            buffCooldownRemaining[buffName] = 0f;
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
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // 순간이동 조건
        if (distanceToPlayer > teleportDistance)
        {
            Vector3 teleportPosition = player.position + offset;
            teleportPosition.y = GetGroundHeight(teleportPosition);
            transform.position = teleportPosition;
            isMoving = false;
        }
        else
        {
            // 플레이어를 따라다니는 동작
            Vector3 targetPosition = player.position + offset;
            targetPosition.y = GetGroundHeight(targetPosition);
            
            float currentSpeed = distanceToPlayer > teleportDistance/2 ? followSpeed * 2f : followSpeed;
            transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * currentSpeed);

            // 회전
            RotateTowardsPlayer();
        }

        UpdateMovementState();
        UpdateTurnDirection();
    }

    private float GetGroundHeight(Vector3 position)
    {
        RaycastHit hit;
        Vector3 rayOrigin = new Vector3(position.x, position.y + 20f, position.z);
        
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 100f, LayerMask.GetMask("Terrain", "Ground")))
        {
            return hit.point.y + hoverHeight; // 땅 위로 hoverHeight만큼 띄움
        }
        
        return position.y; // 레이캐스트 실패 시 현재 높이 유지
    }

    private void UpdateMovementState()
    {
        bool playerIsMoving = Vector3.Distance(player.position, lastPlayerPosition) > 0.01f;

        if (playerIsMoving != isMoving)
        {
            isMoving = playerIsMoving;
            animator.SetBool(IsMoving, isMoving);
            currentState = isMoving ? DragonState.Moving : DragonState.Idle;
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

    private IEnumerator CheckEndCombatCoroutine(float delay)
    {
        isCheckingEndCombat = true;
        yield return new WaitForSeconds(delay);

        if (!IsEnemyInRange(detectRange) && 
           (GameStateMachine.Instance.CurrentState == GameSystemState.Combat || 
            GameStateMachine.Instance.CurrentState == GameSystemState.BossBattle))
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
        }

        isCheckingEndCombat = false;
    }

    private void HandleCombatLogic()
    {
        if (isAttacking) return;
        
        // 플레이어와 너무 멀어졌으면 쫓아감
        float distanceFromPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceFromPlayer > maxDistanceFromPlayer)
        {
            currentTarget = null;
            currentTargetTransform = null;
            currentState = DragonState.Idle;
            FollowPlayerLogic();
            return;
        }

        // 타겟 확인 및 업데이트
        if (currentTarget == null || currentTarget.currentHp <= 0)
        {
            currentTarget = null;
            currentTargetTransform = null;
            FindNearestTarget();

            if (currentTarget == null)
            {
                currentState = DragonState.Idle;
                FollowPlayerLogic();
                return;
            }
        }

        float distanceFromTarget = currentTargetTransform != null ? 
            Vector3.Distance(transform.position, currentTargetTransform.position) : 0f;

        // 공격 우선순위: 스킬 > 원거리 > 근접
        if (!skillCooldown.IsRunning && currentState != DragonState.SkillAttack)
        {
            currentState = DragonState.SkillAttack;
            UseSkillAttack();
        }
        else if (!rangedCooldown.IsRunning && distanceFromTarget < detectRange && 
                 currentState != DragonState.RangedAttack)
        {
            currentState = DragonState.RangedAttack;
            StartCoroutine(UseRangedAttack());
        }
        else if (!meleeCooldown.IsRunning && currentState != DragonState.MeleeAttack)
        {
            currentState = DragonState.MeleeAttack;
            MoveTowardTarget(currentTargetTransform);
        }
        else if (currentState != DragonState.Moving && currentState != DragonState.Idle)
        {
            // 기본 상태로 돌아감
            currentState = distanceFromTarget > meleeRange ? DragonState.Moving : DragonState.Idle;
            
            if (currentState == DragonState.Moving)
            {
                MoveTowardTarget(currentTargetTransform);
            }
        }
    }

    private void FindNearestTarget()
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

    private void MoveTowardTarget(Transform targetTransform)
    {
        if (!targetTransform) return;

        float distanceToTarget = Vector3.Distance(transform.position, targetTransform.position);
        
        if (distanceToTarget > meleeRange)
        {
            currentState = DragonState.Moving;
            
            Vector3 dir = (targetTransform.position - transform.position).normalized;
            Vector3 movePos = transform.position + dir * (followSpeed * Time.deltaTime);
            
            // 지형 높이 확인 및 적용
            movePos.y = GetGroundHeight(movePos);
            
            transform.position = movePos;
            
            Quaternion lookRot = Quaternion.LookRotation(dir, Vector3.up);
            transform.rotation = Quaternion.Euler(0f, lookRot.eulerAngles.y, 0f);
            
            animator.SetBool(IsMoving, true);
        }
        else
        {
            animator.SetBool(IsMoving, false);
            
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
    #endregion

    #region 공격 메서드
    private void UseSkillAttack()
    {
        if (!skillCooldown.IsRunning)
        {
            isAttacking = true;
            
            // 쿨다운이 끝난 버프 스킬 목록 가져오기
            List<string> availableBuffs = new List<string>();
            
            // 버프 선택 우선순위:
            // 1. 쿨다운이 끝났고 현재 적용되지 않은 버프
            // 2. 쿨다운이 끝난 버프 (이미 적용 중이어도 가능)
            foreach (string buffName in buffSkills)
            {
                bool isOnCooldown = buffCooldownRemaining.ContainsKey(buffName) && buffCooldownRemaining[buffName] > 0f;
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
                
                Debug.Log($"[드래곤] 버프 '{buffName}' 상태: 쿨다운={isOnCooldown}, 활성={isActive}");
            }
            
            if (availableBuffs.Count == 0)
            {
                Debug.Log($"[드래곤] 사용 가능한 버프 스킬이 없습니다. 모든 스킬이 쿨다운 중입니다.");
                isAttacking = false;
                return;
            }

            // 랜덤으로 버프 선택
            string chosenBuff = availableBuffs[UnityEngine.Random.Range(0, availableBuffs.Count)];
            Debug.Log($"[드래곤] 선택된 버프 스킬: {chosenBuff}");
            
            // Skills 객체 가져오기
            Skills buffSkill = SkillManager.Instance.GetSkill(EntityType.Dragon, chosenBuff);
            if (buffSkill == null)
            {
                Debug.LogError($"[드래곤] {chosenBuff} 스킬을 찾을 수 없습니다.");
                isAttacking = false;
                return;
            }

            // 버프 정보 가져오기
            float buffDuration = buffSkill.buffDuration;
            float buffValue = buffSkill.buffValue;
            float buffCooldown = buffSkill.cooldown;

            // 애니메이션 재생
            animator.SetTrigger(IsUseSkill);
            
            // 버프 적용
            SkillManager.Instance.ApplyBuff(EntityType.Dragon, chosenBuff);
            
            // 버프 효과 텍스트 생성
            string buffEffectText = chosenBuff.Contains("HP") ? $"+{buffValue} HP" : $"+{buffValue}%";
            
            // 시스템 메시지로 버프 적용 알림
            UIManager.SystemGameMessage($"드래곤이 {GetBuffKoreanName(chosenBuff)}(을)를 적용했습니다 ({buffEffectText}, {buffDuration}초 지속)", MessageTag.플레이어_버프);
            
            // 개별 버프 쿨다운 적용
            buffCooldownRemaining[chosenBuff] = buffCooldown;
            
            // 드래곤 스킬 전체 쿨다운 시작
            skillCooldown = new BasicTimer(dragonSkillCooldown);
            TimerManager.Instance.StartTimer(skillCooldown);

            isAttacking = false;
            currentState = DragonState.Idle;
        }
    }

    private IEnumerator UseRangedAttack()
    {
        if (fireballPrefab != null && currentTarget != null && !rangedCooldown.IsRunning)
        {
            isAttacking = true;
            
            Vector3 targetCenter = GetTargetCenter(currentTargetTransform);
            animator.SetTrigger(IsRangedAttack);
            
            yield return new WaitForSeconds(0.5f); // 발사 전 딜레이
            
            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);
            FireballController fireballController = fireball.GetComponent<FireballController>();
            fireballController.Initialize(targetCenter, dragonData, transform.position, currentTarget, currentTargetTransform);
            
            TimerManager.Instance.StartTimer(rangedCooldown);
            
            yield return new WaitForSeconds(rangedDelayTime);
            
            isAttacking = false;
            currentState = DragonState.Idle;
        }
    }

    private void UseMeleeAttack()
    {
        if (!meleeCooldown.IsRunning)
        {
            isAttacking = true;
            
            animator.SetTrigger(IsMeleeAttack);
            CombatManager.Instance.ProcessDragonAttack(dragonData, currentTarget, currentTargetTransform, false);
            TimerManager.Instance.StartTimer(meleeCooldown);
            
            isAttacking = false;
            currentState = DragonState.Idle;
        }
    }
    
    private void TriggerUltimateAttack()
    {
        if (!ultimateCooldown.IsRunning)
        {
            // 현재 구현되지 않음 - 이후 구현 예정
            Debug.Log("[드래곤] 궁극기 발동!");
            
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
            }
            
            FindAndSetupFirePoint();
            
            // 진화 단계별 크기 조정
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
        
        // 진화 이펙트 재생
        if (evolutionEffectPrefab != null)
        {
            GameObject effect = Instantiate(evolutionEffectPrefab, currentModelInstance.transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        yield return new WaitForSeconds(1.5f);
        
        UpdateDragonModel();
        
        Debug.Log($"드래곤이 {dragonData.evolutionStage} 단계로 진화했습니다!");
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
            Debug.Log($"FirePoint 설정 완료: {firePoint.name}");
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
        Debug.Log("새로운 FirePoint 생성됨");
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

    private void UpdateBuffCooldowns()
    {
        // 쿨다운 타이머 감소
        List<string> cooldownsToUpdate = new List<string>(buffCooldownRemaining.Keys);
        foreach (string buffName in cooldownsToUpdate)
        {
            if (buffCooldownRemaining[buffName] > 0)
            {
                buffCooldownRemaining[buffName] -= Time.deltaTime;
                if (buffCooldownRemaining[buffName] < 0)
                {
                    buffCooldownRemaining[buffName] = 0;
                }
            }
        }
    }
}

using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region ----------Variables----------
    public PlayerData playerData;
    public PlayerCombat playerCombat;
    private WeaponManager weapon;

    [SerializeField] private float staminaRecoveryRate;

    public PlayerState currentState = PlayerState.Idle;
    private CharacterController characterController;
    [SerializeField] private Animator playerAnimator;
    public Animator PlayerAnimator => playerAnimator;

    public bool cheatMode;

    [Header("이동")]
    public Vector2 moveInput;
    public Transform cameraTransform;
    private float walkSpeed;
    private float sprintSpeed;
    private Vector3 direction;
    private Vector3 moveDirection;
    public bool CanMove;
    [SerializeField] private bool isGrounded;
    [SerializeField] private bool isMove;
    public bool isSprinting;

    [Header("점프 / 중력")]
    [SerializeField] private float jumpHeight = 0.5f;
    private float gravity = -9.81f;
    public Vector3 verticalVelocity;
    private float lastGroundHeight;
    [SerializeField] private float fallDamageThreshold = 5f;
    [SerializeField] private float fallDamageMultiplier = 5f;
    [SerializeField] private bool isFreefall;
    [SerializeField] private bool isJumping = false;
    [SerializeField] private GravityState currentGravityState;

    [Header("벽타기")]
    [SerializeField] private float detectionRange = 2f;
    [SerializeField] private float climbRange = 1f;
    private Vector3 currentCliffNormal;
    public bool isClimb;
    public bool CanClimb;
    [SerializeField] private float climbEndCheckOffset = 0.2f; // 절벽 끝 검사 오프셋

    [Header("글라이딩")]
    [SerializeField] private bool isGliding;
    [SerializeField] private bool CanGliding;
    [SerializeField] private float glidableHeight = 6f;
    [SerializeField] private float glidingGravityFactor = 2f;

    [Header("닷지")]
    [SerializeField] private float dodgeDist = 3f;
    [SerializeField] private float dodgeDuration = 0.3f;
    [SerializeField] private bool isDodging = false;
    public bool isInvincible = false;

    [Header("공격")]
    private float dashAttackDuration = 0.5f;
    private float dashAttackMoveDistance = 10f;
    public bool isCombatState;
    public bool CanAttack;
    public bool CanUseSkill;
    public bool CanBlock;
    public bool isAttack;
    public bool isUseSkill;
    public bool isParry;

    [Header("디버그용")]
    public bool CanSprint;
    public float staminaUseAmount;
    [SerializeField] private float currentStamina;
    [SerializeField] private bool isEnoughMana;
    public bool isRecovery;
    [SerializeField] private bool isHit;
    public bool CanWeaponSwitch;

    private BasicTimer RecoveryTimer;
    [SerializeField] private float RecoveryTime = 1f;


    #endregion

    private void OnEnable()
    {
        // 게임매니저에 playerTransform 할당
        GameManager.playerTransform = this.transform;
    }

    private void OnDestroy()
    {
        // Hit 이벤트 구독 해제
        if (playerData != null) playerData.OnTakeDamage -= HitCheck;
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        characterController = GetComponent<CharacterController>();
        playerCombat = GetComponent<PlayerCombat>();
        weapon = playerCombat.weapon;

        SetState(PlayerState.Idle);
        playerAnimator.Play("Idle Walk Run Blend");
        CanWeaponSwitch = true;
        playerData = CharacterManager.PlayerCharacterData;
        RecoveryTimer = new BasicTimer(RecoveryTime);

        // Hit 이벤트 구독
        if (playerData != null) playerData.OnTakeDamage += HitCheck;

        ValueInitialize();
        PlayerBehaviourManager.Instance.AddBehaviour(new MoveBehaviour(characterController, playerAnimator, walkSpeed, sprintSpeed));

    }

    private void Update()
    {
        if (uiCheck != UIManager.Instance.IsUIWindowOpen())
        {
            uiCheck = UIManager.Instance.IsUIWindowOpen();
        }
        if (uiCheck)
        {
            return;
        }

        // 치트
        CheatMode();        // 치트 토글
        HpMpRecovery();     // 치트 활성화시 Hp, Mp 무한

        DeathCheck();       // 죽음 체크
        //HitFinishedCheck();
        StateCheck();
        StateBoolChange();
        isGrounded = characterController.isGrounded;
        playerAnimator.SetBool("Grounded", isGrounded);

        RecoverStats();
        HandleGravity();
        AvoidKeyInput();

        DetectCliff();
        UpdateClimbState();
        if (!isDodging)
        {
            CheckCombatState();
            //ControlMovement();
            ControlJump();
            OnDodge();
        }

        //CanGlidingCheck();
        //OnGliding();
    }

    #region ====================치트====================
    // ==========치트용 임시 변수 선언============
    bool cheatStatSwitch = false;
    float prevPhysicalDamage = 0;
    float prevMagicalDamage = 0;
    // 치트 모드 전환
    private void CheatMode()
    {
        if (InputManager.InputActions.actions["Cheat"].triggered)
        {
            cheatMode = !cheatMode;
            Debug.LogWarning($"CheatMode : {cheatMode}");
            if (cheatMode && cheatStatSwitch == false)
            {
                walkSpeed = 15f;
                sprintSpeed = 30f;
                prevPhysicalDamage = playerData.physicalDamage;
                playerData.physicalDamage += 10000f;
                prevMagicalDamage = playerData.magicDamage;
                playerData.magicDamage += 10000f;
                cheatStatSwitch = true;
            }
            else if (!cheatMode && cheatStatSwitch == true)
            {
                walkSpeed = playerData.moveSpeed;
                sprintSpeed = playerData.moveSpeed * 2f;
                playerData.physicalDamage = prevPhysicalDamage;
                playerData.magicDamage = prevMagicalDamage;
                cheatStatSwitch = false;
            }
        }
    }

    // 치트 전용
    private void HpMpRecovery()
    {
        if (!cheatMode) return;
        if (playerData.maxHp != playerData.currentHp)
        {
            playerData.currentHp = playerData.maxHp;
        }
        if (playerData.maxMp != playerData.currentMp)
        {
            playerData.currentMp = playerData.maxMp;
        }
    }
    #endregion

    public void SetState(PlayerState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
    }

    // PlayerStats 초기화
    private void ValueInitialize()
    {
        if (playerData != null)
        {
            // 2025-01-27 HYO 캐릭터 데이터 변수명 변경으로 speed -> moveSpeed로 수정 및 스탯 확인용 ToString 디버그 호출
            walkSpeed = playerData.moveSpeed;
            sprintSpeed = playerData.moveSpeed * 2f;
            Debug.Log(playerData.ToStringForTMPro());
            //-----------------------------------------------------------------
        }
    }

    // 컴뱃 상태 검사
    private void CheckCombatState()
    {
        playerAnimator.SetBool("Combat", isCombatState);
    }

    // 스프린트 가능 여부 체크 함수
    private void RunableCheck()
    {
        if (InputManager.InputActions.actions["Sprint"].IsPressed())
        {
            currentStamina = playerData.staminaCurrent;
            if (isRecovery && currentStamina > 10f)
            {
                CanSprint = true;
                return;
            }
            else if (!isRecovery)
            {
                CanSprint = currentStamina >= staminaUseAmount ? true : false;
                return;
            }
        }
        else
        {
            CanSprint = false;
            isSprinting = false;
        }
    }

    // 실제 플레이어 스태미나 소모 함수
    public void UsingStamina()
    {
        if (cheatMode) return;  // 치트
        if (isSprinting)
        {
            playerData.UseStamina(staminaUseAmount);
            Debug.Log($"UseStamina");
        }
    }

    // 플레이어 자원 회복 (현재 : 마나, 스태미나 회복)
    private void RecoverStats()
    {
        if (!isSprinting)
        {
            playerData.RegenerateStamina();
            isRecovery = true;
        }
        else
        {
            isRecovery = false;
        }
        if (!RecoveryTimer.IsRunning)
        {
            playerData.RegenerateMp();
            TimerManager.Instance.StartTimer(RecoveryTimer);
        }
    }

    // 상시 중력 적용 및 낙뎀 여부
    private void HandleGravity()
    {
        if (isClimb) return;

        if (!isGrounded)
        {

            if (verticalVelocity.y <= fallDamageThreshold && !isFreefall && !isClimb)
            {
                isFreefall = true;
                lastGroundHeight = transform.position.y;
                playerAnimator.SetBool("Freefall", true);
            }

            if (isGliding)
            {
                isFreefall = false;
                verticalVelocity.y += (gravity / 10) * Time.deltaTime;
            }
            else
            {
                isFreefall = true;
                verticalVelocity.y += gravity * Time.deltaTime;
            }
        }
        else
        {

            if (isFreefall)
            {
                float fallDistance = lastGroundHeight - transform.position.y;

                if (fallDistance > fallDamageThreshold)
                {
                    ApplyFallDamage(fallDistance);
                }
            }
            isFreefall = false;
            isGliding = false;
            isJumping = false;
            playerAnimator.SetBool("Freefall", false);
            playerAnimator.SetBool("Jump", false);
            if (verticalVelocity.y < 0)
            {
                verticalVelocity.y = -0.5f;
            }
        }
    }

    //private void HandleGravity()
    //{
    //    if (isClimb) return;

    //    if (!isGrounded)
    //    {
    //        ApplyGravity();
    //    }
    //    else
    //    {
    //        Land();
    //    }
    //}

    private void SetGravityState(GravityState gravityState)
    {
        if (currentGravityState == gravityState) return;

        currentGravityState = gravityState;
    }

    private void ApplyGravity()
    {
        switch (currentGravityState)
        {
            case GravityState.Grounded:
                verticalVelocity.y = -2f;
                break;
            case GravityState.Freefall:
                verticalVelocity.y += gravity * Time.deltaTime;
                break;
            case GravityState.Gliding:
                verticalVelocity.y += (gravity * glidingGravityFactor) * Time.deltaTime;
                break;
            case GravityState.Jumping:
                verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
                break;
            case GravityState.Climbing:
                verticalVelocity.y = 0f;
                break;
        }

        characterController.Move(new Vector3(0f, verticalVelocity.y, 0f) * Time.deltaTime);
    }

    private void Land()
    {
        if (currentGravityState == GravityState.Freefall)
        {
            float fallDistance = lastGroundHeight - transform.position.y;

            if (fallDistance > fallDamageThreshold)
            {
                ApplyFallDamage(fallDistance);
            }

        }
    }

    // 낙뎀 적용
    private void ApplyFallDamage(float fallDistance)
    {
        float damage = (fallDistance - fallDamageThreshold) * fallDamageMultiplier;
        damage = Mathf.Max(0, damage);
        playerData.TakeDamage((int)damage);
        Debug.Log($"낙하 데미지: {(int)damage}");
    }

    // 플레이어 상태 전환 (bool 변수로 판단해서 체크 중)
    private void StateCheck()
    {
        if (isHit && !isParry)
        {
            SetState(PlayerState.Hit);
        }
        else
        {
            if (isGrounded && !isMove)
            {
                SetState(PlayerState.Idle);
            }
            else if (isGrounded && isMove && !isParry)
            {
                SetState(PlayerState.Move);

            }
            else if (isFreefall || !isGrounded && !isClimb && !isAttack)
            {
                SetState(PlayerState.InAir);
                isJumping = true;
                isAttack = false;
                playerCombat.firstAttack = false;
                playerAnimator.ResetTrigger("NextCombo");
            }

            if (isAttack && isFreefall && !isJumping)
            {
                SetState(PlayerState.Attack);
            }
            else if (playerCombat.isBlocking)
            {
                SetState(PlayerState.Block);
            }
            else if (isUseSkill)
            {
                SetState(PlayerState.UseSkill);
            }
            else if (isClimb)
            {
                SetState(PlayerState.Climb);
            }
        }

    }

    // 플레이어 상태에 따라 실행 가능한 행동들 제어
    private void StateBoolChange()
    {
        switch (currentState)
        {
            case PlayerState.Idle:
                CanMove = true;
                CanAttack = true;
                CanUseSkill = true;
                CanBlock = true;
                SetGravityState(GravityState.Grounded);
                break;
            case PlayerState.Move:
                CanMove = true;
                CanAttack = true;
                CanUseSkill = true;
                CanBlock = true;
                SetGravityState(GravityState.Grounded);
                break;
            case PlayerState.InAir:
                CanMove = true;
                CanAttack = false;
                CanUseSkill = false;
                CanBlock = false;
                SetGravityState(GravityState.Freefall);
                break;
            case PlayerState.Attack:
                CanMove = false;
                CanAttack = true;
                CanUseSkill = false;
                CanBlock = false;
                break;
            case PlayerState.Block:
                CanMove = false;
                CanAttack = true;
                CanUseSkill = false;
                CanBlock = true;
                break;
            case PlayerState.UseSkill:
                CanMove = false;
                CanAttack = false;
                CanUseSkill = false;
                CanBlock = false;
                break;
            case PlayerState.Climb:
                CanMove = true;
                CanAttack = false;
                CanUseSkill = false;
                CanBlock = false;
                SetGravityState(GravityState.Climbing);
                break;
            case PlayerState.Hit:
                CanMove = false;
                CanAttack = true;
                CanUseSkill = false;
                CanBlock = true;
                break;
            case PlayerState.Death:
                CanMove = false;
                CanAttack = false;
                CanUseSkill = false;
                CanBlock = false;
                break;
        }
    }

    AnimatorStateInfo stateInfo;
    AnimatorClipInfo[] clipInfo;
    AnimationClip currentClip;

    public bool AnimFinishCheck()
    {
        stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
        float normalize = stateInfo.normalizedTime;
        if (normalize >= 0.99f)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    // 카메라 회전 기준으로 정면 변경
    public Vector3 GetDirection(Vector2 _moveInput)
    {
        Vector2 moveInput = _moveInput;
        Vector3 forward;
        Vector3 right;
        if (!isClimb)
        {
            forward = cameraTransform.forward;
            right = cameraTransform.right;
        }
        else
        {
            forward = transform.up;
            right = transform.right;
        }

        forward.y = 0f;
        right.y = 0f;
        forward.Normalize();
        right.Normalize();

        return (forward * moveInput.y + right * moveInput.x).normalized;

    }

    // 캐릭터 이동
    private void ControlMovement()
    {
        if (!CanMove) return;

        if (isClimb)
        {
            ControlCliffMovement();
            return;
        }

        moveInput = InputManager.InputActions.actions["Move"].ReadValue<Vector2>();

        direction = GetDirection(moveInput);
        moveDirection = direction;
        moveDirection.y = verticalVelocity.y;

        RunableCheck();
        float currentSpeed = CanSprint ? sprintSpeed : walkSpeed;
        if (isGliding)
        {
            currentSpeed = walkSpeed;
        }

        if (moveInput == Vector2.zero)
        {
            playerAnimator.SetFloat("MotionSpeed", 1);
            playerAnimator.SetFloat("Speed", 0);
            isMove = false;
            isSprinting = false;
        }
        else
        {
            playerAnimator.SetFloat("MotionSpeed", 1);
            playerAnimator.SetFloat("Speed", currentSpeed);
            isMove = true;
            if (CanSprint)
            {
                isSprinting = true;
            }
        }

        playerAnimator.SetBool("Sprint", isSprinting);

        characterController.Move(moveDirection * currentSpeed * Time.deltaTime);
        UsingStamina();

        if (direction != Vector3.zero)
        {
            Vector3 forward = transform.forward;
            float angle = Vector3.SignedAngle(forward, direction, Vector3.up);

            if (Mathf.Abs(angle) > 0.1f)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), 0.2f);
            }
        }
    }

    // 캐릭터 점프 제어
    private void ControlJump()
    {
        if (!CanMove) return;

        if (InputManager.InputActions.actions["Jump"].triggered && isGrounded)
        {
            OnJump();
        }
    }

    // 점프
    private void OnJump()
    {
        playerAnimator.SetBool("Jump", true);

        verticalVelocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
    }

    // 캐릭터 구르기
    private void OnDodge()
    {
        if (InputManager.InputActions.actions["Dodge"].triggered && moveInput != Vector2.zero)
        {
            CanMove = false;
            StartCoroutine(Dodging());
        }
    }

    // 굴리기(무적)
    private IEnumerator Dodging()
    {
        if (isHit) yield break;

        isDodging = true;

        isInvincible = true;

        Vector3 dodgeDirection = GetDirection(moveInput);
        dodgeDirection.y = verticalVelocity.y;
        float elapsedTime = 0f;

        playerAnimator.SetBool("Dodge", isDodging);

        while (elapsedTime < dodgeDuration)
        {
            characterController.Move(dodgeDirection * (dodgeDist / dodgeDuration) * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        CanMove = true;
        isDodging = false;
        playerAnimator.SetBool("Dodge", isDodging);
        isInvincible = false;
    }

    // 플레이어 대쉬 어택 (플레이어를 이동 시키고 있기 때문에 Controller에서 구현 중)
    public IEnumerator DashAttack()
    {
        Vector3 dashDirection = GetDirection(moveInput);
        dashDirection.y = verticalVelocity.y;
        float elapsedTime = 0f;
        while (elapsedTime < dashAttackDuration)
        {
            characterController.Move(dashDirection * (dashAttackMoveDistance / dashAttackDuration) * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        playerAnimator.SetBool("Sprint", false);
        playerAnimator.SetFloat("Speed", 0);
    }

    // 타격 효과를 주기위해 썼던 함수
    public IEnumerator StopPlayer(float duration)
    {
        playerAnimator.speed = 0; // 애니메이션 정지
        yield return new WaitForSecondsRealtime(duration); // 실제 시간 기준 대기
        playerAnimator.speed = 1; // 애니메이션 원래 속도로 복구
    }

    public bool uiCheck = false;

    // 키 활성화 변경
    private void AvoidKeyInput()
    {
        if (uiCheck != UIManager.Instance.IsUIWindowOpen())
        {
            uiCheck = UIManager.Instance.IsUIWindowOpen();
            InputManager.Instance.SetAllInputs(!uiCheck);
            //SetActionStates(!uiCheck);
            Debug.LogWarning($"uiCheck : {uiCheck}");
        }
        InputManager.Instance.SetInputEnabled(CanMove, "Move");
        InputManager.Instance.SetInputEnabled(CanAttack, "Attack");
        InputManager.Instance.SetMultipleInputsEnabled(CanUseSkill, "PlayerSkill_1", "PlayerSkill_2", "PlayerSkill_3");
        InputManager.Instance.SetInputEnabled(CanBlock, "Block");
        //UpdateInputActions(CanMove, "Move");
        //UpdateInputActions(CanAttack, "Attack");
        //UpdateInputActions(CanUseSkill, "PlayerSkill_1", "PlayerSkill_2", "PlayerSkill_3");
        //UpdateInputActions(CanBlock, "Block");
    }

    //private void SetActionStates(bool state)
    //{
    //    CanMove = state;
    //    CanAttack = state;
    //    CanUseSkill = state;
    //    CanBlock = state;
    //    CanWeaponSwitch = state;
    //    CanGliding = state;
    //}

    //private void UpdateInputActions(bool state, params string[] actions)
    //{
    //    foreach (var action in actions)
    //    {
    //        if (state)
    //            InputManager.InputActions.actions[action].Enable();
    //        else
    //            InputManager.InputActions.actions[action].Disable();
    //    }
    //}
    ///////////////////////////////////////////////////////////////////////////////////
    ///// JWS 수정 UI오픈관련 키엑세스 2025.01.26 19:30  End
    ///////////////////////////////////////////////////////////////////////////////////

    #region ---------------Climb---------------
    private void DetectCliff()
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 3f;
        if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, detectionRange))
        {
            if (hit.transform.gameObject.layer == 10) return;

            Debug.DrawLine(rayOrigin, hit.point, Color.red);

            float angle = Vector3.Angle(hit.normal, Vector3.up);

            float heightDifference = hit.point.y - transform.position.y;

            if (Mathf.Abs(heightDifference) < 0.01f)
            {
                heightDifference = 0f;
            }
            if (angle > 75f && angle < 105f)
            {
                CanClimb = true;
                //Debug.Log("매달릴 수 있는 벽");
                if (hit.distance <= climbRange)
                {
                    currentCliffNormal = hit.normal;
                    if (!isClimb && moveInput.y > 0)
                    {
                        StartClimbing(hit.point);
                        //Debug.Log("StartClimbing");
                    }
                }
            }
            else
            {
                CanClimb = false;
                //Debug.Log("매달릴 수 없는 벽");
            }
        }
        else
        {
            CanClimb = false;
            //playerAnimator.SetBool("Climb", false);
            //Debug.Log("Ray에 감지 안 됨");
            CheckClimbEnd();
        }
    }

    private void StartClimbing(Vector3 climbStartPosition)
    {
        if (isClimb) return;
        Debug.Log("StartClimbing");
        //SetState(PlayerState.Climb);
        isClimb = true;
        isFreefall = false;
        isGliding = false;
        playerAnimator.SetBool("Freefall", false);
        playerAnimator.SetBool("Climb", true);
        playerAnimator.SetBool("Jump", false);
        transform.position = climbStartPosition;
        weapon.SwitchWeapon(-1);
        CanWeaponSwitch = false;
        // Anim
    }

    private void CheckClimbEnd()
    {
        if (!isClimb) return;

        Vector3 rayOrigin = transform.position + transform.up * climbEndCheckOffset;
        if (Physics.Raycast(rayOrigin, transform.forward, out RaycastHit hit, detectionRange))
        {
            //Debug.Log("있는 벽 오르는 중");
            //Debug.DrawLine(rayOrigin + Vector3.up * climbEndCheckOffset, hit.point, Color.red);
            //float topEdgeHeight = hit.point.y - transform.position.y;
            //Debug.Log($"topEdgeHeight = {topEdgeHeight}");
            //if (topEdgeHeight < climbEndThreshold)
            //{
            //    EndClimbing(true); // 절벽 끝까지 올라간 경우
            //    Debug.Log("성공적으로 절벽 끝 도달");
            //}
            if (isGrounded)
            {
                EndClimbing(false);
            }
        }
        else
        {
            // 옆벽 검사
            EndClimbing(true);
            Debug.Log("성공적으로 절벽 끝 도달");
            //Debug.DrawLine(rayOrigin + Vector3.up * climbEndCheckOffset, transform.forward * detectionRange, Color.blue);
        }
    }

    private void EndClimbing(bool successful)
    {
        Debug.Log("EndClimbing");
        if (successful)
        {
            isClimb = false;
            CanMove = false;
            playerAnimator.SetBool("ClimbUp", true);

            CanWeaponSwitch = true;

            weapon.SwitchWeapon(-1);

            StartCoroutine(FinishingClimbing());
        }
        else
        {
            isClimb = false;
            playerAnimator.SetBool("Climb", false);
            Debug.Log("절벽타기 취소됨");
            CanWeaponSwitch = true;

            weapon.SwitchWeapon(-1);
            //SetState(PlayerState.Idle);
        }

        // Anim
    }

    private IEnumerator FinishingClimbing()
    {
        float duration = playerAnimator.GetCurrentAnimatorStateInfo(0).length + 0.5f;
        Vector3 climbUpPosition = transform.position + transform.up * 0.8f + transform.forward * 2f;
        Vector3 dir = (climbUpPosition - transform.position).normalized;
        float offsetDist = Vector3.Distance(transform.position, climbUpPosition);
        //transform.position = climbUpPosition;
        //yield return new WaitForSeconds(duration);

        float elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            characterController.Move(dir * (offsetDist / duration) * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        playerAnimator.SetBool("Climb", false);
        playerAnimator.SetBool("ClimbUp", false);
        //SetState(PlayerState.Idle);
    }

    private void HandleClimbJump()
    {
        if (moveInput.y < 0 && InputManager.InputActions.actions["Jump"].triggered)
        {
            EndClimbing(false);
            OnJump();
        }
    }
    private void ControlCliffMovement()
    {
        if (!isClimb) return;

        moveInput = InputManager.InputActions.actions["Move"].ReadValue<Vector2>();

        Vector3 climbUp = transform.up * moveInput.y; // 위/아래 이동
        Vector3 climbSide = transform.right * moveInput.x; // 좌/우 이동

        Vector3 climbDirection = climbUp + climbSide;

        moveDirection = direction;
        moveDirection.y = verticalVelocity.y;

        if (moveInput == Vector2.zero)
        {
            playerAnimator.SetFloat("VelocityY", 0f);

        }
        else
        {
            playerAnimator.SetFloat("VelocityY", moveInput.y > 0 ? 1f : -1f);
        }

        //UpdateCliffNormal();
        if (climbDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(-currentCliffNormal, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);

            climbDirection = Vector3.ProjectOnPlane(climbDirection, currentCliffNormal).normalized;
            characterController.Move(climbDirection * 0.5f * Time.deltaTime);
        }

        StickToCliff();
    }

    private void UpdateCliffNormal()
    {
        if (Physics.Raycast(transform.position, transform.forward, out RaycastHit hit, 1f))
        {
            currentCliffNormal = hit.normal;
        }
    }

    private void StickToCliff()
    {
        if (Physics.Raycast(transform.position, -currentCliffNormal, out RaycastHit hit, 1f))
        {
            // 표면에 가까운 위치로 보정
            transform.position = hit.point + currentCliffNormal * 0.5f; // 약간 띄운다.
            Quaternion surfaceRotation = Quaternion.LookRotation(-currentCliffNormal, Vector3.up);
            transform.rotation = Quaternion.Slerp(transform.rotation, surfaceRotation, Time.deltaTime * 5f);
        }
    }

    private void UpdateClimbState()
    {
        if (isClimb)
        {
            ControlCliffMovement();
            CheckClimbEnd();
            //HandleClimbCancel();
            HandleClimbJump();
        }
    }
    #endregion

    //
    private void HitCheck(Transform Attacker)
    {
        if (isParry)
        {
            isHit = false;
            return;
        }
        else
        {
            isHit = true;
            playerAnimator.SetTrigger("Hit");
            //Debug.LogWarning("Hit신호 감지");
            StartCoroutine(Hit());
        }
    }

    private IEnumerator Hit()
    {
        yield return new WaitForSeconds(0.1f);
        isHit = false;
    }

    private void DeathCheck()
    {
        if (playerData.currentHp <= 0)
        {
            Debug.Log("Player Death");
            InputManager.Instance.SetAllInputs(false);

            // anim
            playerAnimator.SetBool("Death", true);
            SetState(PlayerState.Death);
        }
    }

    #region ====================글라이딩====================
    // 글라이딩
    private void CanGlidingCheck()
    {
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, glidableHeight))
        {
            CanGliding = false;
            Debug.DrawLine(transform.position, hit.point, Color.magenta);
        }
        else
        {
            if (!isGrounded && isFreefall)
            {
                CanGliding = true;
            }
        }
    }

    private void OnGliding()
    {
        if (InputManager.InputActions.actions["Jump"].triggered && CanGliding)
        {
            isGliding = true;
        }
    }
    #endregion

    public void SetVisible(bool isOnOff)
    {
        foreach (Transform obj in transform)
        {
            obj.gameObject.SetActive(isOnOff);
        }
    }
}

public enum GravityState
{
    Grounded,
    Freefall,
    Jumping,
    Gliding,
    Climbing
}

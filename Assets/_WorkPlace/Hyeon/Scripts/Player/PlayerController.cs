using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region ----------Variables----------
    public PlayerData playerData;
    public PlayerCombat playerCombat;
    private WeaponManager weapon;
    private PlayerBehaviourManager behaviour;

    [SerializeField] private float staminaRecoveryRate;

    public PlayerState currentState = PlayerState.Idle;
    private int layerMask;
    public CharacterController characterController;
    [SerializeField] private Animator playerAnimator;
    public Animator PlayerAnimator => playerAnimator;

    public bool cheatMode;

    [Header("행동 확인 용")]
    public bool isGrounded;
    public bool isMove;
    public bool isSprinting;
    public bool isFreefall;
    public bool isJumping = false;
    public bool isClimb;
    [SerializeField] private bool isGliding;
    public bool isDodging = false;
    public bool isAttack;
    public bool isUseSkill;
    public bool isParry;
    [SerializeField] private bool isHit;


    [Header("이동")]
    public Vector2 moveInput;
    public Transform cameraTransform;
    public float walkSpeed;
    public float sprintSpeed;
    private Vector3 direction;
    private Vector3 moveDirection;
    
    [Header("점프 / 중력")]
    public float jumpHeight = 0.5f;
    private float gravity = -9.81f;
    public Vector3 verticalVelocity;
    private float lastGroundHeight;
    [SerializeField] private float fallDamageThreshold = 5f;
    [SerializeField] private float fallDamageMultiplier = 5f;

    [Header("벽타기")]
    [SerializeField] private float detectionRange = 2f;
    [SerializeField] private float climbRange = 1f;
    private Vector3 currentCliffNormal;
    [SerializeField] private float climbEndCheckOffset = 0.2f; // 절벽 끝 검사 오프셋

    [Header("글라이딩")]
    [SerializeField] private float glidableHeight = 6f;

    [Header("닷지")]
    [SerializeField] private float dodgeDist = 6.5f;
    [SerializeField] private float dodgeDuration = 0.67f;

    [Header("공격")]
    [SerializeField] private float dashAttackDuration = 0.5f;
    [SerializeField] private float dashAttackMoveDistance = 10f;
    public bool isCombatState;
    public bool isInvincible = false;

    [Header("디버그용")]
    public float staminaUseAmount;
    public bool isRecovery;

    private BasicTimer RecoveryTimer;
    private float RecoveryTime = 1f;


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
        // 2025.03.16 HYO 추가 -------------------------------------------------------
        if (playerData != null) playerData.OnSpeedChanged -= UpdateMovementSpeed;
        // ---------------------------------------------------------------------------
    }

    private void Start()
    {
        cameraTransform = Camera.main.transform;
        characterController = GetComponent<CharacterController>();
        playerCombat = GetComponent<PlayerCombat>();
        weapon = playerCombat.weapon;
        behaviour = PlayerBehaviourManager.Instance;

        SetState(PlayerState.Idle);
        playerAnimator.Play("Idle Walk Run Blend");
        //CanWeaponSwitch = true;
        playerData = CharacterManager.PlayerCharacterData;
        RecoveryTimer = new BasicTimer(RecoveryTime);
        layerMask = ~LayerMask.GetMask("Ds Player");

        // Hit 이벤트 구독
        if (playerData != null) playerData.OnTakeDamage += HitCheck;
        // 2025.03.16 HYO 추가 -------------------------------------------------------
        if (playerData != null) playerData.OnSpeedChanged += UpdateMovementSpeed;
        // ---------------------------------------------------------------------------

        ValueInitialize();

        // 초기에 움직이기 가능하도록 초기화
        behaviour.CanMove = true;
        behaviour.CanJump = true;
        behaviour.CanDodge = true;
        
        //behaviour.CanClimb = true;
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
        if (!isClimb)
        {
            GroundCheck();
            ApplyGravity();
        }

        RecoverStats();
        AvoidKeyInput();

        //DetectCliff();
        //UpdateClimbState();
        if (!isDodging)
        {
            CheckCombatState();
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
                sprintSpeed = walkSpeed * 1.5f;
                prevPhysicalDamage = playerData.physicalDamage;
                playerData.physicalDamage += 10000f;
                prevMagicalDamage = playerData.magicDamage;
                playerData.magicDamage += 10000f;
                cheatStatSwitch = true;
            }
            else if (!cheatMode && cheatStatSwitch == true)
            {
                walkSpeed = playerData.moveSpeed;
                sprintSpeed = walkSpeed * 2;
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
            walkSpeed = playerData.moveSpeed;
            sprintSpeed = walkSpeed * 2;
            // 2025-01-27 HYO 캐릭터 데이터 변수명 변경으로 speed -> moveSpeed로 수정 및 스탯 확인용 ToString 디버그 호출
            Debug.Log(playerData.ToStringForTMPro());
            //-----------------------------------------------------------------
        }
    }

    // 컴뱃 상태 검사
    private void CheckCombatState()
    {
        playerAnimator.SetBool("Combat", isCombatState);
    }

    // 실제 플레이어 스태미나 소모 함수
    public void UsingStamina()
    {
        if (cheatMode) return;  // 치트
        if (isSprinting)
        {
            playerData.UseStamina(staminaUseAmount);
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

    private void GroundCheck()
    {
        isGrounded = characterController.isGrounded;
        if (!isGrounded && !isJumping)
        {
            //Debug.Log("캐릭터가 공중에 뜸");
            RaycastHit hit;
            if(Physics.Raycast(transform.position + Vector3.up * 0.5f, Vector3.down, out hit, 1f, layerMask))
            {
                //Debug.Log($"Ray맞는 중 : {hit.transform.name}");
                if(hit.point.y < transform.position.y)
                {
                    isGrounded = true;
                    characterController.Move(new Vector3(0, hit.point.y - transform.position.y, 0));
                }
            }
        }
        playerAnimator.SetBool("Grounded", isGrounded);
    }

    public void ApplyGravity()
    {
        if (isGrounded)
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
            playerAnimator.SetBool("Freefall", false);
            if (verticalVelocity.y < 0)
            {
                verticalVelocity.y = -2f;
            }
        }
        else
        {
            if (isGliding)
            {
                // TODO
            }
            else
            {
                isFreefall = true;
                playerAnimator.SetBool("Freefall", true);
                verticalVelocity.y += gravity * 2.5f * Time.deltaTime;
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

    public bool AnimFinishCheck()
    {
        AnimatorStateInfo stateInfo = playerAnimator.GetCurrentAnimatorStateInfo(0);
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

        Vector3 direction = (forward * moveInput.y + right * moveInput.x).normalized;

        return direction;

    }

    // 캐릭터 구르기
    public void Dodge()
    {
        StartCoroutine(DodgeRoutine());
    }

    // 굴리기(무적)
    private IEnumerator DodgeRoutine()
    {
        isDodging = true;
        behaviour.CanMove = false;
        behaviour.CanJump = false;
        behaviour.CanAttack = false;
        behaviour.CanUseSkill = false;
        behaviour.CanBlock = false;
        isInvincible = true;

        Vector3 dodgeDirection = transform.forward;
        dodgeDirection.y = verticalVelocity.y;
        float elapsedTime = 0f;

        

        while (elapsedTime < dodgeDuration)
        {
            characterController.Move(dodgeDirection * (dodgeDist / dodgeDuration) * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        isDodging = false;
        behaviour.CanMove = true;
        behaviour.CanJump = true;
        behaviour.CanAttack = true;
        behaviour.CanUseSkill = true;
        behaviour.CanBlock = true;
        playerAnimator.SetBool("Dodge", false);
        isInvincible = false;
    }

    public void DashAttack()
    {
        StartCoroutine(DashAttackRoutine());
    }

    private IEnumerator DashAttackRoutine()
    {

        Vector3 dashDirection = transform.forward;
        dashDirection.y = verticalVelocity.y;
        float elapsedTime = 0f;

        while (elapsedTime < dashAttackDuration)
        {
            characterController.Move(dashDirection * (dashAttackMoveDistance / dashAttackDuration) * Time.deltaTime);
            elapsedTime += Time.deltaTime;
            yield return null; // 다음 프레임까지 대기
        }

        PlayerBehaviourManager.Instance.CanDodge = true;
        playerAnimator.SetBool("Sprint", false);
        playerAnimator.SetFloat("Speed", 0);
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
        //InputManager.Instance.SetInputEnabled(CanMove, "Move");
        //InputManager.Instance.SetInputEnabled(CanAttack, "Attack");
        //InputManager.Instance.SetMultipleInputsEnabled(CanUseSkill, "PlayerSkill_1", "PlayerSkill_2", "PlayerSkill_3");
        //InputManager.Instance.SetInputEnabled(CanBlock, "Block");
    }

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
                behaviour.CanClimb = true;
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
                behaviour.CanClimb = false;
            }
        }
        else
        {
            behaviour.CanClimb = false;
            //playerAnimator.SetBool("Climb", false);
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
        //CanWeaponSwitch = false;
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
            //CanMove = false;
            playerAnimator.SetBool("ClimbUp", true);

            //CanWeaponSwitch = true;

            weapon.SwitchWeapon(-1);

            StartCoroutine(FinishingClimbing());
        }
        else
        {
            isClimb = false;
            playerAnimator.SetBool("Climb", false);
            Debug.Log("절벽타기 취소됨");
            //CanWeaponSwitch = true;

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
            //OnJump();
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
        if (isParry || isInvincible)
        {
            isHit = false;
            return;
        }
        else if (isUseSkill)
        {
            isHit = true;
            playerAnimator.ResetTrigger("Hit");
            return;
        }
        else
        {
            isHit = true;
            playerAnimator.SetTrigger("Hit");
            behaviour.CanMove = false;
            behaviour.CanAttack = false;
            behaviour.CanJump = false;
            behaviour.CanUseSkill = false;
            behaviour.CanDodge = false;
            StartCoroutine(Hit());
        }
    }

    // 2025.03.16 HYO 추가 -----------------------------
    private void UpdateMovementSpeed(float newSpeed)
    {
        // 새로운 이동 속도를 반영하는 로직
        walkSpeed = newSpeed;

        // 예: 이동 속도에 따라 애니메이션 속도 조정
    }
    // -------------------------------------------------

    private IEnumerator Hit()
    {
        yield return new WaitForSeconds(0.1f);
        isHit = false;
        behaviour.CanMove = true;
        behaviour.CanAttack = true;
        behaviour.CanJump = true;
        behaviour.CanUseSkill = true;
        behaviour.CanDodge = true;
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
            //CanGliding = false;
            Debug.DrawLine(transform.position, hit.point, Color.magenta);
        }
        else
        {
            if (!isGrounded && isFreefall)
            {
                //CanGliding = true;
            }
        }
    }

    //private void OnGliding()
    //{
    //    if (InputManager.InputActions.actions["Jump"].triggered && CanGliding)
    //    {
    //        isGliding = true;
    //    }
    //}
    #endregion

    public void SetVisible(bool isOnOff)
    {
        foreach (Transform obj in transform)
        {
            obj.gameObject.SetActive(isOnOff);
        }
    }

    private void RemoteBehaviours()
    {
        if (isJumping)
        {
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.forward + transform.position);
    }
}



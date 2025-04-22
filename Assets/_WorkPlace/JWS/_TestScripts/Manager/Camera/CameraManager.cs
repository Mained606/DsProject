using System.Collections.Generic;
using UnityEngine;
using System.Linq;


#if UNITY_EDITOR
using UnityEditor;
#endif

public delegate void TransitionEvent();

public class CameraManager : BaseManager<CameraManager>
{
    #region 변수선언
    public event TransitionEvent OnTransitionStart;
    public event TransitionEvent OnTransitionComplete;

    [SerializeField] private CameraPoseSetting cameraPoseSetting;
    [SerializeField] private CameraType currentCameraType;
    [SerializeField] private CameraTransitionType currentTransitionType;
    [SerializeField] private CameraPose[] cameraPoses;
    [SerializeField] private List<CameraPoseList> cameraTransitionList;
    private CameraTransitionType preTransitionType;

    [Header("목표에따른 카멜포즈 전환 설정")]
    [SerializeField] public Transform target;
    [SerializeField] public List<CameraPose> targetPoses;
    [SerializeField] private float poseTransitionSpeed = 2f;
    private int currentPoseIndex = 0;
    private float transitionProgress = 0f;
    private bool isTransitionActive = false;

    [Header("오비트 설정")]
    [SerializeField] private Vector3 orbitTargetOffset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private float orbitDistance = 12f;
    [SerializeField] private Vector2 orbitSensitivity = new Vector2(7f, 7f);
    [SerializeField] private Vector2 orbitClamp = new Vector2(0.1f, 89f);
    [SerializeField] private float orbitYaw = 0f;
    [SerializeField] private float orbitPitch = 0f;

    [Header("팔로우 설정")]
    [SerializeField] private Vector2 followOffset = new Vector2(0f, 0f);

    [Header("마우스커서 설정")]
    [SerializeField] private CursorLockMode mouseCursor;
    [SerializeField] private bool mouseCursorVisible = false;

    // 컬링부분 체크중.
    private Transform orbitTarget, followTarget; // 플레이어 오브젝트

    private Camera mainCamera;
    private Camera portraitCamera;
    public string playerLayerName = "Ds Player";
    public string npcLayerName = "NPC";
    public string enemyLayerName = "Enemy";
    public string dragonLayerName = "Dragon";
    private int playerLayerMask, dragonLayerMask, npcLayerMask, enemyLayerMask;
    private bool isStatusUI = false;
    public static Camera MainCamera => Instance.mainCamera;
    
    /// <summary>
    /// 2025-02-09 HYO 코드 추가 카메라 쉐이크 기능 ============================================
    /// </summary>
    [SerializeField] private float shakeAmount = 0.05f; // 흔들림의 강도
    [SerializeField] private float shakeDuration = 0.2f; // 흔들림 지속 시간
    private BasicTimer shakeTimer; // 현재 쉐이크 타이머
    // ====================================================================================
    #endregion

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
        portraitCamera = mainCamera.transform.GetChild(0).GetComponent<Camera>();
        portraitCamera.gameObject.SetActive(false);

        cameraPoses = cameraPoseSetting.poseList.ToArray();
        cameraTransitionList = cameraPoseSetting.poseTransitionList;
        targetPoses = cameraTransitionList.Find(list => list.transitionType == currentTransitionType)?.poseList;
        preTransitionType = currentTransitionType;
        InitCameraPoses();
        currentCameraType = CameraType.Orbit;
        mouseCursor = CursorLockMode.Locked;
        Cursor.lockState = mouseCursor;
        Cursor.visible = mouseCursorVisible;
        shakeTimer = new BasicTimer(shakeDuration);

        playerLayerMask = 1 << LayerMask.NameToLayer(playerLayerName);
        dragonLayerMask = 1 << LayerMask.NameToLayer(dragonLayerName);
        npcLayerMask = 1 << LayerMask.NameToLayer(npcLayerName);
        enemyLayerMask = 1 << LayerMask.NameToLayer(enemyLayerName);
    }

    private void Update()
    {
        if (!UIManager.Instance.IsUIWindowOpen())
        {
            if (Input.GetKeyDown(KeyCode.LeftAlt))
            {
                CursorLock();
            }
            if (Input.GetKeyUp(KeyCode.LeftAlt))
            {
                CursorUnLock();
            }
        }

        if (mouseCursorVisible) return;

        if (orbitTarget == null || followTarget == null)
        {
            orbitTarget = followTarget = GameManager.playerTransform;
        }
        if (isTransitionActive)
        {
            HandlePoseTransition();
            return;
        }

      /*  if (Input.GetKeyUp(KeyCode.F9))
        {
            StartTransition(orbitTarget, CameraTransitionType.Normal);
        }*/

        if (preTransitionType != currentTransitionType)
        {
            preTransitionType = currentTransitionType;
            targetPoses = cameraTransitionList.Find(list => list.transitionType == currentTransitionType)?.poseList;
        }
        switch (currentCameraType)
        {
            case CameraType.Orbit:
                UpdateOrbitCamera();
                break;
            case CameraType.Follow:
                UpdateFollowCamera();
                break;
            case CameraType.UIview:
                UpdateUIviewCamera();
                break;
        }
        // CheckPlayerVisibility();
        
        // 카메라 쉐이크 적용 2025-02-09 HYO 코드 추가 =====
        UpdateCameraShake();
        // ============================================
    }

    private void InitCameraPoses()
    {
        if (cameraPoseSetting.poseTransitionList == null || cameraPoseSetting.poseTransitionList.Count == 0)
        {
            cameraPoseSetting.poseTransitionList = new List<CameraPoseList>();

            foreach (CameraTransitionType transitionType in System.Enum.GetValues(typeof(CameraTransitionType)))
            {
                cameraPoseSetting.poseTransitionList.Add(new CameraPoseList
                {
                    transitionType = transitionType,
                    poseList = GetDefaultPosesForType(transitionType)
                });
            }
        }

        //if (cameraPoseSetting.poseList == null || cameraPoseSetting.poseList.Count == 0)
        //{
            cameraPoses = new CameraPose[System.Enum.GetValues(typeof(CameraType)).Length];

            cameraPoses[(int)CameraType.Main] = new CameraPose(
                new Vector3(0, 3, 0.6f),
                Vector3.zero,
                3f
            );

            cameraPoses[(int)CameraType.LookAt] = new CameraPose(
                new Vector3(1.8f, 2.4f, 4f),
                new Vector3(10, 180, 0),
                3f
            );

            cameraPoses[(int)CameraType.UIview] = new CameraPose(
                new Vector3(2f, 2.4f, 5f),
                new Vector3(10, 180, 0),
                3f
            );

            cameraPoses[(int)CameraType.Follow] = new CameraPose(
                new Vector3(0, 9, -10),
                new Vector3(30, 0, 0),
                3f
            );

            cameraPoseSetting.poseList = cameraPoses.ToList();
        //}
    }

    private List<CameraPose> GetDefaultPosesForType(CameraTransitionType type)
    {
        switch (type)
        {
            case CameraTransitionType.UiView:
                return new List<CameraPose>
                {
                    new CameraPose(new Vector3(0, 2.1f, 4.2f), new Vector3(10, 180, 0), 3f )
                };

            case CameraTransitionType.Follow:
                return new List<CameraPose>
                {
                    new CameraPose(new Vector3(0, 9, -10), new Vector3(30, 0, 0), 3f )
                };

            case CameraTransitionType.Orbit:
                return new List<CameraPose>
                {
                    new CameraPose(new Vector3(0, 9, -10), new Vector3(30, 0, 0), 3f )
                };

            default:
                return new List<CameraPose>();
        }
    }

    private void UpdateOrbitCamera()
    {
        if (orbitTarget == null || mainCamera == null) return;
        Vector2 lookInput = InputManager.InputActions.actions["Look"].ReadValue<Vector2>();

        float mouseWheelInput = Input.GetAxis("Mouse ScrollWheel");
        if (mouseWheelInput != 0f)
        {
            orbitDistance = Mathf.Clamp(orbitDistance - mouseWheelInput, 2f, 12f);
        }

        orbitYaw += lookInput.x * orbitSensitivity.x * Time.deltaTime;
        if(orbitYaw >= 360f)
        {
            orbitYaw -= 360f;
        }
        else if(orbitYaw < 0f)
        {
            orbitYaw += 360f;
        }
        orbitPitch -= lookInput.y * orbitSensitivity.y * Time.deltaTime;
        orbitPitch = Mathf.Clamp(orbitPitch, orbitClamp.x, orbitClamp.y);
        Vector3 targetPosition = orbitTarget.position + orbitTargetOffset;
        Quaternion rotation = Quaternion.Euler(orbitPitch, orbitYaw, 0f);
        Vector3 position = targetPosition - (rotation * Vector3.forward * orbitDistance);
        mainCamera.transform.position = position;
        mainCamera.transform.rotation = rotation;
    }

    private void UpdateFollowCamera()
    {
        if (followTarget == null || MainCamera == null) return;
        Vector3 targetPosition = followTarget.position + new Vector3(followOffset.x, followOffset.y, 0f);
        CameraPose pose = cameraPoses[(int)CameraType.Follow];

        MainCamera.transform.position = targetPosition + pose.position;
        MainCamera.transform.rotation = Quaternion.Euler(pose.rotation);
    }

    private void UpdateUIviewCamera(Transform gobTarget = null)
    {
        bool isPlayer = gobTarget == GameManager.playerTransform;
        if (orbitTarget == null || MainCamera == null) return;
        if (gobTarget == null)
        {
            isPlayer= true;
            gobTarget = orbitTarget;
        }

        Vector3 targetPosition = gobTarget.position;
        CameraPose pose = cameraPoses[(int)CameraType.UIview];
        Vector3 offsetPosition = gobTarget.rotation * pose.position + 
            (isPlayer ? Vector3.zero : new Vector3(0, 0.6f, 0));
        
        if (gobTarget != orbitTarget)
        {
            targetPosition.y -= isStatusUI ? 1 : 0f;
        }
        else
        {
            targetPosition.y += 0.3f;
        }

        MainCamera.transform.position = targetPosition + offsetPosition;
        MainCamera.transform.rotation = gobTarget.rotation * Quaternion.Euler(pose.rotation);

        if (isStatusUI)
        {
            if (gobTarget == orbitTarget) SetPortraitActive(isStatusUI);
            else SetPortraitActive(isStatusUI, false);
        }
        else if (portraitCamera.gameObject.activeSelf)
        {
            SetPortraitActive(isStatusUI);
        }
    }


    private void SetPortraitActive(bool isActive, bool isPlayer = true)
    {
        if (!isActive)
        {
            DisableLayer(portraitCamera, playerLayerMask);
            DisableLayer(portraitCamera, dragonLayerMask);
            portraitCamera.gameObject.SetActive(false);
            return;
        }

        EnableLayer(portraitCamera, isPlayer ? playerLayerMask : dragonLayerMask);
        DisableLayer(portraitCamera, isPlayer ? dragonLayerMask : playerLayerMask);
        portraitCamera.gameObject.SetActive(true);
    }


    public static void SetCameraActive(Transform target, CameraType nexttype)
    {
        if (Instance == null || Instance.mainCamera == null) return;
        int index = (int)nexttype;
        Instance.currentCameraType = nexttype;
        if (nexttype == CameraType.Orbit)
        {
            Instance.orbitTarget = target;
            return;
        }
        if (nexttype == CameraType.Follow)
        {
            Instance.followTarget = target;
            return;
        }
        CameraPose pose = Instance.cameraPoses[index];
        Instance.mainCamera.transform.position = target.position + target.rotation * pose.position;
        Instance.mainCamera.transform.rotation = target.rotation * Quaternion.Euler(pose.rotation);
    }

    private void HandlePoseTransition()
    {
        if (target == null || targetPoses == null || targetPoses.Count == 0) return;

        if (transitionProgress == 0f)
            OnTransitionStart?.Invoke(); // 트랜지션 시작 이벤트

        CameraPose currentPose = targetPoses[currentPoseIndex];
        CameraPose nextPose = targetPoses[(currentPoseIndex + 1) % targetPoses.Count];
        transitionProgress += (Time.deltaTime * poseTransitionSpeed) / currentPose.transitionTime;

        Vector3 interpolatedPosition = Vector3.Lerp(
            target.position + currentPose.position,
            target.position + nextPose.position,
            transitionProgress);

        Quaternion interpolatedRotation = Quaternion.Lerp(
            Quaternion.Euler(currentPose.rotation),
            Quaternion.Euler(nextPose.rotation),
            transitionProgress);

        mainCamera.transform.position = interpolatedPosition;
        mainCamera.transform.rotation = interpolatedRotation;

        if (transitionProgress >= 1f)
        {
            transitionProgress = 0f;
            currentPoseIndex++;
            if (currentPoseIndex >= targetPoses.Count)
            {
                isTransitionActive = false;
                OnTransitionComplete?.Invoke(); // 트랜지션 완료 이벤트
                currentCameraType = CameraType.Orbit;
                target = null;
            }
        }
    }

    public void StartTransition(Transform newTarget, CameraTransitionType transitionType = CameraTransitionType.Normal)
    {
        if (isTransitionActive) return;

        switch (transitionType)
        {
            case CameraTransitionType.Normal:
                target = newTarget;
                targetPoses = cameraTransitionList.Find(list => list.transitionType == transitionType)?.poseList;
                currentPoseIndex = 0;
                transitionProgress = 0f;
                isTransitionActive = true;
                break;

            case CameraTransitionType.Active:
                // 예제 라인
                // 여기에 활성화 전환에 대한 별도 로직 추가
                break;
        }
    }


    public static void CursorLock()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Instance.mouseCursorVisible = true;
    }

    public static void CursorUnLock()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Instance.mouseCursorVisible = false;
    }

    public void SaveCurrentPose()
    {
        if (target == null)
        {
            Debug.LogWarning("목표가 없습니다.");
            return;
        }
        var poseList = cameraPoseSetting.poseTransitionList.Find(list => list.transitionType == currentTransitionType);
        if (poseList == null)
        {
            Debug.LogWarning($"{currentTransitionType}에 대한 poseList가 존재하지 않습니다.");
            return;
        }
        CameraPose newPose = new CameraPose(
            mainCamera.transform.position - target.position,
            mainCamera.transform.rotation.eulerAngles,
            3f
        );
        poseList.poseList.Add(newPose);
        //Debug.Log($"{currentTransitionType}의 카메라포즈 저장완료");
    }

    // 상태 처리 메서드
    private void HandleShoppingState(object data)
    {
        NPCData npcData = data as NPCData;
        HandleUIviewState(npcData?.currentNPC.transform);
    }

    public void HandleUIviewState(Transform cameraTarget = null)
    {
        currentCameraType = CameraType.UIview;
        if (cameraTarget != null)
        {
            UpdateUIviewCamera(cameraTarget);
            PlayerVisible(false);
        }
        else
        {
            UpdateUIviewCamera();
            NonePlayerVisible(false);
        }
        CursorLock();
    }

    private void HandleDefaultState()
    {
        CursorUnLock();
        currentCameraType = CameraType.Orbit;
        PlayerVisible(true);
        NonePlayerVisible(true);
    }

    private void PlayerVisible(bool visible)
    {
        if (visible)
        {
            EnableLayer(MainCamera, playerLayerMask);
            EnableLayer(MainCamera, dragonLayerMask);
        }
        else
        {
            DisableLayer(MainCamera, playerLayerMask);
            DisableLayer(MainCamera, dragonLayerMask);
        }
    }
    
    private void NonePlayerVisible(bool visible)
    {
        if (visible)
        {
            EnableLayer(MainCamera, npcLayerMask);
            EnableLayer(MainCamera, enemyLayerMask);
        }
        else
        {
            DisableLayer(MainCamera, npcLayerMask);
            DisableLayer(MainCamera, enemyLayerMask);
        }
    }

    public void DisableLayer(Camera targetCamera, LayerMask layerMask)
    {
        targetCamera.cullingMask &= ~layerMask;
    }

    public void EnableLayer(Camera targetCamera, LayerMask layerMask)
    {
        targetCamera.cullingMask |= layerMask;
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        isStatusUI = false;
        switch (newState)
        {
            case GameSystemState.Shopping:
                PlayerVisible(false);
                HandleShoppingState(additionalData);
                break;

            case GameSystemState.InfoMessage:
                CursorLock();
                break;

            case GameSystemState.StatusUI:
                isStatusUI = true;
                PlayerVisible(false);
                HandleUIviewState();
                CursorLock();
                break;

            case GameSystemState.Event:
                PlayerVisible(true);
                HandleUIviewState(); // 커서 언락 + 카메라 전환
                break;

            case GameSystemState.Inventory:
            case GameSystemState.DialogueState:
            case GameSystemState.Pause:
            case GameSystemState.GameOver:
            case GameSystemState.QuestReview:
            case GameSystemState.PetInteraction:
            case GameSystemState.Cook:
            case GameSystemState.Skill:
            case GameSystemState.Enhance:
            case GameSystemState.Option:
                PlayerVisible(true);
                HandleUIviewState();
                break;

            default:
                PlayerVisible(false);
                HandleDefaultState();
                break;
        }
    }

    public static bool IsInFrustum(Transform zone)
    {
        if (zone == null) return false;
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(Instance.mainCamera);

        Collider zoneCollider = zone.GetComponent<Collider>();
        Bounds zoneBounds = zoneCollider ? zoneCollider.bounds : new Bounds(zone.position, Vector3.one);

        return GeometryUtility.TestPlanesAABB(frustumPlanes, zoneBounds);
    }

    private void OnDrawGizmos()
    {
        if (currentCameraType == CameraType.Orbit && orbitTarget != null && mainCamera != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(orbitTarget.position, orbitDistance);

            Gizmos.color = Color.green;
            Gizmos.DrawLine(orbitTarget.position, mainCamera.transform.position);
        }
    }
    
    /// <summary>
    /// 2025-02-09 HYO 코드 추가 카메라 쉐이크 기능 ============================================
    /// </summary>
    private void UpdateCameraShake()
    {
        if (shakeTimer.IsRunning)
        {
            // 랜덤한 방향으로 위치를 살짝 변화시켜서 흔드는 효과를 낸다
            Vector3 shakeOffset = Random.insideUnitSphere * shakeAmount;

            // 카메라의 위치를 흔들리게 할 때, 기존 위치에 변화를 추가
            mainCamera.transform.position += shakeOffset;
        }
    }

    public void StartCameraShake()
    {
        if (shakeTimer == null ) shakeTimer = new BasicTimer(shakeDuration);
        TimerManager.Instance.StartTimer(shakeTimer);
    }
    // ====================================================================================
}

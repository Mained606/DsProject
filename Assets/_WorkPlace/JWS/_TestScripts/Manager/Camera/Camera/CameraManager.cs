using System;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : BaseManager<CameraManager>
{
    private Camera main_Camera;
    private CameraTarget playerCameraTarget = new CameraTarget();
    private CinemachineBrain cinemaBrain;
    private CinemachineCamera cinemaCamera;
    private CinemachineOrbitalFollow cinemaOrbitalFollow;
    private CinemachineInputAxisController cinemaInputAxis;

    [SerializeField] private AnimationCurve defaultBlendCurve;
    [SerializeField] private float defaultBlendTime = 2.0f;
    [SerializeField] private CinemachineBlenderSettings blenderSettings;

    private bool defaultCameraTarget = true;

    public static void MainCameraInputToggle(bool isOn)
    {
        Instance.cinemaInputAxis.enabled = isOn;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    private void InitCameraCinema()
    {

        main_Camera = Camera.main;
        if (main_Camera == null)
        {
            Debug.LogError("Main Camera를 찾을 수 없습니다.");
            return;
        }
        cinemaBrain = main_Camera.GetComponent<CinemachineBrain>();
        if (cinemaBrain == null)
        {
            Debug.LogError("CinemachineBrain 컴포넌트를 찾을 수 없습니다.");
            return;
        }

        var activeVirtualCamera = cinemaBrain.ActiveVirtualCamera;
        if (activeVirtualCamera == null)
        {
            Debug.LogError("활성화된 Virtual Camera가 없습니다.");
            return;
        }
        cinemaCamera = activeVirtualCamera as CinemachineCamera;
        if (cinemaCamera == null)
        {
            Debug.LogError("활성화된 Virtual Camera가 CinemachineCamera 타입이 아닙니다.");
            return;
        }

        cinemaOrbitalFollow = cinemaCamera.GetComponentInChildren<CinemachineOrbitalFollow>();
        cinemaInputAxis = cinemaCamera.GetComponentInChildren<CinemachineInputAxisController>();
    }

    protected override void Start()
    {
        base.Start();
        InitCameraCinema();
        playerCameraTarget = new CameraTarget
        {
            TrackingTarget = GameManager.playerTransform,
            LookAtTarget = GameManager.playerTransform,
            CustomLookAtTarget = false
        };
        SetDefaultCameraTarget();
    }

    private void Update()
    {
        if (InputManager.InputActions.actions["ESC"].triggered)
            SetDefaultCameraTarget();
    }

    public static void SetDefaultCameraTarget()
    {
        if (Instance.cinemaCamera == null)
        {
            Debug.LogError("CinemachineCamera가 설정되지 않았습니다.");
            return;
        }
        ICinemachineCamera currentCamera = Instance.cinemaBrain.ActiveVirtualCamera;
        Instance.cinemaCamera.Target = Instance.playerCameraTarget;
        Instance.cinemaCamera.Lens.FieldOfView = 60f;
        Instance.cinemaOrbitalFollow.VerticalAxis.Value = 17f;
        Instance.cinemaOrbitalFollow.HorizontalAxis.Value = 0f;
        Instance.cinemaCamera.transform.GetComponent<CinemachineRotationComposer>().TargetOffset = new Vector3(0, 2, 0);
        MainCameraInputToggle(true);
        //Instance.ApplyBlend(currentCamera, Instance.cinemaCamera);
    }

    private void ApplyBlend(ICinemachineCamera fromCamera, ICinemachineCamera toCamera)
    {
        if (fromCamera == null || toCamera == null)
        {
            Debug.LogWarning("Blend를 적용할 수 없습니다. 유효한 카메라가 없습니다.");
            return;
        }
        if (blenderSettings == null)
        {
            Debug.LogError("CinemachineBlenderSettings가 설정되지 않았습니다.");
            return;
        }
        cinemaBrain.CustomBlends = blenderSettings;
    }

    private void ApplyChangeLook()
    { 
    }

    public static void SetCameraTarget(Transform trackingTarget, Transform lookAtTarget = null, bool useCustomLookAt = true)
    {
        if (Instance.cinemaCamera == null)
        {
            Debug.LogError("CinemachineCamera가 설정되지 않았습니다.");
            return;
        }
        ICinemachineCamera currentCamera = Instance.cinemaBrain.ActiveVirtualCamera;
        Instance.cinemaCamera.Follow = trackingTarget;
        if (useCustomLookAt && lookAtTarget != null)
        {
            Instance.cinemaCamera.LookAt = lookAtTarget;
        }
        else
        {
            Instance.cinemaCamera.LookAt = trackingTarget;
        }
        Instance.cinemaCamera.Lens.FieldOfView = 30f;
        Instance.cinemaOrbitalFollow.VerticalAxis.Value = 0f;
        Instance.cinemaOrbitalFollow.HorizontalAxis.Value = 0f;
        Instance.cinemaCamera.transform.GetComponent<CinemachineRotationComposer>().TargetOffset = new Vector3(-2, 1, 0);
        MainCameraInputToggle(false);
        //Instance.ApplyBlend(currentCamera, Instance.cinemaCamera);
    }


    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

    }
}

using UnityEditor;
using UnityEngine;

public enum CameraType
{
    Orbit,
    Follow
}

[System.Serializable]
public class CameraPose
{
    public Vector3 position;
    public Vector3 rotation;

    public CameraPose(Vector3 position, Vector3 rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}

public class CameraManager : BaseManager<CameraManager>
{
    [SerializeField] private CameraType currentCameraType;
    [SerializeField] private CameraPose[] cameraPoses;

    [Header("오비트 설정")]
    [SerializeField] private Transform orbitTarget;
    [SerializeField] private Vector3 orbitTargetOffset = new Vector3(0f, 2.5f, 0f);
    [SerializeField] private float orbitDistance = 8f;
    [SerializeField] private Vector2 orbitSensitivity = new Vector2(10f, 10f);
    [SerializeField] private Vector2 orbitClamp = new Vector2(0.1f, 90f);
    [SerializeField] private float orbitYaw = 0f;
    [SerializeField] private float orbitPitch = 0f;

    [Header("팔로우 설정")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector2 followOffset = new Vector2(0f, 0f);

    [Header("마우스커서 설정")]
    [SerializeField] private CursorLockMode mouseCursor;
    [SerializeField] private bool mouseCursorVisible = false;
    private Camera mainCamera;

    public static Camera MainCamera => Instance.mainCamera;

    protected override void Awake()
    {
        base.Awake();
        mainCamera = Camera.main;
        InitCameraPoses();
        currentCameraType = CameraType.Orbit;
        mouseCursor = CursorLockMode.Locked;
        Cursor.lockState = mouseCursor;
        Cursor.visible = mouseCursorVisible;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftAlt))
        {
            CursorLock();
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            CursorUnLock();
        }

        if (mouseCursorVisible) return;

        switch (currentCameraType)
        {
            case CameraType.Orbit:
                UpdateOrbitCamera();
                break;
            case CameraType.Follow:
                UpdateFollowCamera();
                break;
        }
    }

    private void InitCameraPoses()
    {
        cameraPoses = new CameraPose[System.Enum.GetValues(typeof(CameraType)).Length];

        //cameraPoses[(int)CameraType.Main] = new CameraPose(
        //    new Vector3(0, 3, 0.6f),
        //    Vector3.zero
        //);

        //cameraPoses[(int)CameraType.Character] = new CameraPose(
        //    new Vector3(0, 4.5f, -8),
        //    new Vector3(10, 0, 0)
        //);

        //cameraPoses[(int)CameraType.LookCharacter] = new CameraPose(
        //    new Vector3(0, 2.1f, 4.2f),
        //    new Vector3(10, 180, 0)
        //);

        cameraPoses[(int)CameraType.Follow] = new CameraPose(
            new Vector3(0, 9, -10),
            new Vector3(30, 0, 0)
        );

        //cameraPoses[(int)CameraType.TopDown] = new CameraPose(
        //    new Vector3(0, 15, 0),
        //    new Vector3(90, 0, 0)
        //);
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

    public static void SetCameraActive(Transform target, CameraType type)
    {
        if (Instance == null || Instance.mainCamera == null) return;

        int index = (int)type;
        Instance.currentCameraType = type;

        if (type == CameraType.Orbit)
        {
            Instance.orbitTarget = target;
            return;
        }

        if (type == CameraType.Follow)
        {
            Instance.followTarget = target;
            return;
        }

        CameraPose pose = Instance.cameraPoses[index];
        Instance.mainCamera.transform.position = target.position + target.rotation * pose.position;
        Instance.mainCamera.transform.rotation = target.rotation * Quaternion.Euler(pose.rotation);
    }

    public void SetFollowOffset(Vector2 offset)
    {
        followOffset = offset;
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

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        // Default 상태를 제외한 모든 상태가 동일한 처리
        if (newState == GameSystemState.Inventory ||
            newState == GameSystemState.StatusUI ||
            newState == GameSystemState.DialogueState ||
            newState == GameSystemState.Pause ||
            newState == GameSystemState.GameOver ||
            newState == GameSystemState.QuestReview ||
            newState == GameSystemState.PetInteraction)
        {
            CursorLock();
        }
        else
        {
            CursorUnLock();
        }
    }

}
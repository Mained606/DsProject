using UnityEngine;

public class ClimbController : MonoBehaviour
{
    [Header("이동 속도 설정")]
    public float climbSpeed = 3f;
    public float rotateSpeed = 5f;

    [Header("감지 거리")]
    public float detectionDistance = 0.5f;

    private bool isClimbing = false;
    private bool isHanging = false;
    private Vector3 climbNormal;

    private CharacterController controller;
    private Animator animator;
    private ClimbDetection climbDetector;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        climbDetector = GetComponent<ClimbDetection>();
    }

    void Update()
    {
        if (isClimbing)
        {
            ClimbMovement();
        }
        else if (isHanging)
        {
            HangingMovement();
        }
        else
        {
            CheckForClimbableSurface();
        }
    }

    void CheckForClimbableSurface()
    {
        if (climbDetector.CheckForWall(out climbNormal))
        {
            StartClimbing();
        }
    }

    void StartClimbing()
    {
        isClimbing = true;
        animator.SetBool("isClimbing", true);
        AlignToWall();
    }

    void ClimbMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 moveDirection = (transform.right * horizontal + transform.up * vertical).normalized;
        controller.Move(moveDirection * climbSpeed * Time.deltaTime);

        // 🔹 발 위치에 벽이 없으면 매달리기로 전환
        if (!climbDetector.CheckForFootWall() || climbDetector.CheckForEdge())
        {
            StartHanging();
        }

        // 🔹 벽이 너무 기울어져 있으면 발이 떨어짐
        if (climbDetector.IsWallTooSteep(climbNormal))
        {
            StartHanging();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            JumpOffWall();
        }
    }

    void StartHanging()
    {
        isClimbing = false;
        isHanging = true;
        animator.SetBool("isHanging", true);
    }

    void HangingMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");

        Vector3 moveDirection = transform.right * horizontal;
        controller.Move(moveDirection * climbSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            PullUp();
        }
    }

    void AlignToWall()
    {
        Quaternion targetRotation = Quaternion.LookRotation(-climbNormal);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotateSpeed * Time.deltaTime);
    }

    void JumpOffWall()
    {
        isClimbing = false;
        animator.SetTrigger("JumpOff");
        controller.Move(transform.forward * -1.5f);
    }

    void PullUp()
    {
        isHanging = false;
        animator.SetTrigger("PullUp");
    }
}

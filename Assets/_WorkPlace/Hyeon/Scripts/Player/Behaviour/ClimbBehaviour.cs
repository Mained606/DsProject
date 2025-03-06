using UnityEngine;

public class ClimbBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    private IKTest playerIK;
    private float bodyOffset = 0.3f;

    private float detectionRange = 1f;
    private float climbRange = 0.5f;
    private Vector3 currentCliffNormal;
    private Vector3 moveInput;
    private Vector3 inspectionPositionWeight = new Vector3(0.5f, 1f, 0f);
    private RaycastHit ray;
    private Vector3 origin;
    private Vector3 inspectionPoint;

    private LayerMask layer;

    public ClimbBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
        playerIK = controller.GetComponentInChildren<IKTest>();
        layer = ~LayerMask.GetMask("Ds Player");
    }
    public void Enter()
    {
        controller.isClimb = false;
        animator.SetBool("Climb", false);
        animator.SetBool("ClimbUp", false);
        layer = ~LayerMask.GetMask("Ds Player");
    }

    public void Execute()
    {
        if (!controller.isClimb)
        {
            // 절벽 검사
            DetectCliff();
        }
        else
        {
            HandleClimbMovement();
        }
    }

    public void Exit()
    {
        controller.isClimb = false;
        animator.SetBool("Climb", false);
        animator.SetBool("ClimbUp", false);
    }

    private void DetectCliff()
    {
        Vector3 rayOrigin = controller.transform.position + Vector3.up * 3f;
        if (Physics.Raycast(rayOrigin, controller.transform.forward, out RaycastHit hit, detectionRange, layer))
        {
            Debug.DrawLine(rayOrigin, hit.point, Color.red);
            Debug.Log($"hit.point: {hit.transform.name}");
            if (hit.transform.gameObject.layer == 10) return;   // 몬스터 감지시 return

            float angle = Vector3.Angle(hit.normal, Vector3.up);

            float heightDifference = hit.point.y - controller.transform.position.y;

            if (Mathf.Abs(heightDifference) < 0.01f)
            {
                heightDifference = 0f;
            }
            if (angle > 75f && angle < 105f)
            {
                if (hit.distance <= climbRange)
                {
                    currentCliffNormal = hit.normal;
                    StartClimb(hit.point);
                    Debug.Log("StartClimbing");
                }
            }
            else
            {
                Debug.Log("각도가 너무 가파르거나 완만함");
            }
        }
        else
        {
            //playerAnimator.SetBool("Climb", false);
            //CheckClimbEnd();
        }
    }

    private void StartClimb(Vector3 climbStartPosition)
    {
        animator.SetBool("Climb", true);
        controller.isClimb = true;
        animator.SetBool("Freefall", false);
        controller.isFreefall = false;
        animator.SetBool("Jump", false);
        controller.isJumping = false;
        //animator.SetBool("Glide", false);
        //controller.isGliding = false;

        controller.transform.position = climbStartPosition;

        PlayerBehaviourManager.Instance.CanMove = false;
    }

    private void EndClimb()
    {
        animator.SetBool("Climb", false);
        controller.isClimb = false;

        PlayerBehaviourManager.Instance.CanMove = true;
    }

    private void HandleClimbMovement()
    {
        moveInput = InputManager.InputActions.actions["Move"].ReadValue<Vector2>();
        origin = controller.transform.position + Vector3.up * 1.5f;
        Vector3 climbUp = controller.transform.up * moveInput.y; // 위/아래 이동
        Vector3 climbSide = controller.transform.right * moveInput.x; // 좌/우 이동

        Vector3 climbDirection = climbUp + climbSide;

        if(climbDirection != Vector3.zero)
        {
            // 해당 방향의 벽이 이동 가능한 절벽인지 검사
            if (VaildWallInspection(climbDirection))
            {
                // 이동 및 회전
                //controller.characterController.Move(climbDirection * 2f * Time.deltaTime);
            }
        }

        if(Physics.Raycast(origin, controller.transform.forward, 1f, layer))
        {

        }
        else
        {
            EndClimb();
        }
    }

    private bool VaildWallInspection(Vector3 inspectionPosition)
    {
        inspectionPosition.x *= inspectionPositionWeight.x;
        inspectionPosition.y *= inspectionPositionWeight.y;

        RaycastHit hit;


        Vector3 inspectionPoint = origin + inspectionPosition;
        Vector3 targetDirection = controller.transform.position + controller.transform.forward * 1f + Vector3.up * 1.5f;
        Vector3 direction = (targetDirection - inspectionPoint).normalized;

        if(Physics.Raycast(inspectionPoint, controller.transform.forward, out hit, 1f, layer))
        {
            Debug.DrawRay(inspectionPoint, controller.transform.forward, Color.green);
            // Todo : 추가 검사. 벽 같은 게 감지는 됐는데, 실제 매달릴 수 있는가, 똑같이 각도 검사로 들어갈지.
            ray = hit;
            return true;
        }
        else
        {
            // 직각에 가깝게 꺾이는 벽인지 확인(Ray를 안쪽으로 쏴서 감지)
            if (Physics.Raycast(inspectionPoint, direction, out hit, 1f, layer))
            {
                Debug.DrawRay(inspectionPoint, direction, Color.green);
                Debug.Log("직각에 가깝게 꺾이는 벽");
                ray = hit;
                return true;
            }
            return false;
                
        }
    }

    private void AlignToWall(RaycastHit ray)
    {
        Vector3 wallNormal = ray.normal;
        Vector3 wallUp = Vector3.Cross(controller.transform.right, wallNormal).normalized;
        //Vector3 wallForward = Vector3.Cross(Vector3.up, wallNormal).normalized;

        //Quaternion targetRotation = Quaternion.LookRotation(-wallNormal, Vector3.up);
        //controller.transform.rotation = Quaternion.Lerp(controller.transform.rotation, targetRotation, Time.deltaTime * 5f);

        //Vector3 targetPosition = ray.point + wallNormal * bodyOffset;
        //controller.transform.position = Vector3.Lerp(controller.transform.position, targetPosition, Time.deltaTime * 5f);

        controller.transform.position = ray.point + ray.normal * 0.5f; // 약간 띄운다.
        Quaternion surfaceRotation = Quaternion.LookRotation(-ray.normal, Vector3.up);
        controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, surfaceRotation, Time.deltaTime * 5f);
    }
}

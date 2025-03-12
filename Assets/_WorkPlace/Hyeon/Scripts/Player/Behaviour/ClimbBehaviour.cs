using System.Collections;
using UnityEngine;

public class ClimbBehaviour : MonoBehaviour, IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    private IKTest playerIK;
    private float bodyOffset = 0.3f;

    private float detectionRange = 2f;
    private float climbSpeed;
    private float rotationSpeed = 5f;
    private float climbRange = 0.5f;
    private float wallDistOffset = 0.2f;
    private float moveDistance;
    private Vector2 moveInput;
    private Vector3 inspectionPositionWeight = new Vector3(0.5f, 1f, 0f);
    private Vector3 wallNormal;
    private Vector3 targetWallNormal;
    private Vector3 origin;

    private LayerMask layer;

    private bool isTransitioning;

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
        climbSpeed = controller.playerData.moveSpeed;
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
            if (isTransitioning)
            {
                wallNormal = Vector3.Slerp(wallNormal, targetWallNormal, Time.deltaTime * rotationSpeed);

                RotatePlayerToWall();

                //Debug.Log($"현재 wallNormal: {wallNormal}");

                // 일정 각도 이상 회전하면 정지
                if (Vector3.Angle(wallNormal, targetWallNormal) < 0.5f)
                {
                    wallNormal = targetWallNormal; // 정확한 값으로 설정
                    isTransitioning = false; // 전환 완료
                    Debug.Log("✅ 벽 전환 완료");
                }
            }
            else
            {
                HandleClimbMovement();
            }
        }
    }

    public void Exit()
    {
        controller.isClimb = false;
        animator.SetBool("Climb", false);
        animator.SetBool("ClimbUp", false);
    }

    // 절벽 검사
    private void DetectCliff()
    {
        Vector3 detectRayOrigin = controller.transform.position + Vector3.up * 3f;
        if(Physics.Raycast(detectRayOrigin, controller.transform.forward, out RaycastHit hit, detectionRange, layer))
        {
            Debug.DrawLine(detectRayOrigin, hit.point, Color.red);

            if (hit.transform.gameObject.layer == 10) return;   // 레이가 몬스터에게 맞으면 return

            Vector3 wallDirection = Vector3.ProjectOnPlane(hit.normal, Vector3.up).normalized;
            float wallAngle = Vector3.Angle(wallDirection, Vector3.up);

            if(wallAngle > 75f && wallAngle < 105f)
            {
                if(hit.distance <= climbRange)
                {
                    StartClimb(hit.point);
                }
            }
            else
            {
                Debug.Log("기어 오를 수 없는 각도");
            }
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

    private void HandleClimbMovement()
    {
        origin = controller.transform.position + Vector3.up * 1.5f;
        UpdateWallNormal();
        if(wallNormal != Vector3.zero)
        {
            AlignToWallAndAdjustTilt();
            MaintainDistanceFromWall();
        }

        moveInput = InputManager.InputActions.actions["Move"].ReadValue<Vector2>();

        if (moveInput != Vector2.zero)
        {
            Vector3 right = Vector3.Cross(wallNormal, Vector3.up).normalized;
            Vector3 moveDir = (right * moveInput.x + Vector3.Cross(right, wallNormal) * moveInput.y).normalized;

            controller.characterController.Move(moveDir * climbSpeed * Time.deltaTime);

            DetectInnerWall();
        }
    }

    private void UpdateWallNormal()
    {
        if (Physics.Raycast(origin, controller.transform.forward, out RaycastHit hit, 2f, layer))
        {
            wallNormal = hit.normal;
        }
    }

    private void AlignToWallAndAdjustTilt()
    {
        // 벽의 방향을 향해 회전
        Vector3 forward = -wallNormal;

        // 벽과 수평에 가까운 경우 Up 벡터를 회전
        Vector3 referenceUp = (Mathf.Abs(Vector3.Dot(wallNormal, Vector3.up)) > 0.9f) ? Vector3.right : Vector3.up;

        // 벽의 수평 벡터와 Up 벡터를 기준으로 몸의 회전 각도를 맞추기
        Vector3 right = Vector3.Cross(referenceUp, forward).normalized;
        Vector3 adjustedUp = Vector3.Cross(forward, right).normalized;

        Quaternion targetRotation = Quaternion.LookRotation(forward, adjustedUp);
        controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);

        // 벽에 맞춰 몸 기울이기
        AdjustBodyTilt(forward);
    }

    // 몸을 기울이는 함수
    private void AdjustBodyTilt(Vector3 forward)
    {
        // 벽의 기울기에 따라 몸을 기울이기
        float tiltAngle = Vector3.Angle(forward, Vector3.forward);

        // 일정 각도 이상 기울어진 경우 몸을 기울이도록 설정
        if (tiltAngle > 5f) // 예: 30도 이상 기울어지면 기울임
        {
            // 기울어지는 각도에 따라 몸의 자세 조정
            Vector3 tiltDirection = Vector3.Cross(forward, Vector3.up).normalized;
            controller.transform.Rotate(tiltDirection, tiltAngle * Time.deltaTime);
        }
    }

    private void MaintainDistanceFromWall()
    {
        // 캐릭터 위치와 벽 사이의 거리 계산
        if (Physics.Raycast(origin, controller.transform.forward, out RaycastHit hit, 2f, layer))
        {
            float currentDistance = hit.distance;  // 벽과 캐릭터 간의 현재 거리

            // 벽에서 벗어난 거리만큼 벽 쪽으로 밀거나 벽에서 멀어지도록 함
            float distanceDifference = currentDistance - wallDistOffset;

            // 벽과의 간격이 지정된 오프셋 값과 맞지 않으면 조정
            if (Mathf.Abs(distanceDifference) > 0.1f)
            {
                // 벽 법선 방향으로 밀거나 당기기
                Vector3 direction = (distanceDifference > 0) ? wallNormal : -wallNormal;
                moveDistance = Mathf.Min(moveDistance, 0.2f);

                // 오프셋 거리를 맞추기 위해 이동
                controller.characterController.Move(direction * moveDistance);
                Debug.Log("벽과의 거리 조절");
            }
        }
    }

    private void DetectInnerWall()
    {
        // 기존 벽의 방향
        Vector3 right = Vector3.Cross(wallNormal, Vector3.up).normalized;

        // 좌우 벽 감지
        bool leftHit = Physics.Raycast(origin, -right, out RaycastHit hitLeft, 1f, layer);
        bool rightHit = Physics.Raycast(origin, right, out RaycastHit hitRight, 1f, layer);

        // 디버그용 레이 그리기
        Debug.DrawRay(origin, -right * 1f, leftHit ? Color.red : Color.green, 0.1f);
        Debug.DrawRay(origin, right * 1f, rightHit ? Color.red : Color.green, 0.1f);

        if (leftHit && moveInput.x < -0.1f)
        {
            if(hitLeft.collider != null)
            {
                StartWallTransition(hitLeft.normal);
            }
            else
            {
                Debug.Log("왼쪽 벽이 감지되었지만, hitLeft가 NULL");
            }
        }
        // 오른쪽 벽이 감지되었고, 플레이어가 오른쪽으로 이동 중이면
        else if (rightHit && moveInput.x > 0.1f)
        {
            if(hitRight.collider != null)
            {
                Debug.Log($"오른쪽 벽으로 전환 시작, Normal:{hitRight.normal}");
                StartWallTransition(hitRight.normal);
            }
            else
            {
                Debug.Log("오른쪽 벽이 감지되었지만, hitRight가 NULL");
            }
        }
    }

    public void StartWallTransition(Vector3 newWallNormal)
    {
        Debug.Log($"🟢 StartWallTransition 호출됨: New Normal {newWallNormal}");

        targetWallNormal = newWallNormal; // 목표 노멀 설정
        isTransitioning = true; // 전환 시작
    }

    private void RotatePlayerToWall()
    {
        Vector3 forward = -wallNormal;

        // 플레이어가 회전해야 할 방향을 계산
        Quaternion targetRotation = Quaternion.LookRotation(forward, Vector3.up);

        // 회전 속도를 적용하여 플레이어의 Transform을 회전시킴
        controller.transform.rotation = Quaternion.Slerp(controller.transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
    }

    private bool VaildWallInspection(Vector3 inspectionPosition)
    {
        inspectionPosition.x *= inspectionPositionWeight.x;
        inspectionPosition.y *= inspectionPositionWeight.y;

        RaycastHit hit;


        Vector3 inspectionPoint = origin + inspectionPosition;
        Vector3 targetDirection = controller.transform.position + controller.transform.forward * 1f + Vector3.up * 1.5f;
        Vector3 direction = (targetDirection - inspectionPoint).normalized;

        if(Physics.SphereCast(inspectionPoint, 0.2f, controller.transform.forward, out hit, 1f, layer))
        {
            Debug.DrawRay(inspectionPoint, controller.transform.forward, Color.green);
            // Todo : 추가 검사. 벽 같은 게 감지는 됐는데, 실제 매달릴 수 있는가, 똑같이 각도 검사로 들어갈지.
            //ray = hit;
            return true;
        }
        else
        {
            // 직각에 가깝게 꺾이는 벽인지 확인(Ray를 안쪽으로 쏴서 감지)
            if (Physics.SphereCast(inspectionPoint, 0.2f, direction, out hit, 1f, layer))
            {
                Debug.DrawRay(inspectionPoint, direction, Color.green);
                Debug.Log("직각에 가깝게 꺾이는 벽");
                //ray = hit;
                return true;
            }
            return false;
                
        }
    }

    private void AlignToWall()
    {
        if(Physics.Raycast(origin, controller.transform.forward, out RaycastHit ray, 1f, layer))
        {
            controller.transform.forward = -ray.normal;
            controller.transform.position = Vector3.Lerp(controller.transform.position, ray.point + ray.normal * 0.51f, climbSpeed * Time.fixedDeltaTime);
        }
    }

    //private void DetectCliff()
    //{
    //    Vector3 rayOrigin = controller.transform.position + Vector3.up * 3f;
    //    if (Physics.Raycast(rayOrigin, controller.transform.forward, out RaycastHit hit, detectionRange, layer))
    //    {
    //        Debug.DrawLine(rayOrigin, hit.point, Color.red);
    //        Debug.Log($"hit.point: {hit.transform.name}");
    //        if (hit.transform.gameObject.layer == 10) return;   // 몬스터 감지시 return

    //        float angle = Vector3.Angle(hit.normal, Vector3.up);

    //        float heightDifference = hit.point.y - controller.transform.position.y;

    //        if (Mathf.Abs(heightDifference) < 0.01f)
    //        {
    //            heightDifference = 0f;
    //        }
    //        if (angle > 75f && angle < 105f)
    //        {
    //            if (hit.distance <= climbRange)
    //            {
    //                //currentCliffNormal = hit.normal;
    //                StartClimb(hit.point);
    //                //Debug.Log("StartClimbing");
    //            }
    //        }
    //        else
    //        {
    //            Debug.Log("각도가 너무 가파르거나 완만함");
    //        }
    //    }
    //    else
    //    {
    //        //playerAnimator.SetBool("Climb", false);
    //        //CheckClimbEnd();
    //    }
    //}

    //private void HandleClimbMovement()
    //{
    //    moveInput = InputManager.InputActions.actions["Move"].ReadValue<Vector2>();
    //    origin = controller.transform.position + Vector3.up * 1.5f;
    //    Vector3 climbUp = controller.transform.up * moveInput.y; // 위/아래 이동
    //    Vector3 climbSide = controller.transform.right * moveInput.x; // 좌/우 이동

    //    Vector3 climbDirection = climbUp + climbSide;

    //    AlignToWall();

    //    if (climbDirection != Vector3.zero)
    //    {
    //        // 해당 방향의 벽이 이동 가능한 절벽인지 검사
    //        if (VaildWallInspection(climbDirection))
    //        {
    //            // 이동 및 회전
    //            //controller.characterController.Move(climbDirection * 2f * Time.deltaTime);
    //            //climbDirection = Vector3.ProjectOnPlane(climbDirection, currentCliffNormal);

    //            //controller.characterController.Move(climbDirection * climbSpeed * Time.deltaTime);
    //        }
    //    }

    //    if(Physics.SphereCast(origin, 0.2f, controller.transform.forward, out RaycastHit hit, 1f, layer))
    //    {
    //        //controller.transform.forward = -hit.normal;
    //    }
    //    else
    //    {
    //        EndClimb();
    //    }
    //}

    private void EndClimb()
    {
        animator.SetBool("Climb", false);
        controller.isClimb = false;

        PlayerBehaviourManager.Instance.CanMove = true;
    }
}

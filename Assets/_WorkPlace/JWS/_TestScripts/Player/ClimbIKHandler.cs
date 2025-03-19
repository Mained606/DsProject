using UnityEngine;

public class ClimbIKHandler : MonoBehaviour
{
    private Animator animator;
    private bool enableIK = false;

    public Transform leftHandTarget;
    public Transform rightHandTarget;
    public Transform leftFootTarget;
    public Transform rightFootTarget;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (!enableIK) return;

        SetIK(AvatarIKGoal.LeftHand, leftHandTarget);
        SetIK(AvatarIKGoal.RightHand, rightHandTarget);
        SetIK(AvatarIKGoal.LeftFoot, leftFootTarget);
        SetIK(AvatarIKGoal.RightFoot, rightFootTarget);
    }

    void SetIK(AvatarIKGoal goal, Transform target)
    {
        if (target)
        {
            animator.SetIKPositionWeight(goal, 1);
            animator.SetIKRotationWeight(goal, 1);
            animator.SetIKPosition(goal, target.position);
            animator.SetIKRotation(goal, target.rotation);
        }
    }

    public void EnableIK(bool enable)
    {
        enableIK = enable;
    }

    public void UpdateIKTargets()
    {
        // 손과 발의 위치를 현재 벽에 맞춰 업데이트
        RaycastHit hit;
        if (Physics.Raycast(leftHandTarget.position, transform.forward, out hit, 0.5f))
        {
            leftHandTarget.position = hit.point;
        }
        if (Physics.Raycast(rightHandTarget.position, transform.forward, out hit, 0.5f))
        {
            rightHandTarget.position = hit.point;
        }
        if (Physics.Raycast(leftFootTarget.position, transform.forward, out hit, 0.5f))
        {
            leftFootTarget.position = hit.point;
        }
        if (Physics.Raycast(rightFootTarget.position, transform.forward, out hit, 0.5f))
        {
            rightFootTarget.position = hit.point;
        }
    }
}

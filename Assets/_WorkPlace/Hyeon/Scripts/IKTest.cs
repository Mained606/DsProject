using UnityEngine;
using UnityEngine.Splines;

public class IKTest : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;
    public LayerMask LayerMask;

    [SerializeField] private Transform leftToe;
    [SerializeField] private Transform rightToe;

    [SerializeField] private Vector3 leftHandOffset;
    [SerializeField] private Vector3 rightHandOffset;
    
    private void Start()
    {
        playerAnimator = GetComponent<Animator>();   
    }

    private void Update()
    {

    }

    //private void LateUpdate()
    //{
    //    AdjustToeIK(leftToe);
    //    AdjustToeIK(rightToe);
    //}

    [Range(0f, 1f)]
    public float DistanceToGround;

    [Range(0f, 1f)]
    public float DistanceToWall;

    private void OnAnimatorIK(int layerIndex)
    {
        if (playerAnimator)
        {
            playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftFoot, playerAnimator.GetFloat("IKLeftFootWeight"));
            playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftFoot, playerAnimator.GetFloat("IKLeftFootWeight"));
            playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightFoot, playerAnimator.GetFloat("IKRightFootWeight"));
            playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightFoot, playerAnimator.GetFloat("IKRightFootWeight"));

            //playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, 1f);
            //playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, 1f);
            //playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, 1f);
            //playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, 1f);

            playerAnimator.SetIKPositionWeight(AvatarIKGoal.LeftHand, playerAnimator.GetFloat("IKLeftHandWeight"));
            playerAnimator.SetIKRotationWeight(AvatarIKGoal.LeftHand, playerAnimator.GetFloat("IKLeftHandWeight"));
            playerAnimator.SetIKPositionWeight(AvatarIKGoal.RightHand, playerAnimator.GetFloat("IKRightHandWeight"));
            playerAnimator.SetIKRotationWeight(AvatarIKGoal.RightHand, playerAnimator.GetFloat("IKRightHandWeight"));

            AdjustFootIK(AvatarIKGoal.LeftFoot);
            AdjustFootIK(AvatarIKGoal.RightFoot);
            AdjustHandIK(AvatarIKGoal.LeftHand);
            AdjustHandIK(AvatarIKGoal.RightHand);

        }
    }

    private void AdjustFootIK(AvatarIKGoal foot)
    {
        RaycastHit hit;
        Vector3 footPosition = playerAnimator.GetIKPosition(foot);
        Ray ray = new Ray(footPosition + Vector3.up, Vector3.down);

        if(Physics.Raycast(ray, out hit, DistanceToGround + 1.7f, LayerMask))
        {
            if (hit.transform.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                footPosition = hit.point;
                footPosition.y += DistanceToGround;
                playerAnimator.SetIKPosition(foot, footPosition);

                Quaternion footRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
                playerAnimator.SetIKRotation(foot, footRotation * transform.rotation);
            }
        }
    }

    private void AdjustHandIK(AvatarIKGoal hand)
    {
        RaycastHit hit;
        Vector3 handPosition = playerAnimator.GetIKPosition(hand);
        Vector3 offset;

        Ray ray = new Ray(handPosition + transform.forward * -0.5f, transform.forward);

        if(Physics.Raycast(ray, out hit, DistanceToWall + 1f, LayerMask))
        {
            //Debug.Log($"{hand}손 레이 맞는 중: {hit.transform.name}");
            if(hand == AvatarIKGoal.LeftHand)
            {
                offset = leftHandOffset;
            }
            else
            {
                offset = rightHandOffset;
            }
            handPosition = hit.point;
            //handPosition = new Vector3(hit.point.x + offset.x, hit.point.y + offset.y, handPosition.z);
            //handPosition.z += DistanceToWall;
            playerAnimator.SetIKPosition(hand, handPosition);

            Quaternion handRotation = Quaternion.FromToRotation(Vector3.up, hit.normal);
            playerAnimator.SetIKRotation(hand, handRotation * transform.rotation);
        }
    }

    private void AdjustToeIK(Transform toe)
    {
        RaycastHit hit;
        Vector3 toePosition = toe.position;
        Ray ray = new Ray(toePosition + Vector3.up, Vector3.down);

        if (Physics.Raycast(ray, out hit, DistanceToGround + 1f, LayerMask))
        {
            if (hit.transform.CompareTag("Ground"))
            {
                toePosition = hit.point;
                toePosition.y += DistanceToGround;
                toe.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal) * toe.rotation;
            }
            toe.position = hit.point + Vector3.up;
            
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.up * 1f);
    }
}

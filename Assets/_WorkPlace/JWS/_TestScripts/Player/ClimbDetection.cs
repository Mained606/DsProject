using UnityEngine;

public class ClimbDetection : MonoBehaviour
{
    [Header("감지 설정")]
    public float wallDetectionRange = 1.2f;
    public float edgeDetectionRange = 0.5f;
    public float footCheckDistance = 0.2f;

    public LayerMask climbableLayer;

    public bool CheckForWall(out Vector3 wallNormal)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, transform.forward, out hit, wallDetectionRange, climbableLayer))
        {
            wallNormal = hit.normal;
            return true;
        }
        wallNormal = Vector3.zero;
        return false;
    }

    public bool CheckForEdge()
    {
        Vector3 upperPoint = transform.position + transform.up * edgeDetectionRange;
        return !Physics.Raycast(upperPoint, transform.forward, edgeDetectionRange, climbableLayer);
    }

    public bool CheckForFootWall()
    {
        Vector3 footPosition = transform.position - transform.up * 0.5f;
        return Physics.Raycast(footPosition, transform.forward, footCheckDistance, climbableLayer);
    }

    public bool IsWallTooSteep(Vector3 wallNormal)
    {
        float wallAngle = Vector3.Angle(wallNormal, Vector3.up);
        return wallAngle > 100f;
    }
}

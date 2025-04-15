using System.Collections;
using UnityEngine;

public class FishMovement : MonoBehaviour
{
    private float movingRange = 10f;
    private float arrivedDistance = 1f;
    private float turnSpeed = 2f;
    private bool isWait = false;
    [SerializeField] private float minSpeed = 0.5f;
    [SerializeField] private float maxSpeed = 1.5f;
    private float validHeight = 1f;

    [SerializeField] private Vector3 originalPosition;
    [SerializeField] private Vector3 targetPosition;

    [SerializeField] private float minScale;
    [SerializeField] private float maxScale;
    [SerializeField] private float randomScale;

    private void Start()
    {
        originalPosition = transform.position;

        SetNextDestination();

        if (minScale == 0 || maxScale == 0) return;
        randomScale = Random.Range(minScale, maxScale);
        transform.localScale = new Vector3(randomScale, randomScale, randomScale);
    }

    private void Update()
    {
        if (isWait) return;

        Vector3 horizontalPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 horizontalTarget = new Vector3(targetPosition.x, 0, targetPosition.z);
        float distance = Vector3.Distance(horizontalPosition, horizontalTarget);

        if (distance < arrivedDistance)
        {
            StartCoroutine(WaitAfterSwam());
            return;
        }

        Swim();
    }

    private void SetNextDestination()
    {
        Vector3 randomPosition = Vector3.zero;
        int safetyCounter = 0;
        float waterSurfaceY;

        while (safetyCounter < 10)
        {
            Vector3 candidate = originalPosition + new Vector3(
                Random.Range(-movingRange, movingRange),
                0f,
                Random.Range(-movingRange, movingRange)
            );
            safetyCounter++;

            if (IsOnWater(candidate, out waterSurfaceY) &&
                Vector3.Distance(transform.position, candidate) > arrivedDistance * 2f)
            {
                randomPosition = candidate;
                randomPosition.y = waterSurfaceY - Random.Range(1.5f, 2.0f);
                break;
            }
        }

        targetPosition = randomPosition;
    }

    //private bool IsOnWater(Vector3 position, out float surfaceY)
    //{
    //    surfaceY = 0f;
    //    Ray ray = new Ray(position + Vector3.up * 10f, Vector3.down);

    //    if (Physics.Raycast(ray, out RaycastHit hit, 20f, LayerMask.GetMask("Water")))
    //    {
    //        surfaceY = hit.point.y;
    //        return true;
    //    }
    //    return false;
    //}

    private bool IsOnWater(Vector3 position, out float surfaceY)
    {
        surfaceY = 0f;
        Ray ray = new Ray(position + Vector3.up * 10f, Vector3.down);

        if (Physics.Raycast(ray, out RaycastHit waterHit, 20f, LayerMask.GetMask("Water")))
        {
            surfaceY = waterHit.point.y;

            Ray groundRay = new Ray(waterHit.point + Vector3.up * 0.5f, Vector3.down);
            if (Physics.Raycast(groundRay, out RaycastHit groundHit, 5f, LayerMask.GetMask("Ground")))
            {
                float distanceBetween = waterHit.point.y - groundHit.point.y;

                if (distanceBetween >= validHeight)
                {
                    return true;
                }
            }
        }
        return false;
    }

    private void Swim()
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        float moveSpeed = Random.Range(minSpeed, maxSpeed);

        Vector3 nextPosition = transform.position + direction * moveSpeed * Time.deltaTime;

        transform.position = nextPosition;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    private IEnumerator WaitAfterSwam()
    {
        isWait = true;
        float randomTime = Random.Range(1, 3);
        yield return new WaitForSeconds(randomTime);
        isWait = false;
        SetNextDestination();
    }
}

using System.Collections;
using UnityEngine;

public class WanderNpc : MonoBehaviour
{
    [SerializeField] private float walkingRange = 10f;
    [SerializeField] private float idleTime = 8f;
    [SerializeField] private float minMoveSpeed = 2f;
    [SerializeField] private float maxMoveSpeed = 5f;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float arrivedDistance = 0.5f;
    [SerializeField] private bool isMoving = false;

    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] private Vector3 targetPosition;

    private Animator animator;
    public string walkingState = "IsWalking";
    public string animationSpeed = "Speed";
    private Coroutine currentCoroutine = null;

    private void Start()
    {
        animator = GetComponent<Animator>();

        spawnPosition = transform.position;

        SetNextDestination();
    }

    private void Update()
    {
        if(Vector3.Distance(transform.position, targetPosition) <= arrivedDistance)
        {
            if(currentCoroutine == null)
            {
                currentCoroutine = StartCoroutine(IdleAfterWalking());
            }
        }
        else
        {
            if(animator.GetBool(walkingState))
            {
                MoveToWards(targetPosition);
            }
        }
    }

    //이동 위치 설정
    private void SetNextDestination()
    {
        Vector3 randomPosition = spawnPosition + new Vector3(Random.Range(-walkingRange, walkingRange), 0f, Random.Range(-walkingRange, walkingRange));

        targetPosition = randomPosition;
        animator.SetBool(walkingState, true);
    }

    //이동 후 잠시 멈춤
    private IEnumerator IdleAfterWalking()
    {
        animator.SetBool(walkingState, false);
        isMoving = false;

        yield return new WaitForSeconds(idleTime);
        SetNextDestination();

        currentCoroutine = null;
    }

    //이동
    private void MoveToWards(Vector3 destination)
    {
        if(!isMoving)
        {
            moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

            animator.SetFloat(animationSpeed, moveSpeed);

            isMoving = true;
        }
        
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        Vector3 direction = (destination - transform.position).normalized;
        direction.y = 0f;

        if(direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }
}

using System.Collections;
using UnityEngine;

public class WanderNpc : MonoBehaviour
{
    [SerializeField] private float walkingRange = 10f;
    [SerializeField] private float idleTime = 5f;
    [SerializeField] private float minMoveSpeed = 2f;
    [SerializeField] private float maxMoveSpeed = 5f;
    [SerializeField] private float moveSpeed;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private float arrivedDistance = 2f;
    [SerializeField] private bool isMoving = false;

    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] private Vector3 targetPosition;

    private Animator animator;
    public string walkingState = "IsWalking";
    public string talkingState = "IsTalking";
    public string animationSpeed = "Speed";
    public string exitTrigger = "ExitTrigger";
    private Coroutine currentCoroutine = null;

    [SerializeField] private bool isTalking = false;
    private string[] talkingTriggers = { "Talking01Trigger", "Talking02Trigger", "Talking03Trigger" };
    private float talkingDuration = 2f;
    private Transform targetNpc;

    private void Start()
    {
        animator = GetComponent<Animator>();

        spawnPosition = transform.position;

        SetNextDestination();
    }

    private void Update()
    {
        if (isTalking)
        {
            return;
        }

        if (Vector3.Distance(transform.position, targetPosition) <= arrivedDistance)
        {
            if (currentCoroutine == null)
            {
                currentCoroutine = StartCoroutine(IdleAfterWalking());
            }
        }
        else
        {
            if (animator.GetBool(walkingState))
            {
                MoveToWards(targetPosition);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            Debug.Log("npc 만남");
            targetNpc = other.transform;
            StartConversation();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
        {
            Debug.Log("npc 헤어짐");
            targetNpc = null;
            StopConversation();
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
        if (!isMoving)
        {
            moveSpeed = Random.Range(minMoveSpeed, maxMoveSpeed);

            animator.SetFloat(animationSpeed, moveSpeed);

            isMoving = true;
        }

        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        Vector3 direction = (destination - transform.position).normalized;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
            Debug.Log("회전중");
        }
    }

    //대화 시작
    private void StartConversation()
    {
        isTalking = true;

        if (animator.GetBool(walkingState))
        {
            animator.SetBool(walkingState, false);
        }
        animator.SetBool(talkingState, true);

        LookAtTarget(targetNpc);

        //대화 랜덤 선택
        PlayRandomTrigger(talkingTriggers);

        StartCoroutine(ContinueConversation());
    }

    //대화 진행
    private IEnumerator ContinueConversation()
    {
        while (isTalking)
        {
            yield return new WaitForSeconds(talkingDuration);

            if (Random.value <= 0.9)
            {
                PlayRandomTrigger(talkingTriggers);
            }
            else
            {
                isTalking = false;
            }
        }
        StopConversation();
    }

    //대화 종료
    private void StopConversation()
    {
        //isTalking = false;
        animator.SetTrigger(exitTrigger);
        animator.SetBool(talkingState, false);
        SetNextDestination();
        MoveToWards(targetPosition);
    }

    //트리거 랜덤 재생
    private void PlayRandomTrigger(string[] triggers)
    {
        int randomIndex = Random.Range(0, triggers.Length);
        string randomTrigger = triggers[randomIndex];

        if (randomTrigger != null)
        {
            animator.SetTrigger(randomTrigger);
        }
    }

    private void LookAtTarget(Transform target)
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f; // 회전할 때 y축 회전만 하도록 제한

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
    }
}
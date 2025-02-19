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
    [SerializeField] private float arrivedDistance = 1f;
    [SerializeField] private bool isMoving = false;

    [SerializeField] private Vector3 spawnPosition;
    [SerializeField] private Vector3 targetPosition;

    private Animator animator;
    private string walkingState = "IsWalking";
    private string talkingState = "IsTalking";
    private string animationSpeed = "Speed";
    private string exitTrigger = "ExitTrigger";
    private Coroutine currentCoroutine = null;

    [SerializeField] private bool isTalking = false;
    private string[] talkingTriggers = { "Talking01Trigger", "Talking02Trigger", "Talking03Trigger" };
    private float talkingDuration = 2f;
    private Transform targetNpc;

    //대화 쿨타임
    private float conversationCoolTime = 5f;
    private float lastConversationTime = 0f;

    //npc간의 거리가 너무 가까울 경우 경로 재설정
    private float minNpcDistance = 1f;
    private float nextDestinationTime = 0f;
    private float destinationCooldown = 1f;

    //
    private string sittingState = "IsSitting";
    [SerializeField] private bool isSitting = false;
    [SerializeField] private bool isSittingTalking = false;
    [SerializeField] private bool isJustStoodUp = false;
    public Vector3 sittingOffset = new Vector3(1, 0.2f, 0.5f);
    private string[] sittingTriggers = { "SittingTalkingTrigger", "SittingClapTrigger" };
    private Coroutine sittingTalkingCoroutine = null;

    [SerializeField] private float sittingCoolTime = 5f;
    private float lastSittingTime = 0f;

    private Rigidbody rb;

    private void Start()
    {
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        spawnPosition = transform.position;

        SetNextDestination();
    }

    private void Update()
    {
        if (isTalking || isSitting)
        {
            return;
        }

        Bench bench = GetCurrentBench();
        if (isMoving && (IsCloseNpcs(transform.position, minNpcDistance) || bench != null))
        {
            if(Time.time > nextDestinationTime && !isJustStoodUp)
            {
                Debug.Log($"{transform.name} 다른 npc와 너무 가까움, 새로운 목적지 설정");
                SetNextDestination();
                nextDestinationTime = Time.time + destinationCooldown;
            }            
        }

        //도착 판정할때 y축 위치는 고려하지 않음
        Vector3 horizontalPosition = new Vector3(transform.position.x, 0, transform.position.z);
        Vector3 horizontalTarget = new Vector3(targetPosition.x, 0, targetPosition.z);

        if (Vector3.Distance(horizontalPosition, horizontalTarget) <= arrivedDistance)
        {
            //y축 차이가 너무 크면 새로운 목적지 설정
            if(Mathf.Abs(transform.position.y - targetPosition.y) > 2f)
            {
                Debug.Log($"{transform.name} y축 차이가 큼, 새로운 목적지 설정");
                SetNextDestination();
                return;
            }

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

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.transform.CompareTag("TownNPC"))
        {
            if (isSitting)
            {
                Bench bench = GetCurrentBench();
                if (bench != null && !isSittingTalking)
                {
                    isSittingTalking = true;
                    StopCoroutine(SittingDuration(bench));
                    sittingTalkingCoroutine = StartCoroutine(SittingTalking(bench));
                }
            }
            else
            {
                if (Time.time - lastConversationTime < conversationCoolTime)
                {
                    Debug.Log($"{transform.name} 대화 쿨타임 중");
                    return;
                }

                Debug.Log($"{transform.name} npc 만남");
                targetNpc = collision.transform;
                StartConversation();
            }
        }

        if (collision.transform.name.Contains("Bench"))
        {
            if (Time.time - lastSittingTime < sittingCoolTime)
            {
                Debug.Log($"{transform.name} 앉기 쿨타임 중");
                return;
            }

            SittingAtBench(collision);
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.transform.CompareTag("TownNPC"))
        {
            Debug.Log($"{transform.name} npc 헤어짐");
            targetNpc = null;
            StopConversation();

            if (isSittingTalking && !IsCloseNpcs(transform.position, minNpcDistance))
            {
                Debug.Log($"{transform.name} 대화중인 npc 떠남");
                Bench bench = GetCurrentBench();

                if (sittingTalkingCoroutine != null)
                {
                    StopCoroutine(sittingTalkingCoroutine);
                    sittingTalkingCoroutine = null;
                }

                isSittingTalking = false;
                StartCoroutine(SittingDuration(bench));
            }
        }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    if (other.CompareTag("TownNPC"))
    //    {
    //        if (isSitting)
    //        {
    //            Bench bench = GetCurrentBench();
    //            if(bench != null && !isSittingTalking)
    //            {
    //                isSittingTalking = true;
    //                StopCoroutine(SittingDuration(bench));
    //                sittingTalkingCoroutine = StartCoroutine(SittingTalking(bench));
    //            }
    //        }
    //        else
    //        {
    //            if (Time.time - lastConversationTime < conversationCoolTime)
    //            {
    //                Debug.Log($"{transform.name} 대화 쿨타임 중");
    //                return;
    //            }

    //            Debug.Log($"{transform.name} npc 만남");
    //            targetNpc = other.transform;
    //            StartConversation();
    //        }
    //    }

    //    if (other.name.Contains("Bench"))
    //    {
    //        if(Time.time - lastSittingTime < sittingCoolTime)
    //        {
    //            Debug.Log($"{transform.name} 앉기 쿨타임 중");
    //            return;
    //        }

    //        SittingAtBench(other);
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.CompareTag("TownNPC"))
    //    {
    //        Debug.Log($"{transform.name} npc 헤어짐");
    //        targetNpc = null;
    //        StopConversation();

    //        if (isSittingTalking && !isCloseNpcs(transform.position, minNpcDistance))
    //        {
    //            Debug.Log($"{transform.name} 대화중인 npc 떠남");
    //            Bench bench = GetCurrentBench();

    //            if(sittingTalkingCoroutine != null)
    //            {
    //                StopCoroutine(sittingTalkingCoroutine);
    //                sittingTalkingCoroutine = null;
    //            }

    //            isSittingTalking = false;
    //            StartCoroutine(SittingDuration(bench));
    //        }
    //    }
    //}

    //이동 위치 설정
    private void SetNextDestination()
    {
        if (isSitting) return;

        Vector3 randomPosition;
        int safetyCounter = 0;

        if (isJustStoodUp)
        {
            Vector3 forwardDirection = transform.forward.normalized; //NPC가 바라보는 방향
            float forwardDistance = Random.Range(5f, walkingRange);
            randomPosition = transform.position + (forwardDirection * forwardDistance);

            StartCoroutine(JustStoodUp());
        }
        else
        {
            do
            {
                randomPosition = spawnPosition + new Vector3(Random.Range(-walkingRange, walkingRange), 0f, Random.Range(-walkingRange, walkingRange));
                safetyCounter++;
            }
            while (Vector3.Distance(transform.position, randomPosition) < arrivedDistance * 2f && safetyCounter < 10);
        }

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

        //현재 위치에서 목표 방향(y값 제외)
        Vector3 direction = (destination - transform.position).normalized;
        direction.y = 0f;

        //앞으로 이동할 위치
        Vector3 nextPosition = transform.position + direction * moveSpeed * Time.deltaTime;

        //Raycast로 지형 높이 감지
        if (Physics.Raycast(nextPosition + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 2f))
        {
            nextPosition.y = hit.point.y;
        }

        //이동 적용
        transform.position = nextPosition;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    //대화 시작
    private void StartConversation()
    {
        isTalking = true;
        isMoving = false;

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
        int minConversations = Random.Range(3,6);
        int conversationCount = 0;

        while (isTalking)
        {
            yield return new WaitForSeconds(talkingDuration);

            if (conversationCount < minConversations || Random.value <= 0.9)
            {
                PlayRandomTrigger(talkingTriggers);
                conversationCount++;
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
        if(isTalking)
        {
            isTalking = false;
        }

        lastConversationTime = Time.time;
        Debug.Log($"{transform.name} 대화 종료");

        animator.SetTrigger(exitTrigger);
        animator.SetBool(talkingState, false);

        SetNextDestination();
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
        direction.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
    }

    private bool IsCloseNpcs(Vector3 position, float minDistance)
    {
        Collider[] colliders = Physics.OverlapSphere(position, minDistance);

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("TownNPC") && collider.transform != this.transform)
            {
                return true;
            }
        }
        return false;
    }


    /// <summary>
    /// 의자가 근처에 있으면 앉기
    /// </summary>
    private void SittingAtBench(Collision collider)
    {
        Bench bench = collider.transform.GetComponent<Bench>();
        if (bench == null) return;

        Debug.Log($"{transform.name} 의자 인식");

        if (Random.value <= 0.5f)
        {
            if (!animator.GetBool(sittingState))
            {
                isSitting = true;

                if (animator.GetBool(walkingState))
                {
                    Debug.Log($"{transform.name} 걷기 멈춤");
                    animator.SetBool(walkingState, false);
                    isMoving = false;
                }

                if (rb != null)
                    rb.isKinematic = true;

                if(!bench.left)
                {
                    transform.position = bench.leftPosition.position;
                    bench.left = true;
                }
                else if(!bench.right)
                {
                    transform.position = bench.rightPosition.position;
                    bench.right = true;
                }
                else
                {
                    Debug.Log("벤치에 빈 자리 없음");
                    return;
                }
                
                transform.rotation = collider.transform.rotation;
                animator.SetBool(sittingState, true);

                StartCoroutine(SittingDuration(bench));
            }
        }
    }

    private IEnumerator SittingTalking(Bench bench)
    {
        int minConversations = Random.Range(3, 6);
        int conversationCount = 0;

        while (isSitting)
        {
            Debug.Log($"{transform.name} 앉은 상태로 대화 중");
            yield return new WaitForSeconds(Random.Range(5f, 10f));

            if(conversationCount < minConversations)
            {
                PlayRandomTrigger(sittingTriggers);
                conversationCount++;
            }
            else
            {
                isSitting = false;
                Debug.Log($"{transform.name} 앉기 종료");
            }
        }

        Debug.Log($"{transform.name} 앉은 상태에서 대화 종료");

        isSittingTalking = false;
        StopSitting(bench);
    }

    private IEnumerator SittingDuration(Bench bench)
    {
        float sittingTime = Random.Range(15f, 30f);
        Debug.Log($"{transform.name}이 {sittingTime}동안 앉아있음");
        yield return new WaitForSeconds(sittingTime);

        if(!isSittingTalking)
            StopSitting(bench);
    }

    private void StopSitting(Bench bench)
    {
        Debug.Log($"{transform.name}이 일어남");
        
        isSitting = false;
        animator.SetBool(sittingState, false);
        rb.isKinematic = false;

        if (transform.position == bench.leftPosition.position) bench.left = false;
        if (transform.position == bench.rightPosition.position) bench.right = false;
 
        lastSittingTime = Time.time;

        isJustStoodUp = true;
        SetNextDestination();
    }

    private Bench GetCurrentBench()
    {
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, 0.5f);
        foreach (Collider col in hitColliders)
        {
            Bench bench = col.GetComponent<Bench>();
            if (bench != null)
            {
                return bench;
            }
        }
        return null;
    }

    private IEnumerator JustStoodUp()
    {
        yield return new WaitForSeconds(5f);

        isJustStoodUp = false;
    }
}
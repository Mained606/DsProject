using System.Collections;
using UnityEngine;

public class ActivityNpc : MonoBehaviour
{
    [Header("Animation Settings")]
    private Animator animator;
    private static readonly int FishingState = Animator.StringToHash("IsFishing");
    private static readonly int FarmingState = Animator.StringToHash("IsFarming");
    private static readonly int CraftingState = Animator.StringToHash("IsCrafting");
    private static readonly int SittingState = Animator.StringToHash("IsSitting");
    private static readonly int WalkingState = Animator.StringToHash("IsWalking");
    private static readonly int TalkingState = Animator.StringToHash("IsTalking");
    private static readonly int ExitTrigger = Animator.StringToHash("ExitTrigger");
    private static readonly int WipeSweatTrigger = Animator.StringToHash("WipeSweatTrigger");

    [Header("Sitting Setting")]
    [SerializeField] private bool isNearNpc = false;
    [SerializeField] private bool isSitting = false;
    [SerializeField] private Vector3 sittingOffset = new Vector3(0, 0.4f, 0);
    private string[] sittingTriggers = { "SittingTalkingTrigger", "SittingClapTrigger" };

    [Header("Movement Setting")]
    private Quaternion startRotation;
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private float npcDistance = 100f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool isStart = false;
    public bool IsMoving
    {
        get => isMoving;
    }

    [Header("Talking State")]
    private string[] talkingTriggers = { "Talking01Trigger", "Talking02Trigger", "Talking03Trigger" };
    private float talkingChance = 1f;
    private float talkingDuration = 2f;
    [SerializeField] private bool isTalking = false;
    public bool IsTalking
    {
        get => isTalking;
    }

    private NpcController npcController;
    private int defaultState;
    private Coroutine currentCoroutine;
    private System.Func<IEnumerator> defaultRoutineFactory;

    private void Start()
    {
        startRotation = transform.rotation;
        startPosition = transform.position;
        animator = GetComponent<Animator>();
        npcController = GetComponent<NpcController>();
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;

        if (npcController.npcType == NpcType.Fishing)
        {
            defaultState = FishingState;
            defaultRoutineFactory = Fishing;
        }
        else if (npcController.npcType == NpcType.Farmer)
        {
            defaultState = FarmingState;
            defaultRoutineFactory = Farming;
        }
        else if (npcController.npcType == NpcType.Craft)
        {
            defaultState = CraftingState;
            defaultRoutineFactory = Crafting;
        }
        else if (npcController.npcType == NpcType.Sitting)
        {
            defaultState = SittingState;
            defaultRoutineFactory = Sitting;
        }

        currentCoroutine = StartCoroutine(defaultRoutineFactory());
    }

    private void Update()
    {
        if(isMoving)
        {
            MoveToWards(targetPosition);

            float distance = Vector3.Distance(transform.position, startPosition);
            if(distance <= 1f && !isStart)
            {
                isMoving = false;
                StopCurrentAction();
                transform.rotation = startRotation;
                currentCoroutine = StartCoroutine(defaultRoutineFactory());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TownNPC") && other.transform != this.transform)
        {
            if (npcController.npcType == NpcType.Sitting)
            {
                isNearNpc = true;
            }
            else if(!isTalking)
            {
                isStart = false;
                isMoving = false;
                StartCoroutine(StartConversation(other.transform));
            }
        }

        if (npcController.npcType == NpcType.Sitting && other.name.Contains("Bench") && !isSitting)
        {
            SittingAtBench(other.GetComponent<Bench>());
            this.transform.position += sittingOffset;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("TownNPC"))
        {
            if(npcController.npcType == NpcType.Sitting)
            {
                isNearNpc = false;
            }
        }
    }

    private IEnumerator Fishing()
    {
        while (true)
        {
            animator.SetBool(FishingState, true);

            yield return new WaitForSeconds(Random.Range(5f, 5f));            

            animator.SetBool(FishingState, false);

            yield return StartCoroutine(DelayBeforeNextAction(0.5f));

            FindNpcAndMove();

            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }

    private IEnumerator Farming()
    {
        while (true)
        {
            Debug.Log("Farming 실행");
            animator.SetBool(FarmingState, true);

            yield return new WaitForSeconds(Random.Range(20, 40f));

            animator.SetTrigger(WipeSweatTrigger);

            yield return new WaitForSeconds(Random.Range(20f, 40f));

            animator.SetBool(FarmingState, false);

            yield return new WaitForSeconds(Random.Range(3f, 5f));
        }
    }

    private IEnumerator Crafting()
    {
        while (true)
        {
            //if (!animator.GetBool(CraftingState))
            //{
            animator.SetBool(CraftingState, true);
            //}

            yield return new WaitForSeconds(Random.Range(20f, 40f));

            animator.SetTrigger(WipeSweatTrigger);
        }
    }

    private IEnumerator Sitting()
    {
        while (true)
        {
            if (!animator.GetBool(SittingState))
            {
                animator.SetBool(SittingState, true);
            }

            yield return new WaitForSeconds(Random.Range(5f, 10f));

            if (isNearNpc)
            {
                PlayRandomTrigger(sittingTriggers);
            }
        }
    }

    private void PlayRandomTrigger(string[] triggers)
    {
        int randomIndex = Random.Range(0, triggers.Length);
        string randomTrigger = triggers[randomIndex];

        if (randomTrigger != null)
        {
            animator.SetTrigger(randomTrigger);
        }
    }

    private void SittingAtBench(Bench bench)
    {
        if (!bench.right)
        {
            transform.position = bench.rightPosition.position;
            transform.rotation = bench.rightPosition.rotation;
            bench.right = true;
            isSitting = true;
            return;
        }

        if (!bench.left)
        {
            transform.position = bench.leftPosition.position;
            transform.rotation = bench.leftPosition.rotation;
            bench.left = true;
            isSitting = true;
            return;
        }

        Debug.Log("비어있는 벤치 없음");
    }

    private void FindNpcAndMove()
    {
        if (Random.value <= talkingChance)
        {            
            Transform ohterNpc = FindNearestNpc(transform.position, npcDistance);
            ActivityNpc activityNpc = ohterNpc?.GetComponent<ActivityNpc>();
            if (activityNpc != null && activityNpc.IsMoving)
            {
                Debug.Log(ohterNpc.name + "이동 중");
                return;
            }

            if (ohterNpc != null)
            {
                Debug.Log($"{ohterNpc.name}으로 이동");

                StopCurrentAction();

                isStart = true;
                isMoving = true;
                targetPosition = ohterNpc.position;
            }
            else
            {
                Debug.Log("범위내에 다른 npc 없음");
            }
        }
    }

    private Transform FindNearestNpc(Vector3 position, float maxDistance)
    {
        Transform nearestNpc = null;
        Collider[] colliders = Physics.OverlapSphere(position, maxDistance);
        float nearestDistance = maxDistance;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("TownNPC") && collider.transform != this.transform)
            {
                float distance = Vector3.Distance(position, collider.transform.position);

                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestNpc = collider.transform;
                }
            }
        }

        return nearestNpc;
    }

    private void MoveToWards(Vector3 destination)
    {
        animator.SetBool(WalkingState, true);

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

    private IEnumerator LookAtTarget(Vector3 target, float duration = 0.5f)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0f;

        float angle = Vector3.Angle(transform.forward, direction);

        bool shouldWalk = angle > 10f;

        Quaternion startRotation = transform.rotation;

        Quaternion targetRotation = Quaternion.LookRotation(direction);

        float elapsedTime = 0f;

        bool isTurningInPlace = (target == startPosition);
        if (shouldWalk || isTurningInPlace) animator.SetBool(WalkingState, true);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (shouldWalk || isTurningInPlace) animator.SetBool(WalkingState, false);

        transform.rotation = targetRotation;
    }


    private IEnumerator StartConversation(Transform target)
    {
        isTalking = true;
        if(isMoving) isMoving = false;
        //if(targetPosition != Vector3.zero) targetPosition = Vector3.zero;

        StopCurrentAction();

        yield return StartCoroutine(DelayBeforeNextAction(-0.5f));

        yield return StartCoroutine(LookAtTarget(target.position));

        animator.SetBool(TalkingState, true);
        PlayRandomTrigger(talkingTriggers);

        StartCoroutine(ContinueConversation(target));
    }

    private IEnumerator ContinueConversation(Transform target)
    {
        int minConversations = Random.Range(6, 10);
        int conversationCount = 0;

        while (isTalking)
        {
            yield return new WaitForSeconds(talkingDuration);

            if (conversationCount < minConversations)
            {
                PlayRandomTrigger(talkingTriggers);
                conversationCount++;
            }
            else
            {
                StopConversation();
            }

            ActivityNpc targetNpc = target.GetComponent<ActivityNpc>();
            if (targetNpc != null && !targetNpc.IsTalking)
            {
                Debug.Log($"{target.name} IsTalking = false");
                StopConversation();
            }
        }
    }

    private void StopConversation()
    {
        isTalking = false;

        Debug.Log($"{transform.name} 대화 종료");

        animator.SetTrigger(ExitTrigger);
        animator.SetBool(TalkingState, false);

        StartCoroutine(DoAction());
    }

    private void StopCurrentAction()
    {
        if (animator.GetBool(defaultState))
        {
            animator.SetBool(defaultState, false);
        }

        if (animator.GetBool(WalkingState))
        {
            animator.SetBool(WalkingState, false);
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }

    private IEnumerator DoAction()
    {
        float distance = Vector3.Distance(transform.position, startPosition);
        if(distance > 0.1f)
        {   
            targetPosition = startPosition;
            isMoving = true;
        }
        else
        {
            Debug.Log($"{transform.name} DoAction");
            yield return StartCoroutine(LookAtTarget(startPosition));
            currentCoroutine = StartCoroutine(defaultRoutineFactory());
        }
    }

    private IEnumerator DelayBeforeNextAction(float addTime = 0)
    {
        float clipLnegth = animator.GetCurrentAnimatorClipInfo(0).Length;
        yield return new WaitForSeconds(clipLnegth + addTime);
    }
}

using NUnit.Framework;
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
    private static readonly int TerrifyingState = Animator.StringToHash("IsTerrifying");
    private static readonly int AnimationSpeed = Animator.StringToHash("Speed");
    private static readonly int ExitTrigger = Animator.StringToHash("ExitTrigger");

    [Header("Sitting Setting")]
    [SerializeField] private bool isNearNpc = false;
    [SerializeField] private bool isSitting = false;
    [SerializeField] private Vector3 sittingOffset = new Vector3(0, 0.4f, 0);
    private string[] sittingTriggers = { "SittingTalkingTrigger", "SittingClapTrigger" };

    [Header("Movement Setting")]
    [SerializeField] private Quaternion originalRotation;
    [SerializeField] private Vector3 originalPosition;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private Transform targetNpc;
    [SerializeField] private float npcDistance = 100f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float minSpeed = 3f;
    [SerializeField] private float maxSpeed = 5f;
    [SerializeField] private float turnSpeed = 10f;

    [SerializeField] private bool isMoving = false;
    [SerializeField] private bool isStart = false;
    

    public bool IsMoving
    {
        get => isMoving;
        private set
        {
            isMoving = value;
            ActiveTool(!isMoving);
        }
    }


    [Header("Talking State")]
    private string[] talkingTriggers = { "Talking01Trigger", "Talking02Trigger", "Talking03Trigger" };
    [SerializeField] private float talkingChance = 1f;  //수정하기!!
    private float talkingDuration = 2f;
    [SerializeField] private bool isTalking = false;
    public bool IsTalking
    {
        get => isTalking;
        private set
        {
            isTalking = value;
            ActiveTool(!isTalking);
        }
    }

    [Header("Detection Monster")]
    [SerializeField] private float fleeDistance = 10f;
    [SerializeField] private float monsterDistnace = 20f;
    [SerializeField] private bool isNearMonster = false;
    public bool IsNearMonster
    {
        get => isNearMonster;
        private set => isNearMonster = value;
    }

    private NpcController npcController;
    private int defaultState;
    private Coroutine currentCoroutine;
    private System.Func<IEnumerator> defaultRoutineFactory;
    [SerializeField] private GameObject tool;
    private string[] commonTriggers = { "ArmStretching", "NeckStretching", "LookAround", "WipeSweatTrigger" };

    private void Start()
    {
        originalRotation = transform.rotation;
        originalPosition = transform.position;
        animator = GetComponent<Animator>();
        npcController = GetComponent<NpcController>();
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;

        if(npcController.npcTool && npcController.npcType != NpcType.Sitting)
        {
            tool = npcController.npcTool.GetChild((int)npcController.npcType - 1)?.gameObject;
            tool?.SetActive(true);
        }

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
        if(IsMoving)
        {
            if (targetNpc)
                MoveToWards(targetNpc);
            else
                MoveToWards(targetPosition);

            float distance = Vector3.Distance(transform.position, originalPosition);
            if(distance <= 1f && !isStart && !isNearMonster)
            {
                IsMoving = false;
                StopCurrentAction();
                StartCoroutine(LookAtTarget(originalPosition));
                currentCoroutine = StartCoroutine(defaultRoutineFactory());
            }

            if(isNearMonster)
            {
                distance = Vector3.Distance(transform.position, targetPosition);
                if(distance <= 1f)
                {
                    IsMoving = false;
                    StopCurrentAction();
                    StartCoroutine(StartTerrifying());
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TownNPC") && other.transform != this.transform)
        {
            ActivityNpc activityNpc = other.GetComponent<ActivityNpc>();

            if (npcController.npcType == NpcType.Sitting)
            {
                isNearNpc = true;
            }
            else if(!IsTalking && !activityNpc.IsNearMonster && !IsNearMonster)
            {
                if (targetNpc) targetNpc = null;
                isStart = false;
                IsMoving = false;
                StopCurrentAction();
                currentCoroutine = StartCoroutine(StartConversation(other.transform));
            }
            
            if(activityNpc != null && activityNpc.IsNearMonster)
            {
                Debug.Log(transform.name + other.transform.name + "에게서 두려움 전염");
                IsNearMonster = true;
                StopCurrentAction();
                StartCoroutine(StartTerrifying());
            }
        }

        if (npcController.npcType == NpcType.Sitting && other.name.Contains("Bench") && !isSitting)
        {
            SittingAtBench(other.GetComponent<Bench>());
            this.transform.position += sittingOffset;
        }

        //if(other.CompareTag("Monster") && IsNearMonster == false)
        //{
        //    Debug.Log("몬스터 닿음");
        //    if(targetNpc) targetNpc = null;
        //    IsNearMonster = true;
        //    StopCurrentAction();

        //    RunAway(other.transform);
        //}
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

            //yield return new WaitForSeconds(Random.Range(20f, 40f));
            yield return new WaitForSeconds(Random.Range(1f, 1f));

            animator.SetBool(FishingState, false);

            yield return new WaitForSeconds(ClipLength());

            if(Random.value < talkingChance)
            {
                FindNpcAndMove();
            }
            else
            {
                yield return new WaitForSeconds(Random.Range(3f, 5f));
            }
        }
    }

    private IEnumerator Farming()
    {
        while (true)
        {
            animator.SetBool(FarmingState, true);

            yield return new WaitForSeconds(Random.Range(20, 40f));
            //yield return new WaitForSeconds(Random.Range(5f, 5f));

            //animator.SetTrigger(WipeSweatTrigger);

            //yield return StartCoroutine(DelayBeforeNextAction(0.5f));

            //FindNpcAndMove();

            yield return StartCoroutine(StartAction());

            //yield return new WaitForSeconds(Random.Range(20f, 40f));

            //animator.SetBool(FarmingState, false);

            //yield return new WaitForSeconds(Random.Range(3f, 5f));
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
            //yield return new WaitForSeconds(Random.Range(5f, 5f));

            //animator.SetTrigger(WipeSweatTrigger);

            //yield return StartCoroutine(DelayBeforeNextAction(0.5f));

            //FindNpcAndMove();

            yield return StartCoroutine(StartAction(0.5f));

            //yield return new WaitForSeconds(Random.Range(20f, 40f));
            //yield return new WaitForSeconds(Random.Range(5f, 5f));
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

    private IEnumerator StartAction(float delayTime = 0f)
    {
        if(Random.value < talkingChance)
        {
            yield return new WaitForSeconds(ClipLength() + delayTime);

            FindNpcAndMove();
        }
        else
        {
            PlayRandomTrigger(commonTriggers);

            int minCount = Random.Range(0, 3);
            int count = 0;

            while (count < minCount)
            {
                yield return new WaitForSeconds(3f);

                PlayRandomTrigger(commonTriggers);
                count++;
            }
        }
    }

    private string PlayRandomTrigger(string[] triggers)
    {
        if (triggers == null || triggers.Length == 0) return null;

        int randomIndex = Random.Range(0, triggers.Length);
        string randomTrigger = triggers[randomIndex];

        if (!string.IsNullOrEmpty(randomTrigger))
        {
            animator.SetTrigger(randomTrigger);
            return randomTrigger;
        }
        return null;
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
        Transform ohterNpc = FindNearestNpc(transform.position, npcDistance);
        ActivityNpc activityNpc = ohterNpc?.GetComponent<ActivityNpc>();
        if (activityNpc == null || activityNpc.IsNearMonster) return;

        if (ohterNpc != null)
        {
            StopCurrentAction();

            moveSpeed = minSpeed;
            isStart = true;
            IsMoving = true;
            targetNpc = ohterNpc;
        }
        else
        {
            Debug.Log("범위내에 다른 npc 없음");
        }
    }

    private Transform FindNearestNpc(Vector3 position, float maxDistance, string npcTag = "TownNPC")
    {
        Transform nearestNpc = null;
        Collider[] colliders = Physics.OverlapSphere(position, maxDistance);
        float nearestDistance = maxDistance;

        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag(npcTag) && collider.transform != this.transform)
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

    private void RunAway(Transform monster)
    {
        Vector3 fleeDirection = (transform.position - monster.position).normalized;
        fleeDirection.y = 0f;
        Vector3 fleeDestination = transform.position + fleeDirection * fleeDistance;
        targetPosition = fleeDestination;
        moveSpeed = maxSpeed;
        IsMoving = true;
    }

    private void MoveToWards(Vector3 destination)
    {
        animator.SetFloat(AnimationSpeed, moveSpeed);
        animator.SetBool(WalkingState, true);

        Vector3 direction = (destination - transform.position).normalized;
        direction.y = 0f;

        Vector3 nextPosition = transform.position + direction * moveSpeed * Time.deltaTime;

        if (Physics.Raycast(nextPosition + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 2f))
        {
            nextPosition.y = hit.point.y;
        }

        transform.position = nextPosition;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    private void MoveToWards(Transform targetTransform)
    {
        animator.SetFloat(AnimationSpeed, moveSpeed);
        animator.SetBool(WalkingState, true);

        Vector3 direction = (targetTransform.position - transform.position).normalized;
        direction.y = 0f;

        Vector3 nextPosition = transform.position + direction * moveSpeed * Time.deltaTime;

        if (Physics.Raycast(nextPosition + Vector3.up * 1f, Vector3.down, out RaycastHit hit, 2f))
        {
            nextPosition.y = hit.point.y;
        }

        transform.position = nextPosition;

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    private IEnumerator LookAtTarget(Vector3 target)
    {
        float angle;
        Quaternion targetRotation;

        if (target == originalPosition)
        {
            angle = Quaternion.Angle(transform.rotation, originalRotation);
            targetRotation = originalRotation;
        }
        else
        {
            Vector3 direction = (target - transform.position).normalized;
            direction.y = 0f;

            angle = Vector3.Angle(transform.forward, direction);
            targetRotation = Quaternion.LookRotation(direction);
        }

        Quaternion startRotation = transform.rotation;
        bool shouldWalk = angle > 15f;
        float elapsedTime = 0f;
        float duration = SetDuration(angle);

        if (shouldWalk) animator.SetBool(WalkingState, true);

        while (elapsedTime < duration)
        {
            float t = elapsedTime / duration;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, t);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        if (shouldWalk) animator.SetBool(WalkingState, false);

        transform.rotation = targetRotation;
    }

    private float SetDuration(float angle)
    {
        if (angle < 15) return 0.1f;
        else if (angle < 90) return 0.3f;
        else return 0.5f;
    }


    private IEnumerator StartConversation(Transform target)
    {
        IsTalking = true;
        if(IsMoving) IsMoving = false;

        yield return new WaitForSeconds(ClipLength());

        yield return StartCoroutine(LookAtTarget(target.position));

        animator.SetBool(TalkingState, true);

        PlayRandomTrigger(talkingTriggers);

        currentCoroutine = StartCoroutine(ContinueConversation(target));
    }

    private IEnumerator ContinueConversation(Transform target)
    {
        int minConversations = Random.Range(6, 10);
        int conversationCount = 0;

        while (IsTalking)
        {
            yield return new WaitForSeconds(talkingDuration);

            ActivityNpc targetNpc = target.GetComponent<ActivityNpc>();

            if (conversationCount < minConversations)
            {
                PlayRandomTrigger(talkingTriggers);

                conversationCount++;
            }
            else
            {
                StopConversation();
            }

            if (targetNpc != null && !targetNpc.IsTalking)
            {
                StopConversation();
            }
        }
    }

    private void StopConversation()
    {
        IsTalking = false;

        animator.SetTrigger(ExitTrigger);
        animator.SetBool(TalkingState, false);

        currentCoroutine = StartCoroutine(DoAction());
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
            isMoving = false;
            isStart = false;
        }

        if (animator.GetBool(TalkingState))
        {
            animator.SetTrigger(ExitTrigger);
            animator.SetBool(TalkingState, false);
            isTalking = false;
        }

        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
            currentCoroutine = null;
        }
    }

    private IEnumerator DoAction()
    {
        float distance = Vector3.Distance(transform.position, originalPosition);
        if(distance > 0.1f)
        {
            targetPosition = originalPosition;
            moveSpeed = minSpeed;
            IsMoving = true;
        }
        else
        {
            yield return StartCoroutine(LookAtTarget(originalPosition));
            currentCoroutine = StartCoroutine(defaultRoutineFactory());
        }
    }

    private void ActiveTool(bool isActive)
    {
        if(tool != null && tool.activeSelf != isActive)
        {
            tool.SetActive(isActive);
        }
    }

    private void DetectMonsters()
    {
        MonsterData monster = CharacterManager.Instance.GetNearestMonster(transform, monsterDistnace);
    }

    private IEnumerator StartTerrifying()
    {
        while(isNearMonster)
        {
            if (!animator.GetBool(TerrifyingState))
                animator.SetBool(TerrifyingState, true);

            yield return new WaitForSeconds(10f);

            Transform monster = FindNearestNpc(transform.position, monsterDistnace, "Monster");
            if (monster == null) isNearMonster = false;
        }

        animator.SetBool(TerrifyingState, false);
        StartCoroutine(DoAction());
    }

    private float ClipLength()
    {
        float clipLength = animator.GetCurrentAnimatorClipInfo(0).Length;

        return clipLength;
    }

    public string GetCurrentClip()
    {
        string currentClip = animator.GetCurrentAnimatorClipInfo(0)[0].clip.name;

        return currentClip;
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, monsterDistnace);
    }
}

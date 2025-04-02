using System.Collections;
using System.Threading;
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
    private string[] sittingTriggers = { "SittingTalkingTrigger", "SittingClapTrigger" };
    private Bench currentBench;

    [Header("Movement Setting")]
    [SerializeField] private Quaternion originalRotation;
    [SerializeField] private Vector3 originalPosition;
    [SerializeField] private Vector3 targetPosition;
    [SerializeField] private Transform targetNpc;
    private float npcDistance = 100f;
    [SerializeField] private float moveSpeed = 3f;
    private float minSpeed = 3f;
    private float maxSpeed = 5f;
    private float turnSpeed = 10f;
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
    [SerializeField] private float talkingChance = 0.5f;
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
    public Transform currentTalking = null;
    private float conversationCoolTime = 5f;
    private float lastConversationTime = 0f;

    [Header("Detection Monster")]
    private float fleeDistance = 10f;
    private float monsterDetectRange = 20f;
    private float viewAngle = 90f;
    private float checkInterval = 3f;
    [SerializeField] private bool isNearMonster = false;
    public bool IsNearMonster
    {
        get => isNearMonster;
        private set => isNearMonster = value;
    }

    [Header("Default")]
    private Rigidbody rb;
    private NpcController npcController;
    private int defaultState;
    private Coroutine currentCoroutine;
    private System.Func<IEnumerator> defaultRoutineFactory;
    [SerializeField] private GameObject tool;
    private string[] commonTriggers = { "ArmStretching", "NeckStretching", "LookAround", "WipeSweatTrigger" };

    [Header("WanderNpc")]
    private float walkingRange = 10f;
    private float idleTime = 5f;
    private float arrivedDistance = 1f;
    private float sittingChance = 0.5f;
    [SerializeField] private bool isJustStoodUp = false;
    [SerializeField] private bool isSittingTalking = false;

    private void Start()
    {
        originalRotation = transform.rotation;
        originalPosition = transform.position;

        animator = GetComponent<Animator>();
        npcController = GetComponent<NpcController>();
        rb = GetComponent<Rigidbody>();

        if (npcController.npcTool)
        {
            if (npcController.npcType != NpcType.Sitting && npcController.npcType != NpcType.Wander)
            {
                tool = npcController.npcTool.GetChild((int)npcController.npcType - 1)?.gameObject;
                tool?.SetActive(true);
            }
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
            rb.isKinematic = true;
        }
        else if (npcController.npcType == NpcType.Sitting)
        {
            defaultState = SittingState;
            defaultRoutineFactory = Sitting;
            rb.isKinematic = true;
        }
        else if(npcController.npcType == NpcType.Wander)
        {
            SetNextDestination();
            rb.isKinematic = false;
        }

        if (defaultRoutineFactory != null) currentCoroutine = StartCoroutine(defaultRoutineFactory());

        InvokeRepeating(nameof(DetectMonsters), 0f, checkInterval);     //3초마다 한번 실행
    }

    private void Update()
    {
        if(IsMoving)
        {
            if (targetNpc)
                MoveToWards(targetNpc);
            else
                MoveToWards(targetPosition);

            Vector3 horizontalPosition = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 horizontalTarget = new Vector3(targetPosition.x, 0, targetPosition.z);
            float distance = Vector3.Distance(horizontalPosition, horizontalTarget);

            if(distance <= 0.5f && !isStart && !isNearMonster)
            {
                IsMoving = false;
                StopCurrentAction();
                if (npcController.npcType == NpcType.Wander)
                {
                    if(Mathf.Abs(transform.position.y - targetPosition.y) > 2f)
                    {
                        SetNextDestination();
                        return;
                    }

                    currentCoroutine = StartCoroutine(IdleAfterWalking());
                }
                else
                {
                    StartCoroutine(LookAtTarget(originalPosition));
                    transform.position = originalPosition;
                    currentCoroutine = StartCoroutine(defaultRoutineFactory());
                }
            }

            if(isNearMonster && distance <= 1f)
            {
                IsMoving = false;
                StopCurrentAction();
                StartCoroutine(StartTerrifying());
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("TownNPC") && other.transform != this.transform)
        {
            if(npcController.npcType == NpcType.Wander && isSitting && currentBench != null && !isSittingTalking)
            {
                isSittingTalking = true;
                StopCoroutine(SittingDuration(currentBench));
                currentCoroutine = StartCoroutine(SittingTalking(currentBench));
            }

            ActivityNpc activityNpc = other.GetComponent<ActivityNpc>();

            if (npcController.npcType == NpcType.Sitting)
            {
                isNearNpc = true;
            }            
            else if(!IsTalking && !activityNpc.IsNearMonster && !IsNearMonster)
            {
                if (Time.time - lastConversationTime < conversationCoolTime) return;

                if (targetNpc) targetNpc = null;
                isStart = false;
                IsMoving = false;
                StopCurrentAction();
                currentCoroutine = StartCoroutine(StartConversation(other.transform));
            }
            
            if(activityNpc != null && activityNpc.IsNearMonster)
            {
                //Debug.Log(transform.name + other.transform.name + "에게서 두려움 전염");
                IsNearMonster = true;
                StopCurrentAction();
                StartCoroutine(StartTerrifying());
            }
        }

        if (npcController.npcType == NpcType.Sitting || npcController.npcType == NpcType.Wander)
        {
            if(other.name.Contains("Bench") && !isSitting)
            {
                SittingAtBench(other.GetComponent<Bench>());
                this.transform.position += npcController.sittingOffset;
            }            
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

            yield return new WaitForSeconds(Random.Range(20f, 40f));

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

            yield return new WaitForSeconds(Random.Range(20f, 40f));

            yield return StartCoroutine(StartAction());
        }
    }

    private IEnumerator Crafting()
    {
        while (true)
        {
            animator.SetBool(CraftingState, true);

            yield return new WaitForSeconds(Random.Range(20f, 40f));

            yield return StartCoroutine(StartAction(0.5f));
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
        if (isJustStoodUp) return;

        if(npcController.npcType == NpcType.Sitting)
        {
            if (!bench.right)
            {
                transform.position = bench.rightPosition.position;
                transform.rotation = bench.rightPosition.rotation;
                bench.right = true;
                isSitting = true;
                currentBench = bench;
                return;
            }

            if (!bench.left)
            {
                transform.position = bench.leftPosition.position;
                transform.rotation = bench.leftPosition.rotation;
                bench.left = true;
                isSitting = true;
                currentBench = bench;
                return;
            }

            Debug.Log("비어있는 벤치 없음");
        }
        else if(npcController.npcType == NpcType.Wander)
        {
            if (Random.value <= sittingChance)
            {
                StopCurrentAction();
                rb.isKinematic = true;
                isSitting = true;
                animator.SetBool(SittingState, true);
                currentBench = bench;

                if (!bench.left)
                {
                    transform.position = bench.leftPosition.position;
                    transform.rotation = bench.leftPosition.rotation;
                    bench.left = true;
                }
                else if (!bench.right)
                {
                    transform.position = bench.rightPosition.position;
                    transform.rotation = bench.rightPosition.rotation;
                    bench.right = true;
                }

                StartCoroutine(SittingDuration(bench));
            }
            else
                SetNextDestination();
        }
    }

    private void FindNpcAndMove()
    {
        Transform otherNpc = FindNearestNpc(transform.position, npcDistance);
        ActivityNpc activityNpc = otherNpc?.GetComponent<ActivityNpc>();
        if (activityNpc == null || activityNpc.IsNearMonster) return;

        if (otherNpc != null)
        {
            StopCurrentAction();

            moveSpeed = minSpeed;
            isStart = true;
            IsMoving = true;
            targetNpc = otherNpc;
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

        //if (Terrain.activeTerrain != null)
        //{
        //    float terrainHeight = Terrain.activeTerrain.SampleHeight(nextPosition);
        //    if (nextPosition.y < terrainHeight || !IsOnTerrain(nextPosition))
        //    {
        //        nextPosition.y = terrainHeight;
        //    }
        //}

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

        //if (Terrain.activeTerrain != null)
        //{
        //    float terrainHeight = Terrain.activeTerrain.SampleHeight(nextPosition);
        //    if (nextPosition.y < terrainHeight || !IsOnTerrain(nextPosition))
        //    {
        //        nextPosition.y = terrainHeight;
        //    }
        //}

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

        ActivityNpc targetNpc = target.GetComponent<ActivityNpc>();
        if(targetNpc.currentTalking == null)
        {
            currentTalking = this.transform;
            PlayRandomTrigger(talkingTriggers);
        }
        else
        {
            currentTalking = targetNpc.currentTalking;
            animator.SetTrigger("NodTrigger");
        }        

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
                if(currentTalking == transform)
                {
                    currentTalking = target;
                    animator.SetTrigger("NodTrigger");                    
                }
                else
                {
                    currentTalking = transform;
                    PlayRandomTrigger(talkingTriggers);
                }

                conversationCount++;
            }
            else
            {
                StopConversation();
            }

            if (targetNpc != null && !targetNpc.isSitting && !targetNpc.IsTalking)
            {
                StopConversation();
            }
        }
    }

    private void StopConversation()
    {
        IsTalking = false;
        currentTalking = null;

        lastConversationTime = Time.time;

        animator.SetTrigger(ExitTrigger);
        animator.SetBool(TalkingState, false);

        currentCoroutine = StartCoroutine(DoAction());
    }

    private void StopCurrentAction()
    {
        if (defaultState != 0 && animator.GetBool(defaultState))
        {
            Debug.Log($"{transform.name} defaultState 정지");
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
        
        if(animator.GetBool(SittingState))
        {
            StopSitting(currentBench);
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
        if(npcController.npcType == NpcType.Wander)
        {
            SetNextDestination();
        }
        else if (distance > 0.1f)
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
        if (IsNearMonster) return;
        
        Transform monster = GetNearestMonster(transform, monsterDetectRange);

        if (monster != null && IsInshight(monster))
        {
            Debug.Log($"{transform.name}이 {monster.name} 감지함");
            IsNearMonster = true;
            StopCurrentAction();
            RunAway(monster);
        }
    }

    public Transform GetNearestMonster(Transform transform, float detectRange)
    {
        Transform closestMonster = null;
        float closestDistance = float.MaxValue;

        foreach (var character in CharacterManager.Instance.CharacterList)
        {
            if (character is MonsterData monster)
            {
                if (!monster.instance.transform.gameObject.activeInHierarchy) continue;

                float distanceToMonster = Vector3.Distance(transform.position, monster.instance.transform.position);

                if (distanceToMonster < closestDistance && distanceToMonster <= detectRange)
                {
                    closestDistance = distanceToMonster;
                    closestMonster = monster.instance.transform;                    
                }
            }
        }

        return closestMonster;
    }

    private bool IsInshight(Transform target)
    {
        Vector3 direction = (target.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, direction);

        return angle <= viewAngle * 0.5f;
    }

    private IEnumerator StartTerrifying()
    {
        while(isNearMonster)
        {
            if (!animator.GetBool(TerrifyingState))
                animator.SetBool(TerrifyingState, true);

            yield return new WaitForSeconds(10f);

            Transform monster = GetNearestMonster(transform, monsterDetectRange);
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

    private bool IsOnTerrain(Vector3 position)
    {
        if (Terrain.activeTerrain == null) return false;

        float terrainHeight = Terrain.activeTerrain.SampleHeight(position);

        return Mathf.Abs(position.y - terrainHeight) < 0.5f;
    }
    
    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.red;
    //    Gizmos.DrawWireSphere(transform.position, monsterDetectRange);
    //}

    #region WanderNpc
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
                randomPosition = originalPosition + new Vector3(Random.Range(-walkingRange, walkingRange), 0f, Random.Range(-walkingRange, walkingRange));
                safetyCounter++;
            }
            //while (Vector3.Distance(transform.position, randomPosition) < arrivedDistance * 2f && safetyCounter < 10);
            while ((!IsOnTerrain(randomPosition) || Vector3.Distance(transform.position, randomPosition) < arrivedDistance * 2f) && safetyCounter < 10);
        }

        targetPosition = randomPosition;
        isStart = true;
        StartCoroutine(ChangeStartState());
        moveSpeed = Random.Range(minSpeed, maxSpeed);
        IsMoving = true;
        animator.SetBool(WalkingState, true);
    }

    private IEnumerator JustStoodUp()
    {
        yield return new WaitForSeconds(5f);

        isJustStoodUp = false;
    }

    private IEnumerator ChangeStartState()
    {
        yield return new WaitForSeconds(1f);

        isStart = false;

    }

    private IEnumerator IdleAfterWalking()
    {
        animator.SetBool(WalkingState, false);
        IsMoving = false;

        yield return new WaitForSeconds(idleTime);
        SetNextDestination();

        currentCoroutine = null;
    }

    private IEnumerator SittingDuration(Bench bench)
    {
        float sittingTime = Random.Range(15f, 30f);
        
        yield return new WaitForSeconds(sittingTime);

        if (!isSittingTalking)
            StopSitting(bench);
    }

    private void StopSitting(Bench bench)
    {
        isSitting = false;
        animator.SetBool(SittingState, false);
        rb.isKinematic = false;
        currentBench = null;

        if (transform.position == bench.leftPosition.position) bench.left = false;
        if (transform.position == bench.rightPosition.position) bench.right = false;

        //lastSittingTime = Time.time;

        isJustStoodUp = true;
        SetNextDestination();
    }

    private IEnumerator SittingTalking(Bench bench)
    {
        int minConversations = Random.Range(3, 6);
        int conversationCount = 0;

        while (isSitting)
        {
            yield return new WaitForSeconds(Random.Range(5f, 10f));

            if (conversationCount < minConversations)
            {
                PlayRandomTrigger(sittingTriggers);
                conversationCount++;
            }
            else
            {
                isSitting = false;
            }
        }

        isSittingTalking = false;
        StopSitting(bench);
    }
    #endregion
}

using System.Collections;
using Unity.Cinemachine;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ActivityNpc : MonoBehaviour
{
    [Header("Animation Settings")]
    public Animator animator;
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
    [SerializeField] private Vector3 startPosition;
    [SerializeField] private Transform targetNpc;
    [SerializeField] private float npcDistance = 100f;
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private float turnSpeed = 10f;
    [SerializeField] private bool isMoving = false;
    //public bool IsMoving
    //{
    //    get { return isMoving; }
    //    set
    //    {
    //        isMoving = value;
    //        if (isMoving) MoveToWards(targetNpc.position);
    //        else StopCurrentAction();
    //    }
    //}

    [Header("Talking State")]
    private string[] talkingTriggers = { "Talking01Trigger", "Talking02Trigger", "Talking03Trigger" };
    private float talkingChance = 1f;
    private float talkingDuration = 2f;
    [SerializeField] private bool isTalking = false;
    //public bool IsTalking
    //{
    //    get { return isTalking; }
    //    set
    //    {
    //        isTalking = value;
    //        if (isTalking) StartConversation();
    //        else StopConversation();
    //    }
    //}

    private NpcController npcController;
    [SerializeField] private int defaultState;
    private IEnumerator defaultRoutine;

    private void Start()
    {
        startPosition = this.transform.position;
        animator = GetComponent<Animator>();
        npcController = GetComponent<NpcController>();
        Rigidbody rb = GetComponent<Rigidbody>();

        rb.isKinematic = true;

        if (npcController.npcType == NpcType.Fishing)
        {
            defaultState = FishingState;
            defaultRoutine = Fishing();
        }
        else if (npcController.npcType == NpcType.Farmer)
        {
            defaultState = FarmingState;
            defaultRoutine = Farming();
        }
        else if (npcController.npcType == NpcType.Craft)
        {
            defaultState = CraftingState;
            defaultRoutine = Crafting();
        }
        else if (npcController.npcType == NpcType.Sitting)
        {
            defaultState = SittingState;
            defaultRoutine = Sitting();
        }

        StartCoroutine(defaultRoutine);
    }

    private void Update()
    {
        if(isMoving)
        {
            MoveToWards(targetNpc.position);
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
            else
            {
                isTalking = true;
                isMoving = false;
                LookAtTarget(other.transform);
                StartConversation();
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
            else
            {
                isTalking = false;
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

            float clipLnegth = animator.GetCurrentAnimatorClipInfo(0).Length;
            yield return new WaitForSeconds(clipLnegth);

            FindNpcAndMove();

            yield return new WaitForSeconds(Random.Range(5f, 10f));
        }
    }

    private IEnumerator Farming()
    {
        while (true)
        {
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
            if (!animator.GetBool(CraftingState))
            {
                animator.SetBool(CraftingState, true);
            }

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
            if (ohterNpc != null)
            {
                Debug.Log($"{ohterNpc.name}으로 이동");
                if (animator.GetBool(defaultState))
                {
                    animator.SetBool(defaultState, false);
                }

                StopAllCoroutines();

                isMoving = true;
                targetNpc = ohterNpc;
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

    private void LookAtTarget(Transform target)
    {
        if (target == null) return;

        Vector3 direction = (target.position - transform.position).normalized;
        direction.y = 0f;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = targetRotation;
    }

    private void StartConversation()
    {
        if(isMoving) isMoving = false;
        if(targetNpc) targetNpc = null;

        StopCurrentAction();

        animator.SetBool(TalkingState, true);
        PlayRandomTrigger(talkingTriggers);

        StartCoroutine(ContinueConversation());
    }

    private IEnumerator ContinueConversation()
    {
        int minConversations = Random.Range(3, 6);
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
                isTalking = false;
                StopConversation();
            }
        }
    }

    private void StopConversation()
    {
        animator.SetTrigger(ExitTrigger);
        animator.SetBool(TalkingState, false);

        DoAction();
    }

    private void StopCurrentAction()
    {
        if (animator.GetBool(defaultState))
        {
            animator.SetBool(defaultState, false);
        }
        if(animator.GetBool(WalkingState))
        {
            animator.SetBool(WalkingState, false);
        }

        StopAllCoroutines();
    }

    private void DoAction()
    {
        //처음 위치로 이동하거나 그 자리에서 실행
    }
}

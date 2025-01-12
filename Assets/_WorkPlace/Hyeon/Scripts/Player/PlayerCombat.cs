using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public PlayerController controller;
    public GameObject sword;
    private Collider swordCollider;
    [SerializeField] private float swordDamage = 10f;
    [SerializeField] private float attackPerceptionRange = 5f;

    public Animator PlayerAnimator;
    private Transform closestMonster;

    private int currentComboIndex = 0;

    private static readonly int[] ComboHashes =
    {
        Animator.StringToHash("OneHand_Up_Attack_1_InPlace"),
        Animator.StringToHash("OneHand_Up_Attack_2_InPlace"),
        Animator.StringToHash("OneHand_Up_Attack_3_InPlace")
    };
    [SerializeField] private float comboWindow = 0.9f;
    [SerializeField] private float comboResetTimer = 1.0f;
    private float lastAttackTime = 0;
    private bool canChainCombo = false;

    public bool CanReceiveInput { get; set; } = true;

    public bool inputReceived = false;

    public int MaxComboCount { get; set; } = 3;

    public Quaternion targetRotation;

    public HashSet<GameObject> DamagedTargets { get; set; } = new HashSet<GameObject>();
    private static readonly int[] AttackStateHash = {
        Animator.StringToHash("Base Layer.ComboAttack.Attack_1"),
        Animator.StringToHash("Base Layer.ComboAttack.Attack_2"),
        Animator.StringToHash("Base Layer.ComboAttack.Attack_3")
    };

    private void Start()
    {
        swordCollider = sword.GetComponent<Collider>();

        swordCollider.enabled = false;
    }

    private void Update()
    {
        closestMonster = GetClosestMonster();
        if (closestMonster != null)
        {
            Debug.Log("Closest Monster: " + closestMonster.name);
        }
        HandleAttackInput();
        AttackFinishedCheck();
        //if (InputManager.InputActions.actions["Attack"].triggered)
        //{
        //    PerformAttack();
        //}
        //HandleComboReset();
        //MonitorAnimationProgress();

    }

    private void HandleAttackInput()
    {
        if (InputManager.InputActions.actions["Attack"].triggered && CanReceiveInput)
        {
            inputReceived = true;

            if (currentComboIndex == 0 || PlayerAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f)
            {
                
                PerformComboAttack();
            }
        }
    }

    private void PerformComboAttack()
    {
        if(closestMonster != null)
        {
            LookEnemy();
        }
        controller.CanMove = false;
        if (inputReceived && currentComboIndex < MaxComboCount)
        {
            inputReceived = false;
            CanReceiveInput = false;

            PlayerAnimator.SetTrigger("NextCombo");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!swordCollider.enabled) return;
        if (!DamagedTargets.Contains(other.gameObject))
        {
            DamagedTargets.Add(other.gameObject);
            Debug.Log($"Damaged: {other.name}, Damage: {swordDamage}");
        }
    }
    private void EnableCollider()
    {
        swordCollider.enabled = true;
    }

    private void DisableCollider()
    {
        swordCollider.enabled = false;
    }

    private void EnableComboInput()
    {
        CanReceiveInput = true;
    }

    private void StartCombo()
    {
        CanReceiveInput = false;
        DamagedTargets.Clear();
    }

    private void AttackFinishedCheck()
    {
        AnimatorStateInfo stateInfo = PlayerAnimator.GetCurrentAnimatorStateInfo(0);
        if(AttackStateHash.Contains(stateInfo.fullPathHash))
        {

            float normalizedTime = stateInfo.normalizedTime;

            if(normalizedTime >= 0.9f)
            {
                controller.CanMove = true;
            }
        }
    }

    private Transform GetClosestMonster()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, attackPerceptionRange);
        Transform closestTransform = null;
        float closestDistance = Mathf.Infinity;

        foreach(Collider collider in colliders)
        {
            if (collider.CompareTag("Monster"))
            {
                float distance = Vector3.Distance(transform.position, collider.transform.position);
                if(distance < closestDistance)
                {
                    closestDistance = distance;
                    closestTransform = collider.transform;
                }
            }
        }
        return closestTransform;
    }

    private void LookEnemy()
    {
        Vector3 dir = (closestMonster.position - transform.position).normalized;
        dir.y = 0f;
        targetRotation = Quaternion.LookRotation(dir);

        transform.rotation = targetRotation;
    }

    public Quaternion ModifyRotation()
    {
        return Quaternion.Inverse(targetRotation);
    }

    //public void PerformAttack()
    //{
    //    if(Time.time - lastAttackTime > comboResetTimer)
    //    {
    //        currentComboIndex = 0;
    //    }

    //    if(canChainCombo || currentComboIndex == 0)
    //    {
    //        PlayerAnimator.Play(ComboHashes[currentComboIndex]);
    //        currentComboIndex = (currentComboIndex + 1) % ComboHashes.Length;
    //        canChainCombo = false;
    //        lastAttackTime = Time.time;
    //    }
    //}

    //private void MonitorAnimationProgress()
    //{
    //    AnimatorStateInfo stateInfo = PlayerAnimator.GetCurrentAnimatorStateInfo(0);

    //    if (stateInfo.IsName("OneHand_Up_Attack_1_InPlace") ||
    //        stateInfo.IsName("OneHand_Up_Attack_2_InPlace") ||
    //        stateInfo.IsName("OneHand_Up_Attack_3_InPlace"))
    //    {
    //        float normalizedTime = stateInfo.normalizedTime % 1;

    //        if(normalizedTime >= 0.2f && normalizedTime <= 0.6f)
    //        {
    //            swordCollider.enabled = true;
    //        }
    //        else
    //        {
    //            swordCollider.enabled = false;
    //        }

    //        if(normalizedTime >= comboWindow)
    //        {
    //            canChainCombo = true;
    //        }
    //    }
    //}

    //private void HandleComboReset()
    //{
    //    if(Time.time - lastAttackTime > comboResetTimer)
    //    {
    //        currentComboIndex = 0;
    //        canChainCombo = false;
    //    }
    //}

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackPerceptionRange);
    }
}

using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour
{
    private TestInputManager inputManager;
    private float swordDamage = 10f;
    private Animator animator;

    private Collider swordCollider;

    private int currentComboIndex = 0;

    public bool CanReceiveInput { get; set; } = true;
    public bool inputReceived = false;

    public int MaxComboCount { get; set; } = 3;

    public HashSet<GameObject> DamagedTargets { get; set; } = new HashSet<GameObject>();

    private void Start()
    {
        animator = GetComponentInParent<Animator>();
        swordCollider = GetComponentInParent<Collider>();
        inputManager = TestGameManager.Instance.GetManager<TestInputManager>("InputManager");

        inputManager.OnAttackInput += HandleAttackInput;
    }

    private void OnDisable()
    {
        inputManager.OnAttackInput -= HandleAttackInput;
    }

    private void HandleAttackInput()
    {
        if (CanReceiveInput)
        {
            inputReceived = true;

            if(currentComboIndex == 0 || animator.GetCurrentAnimatorStateInfo(0).normalizedTime > 0.5f)
            {
                PerformComboAttack();
            }
        }
    }

    private void PerformComboAttack()
    {
        if(inputReceived && currentComboIndex < MaxComboCount)
        {
            inputReceived = false;
            CanReceiveInput = false;

            animator.SetTrigger("NextCombo");
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
}

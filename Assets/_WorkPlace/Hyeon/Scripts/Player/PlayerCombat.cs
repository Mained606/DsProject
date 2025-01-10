using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public GameObject sword;
    private Collider swordCollider;
    [SerializeField] private float swordDamage = 10f;

    public Animator PlayerAnimator;

    private int currentComboIndex = 0;

    public bool CanReceiveInput { get; set; } = true;
    public bool inputReceived = false;

    public int MaxComboCount { get; set; } = 3;

    public HashSet<GameObject> DamagedTargets { get; set; } = new HashSet<GameObject>();

    private void Start()
    {
        swordCollider = sword.GetComponent<Collider>();
    }

    private void Update()
    {
        HandleAttackInput();
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
}

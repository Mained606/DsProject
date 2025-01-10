using System.Collections.Generic;
using UnityEngine;

public class SwordPivot : MonoBehaviour
{
    private Collider attackCollider;
    private HashSet<GameObject> damagedTargets;

    private Sword sword;

    private void Start()
    {
        attackCollider = GetComponentInChildren<Collider>();
        attackCollider.enabled = false;
        sword = GetComponentInChildren<Sword>();

        damagedTargets = sword.DamagedTargets;
    }
    private void EnableCollider()
    {
        attackCollider.enabled = true;
    }

    private void DisableCollider()
    {
        attackCollider.enabled = false;
    }

    private void EnableComboInput()
    {
        sword.CanReceiveInput = true;
    }

    private void DisableComboInput()
    {
        sword.CanReceiveInput = false;
    }

    private void StartCombo()
    {
        damagedTargets.Clear();
    }
}

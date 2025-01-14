using UnityEngine;

public class empty : MonoBehaviour
{
    PlayerController controller;
    Collider swrodcollider;
    void OnFootstep()
    {

    }

    void OnLand()
    {

    }

    private void EnableCollider()
    {
        transform.GetComponentInParent<PlayerCombat>().SwordCollider.enabled = true;
    }

    private void DisableCollider()
    {
        transform.GetComponentInParent<PlayerCombat>().SwordCollider.enabled = false;
    }

    private void EnableComboInput()
    {
        transform.GetComponentInParent<PlayerCombat>().CanReceiveInput = true;
    }

    private void StartCombo()
    {
        transform.GetComponentInParent<PlayerCombat>().CanReceiveInput = false;
        transform.GetComponentInChildren<Sword>().DamagedTargets.Clear();
    }
}

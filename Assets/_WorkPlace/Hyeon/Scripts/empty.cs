using UnityEngine;

public class empty : MonoBehaviour
{
    PlayerController controller;
    [SerializeField] LayerMask layer;

    void OnFootstep(AnimationEvent animationEvent)
    {
        //Collider[] hitColliders = Physics.OverlapSphere(transform.position + Vector3.up * 1f, 1.5f);
        //foreach(Collider collider in hitColliders)
        //{
        //    if(collider.gameObject.layer == 4)
        //    {
        //        Debug.LogWarning("물 걷는 중");
        //        SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Puddle_Walk_01", transform.position, 10f, false);
        //    }
        //    else
        //    {
        //        Debug.LogWarning("땅 걷는 중");
        //        SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Earth_Run_02", transform.position, 10f, false);
        //    }
        //}

        //RaycastHit hit;
        //if (Physics.Raycast(transform.position + Vector3.up * 1f, Vector3.down, out hit, 10f, LayerMask.GetMask("Default"), QueryTriggerInteraction.Collide))
        //{
        //    Debug.DrawRay(transform.position + Vector3.up * 0.5f, Vector3.down, Color.red);
        //    Debug.LogWarning("레이 맞는 중");
        //    if (hit.collider.gameObject.layer == 4)
        //    {
        //        //Debug.LogWarning("물 걷는 중");
        //        SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Puddle_Walk_01", transform.position, 10f, false);
        //    }
        //    else
        //    {
        //        //Debug.LogWarning("땅 걷는 중");
        //        SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Earth_Run_02", transform.position, 10f, false);
        //    }
        //}
        //else
        //{
        //    //Debug.LogWarning("레이 안 맞는 중");
        //}
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 0.5f, layer))
        {
            if (hit.collider.gameObject.layer == 4)
            {
                SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Puddle_Walk_03", transform.position, 10f, false);
            }
            else
            {
                SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Earth_Run_02", transform.position, 10f, false);
            }
        }
    }

    void OnLand(AnimationEvent animationEvent)
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f, layer))
        {
            if (hit.collider.gameObject.layer == 4)
            {
                SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Puddle_Land_Walk_Forward_Landing_01", transform.position, 0.2f, false);
            }
            else
            {
                SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Earth_Land_Walk_Forward_Landing_01", transform.position, 0.2f, false);
            }
        }
    }

    //private void EnableCollider()
    //{
    //    //transform.GetComponentInParent<PlayerCombat>().weaponCollider.enabled = true;
    //}

    //private void DisableCollider()
    //{
    //    //transform.GetComponentInParent<PlayerCombat>().weaponCollider.enabled = false;
    //}

    //private void EnableComboInput()
    //{
    //    //transform.GetComponentInParent<PlayerCombat>().CanReceiveInput = true;
    //}

    //private void StartCombo()
    //{
    //    //transform.GetComponentInParent<PlayerCombat>().CanReceiveInput = false;
    //}
}

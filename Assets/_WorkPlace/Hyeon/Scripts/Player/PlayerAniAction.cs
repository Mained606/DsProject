using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;

public class PlayerAniAction : MonoBehaviour
{
    private List<string> hitVoices = new List<string> { "Ellen_Emotes_Lands_01", "Ellen_Emotes_Lands_02", "Ellen_Emotes_Lands_03" };
    [SerializeField] private LayerMask layer;
    void OnFootstep(AnimationEvent animationEvent)
    {
        SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Earth_Run_02", transform.position, 50f, false);

    }

    void OnLand(AnimationEvent animationEvent)
    {
        //RaycastHit hit;
        //if (Physics.Raycast(transform.position, Vector3.down, out hit, 1f, layer))
        //{
        //    if (hit.collider.gameObject.layer == 4)
        //    {
        //        SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Puddle_Land_Walk_Forward_Landing_01", transform.position, 0.2f, false);
        //    }
        //    else
        //    {
        //        SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Earth_Land_Walk_Forward_Landing_01", transform.position, 0.2f, false);
        //    }
        //}
        SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Earth_Land_Walk_Forward_Landing_01", transform.position, 1f, false);
    }

    void OnHit(AnimationEvent animationEvent)
    {
        SoundManager.Instance.RandomPlay(hitVoices, this.transform, 0.5f);
    }
}

using UnityEngine;

public class MonsterAnimSounds : MonoBehaviour
{
    [SerializeField] private AudioClip footstep;
    [SerializeField] private float footstepVol = 0.2f;
    [SerializeField] private AudioClip attack;
    [SerializeField] private float attackVol = 0.2f;
    [SerializeField] private AudioClip hit;
    [SerializeField] private float hitVol = 0.2f;
    [SerializeField] private AudioClip dead;
    [SerializeField] private float deadVol = 0.2f;
    public void MonsterFootstep()
    {
        if (footstep == null) return;
        SoundManager.Instance.PlayClipAtPoint(footstep.name, transform.position, footstepVol, false);
    }

    public void MonsterAttackSound()
    {
        if (attack == null) return;
        SoundManager.Instance.PlayClipAtPoint(attack.name, transform.position, attackVol, false);
    }

    public void MonsterHitSound()
    {
        if (hit == null) return;
        SoundManager.Instance.PlayClipAtPoint(hit.name, transform.position, hitVol, false);
    }

    public void MonsterDeadSound()
    {
        if (dead == null) return;
        SoundManager.Instance.PlayClipAtPoint(dead.name, transform.position, deadVol, false);
    }
}

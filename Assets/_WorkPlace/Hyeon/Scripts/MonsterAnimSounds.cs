using UnityEngine;

public class MonsterAnimSounds : MonoBehaviour
{
    [SerializeField] private AudioClip footstep;
    [SerializeField] private AudioClip attack;
    [SerializeField] private AudioClip hit;
    public void MonsterFootstep()
    {
        if (footstep == null) return;
        SoundManager.Instance.PlayClipAtPoint(footstep.name, transform.position, 0.5f, false);
    }

    public void MonsterAttackSound()
    {
        if (attack == null) return;
        SoundManager.Instance.PlayClipAtPoint(attack.name, transform.position, 0.5f, false);
    }
}

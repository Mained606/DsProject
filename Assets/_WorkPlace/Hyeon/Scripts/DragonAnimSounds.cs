using UnityEngine;

public class DragonAnimSounds : MonoBehaviour
{
    [SerializeField] private AudioClip wingSound;
    [SerializeField] private float wingVol = 1f;

    [SerializeField] private AudioClip mileeSound;
    [SerializeField] private float mileeVol = 1f;

    [SerializeField] private AudioClip fireballSound;
    [SerializeField] private float fireballVol = 1f;

    public void WingSound()
    {
        if (wingSound == null) return;
        SoundManager.Instance.PlayClipAtPoint(wingSound.name, transform.position, wingVol, false);
    }

    public void MileeSound()
    {
        if (mileeSound == null) return;
        SoundManager.Instance.PlayClipAtPoint(mileeSound.name, transform.position, mileeVol, false);
    }

    public void FireballSound()
    {
        if (fireballSound == null) return;
        SoundManager.Instance.PlayClipAtPoint(fireballSound.name, transform.position, fireballVol, false);
    }
}

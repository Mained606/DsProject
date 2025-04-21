using UnityEngine;

public class DragonAnimSounds : MonoBehaviour
{
    [SerializeField] private AudioClip wingSound;
    [SerializeField] private float wingVol = 1f;

    [SerializeField] private AudioClip mileeSound;
    [SerializeField] private float mileeVol = 1f;

    [SerializeField] private AudioClip fireballSound;
    [SerializeField] private float fireballVol = 1f;

    [SerializeField] private AudioClip octaWingSound;
    [SerializeField] private float octaWingVol = 1f;

    [SerializeField] private AudioClip octaMileeSound;
    [SerializeField] private float octaMileeVol = 1f;

    [SerializeField] private AudioClip octaFireballSound;
    [SerializeField] private float octaFireballVol = 1f;

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

    public void OctaWingSound()
    {
        if (octaWingSound == null) return;
        SoundManager.Instance.PlayClipAtPoint(octaWingSound.name, transform.position, octaWingVol, false);
    }

    public void OctaMileeSound()
    {
        if (octaMileeSound == null) return;
        SoundManager.Instance.PlayClipAtPoint(octaMileeSound.name, transform.position, octaMileeVol, false);
    }

    public void OctaFireballSound()
    {
        if (octaFireballSound == null) return;
        SoundManager.Instance.PlayClipAtPoint(octaFireballSound.name, transform.position, octaFireballVol, false);
    }
}

using System.Collections.Generic;
using UnityEngine;


public enum NpcType
{
    Wander,
    Farmer,
    Craft,
    Fishing,
    Sitting
}

public class NpcController : MonoBehaviour
{
    public bool isFemale;
    public NpcType npcType;
    public Transform npcTool;
    public Vector3 sittingOffset;
    [SerializeField] private float voicePitch;

    [HideInInspector][SerializeField] LayerMask layer;
    private List<string> speakVoices = new List<string>{ "cartoon voice1", "cartoon voice2", "cartoon voice3", "cartoon voice4" };
    private const int COMMON_LAYER_INDEX = 1;

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        gameObject.AddComponent<ActivityNpc>();

        if (isFemale)
        {
            voicePitch = Random.Range(0.9f, 1.0f);
        }
        else
        {
            voicePitch = Random.Range(0.55f, 0.65f);
        }
    }

    void OnFootStep(AnimationEvent animationEvent)
    {
        float distance = Vector3.Distance(GameManager.playerTransform.position, transform.position);
        if (distance <= 10f)
        {
            SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Earth_Run_02", transform.position, 10f, false);
        }
            
    }

    void OnSpeak(AnimationEvent animationEvent)
    {
        float distance = Vector3.Distance(GameManager.playerTransform.position, transform.position);
        if (distance <= 10f)
        {
            SoundManager.Instance.RandomPlay(speakVoices, transform, 30f, voicePitch);
        }
    }

    void OnClapping(AnimationEvent animationEvent)
    {
        float distance = Vector3.Distance(GameManager.playerTransform.position, transform.position);
        if (distance <= 10f)
        {
            SoundManager.Instance.PlayClipAtPoint("Clap", transform.position, 0.3f, false);
        }
    }

    void OnCrafting(AnimationEvent animationEvent)
    {
        if (animator.GetCurrentAnimatorStateInfo(COMMON_LAYER_INDEX).normalizedTime < 1f) return;

        float distance = Vector3.Distance(GameManager.playerTransform.position, transform.position);
        if (distance <= 10f)
        {
            SoundManager.Instance.PlayClipAtPoint("Crafting", transform.position, 0.4f, false);
        }
    }

    void OnFarming(AnimationEvent animationEvent)
    {
        if (animator.GetCurrentAnimatorStateInfo(COMMON_LAYER_INDEX).normalizedTime < 1f) return;

        float distance = Vector3.Distance(GameManager.playerTransform.position, transform.position);
        if (distance <= 10f)
        {
            SoundManager.Instance.PlayClipAtPoint("Farming", transform.position, 5f, false);
        }
    }
}

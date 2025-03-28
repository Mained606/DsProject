using System.Collections.Generic;
using UnityEngine;


public enum NpcType
{
    Wander,
    Farmer,
    Craft,
    Fishing,
    Sitting,
    Animal
}

public class NpcController : MonoBehaviour
{
    public NpcType npcType;
    public Transform npcTool;
    public Vector3 sittinOffset;
    public bool isFemale;
    [SerializeField] private float voicePitch;
    [SerializeField] LayerMask layer;
    private List<string> speakVoices = new List<string>{ "cartoon voice1", "cartoon voice2", "cartoon voice3", "cartoon voice4" };
    

    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        gameObject.AddComponent<ActivityNpc>();

        //if (npcType == NpcType.Wander)
        //{
        //    gameObject.AddComponent<WanderNpc>();
        //}
        //else if(npcType == NpcType.Animal)
        //{
        //    gameObject.AddComponent<AnimalNpc>();
        //}
        //else
        //{
        //    gameObject.AddComponent<ActivityNpc>();
        //}

        if(isFemale)
        {
            voicePitch = Random.Range(0.8f, 0.9f);
        }
        else
        {
            voicePitch = Random.Range(0.65f, 0.75f);
        }
    }

    void Speak(AnimationEvent animationEvent)
    {
        SoundManager.Instance.RandomPlay(speakVoices, transform, 0.5f, voicePitch);
    }
}

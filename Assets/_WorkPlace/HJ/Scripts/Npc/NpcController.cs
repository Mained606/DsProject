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
    [SerializeField] LayerMask layer;
    private List<string> speakVoices = new List<string>{ "cartoon voice1", "cartoon voice2", "cartoon voice3", "cartoon voice4" };
    
    private Animator animator;

    private void Awake()
    {
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
        if(distance <= 10f)
        {
            //Debug.Log("플레이어와의 거리" + distance);
            SoundManager.Instance.PlayClipAtPoint("Ellen_Footsteps_Earth_Run_02", transform.position, 10f, false);
        }
            
    }

    void OnSpeak(AnimationEvent animationEvent)
    {
        float distance = Vector3.Distance(GameManager.playerTransform.position, transform.position);        
        if (distance <= 10f)
        {
            //Debug.Log("플레이어와의 거리" + distance);
            SoundManager.Instance.RandomPlay(speakVoices, transform, 10f, voicePitch);
        }
    }
}

using UnityEngine;
public enum NpcType
{
    Wander,
    Farmer,
    BlackSmith,
    Fishing,
    Guard,
    Sitting
}

public class NpcController : MonoBehaviour
{
    public NpcType npcType;

    private void Awake()
    {
        if(npcType == NpcType.Wander)
        {
            gameObject.AddComponent<WanderNpc>();
        }
        else if(npcType == NpcType.Farmer || npcType == NpcType.BlackSmith || npcType == NpcType.Fishing)
        {
            gameObject.AddComponent<ActivityNpc>();
        }
    }
}

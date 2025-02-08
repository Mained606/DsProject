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
    public NpcType npcType;

    private void Awake()
    {
        if (npcType == NpcType.Wander)
        {
            gameObject.AddComponent<WanderNpc>();
        }
        else if (npcType == NpcType.Farmer || npcType == NpcType.Craft || npcType == NpcType.Fishing || npcType == NpcType.Sitting)
        {
            gameObject.AddComponent<ActivityNpc>();
        }
    }
}

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
    //public Transform leftLeg;
    //public Transform rightLeg;

    private void Awake()
    {
        if (npcType == NpcType.Wander)
        {
            gameObject.AddComponent<WanderNpc>();
        }
        else if(npcType == NpcType.Animal)
        {
            gameObject.AddComponent<AnimalNpc>();
        }
        else
        {
            gameObject.AddComponent<ActivityNpc>();
        }
    }
}

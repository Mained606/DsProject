using System.Collections.Generic;
using UnityEngine;

public class InteractArrangementer : MonoBehaviour
{
    public static InteractArrangementer Instance { get; private set; }

    public List<Transform> conversationable = new List<Transform>();

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void IsNearestNPC()
    {
        Transform closestNpc = null;
        float minDistance = Mathf.Infinity;

        foreach(Transform target in conversationable)
        {
            float distance = Vector3.Distance(GameManager.playerTransform.position, target.position);
            if(distance < minDistance)
            {
                minDistance = distance;
                closestNpc = target;
            }
        }

        closestNpc.GetComponent<NonePlayerCharacter>().isNearest = true;
    }
}

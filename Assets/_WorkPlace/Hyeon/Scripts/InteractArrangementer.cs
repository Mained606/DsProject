using System.Collections.Generic;
using UnityEngine;

public class InteractArrangementer : MonoBehaviour
{
    public static InteractArrangementer Instance { get; private set; }

    public List<Transform> conversationable = new List<Transform>();

    private NonePlayerCharacter npc;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }
    private void Start()
    {
        //InitDictionary();
    }

    //private void InitDictionary()
    //{
    //    interactableNPC.Clear();

    //    NonePlayerCharacter[] npcs = GetComponentsInChildren<NonePlayerCharacter>(true); // true: 비활성화 포함

    //    foreach(NonePlayerCharacter npc in npcs)
    //    {
    //        Transform key = npc.transform;

    //        if (!interactableNPC.ContainsKey(key))
    //        {
    //            interactableNPC.Add(key, false);
    //        }
    //        else
    //        {
    //            Debug.Log($"이미 딕셔너리에 등록된 NPC 입니다.");
    //        }
    //    }

    //    Debug.Log($"총 {interactableNPC.Count} 명의 npc 딕셔너리에 등록 완료");
    //}

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

        Debug.Log($"closetNpc : {closestNpc.name}");
        closestNpc.GetComponent<NonePlayerCharacter>().isNearest = true;
    }
}

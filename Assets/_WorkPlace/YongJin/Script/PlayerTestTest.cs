using UnityEngine;

public class PlayerTestTest : MonoBehaviour
{
    [SerializeField] private NpcTest Npc;
    private void Update() 
    {

        if(Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("e누름");
            Npc.ReceiveQuest();

        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("q누름");
            Npc.ClearQuestCheck();
        }
    }
}

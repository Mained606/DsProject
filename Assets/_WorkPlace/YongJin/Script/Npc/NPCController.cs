using System;
using UnityEngine;
using System.Collections.Generic;

public class NPCController : MonoBehaviour
{
    public float interactionRange = 3f; // Range within which interaction is possible
    [SerializeField] private int NpcId;
    private Transform playerTransform; // Reference to the player
    private bool isPlayerInRange = false;
    
    public NPCTable contextNPC;
    private NPCStringTable NPCStringTable;


    private List<QuestTable> hasQuestList = new List<QuestTable>();
    private List<QuestTable> giveQuestList = new List<QuestTable>();
    public List<int> hasQuestIdList = new List<int>();

    public string NPCName;
    public string NPCDesc;
    

    void Start()
    {
        //QuestManager.Instance.OnInitQuestManager += OnInitQuestManager_QuestManager;
        NPCManager.Instance.OnInitNpcManager += OnInitNpcManager_NPCManager;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player 태그가 없어용");
        }
    }
    private void OnDisable() 
    {
        //QuestManager.Instance.OnInitQuestManager -= OnInitQuestManager_QuestManager;
        NPCManager.Instance.OnInitNpcManager -= OnInitNpcManager_NPCManager; 
    }
    void Update()
    {
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(playerTransform.position, transform.position);
            isPlayerInRange = distance <= interactionRange;
            if (isPlayerInRange && InputManager.InputActions.actions["Interact"].triggered)
            {
                Interact();
            }
            if(!isPlayerInRange)
            {
                //거리내에 있지 않을 때
            }
        }
       
        if(hasQuestList.Contains(QuestManager.Instance.GetQuestIdToQuestTable(QuestManager.Instance.nextMainQuestId))) // 퀘스트 기부 파트
        {
            contextNPC.NPC_isMainQuestGiver = true;
        }
        else
        {
            contextNPC.NPC_isMainQuestGiver = false;
        }
        
    }
    
    
    private void OnInitNpcManager_NPCManager(object sender, EventArgs e)
    {
        NPCInit();
    }
    private void QuestInit()
    {
        foreach(var questId in hasQuestIdList)
        {
            hasQuestList.Add(QuestManager.Instance.GetQuestIdToQuestTable(questId));
        }
    }
    private void NPCInit()
    {
        if(NpcId > 0)
        {
            contextNPC = NPCManager.Instance.GetNPCIdToNPCTable(NpcId);            
            NPCStringTable = NPCManager.Instance.GetNPCStringData(contextNPC.NPC_stringIdx);
            NPCName = NPCStringTable.NPCString_name;
            NPCDesc = NPCStringTable.NPCString_desc;
            contextNPC.NPC_isInteractable = true;
            hasQuestIdList = contextNPC.NPC_questIds;
            QuestInit();
        }
        else
        {
            Debug.LogError($"NPC아이디 입력 안함 + {gameObject.name}");
        }
    }

    public void Interact()
    {
        if (!contextNPC.NPC_isInteractable)
        {
            Debug.Log($"{NPCStringTable.NPCString_name}은(는) 상호작용이 불가능합니다.");
            return;
        }
        else
        {
            switch(contextNPC.NPC_currentState)
            {
                case NPCState.Common :
                    UIManagerTest.Instance.DialogUI.StartDialogue(this, NPCStringTable.NPCString_dialogue);
                    break;
                case NPCState.MainQuestGiver :
                    UIManagerTest.Instance.DialogUI.StartDialogue(QuestManager.Instance.GetQuestStringData(QuestManager.Instance.nextMainQuestId).questString_diologue);
                    break;
                case NPCState.SubQuestGiver :
                    break;
                case NPCState.Enemy :
                    break;
            }
        }
    }
    public bool SameQuestCheck()
    {
        if(giveQuestList.Count != 0)
        {
            foreach(var questData in giveQuestList)
            {
                if(QuestManager.Instance.currnetQuests.Contains(questData))
                {
                    return true;
                }
            }
            return false;
        }
        else
        {
            return false;
        }
        
        
    }
    public void GiveQuest()
    {
        
        if(hasQuestList.Contains(QuestManager.Instance.GetQuestIdToQuestTable(QuestManager.Instance.nextMainQuestId)))
        {
            var data = hasQuestList.Find(x => x.quest_index == QuestManager.Instance.nextMainQuestId);
            giveQuestList.Add(data);
            QuestManager.Instance.currnetQuests.Add(data);
        }
        else
        {
            Debug.Log("퀘스트 조건 X");
            return;
        }   
        
    }
    public void ChangeState(NPCState NextNPCState)
    {
        
        if(contextNPC.NPC_currentState == NextNPCState)
        {
            return;
        }
        contextNPC.NPC_currentState = NextNPCState;
        Debug.Log($"스테이터스 체인지{contextNPC.NPC_currentState}=>{NextNPCState}");
    }
    public void ReceveQuest()
    {

    }

    /* private void OnDrawGizmosSelected()
    {
        // Draw a sphere in the editor to visualize the interaction range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    } */
    
}

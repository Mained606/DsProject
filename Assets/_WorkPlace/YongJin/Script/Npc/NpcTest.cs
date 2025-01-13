using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

public class NpcTest : MonoBehaviour
{
    
    
    public List<int> hasQuestIdList = new List<int>();
    private List<QuestTable> hasQuestList = new List<QuestTable>();
    private List<QuestTable> giveQuestList = new List<QuestTable>();
    //public bool canRequireQuest;
    
    
    private void Start() 
    {
        QuestManager.Instance.OnInitQuestManager += OnInitQuestManager_QuestManager;    
    }
    
    private void OnDisable() 
    {
        QuestManager.Instance.OnInitQuestManager -= OnInitQuestManager_QuestManager;    
    }
    private void OnInitQuestManager_QuestManager(object sender, EventArgs e)
    {
        Init();
        
    }
    private void Update()
    {
        ReceiveQuest();
        
    }
    private void Init()
    {
        foreach(var questId in hasQuestIdList)
        {
            hasQuestList.Add(QuestManager.Instance.GetQuestIdToQuestTable(questId));
        }
        
    }
    public void ReceiveQuest()
    {
        if(InputManager.InputActions.actions["Interect"].triggered)
        {
            foreach(var questData in giveQuestList)
            {
                if(QuestManager.Instance.currnetQuests.Contains(questData))
                {
                    
                    Debug.Log("이미 퀘스트 진행 중임");
                    return;
                }
            }
            if(hasQuestList.Contains(QuestManager.Instance.GetQuestIdToQuestTable(QuestManager.Instance.nextQuestId)))
            {
                var data = hasQuestList.Find(x=> x.quest_index == QuestManager.Instance.nextQuestId);
                giveQuestList.Add(data);
                QuestManager.Instance.currnetQuests.Add(data);
            }
            else
            {
                Debug.Log("퀘스트 조건 X");
                return;
            }   
        }
             
        
    }
    public void ClearQuestCheck()
    {
        foreach(var quest in QuestManager.Instance.currnetQuests)
        {
            var data = giveQuestList.Find(x => x.quest_index == quest.quest_index);
            if(data != null)
            {
                //현재 진행중인 퀘스트와 NPC가 준 퀘스트를 비교 및 조건 완료 확인이 있다는 가정하에 진행 Player
                giveQuestList.Remove(data);
                QuestManager.Instance.SuccessQuest(data.quest_index);
                QuestManager.Instance.nextQuestId++;
                Debug.Log("퀘스트 클리어");
                break;
            }
            else
            {
                Debug.Log("클리어할 퀘스트 없음");
            }
        }
        
    }
    

    
    

    
    

}

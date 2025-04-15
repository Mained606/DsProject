using UnityEngine;

public class NPCQuestDialogueInitializer : MonoBehaviour
{
    [System.Serializable]
    public class QuestDialogueInfo
    {
        public string npcId = "SubNPC_1";
        public string questId = "1_2001";
        
        [Tooltip("퀘스트 제공자인지 또는 퀘스트 조건 NPC인지 설정")]
        public bool isQuestProvider = false;  // true: 퀘스트 제공자, false: 퀘스트 조건 NPC
        
        [TextArea(2, 5)]
        public string[] giveDialogues = new string[] { "퀘스트 지급 대화" };
        [TextArea(2, 5)]
        public string[] progressDialogues = new string[] { "퀘스트 진행 중 대화" };
        [TextArea(2, 5)]
        public string[] completeDialogues = new string[] { "퀘스트 완료 대화" };
    }
    
    [SerializeField] private QuestDialogueInfo[] questDialogues;
    
    void Start()
    {
        InitializeQuestDialogues();
    }
    
    private void InitializeQuestDialogues()
    {
        foreach (var dialogueInfo in questDialogues)
        {
            // 1. 퀘스트 대화 데이터 설정
            QuestManager.SetQuestDialogues(
                dialogueInfo.npcId,
                dialogueInfo.questId,
                dialogueInfo.giveDialogues,
                dialogueInfo.progressDialogues,
                dialogueInfo.completeDialogues
            );
            
            // 2. NPC가 퀘스트 제공자가 아닌 경우(조건 NPC인 경우) NPCData의 relatedQuestIds에 직접 추가
            if (!dialogueInfo.isQuestProvider)
            {
                NPCData npcData = QuestManager.FindNpcDataById(dialogueInfo.npcId);
                if (npcData != null)
                {
                    if (npcData.relatedQuestIds == null)
                    {
                        npcData.relatedQuestIds = new System.Collections.Generic.List<string>();
                    }
                    
                    if (!npcData.relatedQuestIds.Contains(dialogueInfo.questId))
                    {
                        npcData.relatedQuestIds.Add(dialogueInfo.questId);
                        Debug.Log($"관련 퀘스트 목록에 {dialogueInfo.questId} 추가됨 (NPC: {dialogueInfo.npcId})");
                    }
                }
            }
            
            Debug.Log($"NPC {dialogueInfo.npcId}의 퀘스트 {dialogueInfo.questId} 대화 초기화 완료");
        }
    }
    
    // 게임 실행 중에 대화 설정을 업데이트하는 메서드 (테스트용)
    public void UpdateQuestDialogue(string npcId, string questId, string[] giveDialogues, string[] progressDialogues, string[] completeDialogues, bool isQuestProvider = false)
    {
        // 1. 퀘스트 대화 데이터 설정
        QuestManager.SetQuestDialogues(npcId, questId, giveDialogues, progressDialogues, completeDialogues);
        
        // 2. 퀘스트 관계 설정
        NPCData npcData = QuestManager.FindNpcDataById(npcId);
        if (npcData != null && !isQuestProvider)
        {
            // 퀘스트 제공자가 아닌 경우, 관련 퀘스트 목록에 추가
            if (npcData.relatedQuestIds == null)
            {
                npcData.relatedQuestIds = new System.Collections.Generic.List<string>();
            }
            
            if (!npcData.relatedQuestIds.Contains(questId))
            {
                npcData.relatedQuestIds.Add(questId);
                Debug.Log($"관련 퀘스트 목록에 {questId} 추가됨 (NPC: {npcId})");
            }
        }
        
        Debug.Log($"NPC {npcId}의 퀘스트 {questId} 대화 업데이트 완료");
    }
} 
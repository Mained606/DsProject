using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Rendering;

public class QuestManager : BaseManager<QuestManager>
{
    [SerializeField] private NPCList npcDataList;
    // [SerializeField] private QuestList questList;
    [SerializeField] private List<NPCData> npcDatabase = new List<NPCData>();
    [SerializeField] private List<Quest> questDatabase = new List<Quest>();
    [SerializeField] private List<Quest> mainQuestDatabase = new List<Quest>();
    [SerializeField] private List<Quest> subQuestDatabase = new List<Quest>();
    [SerializeField] private List<Quest> completedQuests = new List<Quest>();
    [SerializeField] private Dictionary<string, Transform> questConditionPoint = new Dictionary<string, Transform>();

    public static List<Quest> QuestDatabase => Instance.questDatabase;
    public static List<Quest> CompletedQuests => Instance.completedQuests;
    public static Dictionary<string, Transform> QuestConditionPoint => Instance.questConditionPoint;

    public static Transform GetQuestConditionPoint(string point)
    {
        if (QuestConditionPoint.ContainsKey(point)) { return Instance.questConditionPoint[point]; }
        else { return null; }
    }

    public static NPCList NpcDatabase => Instance.npcDataList;

    private int currentMainQuestIndex = 0;
    public static int CurrentMainQuestIndex => Instance.currentMainQuestIndex;

    [SerializeField] private GameObject dragon;
    private PlayerController player;


    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void Start()
    {
        base.Start();
        //npcDataList.npcLists.Clear();
        GenerateData generater = new GenerateData();
        mainQuestDatabase = generater.GenerateMainQuestLists();
        subQuestDatabase = generater.GenerateQuestLists();

        //generater.GenerateRandomNPCs(200, subQuestDatabase, ItemManager.ItemDatabase, subQuestDatabase, npcDataList);
        npcDatabase = npcDataList.npcLists;
        player = GameManager.playerTransform.GetComponent<PlayerController>();
        GameStateMachine.Instance.ChangeState(GameSystemState.MainQuestPlay);
    }

    public Quest GiveQuests()
    {
        Quest quest = GenerateQuests();
        return quest;
    }

    public void AssignQuestToNPC(string questId, string npcId)
    {
        var quest = questDatabase.Find(q => q.id == questId);
        var npc = npcDatabase.Find(n => n.id == npcId.ToString());
        if (quest != null && npc != null)
        {
            quest.targetID = npcId;
        }
    }

    public void AddQuest(Quest quest)
    {
        // 로깅용 접두사 생성
        string logPrefix = $"[AddQuest] '{quest.id}'";
        
        // 첫 번째 메인 퀘스트이거나 DialogueState에서 호출된 경우는 별도 처리
        bool isFromDialogueState = GameStateMachine.Instance.CurrentState == GameSystemState.DialogueState;
        bool isFirstQuest = quest.id == "1_1001"; // 첫 번째 퀘스트 ID 확인
        
        // 중복 퀘스트 체크
        if (questDatabase.Exists(q => q.id == quest.id))
        {
            Debug.Log($"{logPrefix}: 이미 등록된 퀘스트입니다.");
            GameStateMachine.Instance.ChangeState(GameSystemState.InfoMessage, $"퀘스트 '{quest.id}'는 이미 등록되어 있습니다.");
            return;
        }
        
        // 완료된 메인 퀘스트 체크
        if (completedQuests.Exists(q => q.id == quest.id) && quest.questType == "메인퀘스트")
        {
            Debug.Log($"{logPrefix}: 이미 완료된 메인 퀘스트입니다.");
            GameStateMachine.Instance.ChangeState(GameSystemState.InfoMessage, $"완료된 메인 퀘스트 '{quest.id}'는 추가할 수 없습니다.");
            return;
        }
        
        // 메인 퀘스트 진행중 체크 (첫 번째 퀘스트나 대화 상태에서는 예외)
        if (!isFirstQuest && !isFromDialogueState && quest.questType == "메인퀘스트" && 
            questDatabase.Exists(q => q.questType == "메인퀘스트"))
        {
            Debug.Log($"{logPrefix}: 이미 진행 중인 메인 퀘스트가 있습니다.");
            GameStateMachine.Instance.ChangeState(GameSystemState.InfoMessage, $"이미 진행 중인 메인 퀘스트가 있습니다.");
            return;
        }
        
        // 퀘스트 추가 및 UI 업데이트
        questDatabase.Add(quest);
        UIManager.Instance.QuestUpdate();
        Debug.Log($"{logPrefix}: 퀘스트가 성공적으로 추가되었습니다.");
    }

    public void RemoveQuest(string questId)
    {
        Quest quest = questDatabase.Find(q => q.id == questId);
        if (quest != null)
        {
            questDatabase.Remove(quest);
            UIManager.SystemGameMessage($"퀘스트 '{quest.name}' 삭제됨", MessageTag.아이템_획득);
        }
        else
        {
            Debug.LogWarning($"[QuestManager] ID: {questId} 퀘스트를 찾을 수 없습니다.");
        }
    }

    public Quest GetQuestById(string questId)
    {
        // 먼저 메인 퀘스트 데이터베이스에서 찾기
        Quest quest = mainQuestDatabase.Find(q => q.id == questId);
        
        // 없으면 서브 퀘스트 데이터베이스에서 찾기
        if (quest == null)
        {
            quest = subQuestDatabase.Find(q => q.id == questId);
        }
        
        // 없으면 전체 퀘스트 데이터베이스에서 찾기
        if (quest == null)
        {
            quest = questDatabase.Find(q => q.id == questId);
        }
        
        if (quest == null)
        {
            Debug.LogWarning($"[QuestManager] ID: {questId} 퀘스트를 찾을 수 없습니다.");
        }
        
        return quest;
    }

    //TODO
    //모험, 처치, 만남떄 이곳을 호출하는부분 추가해줘야함.
    public void UpdateQuestProgress(QuestConditionType conditionType, string targetId, int quantity = 1, NPCData npcData = null)
    {
        for (int i = questDatabase.Count - 1; i >= 0; i--)
        {
            if (i < 0 || i >= questDatabase.Count) continue;
            Quest quest = questDatabase[i];
            if (quest.isCompleted) continue;
            
            // Collect 타입 조건이면 CheckQuestCondition 호출하여 인벤토리 수량 기준으로 업데이트
            if (conditionType == QuestConditionType.Collect)
            {
                quest.CheckQuestCondition();
                
                // CheckQuestCondition 이후 퀘스트 완료 여부 재확인
                if (IsQuestCompleted(quest))
                {
                    quest.isCompleted = true;
                    UIManager.SystemGameMessage($"퀘스트 '{quest.name}' 완료!", MessageTag.아이템_획득);
                    UIManager.Instance.QuestUpdate();
                }
                
                // Collect 타입 조건은 CheckQuestCondition에서 처리되었으므로 여기서는 처리하지 않음
                // 이후 다른 조건 타입(Explore, Kill, Meet 등)을 처리
                continue;
            }
            
            foreach (var conditionKeyValue in quest.requiredConditions)
            {
                var conditionId = conditionKeyValue.Key;
                var condition = conditionKeyValue.Value;
                if (condition.type == conditionType && condition.targetId == targetId)
                {
                    if (!quest.progress.ContainsKey(conditionId))
                    {
                        quest.progress[conditionId] = 0;
                    }
                    quest.progress[conditionId] += quantity;
                    if (quest.progress[conditionId] >= condition.requiredQuantity)
                    {
                        quest.progress[conditionId] = condition.requiredQuantity;
                        condition.isCompleted = true;
                        UIManager.SystemGameMessage($"퀘스트 조건 '{condition.targetName}' 완료!", MessageTag.아이템_획득);
                    }
                }
            }
            if (IsQuestCompleted(quest))
            {
                quest.isCompleted = true;
                UIManager.SystemGameMessage($"퀘스트 '{quest.name}' 완료!", MessageTag.아이템_획득);
                UIManager.Instance.QuestUpdate();
            }
        }
    }

    private bool IsQuestCompleted(Quest quest)
    {
        foreach (var condition in quest.requiredConditions)
        {
            string conditionId = condition.Key;
            QuestCondition questCondition = condition.Value;

            // Collect 타입인 경우, 인벤토리의 실제 아이템 수량을 확인하여 검증
            if (questCondition.type == QuestConditionType.Collect)
            {
                int actualItemQuantity = InventoryManager.Instance.GetItemQuantity(questCondition.targetId);
                if (actualItemQuantity < questCondition.requiredQuantity)
                {
                    return false;
                }
            }
            // 기타 타입의 경우 progress 값으로 확인
            else if (!quest.progress.ContainsKey(conditionId) || quest.progress[conditionId] < questCondition.requiredQuantity)
            {
                return false;
            }
        }
        return true;
    }

    public void CompleteQuest(Quest quest)
    {
        if (questDatabase.Contains(quest))
        {
            questDatabase.Remove(quest);
        }
        completedQuests.Add(quest);

        foreach (var condition in quest.requiredConditions)
        {
            // 퀘스트 완료 시 CompassIndicater에서 해당 타겟 제거
            if (QuestConditionPoint.ContainsKey(condition.Key))
            {
                Transform targetPoint = QuestConditionPoint[condition.Key];
                if (targetPoint != null)
                {
                    CompassIndicater.RemoveTarget(targetPoint);
                }
            }

            switch (condition.Value.type)
            {
                case QuestConditionType.Collect:
                    InventoryManager.Instance.RemoveItemLogic(condition.Key, condition.Value.requiredQuantity);
                    break;

                case QuestConditionType.Kill:
                    UIManager.SystemGameMessage($"처치 조건 '{condition.Value.targetName}' 완료 처리.", MessageTag.퀘스트);
                    break;

                case QuestConditionType.Explore:
                    UIManager.SystemGameMessage($"탐험 조건 '{condition.Value.targetName}' 완료 처리.", MessageTag.퀘스트);
                    break;

                case QuestConditionType.Meet:
                    UIManager.SystemGameMessage($"만남 조건 '{condition.Value.targetName}' 완료 처리.", MessageTag.퀘스트);
                    break;

                default:
                    Debug.LogWarning($"[QuestManager] 알 수 없는 퀘스트 조건: {condition.Value.type}");
                    break;
            }
        }

        foreach (Reward reward in quest.rewards)
        {
            if (!string.IsNullOrEmpty(reward.itemId))
            {
                ItemManager.Instance.AddItemLogic(reward.itemId, reward.quantity);
                UpdateQuestProgress(QuestConditionType.Collect, reward.itemId, reward.quantity);
                UIManager.SystemGameMessage($"보상 아이템 '{reward.itemId}' {reward.quantity}개 지급됨.", MessageTag.아이템_획득);
            }

            if (reward.experience > 0)
            {
                CharacterManager.PlayerCharacterData.AddExperience(reward.experience);
                UIManager.SystemGameMessage($"경험치 {reward.experience} 지급됨.", MessageTag.퀘스트);
            }

            if (reward.gold > 0)
            {
                CharacterManager.PlayerCharacterData.AddGold(reward.gold);
                UIManager.SystemGameMessage($"골드 {reward.gold} 지급됨.", MessageTag.금화_획득);
            }
        }

        if (quest.questType == "메인퀘스트" && mainQuestDatabase.Count > currentMainQuestIndex)
        {
            currentMainQuestIndex++;
            if(quest.questType == "메인퀘스트" && quest.needsDialog)
            {
                GameStateMachine.Instance.ChangeState(GameSystemState.DialogueState);
            }
            else
            {
                GameStateMachine.Instance.ChangeState(GameSystemState.MainQuestPlay);
            }
        }
        // 서브퀘스트도 완료 상태를 유지하도록 수정
        UIManager.SystemGameMessage($"퀘스트 '{quest.name}' 보상이 지급되었습니다.", MessageTag.퀘스트);
        
        // 퀘스트 보상 처리 (스킬 언락, 오브젝트 활성화 등)
        ProcessQuestRewards(quest.id);
        
        UIManager.Instance.QuestUpdate();
    }

    // 퀘스트 보상 처리 메서드 (스킬 언락 및 오브젝트 활성화)
    private void ProcessQuestRewards(string questId)
    {
        Debug.Log($"[QuestManager] 퀘스트 {questId} 보상 처리 중...");
        
        // 특정 퀘스트 ID에 따른 보상 처리
        if (questId == "1_1004")
        {
            // 1. FireStrike 스킬 언락
            UnlockSkill("FireStrike");
            
            // 2. 용 오브젝트 활성화
            ActivateDragon();
            
            ActivateGameObject("용", false);

            player.unlockGlide = true;
        }
        
        // 필요에 따라 다른 퀘스트 ID 케이스 추가
    }
    
    // 스킬 언락 메서드
    private void UnlockSkill(string skillName)
    {
        // SkillList에서 스킬 찾기
        if (SkillManager.Instance != null && SkillManager.SkillDatabase != null)
        {
            foreach (var skill in SkillManager.SkillDatabase.playerSkills)
            {
                if (skill.skillName == skillName)
                {
                    skill.unLockSkill = true;
                    Debug.Log($"[QuestManager] 스킬 {skillName}을(를) 언락했습니다.");
                    return;
                }
            }
            
            Debug.LogWarning($"[QuestManager] 스킬 {skillName}을(를) 플레이어 스킬 목록에서 찾을 수 없습니다.");
        }
        else
        {
            Debug.LogError("[QuestManager] SkillManager 또는 SkillDatabase가 null입니다.");
        }
    }
    
    // 게임 오브젝트 활성화/비활성화 메서드
    private void ActivateGameObject(string objectName, bool activate)
    {
        GameObject obj = GameObject.Find(objectName);
        if (obj != null)
        {
            obj.SetActive(activate);
            Debug.Log($"[QuestManager] 게임 오브젝트 {objectName}을(를) {(activate ? "활성화" : "비활성화")}했습니다.");
        }
        else
        {
            Debug.LogWarning($"[QuestManager] 게임 오브젝트 {objectName}을(를) 찾을 수 없습니다.");
        }
    }

    private void ActivateDragon()
    {
        if(dragon != null)
        {
            dragon.SetActive(true);
        }
    }

    public Quest GenerateQuests()
    {
        return subQuestDatabase[Random.Range(0, subQuestDatabase.Count)];
    }

    private void MainQuestSequenceStart(int index)
    {
        string logPrefix = $"[MainQuestSequenceStart] 인덱스: {index}";
        Debug.Log($"{logPrefix}, 메인 퀘스트 DB 크기: {mainQuestDatabase.Count}");
        
        // 인덱스 범위 체크
        if (index >= mainQuestDatabase.Count)
        {
            Debug.LogWarning($"{logPrefix}: 인덱스가 범위를 벗어났습니다. 사용 가능한 퀘스트 수: {mainQuestDatabase.Count}");
            return;
        }
        
        // 첫 번째 퀘스트이거나, 이전 퀘스트가 완료된 경우에만 다음 퀘스트 진행
        bool isFirstQuest = index == 0;
        bool isPreviousQuestCompleted = index > 0 && mainQuestDatabase[index-1].isCompleted;
        
        if (isFirstQuest || isPreviousQuestCompleted)
        {
            // 현재 진행할 퀘스트
            Quest questToAdd = mainQuestDatabase[index];
            
            // 이미 진행 중인 같은 ID의 퀘스트가 있는지 확인
            bool alreadyHasQuest = questDatabase.Any(q => q.id == questToAdd.id);
            
            if (!alreadyHasQuest)
            {
                questToAdd.questGiver = "메인퀘스터";
                AddQuest(questToAdd);
                currentMainQuestIndex = index;
                Debug.Log($"{logPrefix}: 퀘스트 '{questToAdd.id}' 진행 시작");
            }
            else
            {
                Debug.Log($"{logPrefix}: 퀘스트 '{questToAdd.id}'가 이미 진행 중입니다.");
            }
        }
        else
        {
            Debug.Log($"{logPrefix}: 이전 퀘스트가 완료되지 않아 다음 퀘스트를 진행할 수 없습니다.");
        }
    }

    public static string GetDistanceColor(float distance)
    {
        if (distance < 60) return "green";
        if (distance < 100) return "yellow";
        return "red";
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        switch(newState)
        {
            case GameSystemState.MainQuestPlay:
                GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
                MainQuestSequenceStart(currentMainQuestIndex);
                break;
            case GameSystemState.DialogueState:
                // Dialogue창이 닫히면 다음 퀘스트 진행
                MainQuestSequenceStart(currentMainQuestIndex);
                break;
        }
    }
}
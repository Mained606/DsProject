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
    [SerializeField] private List<Quest> completableQuests = new List<Quest>();

    public static List<Quest> QuestDatabase => Instance.questDatabase;
    public static List<Quest> CompletedQuests => Instance.completedQuests;
    public static Dictionary<string, Transform> QuestConditionPoint => Instance.questConditionPoint;
    public static List<Quest> CompletableQuests => Instance.completableQuests;

    public static Transform GetQuestConditionPoint(string point)
    {
        if (QuestConditionPoint.ContainsKey(point)) { return Instance.questConditionPoint[point]; }
        else { return null; }
    }

    public static NPCList NpcDatabase => Instance.npcDataList;

    private int currentMainQuestIndex = 0;
    public static int CurrentMainQuestIndex => Instance.currentMainQuestIndex;

    [SerializeField] private GameObject dragon;
    [SerializeField] private GameObject dragonEgg;
    private PlayerController player;

    // 퀘스트 상태 정의 (내부 사용 열거형)
    private enum QuestState
    {
        Give,       // 퀘스트 지급 상태
        Progress,   // 퀘스트 진행 중 상태
        Complete    // 퀘스트 완료 상태
    }

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
        
        // 서브 퀘스트 선행 조건 체크
        if (quest.questType == "서브퀘스트" && !string.IsNullOrEmpty(quest.prerequisiteQuestId))
        {
            bool prerequisiteCompleted = completedQuests.Exists(q => q.id == quest.prerequisiteQuestId);
            if (!prerequisiteCompleted)
            {
                Debug.Log($"{logPrefix}: 선행 퀘스트 '{quest.prerequisiteQuestId}'를 완료해야 합니다.");
                GameStateMachine.Instance.ChangeState(GameSystemState.InfoMessage, $"퀘스트 '{quest.id}'를 수락하려면 먼저 퀘스트 '{quest.prerequisiteQuestId}'를 완료해야 합니다.");
                return;
            }
            Debug.Log($"{logPrefix}: 선행 퀘스트 '{quest.prerequisiteQuestId}' 완료 확인됨, 퀘스트 추가 진행.");
        }
        
        // Meet, Explore 타입 퀘스트 조건에 대한 Transform 등록 시도
        foreach (var condition in quest.requiredConditions)
        {
            string conditionId = condition.Key;
            QuestCondition questCondition = condition.Value;
            
            // Meet 또는 Explore 조건이고 아직 등록되지 않은 경우
            if ((questCondition.type == QuestConditionType.Meet || 
                 questCondition.type == QuestConditionType.Explore) &&
                !questConditionPoint.ContainsKey(conditionId))
            {
                // 조건의 targetId를 기반으로 Transform 찾기 시도
                Transform targetTransform = FindTransformForCondition(questCondition);
                if (targetTransform != null)
                {
                    questConditionPoint[conditionId] = targetTransform;
                    Debug.Log($"[QuestManager] 퀘스트 '{quest.id}'의 조건 '{conditionId}'에 대한 Transform을 등록했습니다.");
                }
                else
                {
                    Debug.LogWarning($"[QuestManager] 퀘스트 '{quest.id}'의 조건 '{conditionId}'에 대한 Transform을 찾을 수 없습니다.");
                }
            }
            
            // 진행 상황 초기화
            if (!quest.progress.ContainsKey(conditionId))
            {
                quest.progress[conditionId] = 0;
            }
        }
        
        // 퀘스트 추가 및 UI 업데이트
        questDatabase.Add(quest);
        UIManager.Instance.QuestUpdate();
        Debug.Log($"{logPrefix}: 퀘스트가 성공적으로 추가되었습니다.");
    }

    private Transform FindTransformForCondition(QuestCondition condition)
    {
        // 조건에 맞는 Transform 찾기
        switch (condition.type)
        {
            case QuestConditionType.Meet:
                // 1. NPC Database에서 찾기
                NPCData npcData = npcDatabase.Find(n => n.id == condition.targetId);
                if (npcData != null && npcData.currentNPC != null)
                {
                    return npcData.currentNPC.transform;
                }
                
                // 2. Scene에서 이름으로 찾기
                GameObject npcObj = GameObject.Find(condition.targetId);
                if (npcObj != null) return npcObj.transform;
                
                // 3. Tag로 찾기
                GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
                foreach (var npc in npcs)
                {
                    if (npc.name.Contains(condition.targetId))
                    {
                        return npc.transform;
                    }
                }
                break;
                
            case QuestConditionType.Explore:
                // 1. 위치 마커로 찾기
                GameObject locationObj = GameObject.Find(condition.targetId);
                if (locationObj != null) return locationObj.transform;
                
                // 2. 위치 태그로 찾기
                GameObject[] locations = GameObject.FindGameObjectsWithTag("QuestLocation");
                foreach (var loc in locations)
                {
                    if (loc.name.Contains(condition.targetId))
                    {
                        return loc.transform;
                    }
                }
                break;
        }
        
        return null;
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
                    // 자동 완료 설정이 되어 있는 경우 즉시 완료 처리
                    if (quest.autoComplete)
                    {
                        CompleteQuest(quest);
                    }
                    // 완료 조건이 충족되면 isCompleted 대신 isCompletable 설정
                    else if (!quest.isCompletable)
                    {
                        quest.isCompletable = true;
                        if (!completableQuests.Contains(quest))
                        {
                            completableQuests.Add(quest);
                        }
                        UIManager.SystemGameMessage($"퀘스트 '{quest.name}' 완료 가능!", MessageTag.아이템_획득);
                        UIManager.Instance.QuestUpdate();
                    }
                }
                
                // Collect 타입 조건은 CheckQuestCondition에서 처리되었으므로 여기서는 처리하지 않음
                // 이후 다른 조건 타입(Explore, Kill, Meet 등)을 처리
                continue;
            }
            
            // Meet 타입의 경우 NPC 데이터 확인 및 Transform 등록
            if (conditionType == QuestConditionType.Meet && npcData != null)
            {
                foreach (var conditionKeyValue in quest.requiredConditions)
                {
                    var conditionId = conditionKeyValue.Key;
                    var condition = conditionKeyValue.Value;
                    
                    // NPC ID가 일치하는지 확인
                    if (condition.type == QuestConditionType.Meet && condition.targetId == targetId)
                    {
                        // QuestConditionPoint에 NPC 등록 (아직 없는 경우에만)
                        if (!questConditionPoint.ContainsKey(conditionId))
                        {
                            // Transform을 찾을 수 있는 로직 추가 필요
                            Transform npcTransform = FindNPCTransform(npcData.id);
                            if (npcTransform != null)
                            {
                                questConditionPoint[conditionId] = npcTransform;
                                Debug.Log($"[QuestManager] NPC '{npcData.name}' (ID: {npcData.id})의 Transform을 퀘스트 조건 '{conditionId}'에 등록했습니다.");
                            }
                        }
                        
                        // 진행 상황 업데이트
                        if (!quest.progress.ContainsKey(conditionId))
                        {
                            quest.progress[conditionId] = 0;
                        }
                        quest.progress[conditionId] += quantity;
                        
                        // 완료 조건 체크
                        if (quest.progress[conditionId] >= condition.requiredQuantity)
                        {
                            quest.progress[conditionId] = condition.requiredQuantity;
                            condition.isCompleted = true;
                            UIManager.SystemGameMessage($"퀘스트 조건 '{condition.targetName}' 완료!", MessageTag.아이템_획득);
                        }
                    }
                }
            }
            else
            {
                // 일반적인 다른 타입 처리 (Kill, Explore 등)
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
            }
            
            // 퀘스트 완료 여부 체크
            if (IsQuestCompleted(quest))
            {
                // 자동 완료 설정이 되어 있는 경우 즉시 완료 처리
                if (quest.autoComplete)
                {
                    CompleteQuest(quest);
                }
                // 완료 조건이 충족되면 isCompleted 대신 isCompletable 설정
                else if (!quest.isCompletable)
                {
                    quest.isCompletable = true;
                    if (!completableQuests.Contains(quest))
                    {
                        completableQuests.Add(quest);
                    }
                    UIManager.SystemGameMessage($"퀘스트 '{quest.name}' 완료 가능!", MessageTag.아이템_획득);
                    UIManager.Instance.QuestUpdate();
                }
            }
        }
    }
    public bool IsQuestCompleted(Quest quest)
    {
        foreach (var condition in quest.requiredConditions)
        {
            string conditionId = condition.Key;
            QuestCondition questCondition = condition.Value;

            // Collect 타입인 경우, 인벤토리의 실제 아이템 수량을 확인하여 검증
            if (questCondition.type == QuestConditionType.Collect)
            {
                int actualItemQuantity;
                
                // 강화 레벨 체크가 필요한 경우
                if (questCondition.requiredLevel > 0)
                {
                    actualItemQuantity = InventoryManager.Instance.GetItemQuantityWithLevel(questCondition.targetId, questCondition.requiredLevel);
                }
                else
                {
                    actualItemQuantity = InventoryManager.Instance.GetItemQuantity(questCondition.targetId);
                }
                
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
        
        // 완료 가능 퀘스트 목록에서도 제거
        if (completableQuests.Contains(quest))
        {
            completableQuests.Remove(quest);
        }
        
        // 퀘스트 지급자 및 관련 타겟을 콤파스에서 제거
        RemoveQuestTargetsFromCompass(quest);
        
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
        
        // 퀘스트 완료 후 자동으로 다음 퀘스트 시작
        if (quest.autoStartNextQuest && !string.IsNullOrEmpty(quest.nextQuestId))
        {
            Quest nextQuest = mainQuestDatabase.Find(q => q.id == quest.nextQuestId) ?? 
                             subQuestDatabase.Find(q => q.id == quest.nextQuestId);
            
            if (nextQuest != null && !questDatabase.Contains(nextQuest))
            {
                // 다음 퀘스트 추가 전, 관련 NPC 대화 데이터 사전 업데이트
                string prevQuestId = quest.id;
                string nextQuestId = nextQuest.id;
                
                // NPC 대화 상태 업데이트 - 특히 퀘스트 제공자와 관련 NPC
                if (!string.IsNullOrEmpty(nextQuest.targetID))
                {
                    NPCData targetNpcData = FindNpcDataById(nextQuest.targetID);
                    if (targetNpcData != null)
                    {
                        // 이전 퀘스트 대화 참조를 중지하고 다음 퀘스트로 전환
                        Debug.Log($"다음 퀘스트 '{nextQuestId}' 준비: NPC '{targetNpcData.id}'의 대화 데이터 업데이트");
                        
                        // 이전 퀘스트 대화 초기화 (필요한 경우)
                        if (targetNpcData.questDialogues != null && targetNpcData.questDialogues.ContainsKey(prevQuestId))
                        {
                            // 완료된 퀘스트의 대화 데이터 초기화 (방법 2)
                            QuestDialogueData prevDialogueData = targetNpcData.questDialogues[prevQuestId];
                            if (prevDialogueData != null && targetNpcData.dialogue != null && targetNpcData.dialogue.Length > 0)
                            {
                                prevDialogueData.giveDialogues = targetNpcData.dialogue;
                                prevDialogueData.progressDialogues = targetNpcData.dialogue;
                            }
                        }
                    }
                }
                
                // 퀘스트 추가
                AddQuest(nextQuest);
                UIManager.SystemGameMessage($"새로운 퀘스트 '{nextQuest.name}'가 시작되었습니다.", MessageTag.퀘스트);
            }
        }

        // 퀘스트 완료 처리
        quest.isCompleted = true;
        questDatabase.Remove(quest);
        completedQuests.Add(quest);
        
        // 퀘스트 완료 시 관련 NPC의 대화 상태 정리
        if (quest != null && !string.IsNullOrEmpty(quest.id))
        {
            // 퀘스트 제공자 NPC의 대화 상태 확인
            if (!string.IsNullOrEmpty(quest.targetID))
            {
                NPCData npcData = FindNpcDataById(quest.targetID);
                if (npcData != null && npcData.questDialogues != null && npcData.questDialogues.ContainsKey(quest.id))
                {
                    Debug.Log($"퀘스트 '{quest.id}' 완료: 타겟 NPC '{quest.targetID}'의 대화 상태 업데이트됨");
                }
            }
            
            // 관련된 모든 NPC의 대화 상태 확인
            foreach (NPCData npcData in npcDataList.npcLists)
            {
                if (npcData != null && npcData.questDialogues != null && npcData.questDialogues.ContainsKey(quest.id))
                {
                    // 다음 퀘스트 ID 확인 (연속된 퀘스트인 경우)
                    if (quest.id.Contains("_"))
                    {
                        string[] parts = quest.id.Split('_');
                        if (parts.Length >= 2)
                        {
                            string questPrefix = parts[0];
                            if (int.TryParse(parts[1], out int questNumber))
                            {
                                // 다음 퀘스트 ID 생성 (번호 하나 늘리기)
                                string nextQuestId = $"{questPrefix}_{questNumber + 1:D4}";
                                
                                // 해당 NPC에 다음 퀘스트 대화 데이터가 없으면, 이 퀘스트의 대화 데이터 유지
                                if (!npcData.questDialogues.ContainsKey(nextQuestId))
                                {
                                    Debug.Log($"퀘스트 '{quest.id}' 완료: NPC '{npcData.id}'에 다음 퀘스트 '{nextQuestId}' 대화 데이터가 없음");
                                    
                                    // 대화 중복 방지를 위한 처리
                                    // 주의: 대화를 기본 대화로 초기화하지 않고 완료 대화 상태 유지
                                    if (!npcData.relatedQuestIds.Contains(nextQuestId))
                                    {
                                        Debug.Log($"퀘스트 '{quest.id}' 완료: NPC '{npcData.id}'의 대화 데이터 상태 유지");
                                        
                                        // 현재 퀘스트에 대한 대화 데이터 처리
                                        if (npcData.questDialogues.ContainsKey(quest.id))
                                        {
                                            // 완료 대화 상태 유지 - 기본 대화로 초기화하지 않음
                                            QuestDialogueData dialogueData = npcData.questDialogues[quest.id];
                                            if (dialogueData != null)
                                            {
                                                // 완료 대화가 없는 경우에만 기본 대화 설정 고려
                                                if ((dialogueData.completeDialogues == null || dialogueData.completeDialogues.Length == 0) && 
                                                    npcData.dialogue != null && npcData.dialogue.Length > 0)
                                                {
                                                    dialogueData.completeDialogues = npcData.dialogue;
                                                    Debug.Log($"NPC '{npcData.id}'의 퀘스트 '{quest.id}' 완료 대화가 없어 기본 대화로 설정");
                                                }
                                                else
                                                {
                                                    Debug.Log($"NPC '{npcData.id}'의 퀘스트 '{quest.id}' 완료 대화 상태 유지");
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    Debug.Log($"퀘스트 '{quest.id}' 완료: NPC '{npcData.id}'에 다음 퀘스트 '{nextQuestId}' 대화 데이터 있음 - 상태 유지");
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    // 퀘스트와 관련된 모든 타겟을 콤파스에서 제거하는 메서드
    private void RemoveQuestTargetsFromCompass(Quest quest)
    {
        if (quest == null) return;
        
        // 1. 퀘스트 지급자 제거
        if (!string.IsNullOrEmpty(quest.questGiver))
        {
            Debug.Log($"[QuestManager] 퀘스트 '{quest.id}' 완료: 퀘스트 지급자 '{quest.questGiver}' 콤파스에서 제거 시도");
            
            Transform questGiverTransform = null;
            
            // 서브퀘스트 NPC 검색
            foreach (var npc in npcDataList.subQuestNpcLists)
            {
                if (npc.name == quest.questGiver)
                {
                    if (questConditionPoint.TryGetValue(npc.id, out questGiverTransform))
                    {
                        Debug.Log($"[QuestManager] 콤파스에서 서브퀘스트 NPC '{npc.name}' 제거");
                        CompassIndicater.RemoveTarget(questGiverTransform);
                    }
                    break;
                }
            }
            
            // 일반 NPC 검색
            if (questGiverTransform == null)
            {
                foreach (var npc in npcDataList.npcLists)
                {
                    if (npc.name == quest.questGiver)
                    {
                        if (questConditionPoint.TryGetValue(npc.id, out questGiverTransform))
                        {
                            Debug.Log($"[QuestManager] 콤파스에서 일반 NPC '{npc.name}' 제거");
                            CompassIndicater.RemoveTarget(questGiverTransform);
                        }
                        break;
                    }
                }
            }
            
            // Transform을 찾지 못한 경우 GameObject.Find 시도
            if (questGiverTransform == null)
            {
                // NPC 직접 찾기
                GameObject npcObj = GameObject.Find(quest.questGiver);
                if (npcObj != null)
                {
                    Debug.Log($"[QuestManager] GameObject.Find로 '{quest.questGiver}' 찾아 콤파스에서 제거");
                    CompassIndicater.RemoveTarget(npcObj.transform);
                }
                else
                {
                    // Tag로 찾기 시도
                    GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
                    foreach (var npc in npcs)
                    {
                        if (npc.name.Contains(quest.questGiver))
                        {
                            Debug.Log($"[QuestManager] Tag로 '{quest.questGiver}' 찾아 콤파스에서 제거");
                            CompassIndicater.RemoveTarget(npc.transform);
                            break;
                        }
                    }
                }
            }
        }
        
        // 2. 퀘스트 조건 관련 타겟 제거
        foreach (var condition in quest.requiredConditions)
        {
            string keyWord = condition.Key;
            Transform targetTransform = GetQuestConditionPoint(keyWord);
            if (targetTransform != null)
            {
                Debug.Log($"[QuestManager] 퀘스트 '{quest.id}' 완료: 조건 '{keyWord}' 콤파스에서 제거");
                CompassIndicater.RemoveTarget(targetTransform);
            }
        }
    }

    // 퀘스트 보상 처리 메서드 (스킬 언락 및 오브젝트 활성화)
    private void ProcessQuestRewards(string questId)
    {
        Debug.Log($"[QuestManager] 퀘스트 {questId} 보상 처리 중...");
        
        // 특정 퀘스트 ID에 따른 보상 처리
        if(questId == "1_1002")
        {
            ActivateDragonEgg();
        }

        if (questId == "1_1004")
        {
            // 1. 스킬 언락
            UnlockSkill("FireStrike");
            UnlockSkill("DownCut");

            // 2. 용 오브젝트 활성화
            ActivateDragon();
            
            ActivateGameObject("용", false);

            //player.unlockGlide = true;
        }

        if(questId == "1_2001")
        {
            // 1_2001 퀘스트 완료 후 SubNPC_1의 대화 상태 특별 처리
            NPCData subNpc1 = FindNpcDataById("SubNPC_1");
            if (subNpc1 != null && subNpc1.questDialogues != null)
            {
                Debug.Log("[QuestManager] 1_2001 퀘스트 완료 후 SubNPC_1의 대화 데이터 처리");
                
                // 1_2001 퀘스트 대화 초기화 대신 완료 상태 유지 
                if (subNpc1.questDialogues.ContainsKey("1_2001"))
                {
                    QuestDialogueData dialogData = subNpc1.questDialogues["1_2001"];
                    if (dialogData != null && dialogData.completeDialogues != null && dialogData.completeDialogues.Length > 0)
                    {
                        Debug.Log("[QuestManager] SubNPC_1의 1_2001 퀘스트 완료 대화 상태 유지");
                    }
                }
                
                // 1_2002 퀘스트 대화 설정 (이미 있다면 처리하지 않음)
                if (subNpc1.questDialogues.ContainsKey("1_2002"))
                {
                    Debug.Log("[QuestManager] SubNPC_1의 1_2002 퀘스트 대화 데이터 확인");
                    QuestDialogueData dialogData = subNpc1.questDialogues["1_2002"];
                    
                    // 1_2002 퀘스트 대화가 올바르게 설정되어 있는지 확인
                    if (dialogData != null)
                    {
                        // 필요한 경우 대화 데이터 추가 검증
                        if (dialogData.giveDialogues == null || dialogData.giveDialogues.Length == 0)
                        {
                            Debug.LogWarning("[QuestManager] SubNPC_1의 1_2002 퀘스트 Give 대화가 비어있습니다.");
                        }
                        
                        if (dialogData.progressDialogues == null || dialogData.progressDialogues.Length == 0)
                        {
                            Debug.LogWarning("[QuestManager] SubNPC_1의 1_2002 퀘스트 Progress 대화가 비어있습니다.");
                        }
                        
                        if (dialogData.completeDialogues == null || dialogData.completeDialogues.Length == 0)
                        {
                            Debug.LogWarning("[QuestManager] SubNPC_1의 1_2002 퀘스트 Complete 대화가 비어있습니다.");
                        }
                    }
                }
                
                // 관련 퀘스트 목록 관리
                if (subNpc1.relatedQuestIds != null)
                {
                    // 이미 1_2002가 relatedQuestIds에 있는지 확인
                    if (!subNpc1.relatedQuestIds.Contains("1_2002"))
                    {
                        // 필요시 1_2002 퀘스트 ID 추가 (일반적으로 자동으로 처리됨)
                        Debug.Log("[QuestManager] 1_2002 퀘스트 ID가 SubNPC_1의 관련 퀘스트 목록에 없습니다.");
                    }
                }
            }
        }

        if(questId == "1_2005")
        {
            //돌맹이 스킬
            UnlockSkill("TerraSurge");
            CharacterManager.DragonData.AddBondExperience(5);
        }

        if (questId == "1_2006")
        {
            //글라이딩 언락
            player.unlockGlide = true;
            CharacterManager.DragonData.AddBondExperience(5);
        }

        if (questId == "1_2007")
        {
            //올려치기
            UnlockSkill("UpperAttack");
            CharacterManager.DragonData.AddBondExperience(5);
        }

        if (questId == "1_2008")
        {
            //얼음 스킬
            UnlockSkill("GlacierSpear");
            CharacterManager.DragonData.AddBondExperience(5);
        }

        if (questId == "1_2009")
        {
            //전기 스킬, 드래곤 스킬
            UnlockSkill("ElectroComet");
            UnlockSkill("UltimateDragon");

            //드래곤 진화
            CharacterManager.DragonData.Evolve();
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

    private void ActivateDragonEgg()
    {
        if(dragonEgg != null)
        {
            dragonEgg.SetActive(true);
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

    private Transform FindNPCTransform(string npcId)
    {
        // NPC Database에서 해당 ID의 NPC 찾기
        NPCData npcData = npcDatabase.Find(n => n.id == npcId);
        
        // NPC 데이터가 있고 GameObject가 활성화되어 있으면 Transform 반환
        if (npcData != null && npcData.currentNPC != null)
        {
            return npcData.currentNPC.transform;
        }
        
        // Scene에서 NPC GameObject 찾기 (ID로 이름이 지정된 경우)
        GameObject npcObject = GameObject.Find(npcId);
        if (npcObject != null)
        {
            return npcObject.transform;
        }
        
        // 태그로 모든 NPC 찾기 및 일치하는지 확인
        GameObject[] npcObjects = GameObject.FindGameObjectsWithTag("NPC");
        foreach (var obj in npcObjects)
        {
            // 오브젝트의 이름이나 ID 비교 (스크립트 구현에 따라 수정 필요)
            if (obj.name.Contains(npcId))
            {
                return obj.transform;
            }
            
            // 직접적인 ID 비교 방법이 없기 때문에 간단히 이름으로 비교
        }
        
        Debug.LogWarning($"[QuestManager] NPC ID: {npcId}에 해당하는 Transform을 찾을 수 없습니다.");
        return null;
    }

    // NPC의 특정 퀘스트에 대한 대화를 가져오는 메서드
    public static string[] GetQuestDialogues(string npcId, string questId)
    {
        // NPC 데이터 찾기
        NPCData npcData = FindNpcDataById(npcId);
        if (npcData == null)
        {
            Debug.LogWarning($"[QuestManager] NPC ID: {npcId}를 찾을 수 없습니다.");
            return null;
        }
        
        // 퀘스트 대화 데이터 로깅 - 디버그 목적
        if (npcData.questDialogues != null)
        {
            Debug.Log($"[QuestManager] NPC {npcId}의 퀘스트 대화 데이터: {npcData.questDialogues.Count}개");
            foreach (var kvp in npcData.questDialogues)
            {
                bool hasGiveDialogue = kvp.Value.giveDialogues != null && kvp.Value.giveDialogues.Length > 0;
                bool hasProgressDialogue = kvp.Value.progressDialogues != null && kvp.Value.progressDialogues.Length > 0;
                bool hasCompleteDialogue = kvp.Value.completeDialogues != null && kvp.Value.completeDialogues.Length > 0;
                
                Debug.Log($"[QuestManager] 퀘스트 {kvp.Key}: Give({hasGiveDialogue}), Progress({hasProgressDialogue}), Complete({hasCompleteDialogue})");
            }
        }
        
        // 관련 퀘스트 ID 목록 가져오기
        List<string> relatedQuestIds = GetAllRelatedQuestIds(npcId);
        
        // 특정 NPC에 대한 강화된 검증 로직 
        if (npcId == "SubNPC_1" || npcId == "SubNPC_3" || npcId.Contains("대장장이") || npcId.Contains("요리사"))
        {
            Debug.Log($"[QuestManager] 특별 NPC({npcId})에 대한 상세 검증 수행");
            
            // 진행 중인 모든 퀘스트 검사
            foreach (var quest in Instance.questDatabase)
            {
                // 이 NPC와 관련이 있는지 확인
                bool isRelated = false;
                
                // 직접 targetID로 지정된 경우 (퀘스트 제공자)
                if (quest.targetID == npcId)
                {
                    isRelated = true;
                }
                
                // Meet 조건으로 사용된 경우
                if (!isRelated)
                {
                    foreach (var condition in quest.requiredConditions)
                    {
                        if (condition.Value.type == QuestConditionType.Meet && condition.Value.targetId == npcId)
                        {
                            isRelated = true;
                            break;
                        }
                    }
                }
                
                // 관련 있는 퀘스트이지만 relatedQuestIds에 없으면 추가
                if (isRelated && !relatedQuestIds.Contains(quest.id))
                {
                    Debug.LogWarning($"[QuestManager] 누락된 관련 퀘스트 발견: NPC {npcId}와 관련된 퀘스트 {quest.id}가 relatedQuestIds에 없어 추가함");
                    relatedQuestIds.Add(quest.id);
                    
                    // NPCData에도 추가 (영구적으로 저장되도록)
                    if (npcData.relatedQuestIds == null)
                    {
                        npcData.relatedQuestIds = new List<string>();
                    }
                    
                    if (!npcData.relatedQuestIds.Contains(quest.id))
                    {
                        npcData.relatedQuestIds.Add(quest.id);
                    }
                }
            }
        }
        
        // 모든 관련 퀘스트를 상태별로 분류
        List<Quest> activeQuestsWithProgressDialogue = new List<Quest>();  // Progress 대화가 있는 진행 중인 퀘스트
        List<Quest> activeQuestsWithoutProgressDialogue = new List<Quest>(); // Progress 대화가 없는 진행 중인 퀘스트
        List<Quest> completedQuests = new List<Quest>(); // 완료된 퀘스트
        List<Quest> pendingQuests = new List<Quest>();   // 대기 중인 퀘스트
        
        // 상태별 퀘스트 분류
        foreach (string relatedQuestId in relatedQuestIds)
        {
            // 진행 중인 퀘스트
            Quest quest = Instance.questDatabase.Find(q => q.id == relatedQuestId);
            if (quest != null)
            {
                // 퀘스트 대화 데이터 확인
                bool hasProgressDialogue = false;
                if (npcData.questDialogues != null && npcData.questDialogues.ContainsKey(quest.id))
                {
                    QuestDialogueData dialogueData = npcData.questDialogues[quest.id];
                    hasProgressDialogue = dialogueData.progressDialogues != null && dialogueData.progressDialogues.Length > 0;
                }
                
                // Progress 대화 유무에 따라 분류
                if (hasProgressDialogue)
                {
                    activeQuestsWithProgressDialogue.Add(quest);
                    Debug.Log($"[QuestManager] Progress 대화가 있는 진행 중 퀘스트: {quest.id}");
                }
                else
                {
                    activeQuestsWithoutProgressDialogue.Add(quest);
                    Debug.Log($"[QuestManager] Progress 대화가 없는 진행 중 퀘스트: {quest.id}");
                }
                continue;
            }
            
            // 완료된 퀘스트
            quest = Instance.completedQuests.Find(q => q.id == relatedQuestId);
            if (quest != null)
            {
                completedQuests.Add(quest);
                continue;
            }
            
            // 메인/서브 퀘스트 DB에서 아직 시작되지 않은 퀘스트 찾기
            quest = Instance.mainQuestDatabase.Find(q => q.id == relatedQuestId) ?? 
                    Instance.subQuestDatabase.Find(q => q.id == relatedQuestId);
            if (quest != null)
            {
                pendingQuests.Add(quest);
            }
        }
        
        // 상태에 따른 우선순위 디버그 로그
        Debug.Log($"[QuestManager] NPC {npcId} 관련 퀘스트: 진행 중(Progress 대화 있음: {activeQuestsWithProgressDialogue.Count}, 없음: {activeQuestsWithoutProgressDialogue.Count}), 완료됨({completedQuests.Count}), 대기 중({pendingQuests.Count})");
        
        // 퀘스트 ID가 제공되었으면 해당 퀘스트 처리
        if (!string.IsNullOrEmpty(questId))
        {
            // 특정 퀘스트 상태 확인
            QuestState questState = GetQuestState(questId);
            Debug.Log($"[QuestManager] NPC {npcId}에 대해 요청된 퀘스트 {questId} 상태: {questState}");
            
            // 퀘스트 대화 데이터 확인
            if (npcData.questDialogues != null && npcData.questDialogues.ContainsKey(questId))
            {
                QuestDialogueData dialogueData = npcData.questDialogues[questId];
                
                // 상태별 대화 반환 (null 또는 빈 배열이 아닌 경우에만)
                switch (questState)
                {
                    case QuestState.Give:
                        if (dialogueData.giveDialogues != null && dialogueData.giveDialogues.Length > 0)
                        {
                            // 중요: 특별 NPC의 경우 진행 중인 퀘스트(Progress 대화 있는)가 있으면 Give 대화 반환 방지
                            if ((npcId == "SubNPC_1" || npcId == "SubNPC_3" || npcId.Contains("대장장이") || npcId.Contains("요리사")) && 
                                activeQuestsWithProgressDialogue.Count > 0)
                            {
                                Debug.Log($"[QuestManager] 특별 NPC({npcId})의 경우 진행 중인 퀘스트({activeQuestsWithProgressDialogue[0].id})가 있어 Give 대화 대신 Progress 대화 우선 처리");
                                break; // 진행 중인 퀘스트 처리로 넘어감
                            }
                            
                            Debug.Log($"[QuestManager] 요청된 퀘스트 {questId}의 Give 대화 반환 (NPC {npcId})");
                            return dialogueData.giveDialogues;
                        }
                        break;
                        
                    case QuestState.Progress:
                        if (dialogueData.progressDialogues != null && dialogueData.progressDialogues.Length > 0)
                        {
                            Debug.Log($"[QuestManager] 요청된 퀘스트 {questId}의 Progress 대화 반환 (NPC {npcId})");
                            return dialogueData.progressDialogues;
                        }
                        break;
                        
                    case QuestState.Complete:
                        if (dialogueData.completeDialogues != null && dialogueData.completeDialogues.Length > 0)
                        {
                            Debug.Log($"[QuestManager] 요청된 퀘스트 {questId}의 Complete 대화 반환 (NPC {npcId})");
                            return dialogueData.completeDialogues;
                        }
                        break;
                }
            }
            
            // 특별 케이스: 1_2001 이장 퀘스트
            if (questId == "1_2001" && npcId == "SubNPC_1")
            {
                Debug.Log($"[QuestManager] 1_2001 이장 퀘스트에 대한 특별 처리 (NPC {npcId})");
                if (npcData.questDialogues != null && npcData.questDialogues.ContainsKey(questId))
                {
                    QuestDialogueData dialogueData = npcData.questDialogues[questId];
                    if (dialogueData.giveDialogues != null && dialogueData.giveDialogues.Length > 0)
                    {
                        return dialogueData.giveDialogues;
                    }
                }
            }
        }
        
        // *** 가장 중요: Progress 대화가 있는 진행 중인 퀘스트가 있으면 무조건 우선시 ***
        if (activeQuestsWithProgressDialogue.Count > 0)
        {
            Debug.Log($"[QuestManager] NPC {npcId}에 Progress 대화가 있는 진행 중 퀘스트가 있어 우선 처리");
            
            // 다중 퀘스트 상황에서 특정 NPC (대장장이, 요리사 등)에 대한 추가 로직
            if ((npcId == "SubNPC_1" || npcId == "SubNPC_3" || npcId.Contains("대장장이") || npcId.Contains("요리사")) && 
                activeQuestsWithProgressDialogue.Count > 1)
            {
                Debug.Log($"[QuestManager] 다중 퀘스트 NPC({npcId})에 대한 특별 처리: Progress 대화가 있는 진행 중 퀘스트 {activeQuestsWithProgressDialogue.Count}개");
                
                // 진행 중인 퀘스트들을 ID 기준 정렬 (최신 퀘스트가 우선)
                activeQuestsWithProgressDialogue.Sort((a, b) => string.Compare(b.id, a.id));
                
                // 가장 최신/우선순위 높은 퀘스트의 Progress 대화 반환
                foreach (Quest quest in activeQuestsWithProgressDialogue)
                {
                    if (npcData.questDialogues != null && npcData.questDialogues.ContainsKey(quest.id))
                    {
                        QuestDialogueData dialogueData = npcData.questDialogues[quest.id];
                        if (dialogueData.progressDialogues != null && dialogueData.progressDialogues.Length > 0)
                        {
                            Debug.Log($"[QuestManager] 다중 퀘스트 특별 NPC: 진행 중인 퀘스트 {quest.id}의 Progress 대화 반환 (NPC {npcId})");
                            return dialogueData.progressDialogues;
                        }
                    }
                }
            }
            else
            {
                // 일반적인 진행 중인 퀘스트 대화 처리
                foreach (Quest quest in activeQuestsWithProgressDialogue)
                {
                    if (npcData.questDialogues != null && npcData.questDialogues.ContainsKey(quest.id))
                    {
                        QuestDialogueData dialogueData = npcData.questDialogues[quest.id];
                        if (dialogueData.progressDialogues != null && dialogueData.progressDialogues.Length > 0)
                        {
                            Debug.Log($"[QuestManager] 진행 중인 퀘스트 {quest.id}의 Progress 대화 반환 (NPC {npcId})");
                            return dialogueData.progressDialogues;
                        }
                    }
                }
            }
        }
        
        // 완료된 퀘스트 대화 확인 (가장 최근에 완료된 퀘스트 우선)
        if (completedQuests.Count > 0)
        {
            Debug.Log($"[QuestManager] NPC {npcId}에 Progress 대화 있는 진행 중 퀘스트가 없고, 완료된 퀘스트가 있음");
            
            // 퀘스트 ID 기준으로 내림차순 정렬 (가장 최근 퀘스트가 앞으로)
            completedQuests.Sort((a, b) => string.Compare(b.id, a.id));
            
            foreach (Quest quest in completedQuests)
            {
                if (npcData.questDialogues != null && npcData.questDialogues.ContainsKey(quest.id))
                {
                    QuestDialogueData dialogueData = npcData.questDialogues[quest.id];
                    if (dialogueData.completeDialogues != null && dialogueData.completeDialogues.Length > 0)
                    {
                        Debug.Log($"[QuestManager] 완료된 퀘스트 {quest.id}의 Complete 대화 반환 (NPC {npcId})");
                        return dialogueData.completeDialogues;
                    }
                }
            }
        }
        
        // Progress 대화가 없는 진행 중인 퀘스트 처리 (특별 NPC의 경우 진행 중인 퀘스트가 있으면 대기 중인 퀘스트 대화 반환 방지)
        if ((npcId == "SubNPC_1" || npcId == "SubNPC_3" || npcId.Contains("대장장이") || npcId.Contains("요리사")) && 
            activeQuestsWithoutProgressDialogue.Count > 0)
        {
            Debug.Log($"[QuestManager] 특별 NPC({npcId})의 경우 Progress 대화는 없지만 진행 중인 퀘스트({activeQuestsWithoutProgressDialogue[0].id})가 있어 기본 대화 반환");
            return npcData.dialogue;
        }
        
        // 대기 중인 퀘스트 대화 확인 (다음 시작할 퀘스트 우선)
        // 중요: 진행 중인 퀘스트가 없을 때만 대기 중인 퀘스트의 Give 대화 고려
        if (activeQuestsWithProgressDialogue.Count == 0 && activeQuestsWithoutProgressDialogue.Count == 0 && pendingQuests.Count > 0)
        {
            Debug.Log($"[QuestManager] NPC {npcId}에 진행 중인 퀘스트 없고, 대기 중인 퀘스트 있음 - Give 대화 고려");
            
            // 펜딩 퀘스트를 ID 순서로 정렬 (낮은 번호가 우선)
            pendingQuests.Sort((a, b) => string.Compare(a.id, b.id));
            
            // 먼저 이전 퀘스트가 완료된 퀘스트를 찾음
            foreach (Quest pendingQuest in pendingQuests)
            {
                // 선행 퀘스트가 있는지 확인
                string prevQuestId = null;
                if (pendingQuest.id.Contains("_"))
                {
                    string[] parts = pendingQuest.id.Split('_');
                    if (parts.Length >= 2 && int.TryParse(parts[1], out int questNumber) && questNumber > 0)
                    {
                        prevQuestId = $"{parts[0]}_{questNumber - 1:D4}";
                    }
                }
                
                // 선행 퀘스트가 완료되었는지 확인
                bool prevQuestCompleted = string.IsNullOrEmpty(prevQuestId) || 
                                         Instance.completedQuests.Exists(q => q.id == prevQuestId);
                
                Debug.Log($"[QuestManager] 대기 중인 퀘스트 {pendingQuest.id}의 선행 퀘스트 {prevQuestId ?? "없음"} 완료 여부: {prevQuestCompleted}");
                
                // 선행 퀘스트가 완료되었거나 없다면 이 퀘스트의 Give 대화 반환
                if (prevQuestCompleted)
                {
                    if (npcData.questDialogues != null && npcData.questDialogues.ContainsKey(pendingQuest.id))
                    {
                        QuestDialogueData dialogueData = npcData.questDialogues[pendingQuest.id];
                        if (dialogueData.giveDialogues != null && dialogueData.giveDialogues.Length > 0)
                        {
                            Debug.Log($"[QuestManager] 대기 중인 퀘스트 {pendingQuest.id}의 Give 대화 반환 (NPC {npcId})");
                            return dialogueData.giveDialogues;
                        }
                    }
                }
                else
                {
                    Debug.Log($"[QuestManager] 대기 중인 퀘스트 {pendingQuest.id}의 선행 퀘스트 {prevQuestId}가 완료되지 않아 Give 대화 반환 불가");
                }
            }
        }
        
        // 마지막으로 기본 대화 반환
        Debug.Log($"[QuestManager] NPC {npcId}에 적절한 퀘스트 대화가 없어 기본 대화 사용");
        return npcData.dialogue;
    }
    
    // 퀘스트 상태 확인 (지급/진행중/완료)
    private static QuestState GetQuestState(string questId)
    {
        // 완료된 퀘스트 목록에서 확인
        if (Instance.completedQuests.Any(q => q.id == questId))
        {
            return QuestState.Complete;
        }
        
        // 현재 진행 중인 퀘스트 목록에서 확인
        if (Instance.questDatabase.Any(q => q.id == questId))
        {
            return QuestState.Progress;
        }
        
        // 위 조건에 해당하지 않으면 지급 상태로 간주
        return QuestState.Give;
    }
    
    // NPC ID로 NPCData 찾기
    public static NPCData FindNpcDataById(string npcId)
    {
        // 메인 퀘스트 NPC 목록에서 찾기
        NPCData npcData = NpcDatabase.mainQuestNpcLists.FirstOrDefault(npc => npc.id == npcId);
        
        // 서브 퀘스트 NPC 목록에서 찾기
        if (npcData == null)
        {
            npcData = NpcDatabase.subQuestNpcLists.FirstOrDefault(npc => npc.id == npcId);
        }
        
        // 상점 NPC 목록에서 찾기
        if (npcData == null)
        {
            npcData = NpcDatabase.shopNpcLists.FirstOrDefault(npc => npc.id == npcId);
        }
        
        // 일반 NPC 목록에서 찾기
        if (npcData == null)
        {
            npcData = NpcDatabase.npcLists.FirstOrDefault(npc => npc.id == npcId);
        }
        
        return npcData;
    }
    
    // NPC의 모든 관련 퀘스트 ID 목록을 가져오는 메서드 (새로 추가)
    public static List<string> GetAllRelatedQuestIds(string npcId)
    {
        NPCData npcData = FindNpcDataById(npcId);
        if (npcData == null)
        {
            Debug.LogWarning($"[QuestManager] NPC ID: {npcId}를 찾을 수 없습니다.");
            return new List<string>();
        }
        
        // 결과 목록 생성
        List<string> relatedQuestIds = new List<string>();
        
        // NPC가 직접 제공하는 퀘스트 추가
        if (npcData.quests != null)
        {
            foreach (var quest in npcData.quests)
            {
                if (quest != null && !string.IsNullOrEmpty(quest.id) && !relatedQuestIds.Contains(quest.id))
                {
                    relatedQuestIds.Add(quest.id);
                }
            }
        }
        
        // NPC가 조건 NPC로 사용되는 퀘스트 추가
        if (npcData.relatedQuestIds != null)
        {
            foreach (var questId in npcData.relatedQuestIds)
            {
                if (!string.IsNullOrEmpty(questId) && !relatedQuestIds.Contains(questId))
                {
                    relatedQuestIds.Add(questId);
                }
            }
        }
        
        // 퀘스트 데이터베이스에서 이 NPC가 타겟으로 지정된 퀘스트 찾기
        foreach (var quest in Instance.questDatabase)
        {
            if (quest.targetID == npcId && !relatedQuestIds.Contains(quest.id))
            {
                relatedQuestIds.Add(quest.id);
            }
            
            // 조건에서 이 NPC가 사용되는지 확인
            foreach (var condition in quest.requiredConditions)
            {
                if (condition.Value.type == QuestConditionType.Meet && 
                    condition.Value.targetId == npcId &&
                    !relatedQuestIds.Contains(quest.id))
                {
                    relatedQuestIds.Add(quest.id);
                    break;
                }
            }
        }
        
        return relatedQuestIds;
    }
    
    // 특정 NPC에 퀘스트 대화 데이터 추가하는 헬퍼 메서드
    public static void SetQuestDialogues(string npcId, string questId, string[] giveDialogues, string[] progressDialogues, string[] completeDialogues)
    {
        NPCData npcData = FindNpcDataById(npcId);
        if (npcData == null)
        {
            Debug.LogWarning($"[QuestManager] NPC ID: {npcId}를 찾을 수 없습니다.");
            return;
        }
        
        // 퀘스트 대화 데이터 설정
        QuestDialogueData dialogueData = new QuestDialogueData
        {
            giveDialogues = giveDialogues,
            progressDialogues = progressDialogues,
            completeDialogues = completeDialogues
        };
        
        // questDialogues가 null이면 새로 생성
        if (npcData.questDialogues == null)
        {
            npcData.questDialogues = new Dictionary<string, QuestDialogueData>();
        }
        
        // 기존 데이터가 있으면 업데이트, 없으면 추가
        npcData.questDialogues[questId] = dialogueData;
        
        // NPC가 이 퀘스트의 제공자가 아니고, 관련 퀘스트 목록에도 없다면 관련 퀘스트에 추가
        bool isQuestProvider = false;
        if (npcData.quests != null)
        {
            foreach (var quest in npcData.quests)
            {
                if (quest != null && quest.id == questId)
                {
                    isQuestProvider = true;
                    break;
                }
            }
        }
        bool isAlreadyRelated = npcData.relatedQuestIds != null && npcData.relatedQuestIds.Contains(questId);
        
        if (!isQuestProvider && !isAlreadyRelated)
        {
            // relatedQuestIds가 null이면 새로 생성
            if (npcData.relatedQuestIds == null)
            {
                npcData.relatedQuestIds = new List<string>();
            }
            
            npcData.relatedQuestIds.Add(questId);
            Debug.Log($"[QuestManager] NPC {npcId}에 관련 퀘스트 {questId} 추가됨");
        }
        
        // NPCList 저장
        NpcDatabase.SaveAsset();
        
        Debug.Log($"[QuestManager] NPC {npcId}에 퀘스트 {questId}에 대한 대화 데이터 설정 완료");
    }
}
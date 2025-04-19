using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class NonePlayerCharacter : MonoBehaviour
{
    [SerializeField] private NPCType npcType;
    [SerializeField] private int npcIndex;
    [SerializeField] private ItemType shopItemType;
    [SerializeField] private int shopIndex;
    [SerializeField] private NPCData currentNPCData = null;
    
    // [기능] 퀘스트 NPC 구분을 위한 필드 추가
    [Header("퀘스트 NPC 설정")]
    [SerializeField] private bool isMainQuestNpc = false;
    [SerializeField] private bool isSubQuestNpc = false;
    
    // [기능] 서브 퀘스트 직접 지정 기능을 위한 필드 추가
    [SerializeField] private string subQuestId = "";

    private CapsuleCollider[] capsuleCollider;
    private InterActText interActText = null;
    private Transform targetlook;
    private float offSetHeight = 0;
    private float interactionRadius = 3f;
    private float turnSpeed = 5f;
    private bool isPlayerInRange = false;
    private bool isInitNPC = false;

    public bool isNearest = false;

    private void Start()
    {
        capsuleCollider = GetComponents<CapsuleCollider>();
        interActText = GetComponentInChildren<InterActText>(true);
        if (interActText != null)
        {
            interActText.gameObject.SetActive(false);
        }
        else
        {
            Debug.LogWarning("interActText가 존재하지 않습니다.");
        }

        if (capsuleCollider.Length > 0)
        {
            capsuleCollider[0].radius = interactionRadius;
            if (capsuleCollider.Length > 1)
            {
                offSetHeight = !capsuleCollider[1].isTrigger ? capsuleCollider[1].bounds.max.y : 0;
            }
            else
            {
                Debug.LogWarning("두 번째 CapsuleCollider가 없습니다.");
            }
        }
        else
        {
            Debug.LogError("이 게임 오브젝트에는 CapsuleCollider가 없습니다.");
        }
        
        // 시작 시 NPC 데이터가 없으면 초기화
        if (currentNPCData == null)
        {
            //Debug.Log("Start에서 NPC 데이터 초기화 필요");
            isInitNPC = false;
            // 다음 Update에서 GetMainNpcData 호출됨
        }
    }


    private void Update()
    {
        // 초기화되지 않았거나 강제 리프레시가 필요한 경우
        if (!isInitNPC || (currentNPCData != null && currentNPCData.quests != null && currentNPCData.quests.Length > 0 && 
            currentNPCData.quests[0].isCompleted != QuestManager.CompletedQuests.Any(q => q.id == currentNPCData.quests[0].id)))
        {
            //Debug.Log("NPC 상태 업데이트 필요: " + (isInitNPC ? "퀘스트 상태 동기화 필요" : "초기화 필요"));
            GetMainNpcData();
        }

        if (isPlayerInRange)
        {
            SmoothLookAt(targetlook, turnSpeed);
            if (UIManager.Instance.IsUIWindowOpen())
            {
                if (interActText.gameObject.activeSelf) interActText.gameObject.SetActive(false);
            }
            else
            {
                if (!interActText.gameObject.activeSelf) 
                {
                    interActText.gameObject.SetActive(true);
                    // UI 텍스트가 활성화될 때 GetMainNpcData에서 설정한 값을 사용합니다.
                    // 이렇게 하면 퀘스트 상태에 따라 UI가 자동으로 업데이트됩니다.
                }
                
                if (InputManager.InputActions.actions["Interact"].triggered)
                {
                    InteractArrangementer.Instance.IsNearestNPC();
                    if (isNearest)
                    {
                        Interact();
                        isNearest = false;
                    }
                }
            }
        }
    }

    void SmoothLookAt(Transform targetlook, float rotationSpeed)
    {
        Vector3 targetPosition = new Vector3(targetlook.position.x, transform.position.y, targetlook.position.z);
        Vector3 direction = targetPosition - transform.position;
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }


    private void GetMainNpcData()
    {
        List<NPCData> npclist = QuestManager.NpcDatabase.npcLists;

        // [기능] 퀘스트 NPC 구분을 위한 분기 수정
        if (npcType == NPCType.상점)
        {
            npclist = QuestManager.NpcDatabase.shopNpcLists;
            isInitNPC = true;
            currentNPCData = npclist[shopIndex].Clone(false);
            currentNPCData.currentNPC = this.gameObject;
        }
        else if(npcType == NPCType.퀘스트)
        {
            // [기능] 메인 퀘스트 NPC 처리
            if (isMainQuestNpc)
            {
                npclist = QuestManager.NpcDatabase.mainQuestNpcLists;
                isInitNPC = true;
                currentNPCData = npclist[npcIndex].Clone(false);
                currentNPCData.currentNPC = this.gameObject;
                
                //Debug.Log($"메인 퀘스트 NPC 초기화: {currentNPCData.name}, CompletedQuests 수: {QuestManager.CompletedQuests.Count}");
            }
            // [기능] 서브 퀘스트 NPC 처리
            else if (isSubQuestNpc)
            {
                // 서브 퀘스트 NPC 리스트가 존재하는지 확인하고, 없으면 생성
                if (QuestManager.NpcDatabase.subQuestNpcLists == null)
                {
                    QuestManager.NpcDatabase.subQuestNpcLists = new List<NPCData>();
                }
                
                // 서브 퀘스트 NPC 리스트에서 먼저 같은 ID를 가진 NPC를 찾음
                NPCData existingNpcData = null;
                List<NPCData> subQuestNpcs = QuestManager.NpcDatabase.subQuestNpcLists;
                
                if (subQuestNpcs != null && subQuestNpcs.Count > 0)
                {
                    // 이미 같은 NPC 인덱스로 등록된 서브 퀘스트 NPC가 있는지 확인
                    existingNpcData = subQuestNpcs.Find(npc => npc.id == $"SubNPC_{npcIndex}");
                }
                
                if (existingNpcData != null)
                {
                    // 기존 서브 퀘스트 NPC 데이터 사용
                    isInitNPC = true;
                    currentNPCData = existingNpcData.Clone(false);
                    //Debug.Log($"기존 서브 퀘스트 NPC 데이터 사용: {currentNPCData.name}");
                }
                else
                {
                    // 일반 NPC 리스트에서 기본 데이터를 가져와 서브 퀘스트 NPC 데이터 생성
                    isInitNPC = true;
                    
                    // 일반 NPC 리스트에 있는 NPC를 기반으로 데이터 복제
                    if (npcIndex >= 0 && npcIndex < npclist.Count)
                    {
                        currentNPCData = npclist[npcIndex].Clone(false);
                        
                        // 서브 퀘스트 NPC 속성 업데이트
                        currentNPCData.id = $"SubNPC_{npcIndex}";
                        currentNPCData.npcType = NPCType.퀘스트;
                        currentNPCData.isInteractable = true;
                        
                        // [수정] 서브 퀘스트 ID가 지정된 경우 퀘스트 지급자로 설정
                        // 활성자는 항상 true로 설정하여 NPC가 두 역할을 동시에 수행할 수 있도록 함
                        currentNPCData.isQuestGiver = !string.IsNullOrEmpty(subQuestId);
                        currentNPCData.isQuestActivator = true;
                        
                        // 서브 퀘스트 NPC 리스트에 추가 (원본 데이터 보존)
                        NPCData originalData = currentNPCData.Clone(true);
                        originalData.currentNPC = null;
                        QuestManager.NpcDatabase.subQuestNpcLists.Add(originalData);
                        
                        //Debug.Log($"새 서브 퀘스트 NPC 생성 및 등록: {currentNPCData.name}, 지급자: {currentNPCData.isQuestGiver}, 활성자: {currentNPCData.isQuestActivator}");
                    }
                    else
                    {
                        Debug.LogError($"NPC 인덱스 {npcIndex}가 범위를 벗어났습니다.");
                        return;
                    }
                }
                
                // NPC 게임 오브젝트 연결
                currentNPCData.currentNPC = this.gameObject;
                
                // ID가 지정된 경우만 서브 퀘스트 할당
                if (!string.IsNullOrEmpty(subQuestId))
                {
                    // 서브 퀘스트 할당
                    Quest subQuest = QuestManager.Instance.GetQuestById(subQuestId);
                    if (subQuest != null)
                    {
                        // 퀘스트가 이미 존재하면 덮어쓰기, 아니면 새로 할당
                        if (currentNPCData.quests == null || currentNPCData.quests.Length == 0)
                        {
                            currentNPCData.quests = new Quest[] { subQuest };
                        }
                        else
                        {
                            // 이미 같은 퀘스트가 있는지 확인
                            bool hasQuest = false;
                            foreach (Quest q in currentNPCData.quests)
                            {
                                if (q.id == subQuest.id)
                                {
                                    hasQuest = true;
                                    break;
                                }
                            }
                            
                            // 같은 퀘스트가 없을 경우에만 추가
                            if (!hasQuest)
                            {
                                Quest[] newQuests = new Quest[currentNPCData.quests.Length + 1];
                                currentNPCData.quests.CopyTo(newQuests, 0);
                                newQuests[currentNPCData.quests.Length] = subQuest;
                                currentNPCData.quests = newQuests;
                            }
                        }
                        //Debug.Log($"서브 퀘스트 NPC 초기화: {currentNPCData.name}, 할당된 퀘스트: {subQuestId}");
                    }
                    else
                    {
                        Debug.LogWarning($"서브 퀘스트 ID {subQuestId}를 찾을 수 없습니다.");
                    }
                }
                else
                {
                    //Debug.Log($"서브 퀘스트 NPC {currentNPCData.name}이(가) 퀘스트 활성자로만 설정되었습니다.");
                }
            }
            else
            {
                // 호환성 유지: 타입은 퀘스트이지만 특정 퀘스트 타입이 지정되지 않은 경우
                // 기본적으로 메인 퀘스트 NPC로 처리 (기존 코드 동작 방식 유지)
                npclist = QuestManager.NpcDatabase.mainQuestNpcLists;
                isInitNPC = true;

                // npcIndex가 범위 내에 있는지 확인 (오류 방지)
                if (npcIndex >= 0 && npcIndex < npclist.Count)
                {
                    currentNPCData = npclist[npcIndex].Clone(false);
                    currentNPCData.currentNPC = this.gameObject;
                    
                    //Debug.Log($"기존 퀘스트 NPC 호환성 처리 (메인 퀘스트 NPC로 초기화): {currentNPCData.name}");
                    
                    // 이 NPC가 기존 메인 퀘스트 NPC임을 표시
                    isMainQuestNpc = true;
                }
                else
                {
                    // npcIndex가 범위를 벗어났을 경우 기본 NPC 리스트 사용
                    Debug.LogWarning($"퀘스트 NPC의 npcIndex({npcIndex})가 범위를 벗어났습니다. 일반 NPC로 초기화합니다.");
                    isInitNPC = true;
                    currentNPCData = QuestManager.NpcDatabase.npcLists[npcIndex].Clone(false);
                    currentNPCData.currentNPC = this.gameObject;
                }
            }
            
            // NPC 퀘스트 상태 동기화 - asset 파일의 isCompleted가 CompletedQuests 목록과 일치하도록 설정
            if (currentNPCData.quests != null && currentNPCData.quests.Length > 0)
            {
                SynchronizeQuestStatus(currentNPCData.quests);
            }
        }
        else
        {
            isInitNPC = true;
            currentNPCData = npclist[npcIndex].Clone(false);
            currentNPCData.currentNPC = this.gameObject;
        }

        UpdateNpcUI();
        Canvas.ForceUpdateCanvases();
    }
    
    // 퀘스트 완료 상태를 CompletedQuests와 동기화하는 메서드
    private void SynchronizeQuestStatus(Quest[] quests)
    {
        if (quests == null || quests.Length == 0) return;
        
        //Debug.Log($"퀘스트 목록 동기화: {quests.Length}개");
        foreach (Quest quest in quests)
        {
            // CompletedQuests에 있는지 확인
            bool isCompleted = QuestManager.CompletedQuests.Any(q => q.id == quest.id);
            
            // 상태가 다르면 업데이트
            if (quest.isCompleted != isCompleted)
            {
                //Debug.Log($"퀘스트 {quest.id} 상태 업데이트: {quest.isCompleted} -> {isCompleted}");
                quest.isCompleted = isCompleted;
            }
        }
    }
    
    // NPC 유형에 따라 UI 업데이트
    private void UpdateNpcUI()
    {
        switch (currentNPCData.npcType)
        {
            case NPCType.퀘스트:
                // 활성화된 퀘스트가 있는지 확인
                bool hasActiveQuest = false;
                
                if (currentNPCData.quests != null && currentNPCData.quests.Length > 0)
                {
                    foreach (Quest quest in currentNPCData.quests)
                    {
                        if (!quest.isCompleted)
                        {
                            hasActiveQuest = true;
                            break;
                        }
                    }
                }
                
                // UI 텍스트 설정
                if (hasActiveQuest)
                {
                    // //Debug.Log($"NPC {currentNPCData.name}의 UI를 '대화하기'로 설정");
                    interActText.InteractTextSetting("대화하기", 0, offSetHeight, true);
                }
                else
                {
                    // //Debug.Log($"NPC {currentNPCData.name}의 UI를 '정보 듣기'로 설정");
                    interActText.InteractTextSetting("대화하기", 0, offSetHeight, false);
                }
                break;
                
            case NPCType.상점:
                interActText.InteractTextSetting("상점열기", 0, offSetHeight);
                break;
                
            default:
                capsuleCollider[0].radius = interactionRadius;
                string msg = string.Empty;
                if (currentNPCData.dialogue != null)
                {
                    msg = string.Empty;
                    foreach (string str in currentNPCData.dialogue)
                    {
                        msg += $"{str} + \n";
                    }
                }
                else
                {
                    msg = currentNPCData.description;
                }
                interActText.InteractTextSetting(msg, 1, offSetHeight);
                break;
        }
    }


    private void Interact()
    {
        if (!currentNPCData.isInteractable)
        {
            //Debug.Log($"{currentNPCData.name}은(는) 상호작용이 불가능합니다.");
            return;
        }
        
        //Debug.Log($"NPC {currentNPCData.name}와 상호작용 시작, NPC 타입: {currentNPCData.npcType}");

        switch (currentNPCData.npcType)
        {
            case NPCType.상점:
                OpenShop();
                break;
            case NPCType.퀘스트:
                // 먼저 퀘스트 상태를 동기화
                SynchronizeQuestStatus(currentNPCData.quests);
                
                // [기능] 서브퀘스트 클리어 조건 체크 (Meet 타입)
                CheckQuestMeetCondition();
                
                // [수정] 퀘스트 지급자인 경우에만 퀘스트 다이얼로그 표시
                if (currentNPCData.isQuestGiver)
                {
                    // 현재 진행 가능한 퀘스트가 있는지 확인
                    bool hasActiveQuest = false;
                    Quest activeQuest = null;
                    
                    // 완료 가능한 퀘스트가 있는지 확인
                    bool hasCompletableQuest = false;
                    Quest completableQuest = null;
                    
                    if (currentNPCData.quests != null && currentNPCData.quests.Length > 0)
                    {
                        // 먼저 완료 가능한 퀘스트가 있는지 확인
                        foreach (Quest quest in currentNPCData.quests)
                        {
                            // 이 NPC가 제공한 퀘스트 중에서 완료 가능한 퀘스트를 찾음
                            if (quest.isCompletable && !quest.isCompleted && QuestManager.CompletableQuests.Contains(quest))
                            {
                                hasCompletableQuest = true;
                                completableQuest = quest;
                                //Debug.Log($"완료 가능한 퀘스트 발견: {quest.id}");
                                break;
                            }
                        }
                        
                        // 완료 가능한 퀘스트가 없다면 활성화된 퀘스트를 찾음
                        if (!hasCompletableQuest)
                        {
                            // 완료되지 않은 첫 번째 퀘스트 찾기
                            foreach (Quest quest in currentNPCData.quests)
                            {
                                if (!quest.isCompleted)
                                {
                                    hasActiveQuest = true;
                                    activeQuest = quest;
                                    //Debug.Log($"활성화된 퀘스트 발견: {quest.id}");
                                    break;
                                }
                            }
                        }
                    }
                    
                    if (hasCompletableQuest)
                    {
                        // 완료 가능한 퀘스트가 있으면 완료 다이얼로그 표시
                        //Debug.Log($"완료 가능한 퀘스트 다이얼로그 표시: {completableQuest.id}");
                        GameStateMachine.Instance.ChangeState(GameSystemState.DialogueState);
                        UIManager.Instance.DisplayCompletableQuestDialog(currentNPCData, completableQuest);
                    }
                    else if (hasActiveQuest)
                    {
                        // [추가] 서브 퀘스트도 DialogUI 통해 받을 수 있도록 구조 확장
                        bool isMainQuest = activeQuest.questType.Equals("메인퀘스트", System.StringComparison.OrdinalIgnoreCase);
                        
                        // 메인 퀘스트인 경우 또는 서브 퀘스트인 경우 모두 DialogUI를 표시
                        UIManager.Instance.OpenQuestDialogUI(currentNPCData, activeQuest, isMainQuest);
                    }
                    else
                    {
                        // 완료된 퀘스트만 있거나 퀘스트가 없는 경우
                        ShowDialogue();
                    }
                }
                else
                {
                    // 퀘스트 지급자가 아닌 경우 (활성화자나 일반 NPC) 일반 대화 표시
                    ShowDialogue();
                }
                break;
            default:
                ShowDialogue();
                break;
        }

        PlayInteractionEffect();
        PlayVoice();
    }
    
    // [기능] 서브퀘스트 클리어 조건으로 NPC 대화 적용 메서드
    private void CheckQuestMeetCondition()
    {
        if (string.IsNullOrEmpty(currentNPCData.id))
            return;
            
        // NPC ID를 사용하여 이 NPC와의 대화를 조건으로 하는 활성화된 퀘스트 찾기
        QuestManager.Instance.UpdateQuestProgress(QuestConditionType.Meet, currentNPCData.id);
        
        // 디버그 로그
        //Debug.Log($"NPC '{currentNPCData.name}'({currentNPCData.id})와의 Meet 조건 체크 완료");
    }

    private void HandleState()
    {
        switch (currentNPCData.currentState)
        {
            case NPCState.중립:  // 일반 NPC 대화.
                //Debug.Log($"{currentNPCData.name}은(는) 중립 상태입니다.");
                //ShowDialogue();
                break;
            case NPCState.동료:
                //FollowPlayer();
                break;
            case NPCState.적:
                //AttackPlayer();
                break;
        }
    }

    public void ChangeState(NPCState newState)
    {
        //Debug.Log($"{currentNPCData.name} 상태가 {currentNPCData.currentState}에서 {newState}(으)로 변경되었습니다.");
        currentNPCData.currentState = newState;
    }

    private void FollowPlayer()
    {
        //Debug.Log($"{currentNPCData.name}은(는) 동료로서 플레이어를 따라다닙니다.");
        // 스토리 진행하다 동료로 활동하는 로직
    }

    private void AttackPlayer()
    {
        //Debug.Log($"{currentNPCData.name}이(가) 적으로 변해 플레이어를 공격합니다!");
        // 스토리 진행하다 공격하는 방식의 로직
    }

    private void OpenShop()
    {
        GameStateMachine.Instance.ChangeState(GameSystemState.Shopping, currentNPCData);
    }

    private void GiveQuest()
    {
        // [수정] 퀘스트 지급자가 아닌 경우 퀘스트를 지급하지 않음
        if (!currentNPCData.isQuestGiver)
        {
            //Debug.Log($"{currentNPCData.name}은(는) 퀘스트 지급자가 아니라 활성화자입니다.");
            ShowDialogue();
            return;
        }
        
        if (currentNPCData.quests == null || currentNPCData.quests.Length == 0)
        {
            //Debug.Log("퀘스트가 없습니다.");
            return;
        }

        // 퀘스트 상태 동기화
        SynchronizeQuestStatus(currentNPCData.quests);
        
        Quest currentQuest = currentNPCData.quests[0];
        //Debug.Log($"GiveQuest 호출: 퀘스트 ID {currentQuest.id}, 완료 상태: {currentQuest.isCompleted}");

        // 이미 완료된 퀘스트인지 확인
        if (currentQuest.isCompleted)
        {
            // 이미 완료된 퀘스트인 경우, 일반 대화 다이얼로그를 표시합니다.
            //Debug.Log($"퀘스트가 완료되어 일반 대화창 표시");
            ShowDialogue();
        }
        else
        {
            // 아직 완료되지 않은 퀘스트는 퀘스트 다이얼로그 표시
            // 퀘스트 진행 업데이트는 DialogUI.HandleQuest에서 수락 후 처리됨
            //Debug.Log($"완료되지 않은 퀘스트, 퀘스트 대화창 표시: {currentQuest.id}");
            UIManager.Instance.DisplayQuestDialogWindow(currentNPCData.name, currentQuest);
        }
    }

    private void ShowDialogue()
    {
        // 게임 상태를 다이얼로그 상태로 먼저 전환
        GameStateMachine.Instance.ChangeState(GameSystemState.DialogueState);
        
        // 다이얼로그 창을 표시
        UIManager.Instance.DisplayDialogWindow(currentNPCData);
    }

    private void PlayInteractionEffect()
    {
        if (currentNPCData.interactionEffect != null)
        {
            Instantiate(currentNPCData.interactionEffect, transform.position, Quaternion.identity);
        }
    }

    private void PlayVoice()
    {
        if (currentNPCData.voiceLines != null && currentNPCData.voiceLines.Length > 0)
        {
            var clip = currentNPCData.voiceLines[Random.Range(0, currentNPCData.voiceLines.Length)];
            SoundManager.Instance.PlayClipAtPoint("QuestVoice", transform.position);
        }
    }

    private float GetHeight()
    {
        Collider collider = GetComponent<Collider>();

        if (collider != null)
        {
            float height = collider.bounds.size.y; // y축 크기가 높이
            return height;
        }
        return -1;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            targetlook = other.transform;
            InteractArrangementer.Instance.conversationable.Add(transform);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            targetlook = null;
            if (interActText.gameObject.activeSelf) interActText.gameObject.SetActive(false);
            UIManager.Instance.UIClose();
            InteractArrangementer.Instance.conversationable.Remove(transform);
        }
    }
}
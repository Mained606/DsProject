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

    private CapsuleCollider[] capsuleCollider;
    private InterActText interActText = null;
    private Transform targetlook;
    private float offSetHeight = 0;
    private float interactionRadius = 3f;
    private float turnSpeed = 5f;
    private bool isPlayerInRange = false;
    private bool isInitNPC = false;

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
            Debug.Log("Start에서 NPC 데이터 초기화 필요");
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
            Debug.Log("NPC 상태 업데이트 필요: " + (isInitNPC ? "퀘스트 상태 동기화 필요" : "초기화 필요"));
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
                    Interact();
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

        if (npcType == NPCType.상점)
        {
            npclist = QuestManager.NpcDatabase.shopNpcLists;
            isInitNPC = true;
            currentNPCData = npclist[shopIndex].Clone(false);
            currentNPCData.currentNPC = this.gameObject;
        }
        else if(npcType == NPCType.퀘스트)  //04.08 HJ 추가
        {
            npclist = QuestManager.NpcDatabase.mainQuestNpcLists;
            isInitNPC = true;
            currentNPCData = npclist[npcIndex].Clone(false);
            currentNPCData.currentNPC = this.gameObject;
            
            Debug.Log($"NPC 초기화: {currentNPCData.name}, CompletedQuests 수: {QuestManager.CompletedQuests.Count}");
            
            // NPC 퀘스트 상태 동기화 - asset 파일의 isCompleted가 CompletedQuests 목록과 일치하도록 설정
            SynchronizeQuestStatus(currentNPCData.quests);
        }
        else //04.08 HJ 추가
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
        
        Debug.Log($"퀘스트 목록 동기화: {quests.Length}개");
        foreach (Quest quest in quests)
        {
            // CompletedQuests에 있는지 확인
            bool isCompleted = QuestManager.CompletedQuests.Any(q => q.id == quest.id);
            
            // 상태가 다르면 업데이트
            if (quest.isCompleted != isCompleted)
            {
                Debug.Log($"퀘스트 {quest.id} 상태 업데이트: {quest.isCompleted} -> {isCompleted}");
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
                    // Debug.Log($"NPC {currentNPCData.name}의 UI를 '대화하기'로 설정");
                    interActText.InteractTextSetting("대화하기", 0, offSetHeight, true);
                }
                else
                {
                    // Debug.Log($"NPC {currentNPCData.name}의 UI를 '정보 듣기'로 설정");
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


    public void Interact()
    {
        if (!currentNPCData.isInteractable)
        {
            Debug.Log($"{currentNPCData.name}은(는) 상호작용이 불가능합니다.");
            return;
        }
        
        Debug.Log($"NPC {currentNPCData.name}와 상호작용 시작, NPC 타입: {currentNPCData.npcType}");
        
        switch (currentNPCData.npcType)
        {
            case NPCType.상점:
                OpenShop();
                break;
            case NPCType.퀘스트:
                // 먼저 퀘스트 상태를 동기화
                SynchronizeQuestStatus(currentNPCData.quests);
                
                // 현재 진행 가능한 퀘스트가 있는지 확인
                bool hasActiveQuest = false;
                Quest activeQuest = null;
                
                if (currentNPCData.quests != null && currentNPCData.quests.Length > 0)
                {
                    // 완료되지 않은 첫 번째 퀘스트 찾기
                    foreach (Quest quest in currentNPCData.quests)
                    {
                        if (!quest.isCompleted)
                        {
                            hasActiveQuest = true;
                            activeQuest = quest;
                            Debug.Log($"활성화된 퀘스트 발견: {quest.id}");
                            break;
                        }
                    }
                }
                
                // 게임 상태를 다이얼로그 상태로 전환
                Debug.Log($"게임 상태를 DialogueState로 변경");
                GameStateMachine.Instance.ChangeState(GameSystemState.DialogueState);
                
                if (hasActiveQuest && activeQuest != null)
                {
                    // 활성화된 퀘스트가 있으면 퀘스트 대화 표시
                    // 퀘스트 업데이트는 DialogUI.HandleQuest에서 수락 후 처리
                    Debug.Log($"활성화된 퀘스트가 있어 퀘스트 대화창 표시: {activeQuest.id}");
                    UIManager.Instance.DisplayQuestDialogWindow(currentNPCData.name, activeQuest);
                }
                else
                {
                    // 모든 퀘스트가 완료되었거나 퀘스트가 없으면 일반 대화 표시
                    Debug.Log($"모든 퀘스트가 완료되었거나 없어서 일반 대화창 표시");
                    ShowDialogue();
                }
                break;
            default:
                // 게임 상태를 다이얼로그 상태로 전환
                Debug.Log($"게임 상태를 DialogueState로 변경 (기본 대화)");
                GameStateMachine.Instance.ChangeState(GameSystemState.DialogueState);
                ShowDialogue();
                break;
        }

        PlayInteractionEffect();
        PlayVoice();
    }

    private void HandleState()
    {
        switch (currentNPCData.currentState)
        {
            case NPCState.중립:  // 일반 NPC 대화.
                Debug.Log($"{currentNPCData.name}은(는) 중립 상태입니다.");
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
        Debug.Log($"{currentNPCData.name} 상태가 {currentNPCData.currentState}에서 {newState}(으)로 변경되었습니다.");
        currentNPCData.currentState = newState;
    }

    private void FollowPlayer()
    {
        Debug.Log($"{currentNPCData.name}은(는) 동료로서 플레이어를 따라다닙니다.");
        // 스토리 진행하다 동료로 활동하는 로직
    }

    private void AttackPlayer()
    {
        Debug.Log($"{currentNPCData.name}이(가) 적으로 변해 플레이어를 공격합니다!");
        // 스토리 진행하다 공격하는 방식의 로직
    }

    private void OpenShop()
    {
        GameStateMachine.Instance.ChangeState(GameSystemState.Shopping, currentNPCData);
    }

    private void GiveQuest()
    {
        if (currentNPCData.quests == null || currentNPCData.quests.Length == 0)
        {
            Debug.Log("퀘스트가 없습니다.");
            return;
        }

        // 퀘스트 상태 동기화
        SynchronizeQuestStatus(currentNPCData.quests);
        
        Quest currentQuest = currentNPCData.quests[0];
        Debug.Log($"GiveQuest 호출: 퀘스트 ID {currentQuest.id}, 완료 상태: {currentQuest.isCompleted}");

        // 이미 완료된 퀘스트인지 확인
        if (currentQuest.isCompleted)
        {
            // 이미 완료된 퀘스트인 경우, 일반 대화 다이얼로그를 표시합니다.
            Debug.Log($"퀘스트가 완료되어 일반 대화창 표시");
            ShowDialogue();
        }
        else
        {
            // 아직 완료되지 않은 퀘스트는 퀘스트 다이얼로그 표시
            // 퀘스트 진행 업데이트는 DialogUI.HandleQuest에서 수락 후 처리됨
            Debug.Log($"완료되지 않은 퀘스트, 퀘스트 대화창 표시: {currentQuest.id}");
            UIManager.Instance.DisplayQuestDialogWindow(currentNPCData.name, currentQuest);
        }
    }

    private void ShowDialogue()
    {
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
        }
    }
}
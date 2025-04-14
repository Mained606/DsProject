using System.Collections.Generic;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

public enum DialogType
{
    StorySequenceDialog,
    MainQuestDialog,
    NormalDialog,
    SceneDialog,
}

public class DialogUI : MonoBehaviour
{
    private TextMeshProUGUI[] subDisplay;
    private Button acceptButton;
    
    // 순차적 다이얼로그 처리를 위한 변수
    private string[] currentDialogues;
    private int currentDialogueIndex = 0;
    private bool isDialogueInProgress = false;
    private NPCData currentNpcData = null;
    private Quest currentQuest = null;
    private DialogType currentDialogType = DialogType.NormalDialog;
    
    // 현재 실행 중인 애니메이션 코루틴 참조 추가
    private Coroutine currentAnimationCoroutine = null;

    public void Awake()
    {
        subDisplay = GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
        acceptButton = GetComponentInChildren<Button>(includeInactive: true);
    }
    
    private void OnDisable()
    {
        // 비활성화시 코루틴 정리
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
    }

    public void DisplayDialogText(List<string> textList, DialogType dType)
    {
        subDisplay[0].text = textList[0];
        UIManager.Instance.ToggleDialog();
        SetupDialog("스페이스", textList[1], ()=>HandleDialog(dType));
    }

    public void DisplayDialogWindow(NPCData nPCData)
    {
        if (subDisplay.Length < 2)
        {
            Debug.LogError("텍스트 박스가 없습니다.");
            return;
        }

        // NPC 정보와 다이얼로그 상태 초기화
        currentNpcData = nPCData;
        currentDialogueIndex = 0;
        currentQuest = null;
        isDialogueInProgress = true;
        currentDialogType = DialogType.NormalDialog;

        acceptButton.onClick.RemoveAllListeners();
        subDisplay[0].text = nPCData.name;

        // 퀘스트 완료 여부에 따라 다른 대화 내용 표시
        bool hasCompletedQuest = false;
        
        if (nPCData.quests != null && nPCData.quests.Length > 0)
        {
            Debug.Log($"NPC {nPCData.name} 퀘스트 확인: {nPCData.quests.Length}개, CompletedQuests: {QuestManager.CompletedQuests.Count}개");
            
            foreach (Quest quest in nPCData.quests)
            {
                Debug.Log($"퀘스트 ID: {quest.id}, 완료 여부(asset): {quest.isCompleted}");
                
                // 완료된 퀘스트 목록에서 ID로 정확히 일치하는 항목 확인
                bool isQuestCompleted = false;
                foreach (Quest completedQuest in QuestManager.CompletedQuests)
                {
                    Debug.Log($"완료된 퀘스트 ID: {completedQuest.id}");
                    if (completedQuest.id == quest.id)
                    {
                        isQuestCompleted = true;
                        break;
                    }
                }
                
                if (isQuestCompleted)
                {
                    Debug.Log($"퀘스트 {quest.id} 완료됨으로 확인됨!");
                    hasCompletedQuest = true;
                    break;
                }
            }
        }
        
        // 다이얼로그 설정
        if (hasCompletedQuest)
        {
            // 퀘스트를 완료한 NPC에게는 감사 메시지
            currentDialogues = new string[] { "이미 도와주셔서 정말 감사합니다. 다른 일이 있으면 다시 찾아뵙겠습니다." };
            Debug.Log($"NPC {nPCData.name}이(가) 완료 메시지 표시");
        }
        else if (nPCData.dialogue != null && nPCData.dialogue.Length > 0)
        {
            // NPC에 설정된 모든 대화 내용을 순차적으로 표시
            currentDialogues = nPCData.dialogue;
            Debug.Log($"NPC {nPCData.name}이(가) 대화 시작, 총 {currentDialogues.Length}개 대화");
        }
        else
        {
            // 기본 대화 메시지
            currentDialogues = new string[] { "안녕하세요!" };
            Debug.Log($"NPC {nPCData.name}이(가) 기본 인사말 표시");
        }

        // 첫 번째 다이얼로그 표시 시작
        DisplayNextDialogue();
    }

    public void DisplayQuestDialogWindow(string title, Quest quest)
    {
        if (subDisplay.Length < 2)
        {
            Debug.LogError("텍스트 박스가 없습니다.");
            return;
        }

        // 상태 초기화
        currentNpcData = null;
        currentDialogueIndex = 0;
        currentQuest = quest;
        isDialogueInProgress = true;
        currentDialogType = DialogType.MainQuestDialog;

        acceptButton.onClick.RemoveAllListeners();
        subDisplay[0].text = title;

        // NPC 정보 찾기
        NPCData npcData = null;
        foreach (var npc in QuestManager.NpcDatabase.mainQuestNpcLists)
        {
            if (npc.name == title)
            {
                npcData = npc;
                break;
            }
        }
        
        string npcId = npcData != null ? npcData.id : "";
        Debug.Log($"퀘스트 다이얼로그 표시: NPC ID = {npcId}, 이름 = {title}, 퀘스트 ID = {quest.id}");

        if (quest.isCompleted)
        {
            currentDialogues = new string[] { "감사합니다! 퀘스트 완료에 따른 보상을 지급하겠습니다." };
            DisplayNextDialogue(true, () => HandleQuest(quest, true, npcId));
        }
        else
        {
            bool isQuestInProgress = QuestManager.QuestDatabase.Contains(quest);
            bool canAccept = !isQuestInProgress && quest.acceptCount < 3;

            // 다이얼로그 설정
            if (canAccept && npcData != null && npcData.dialogue != null && npcData.dialogue.Length > 0)
            {
                // NPC의 대화 내용을 모두 사용
                currentDialogues = npcData.dialogue;
                Debug.Log($"NPC {title}의 모든 대화 사용: 총 {currentDialogues.Length}개");
            }
            else if (canAccept)
            {
                // 퀘스트 설명
                currentDialogues = new string[] { quest.description };
                Debug.Log($"퀘스트 {quest.id} 설명 사용: {quest.description}");
            }
            else
            {
                // 수락 불가능한 경우
                currentDialogues = new string[] { "퀘스트를 완료하고 다시 찾아주세요." };
                Debug.Log($"퀘스트 {quest.id} 수락 불가 메시지 사용");
            }

            DisplayNextDialogue(isLastDialogue: false, () => HandleQuest(quest, canAccept, npcId));
        }
    }

    // 완료 가능한 퀘스트 다이얼로그를 표시하는 메서드 추가
    public void DisplayCompletableQuestDialog(NPCData npcData, Quest quest)
    {
        if (subDisplay.Length < 2)
        {
            Debug.LogError("텍스트 박스가 없습니다.");
            return;
        }

        // 상태 초기화
        currentNpcData = npcData;
        currentDialogueIndex = 0;
        currentQuest = quest;
        isDialogueInProgress = true;
        currentDialogType = DialogType.MainQuestDialog;

        acceptButton.onClick.RemoveAllListeners();
        subDisplay[0].text = npcData.name;
        
        string npcId = npcData != null ? npcData.id : "";
        Debug.Log($"완료 가능한 퀘스트 다이얼로그 표시: NPC ID = {npcId}, 이름 = {npcData.name}, 퀘스트 ID = {quest.id}");

        // 완료 가능한 퀘스트에 대한 대화 메시지 설정
        currentDialogues = new string[] { 
            $"'{quest.name}' 퀘스트의 모든 조건을 충족하셨습니다! 보상을 받으시겠습니까?" 
        };
        
        // 완료 다이얼로그 표시 및 퀘스트 완료 처리
        DisplayNextDialogue(true, () => HandleCompletableQuest(quest, true, npcId));
    }

    // 다음 다이얼로그를 표시하는 메서드
    private void DisplayNextDialogue(bool isLastDialogue = false, UnityEngine.Events.UnityAction finalAction = null)
    {
        if (currentDialogueIndex < currentDialogues.Length)
        {
            string currentMessage = currentDialogues[currentDialogueIndex];
            bool isReallyLastDialogue = currentDialogueIndex == currentDialogues.Length - 1;
            
            // 마지막 대화이거나 명시적으로 마지막 대화로 지정된 경우
            if (isReallyLastDialogue || isLastDialogue)
            {
                string buttonText = GetFinalButtonText();
                UnityEngine.Events.UnityAction action = finalAction ?? (() => HandleDialog(currentDialogType));
                SetupDialog(buttonText, currentMessage, action);
            }
            else
            {
                // 아직 대화가 더 남은 경우
                SetupDialog("다음", currentMessage, () => {
                    currentDialogueIndex++;
                    DisplayNextDialogue(isLastDialogue, finalAction);
                });
            }
        }
        else
        {
            // 모든 대화가 끝난 경우 (예외 처리)
            Debug.LogWarning("모든 대화가 끝났습니다.");
            HandleDialog(currentDialogType);
        }
    }

    // 마지막 대화의 버튼 텍스트 결정
    private string GetFinalButtonText()
    {
        if (currentQuest != null)
        {
            return currentQuest.isCompleted ? "보상수령" : 
                   (!QuestManager.QuestDatabase.Contains(currentQuest) && currentQuest.acceptCount < 3) ? "수락" : "닫기";
        }
        else
        {
            return "닫기";
        }
    }

    private void SetupDialog(string buttonText, string message, UnityEngine.Events.UnityAction onClickAction)
    {
        // 버튼 텍스트 설정
        acceptButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
        
        // 이전 리스너 제거 후 새 리스너 추가
        acceptButton.onClick.RemoveAllListeners();
        acceptButton.onClick.AddListener(onClickAction);
        
        // 이전 애니메이션 코루틴이 있으면 중지
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        
        // 새 애니메이션 코루틴 시작
        currentAnimationCoroutine = StartCoroutine(AnimateText(message));
    }

    private IEnumerator AnimateText(string message, float delay = 0.05f)
    {
        // 애니메이션 중에는 버튼 비활성화
        acceptButton.gameObject.SetActive(false);
        
        // 텍스트 완전히 초기화
        subDisplay[1].text = "";
        
        // 한 글자씩 텍스트 표시
        foreach (char c in message)
        {
            // 250411 SH 추가 ====================
            if (InputManager.InputActions.actions["Attack"].IsPressed())    // 현재 대화 빠르게 넘기기
            {
                subDisplay[1].text = message;
                break;
            }
            // 250411 SH 추가 ====================
            subDisplay[1].text += c;
            yield return new WaitForSeconds(delay);
        }
        
        // 애니메이션 완료 후 약간 대기
        yield return new WaitForSeconds(0.5f);
        
        // 버튼 활성화 및 코루틴 참조 초기화
        acceptButton.gameObject.SetActive(true);
        currentAnimationCoroutine = null;
    }

    private void HandleQuest(Quest quest, bool state, string npcId = "")
    {
        // 코루틴 정리
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        
        if (state)
        {
            if (!quest.isCompleted)
            {
                Debug.Log($"퀘스트 '{quest.id}' 수락됨, npcId: {npcId}");
                QuestManager.Instance.AddQuest(quest);
                
                // 퀘스트를 수락한 경우, 이제 NPC와의 상호작용 진행 업데이트
                if (!string.IsNullOrEmpty(npcId))
                {
                    Debug.Log($"NPC '{npcId}'와의 상호작용 진행 업데이트");
                    QuestManager.Instance.UpdateQuestProgress(QuestConditionType.Meet, npcId, 1, null);
                }
            }
        }
        UIManager.Instance.ToggleDialog();
        GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
    }

    // 완료 가능한 퀘스트 처리 메서드 추가
    private void HandleCompletableQuest(Quest quest, bool state, string npcId = "")
    {
        // 코루틴 정리
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        
        if (state)
        {
            // 퀘스트 완료 처리
            Debug.Log($"퀘스트 '{quest.id}' 완료 처리 및 보상 지급");
            QuestManager.Instance.CompleteQuest(quest);
        }
        
        UIManager.Instance.ToggleDialog();
        GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
    }

    private void HandleDialog(DialogType dType)
    {
        // 코루틴 정리
        if (currentAnimationCoroutine != null)
        {
            StopCoroutine(currentAnimationCoroutine);
            currentAnimationCoroutine = null;
        }
        
        switch (dType)
        {
            case DialogType.NormalDialog:
                UIManager.Instance.ToggleDialog();
                GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
                break;
            case DialogType.SceneDialog:
                UIManager.Instance.ToggleDialog();
                GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
                break;
            case DialogType.MainQuestDialog:
                UIManager.Instance.ToggleDialog();
                GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
                break;
            case DialogType.StorySequenceDialog:
                UIManager.Instance.ToggleDialog();
                GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
                break;
        }
    }
}

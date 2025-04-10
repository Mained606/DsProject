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

    public void Awake()
    {
        subDisplay = GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
        acceptButton = GetComponentInChildren<Button>(includeInactive: true);
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
        
        string dialogMessage;
        
        if (hasCompletedQuest)
        {
            // 퀘스트를 완료한 NPC에게는 감사 메시지 표시
            dialogMessage = "이미 도와주셔서 정말 감사합니다. 다른 일이 있으면 다시 찾아뵙겠습니다.";
            Debug.Log($"NPC {nPCData.name}이(가) 완료 메시지 표시");
        }
        else if (nPCData.dialogue != null && nPCData.dialogue.Length > 0)
        {
            // NPC에 설정된 대화 내용이 있으면 사용
            dialogMessage = nPCData.dialogue[Random.Range(0, nPCData.dialogue.Length)];
            Debug.Log($"NPC {nPCData.name}이(가) 기본 대화 표시: {dialogMessage}");
        }
        else
        {
            // 기본 대화 메시지
            dialogMessage = "안녕하세요!";
            Debug.Log($"NPC {nPCData.name}이(가) 기본 인사말 표시");
        }

        SetupDialog(
            "닫기",
            dialogMessage,
            () => HandleDialog(DialogType.NormalDialog)
        );
    }

    public void DisplayQuestDialogWindow(string title, Quest quest)
    {
        if (subDisplay.Length < 2)
        {
            Debug.LogError("텍스트 박스가 없습니다.");
            return;
        }

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
            SetupDialog(
                "보상수령",
                "감사합니다! 퀘스트 완료에 따른 보상을 지급하겠습니다.",
                () => HandleQuest(quest, true, npcId)
            );
        }
        else
        {
            bool isQuestInProgress = QuestManager.QuestDatabase.Contains(quest);
            bool canAccept = !isQuestInProgress && quest.acceptCount < 3;

            // 대화 내용 결정
            string dialogMessage;
            
            // 미리 설정된 NPC 대화가 있고, 수락 가능한 퀘스트인 경우
            if (canAccept && npcData != null && npcData.dialogue != null && npcData.dialogue.Length > 0)
            {
                // NPC의 대화 내용을 우선적으로 사용
                dialogMessage = npcData.dialogue[Random.Range(0, npcData.dialogue.Length)];
                Debug.Log($"NPC {title}의 기본 대화 사용: {dialogMessage}");
            }
            else if (canAccept)
            {
                // 미리 설정된 대화가 없지만 수락 가능한 경우 퀘스트 설명 사용
                dialogMessage = quest.description;
                Debug.Log($"퀘스트 {quest.id} 설명 사용: {dialogMessage}");
            }
            else
            {
                // 수락 불가능한 경우 기본 메시지 사용
                dialogMessage = "퀘스트를 완료하고 다시 찾아주세요.";
                Debug.Log($"퀘스트 {quest.id} 수락 불가 메시지 사용");
            }

            SetupDialog(
                canAccept ? "수락" : "닫기",
                dialogMessage,
                () => HandleQuest(quest, canAccept, npcId)
            );
        }
    }

    private void SetupDialog(string buttonText, string message, UnityEngine.Events.UnityAction onClickAction)
    {
        acceptButton.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
        acceptButton.onClick.AddListener(onClickAction);
        StartCoroutine(AnimateText(message));
    }

    private IEnumerator AnimateText(string message, float delay = 0.05f)
    {
        acceptButton.gameObject.SetActive(false);
        subDisplay[1].text = "";
        foreach (char c in message)
        {
            string key = c.ToString().ToUpper();
            subDisplay[1].text += c;
            yield return new WaitForSeconds(delay);
        }
        yield return new WaitForSeconds(0.5f);
        acceptButton.gameObject.SetActive(true);
    }

    private void HandleQuest(Quest quest, bool state, string npcId = "")
    {
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
            else
            {
                // 퀘스트 완료 처리 진행
                // QuestManager.Instance.CompleteQuest(quest);
            }
        }
        UIManager.Instance.ToggleDialog();
        GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
    }

    private void HandleDialog(DialogType dType)
    {
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

using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;
using Unity.VisualScripting;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;


public class QuestUI : MonoBehaviour
{
    [SerializeField] private GameObject questPrefab;
    [SerializeField] private GameObject rewardItemViewPrefab;
    [SerializeField] private Transform questParent;
    [SerializeField] private Transform questInfo;
    [SerializeField] private Transform rewardItemView;
    private Button[] buttons;
    private TextMeshProUGUI[] commentText;
    private int currentButtonIndex = 0;
    private Quest preQuest;
    private Dictionary<string, List<Quest>> categorizedQuest = new Dictionary<string, List<Quest>>();
    private TextMeshProUGUI[] conditionDisplayText;


    private void Awake()
    {
        buttons = transform.GetComponentsInChildren<Button>();
        commentText = transform.GetComponentsInChildren<TextMeshProUGUI>();
        conditionDisplayText = commentText[7].transform.GetChild(0).GetComponentsInChildren<TextMeshProUGUI>(true);

        foreach (TextMeshProUGUI child in conditionDisplayText)
        {
            if (child != null) child.gameObject.SetActive(false);
        }

        questInfo.gameObject.SetActive(false);
        commentText[3].text = "현재 진행 챕터";
    }

    private void OnEnable()
    {
        AddButtonListeners();
        UpdateUI();
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
        questInfo.gameObject.SetActive(false);
    }

    private void CategorizeItems()
    {
        categorizedQuest.Clear();

        foreach (var quest in QuestManager.QuestDatabase)
        {
            string questtag = quest.questType.ToString();
            if (!categorizedQuest.ContainsKey(questtag))
            {
                categorizedQuest[questtag] = new List<Quest>();
            }
            categorizedQuest[questtag].Add(quest);
        }
    }

    public void UpdateUI()
    {
        CategorizeItems();
        ClearUI();

        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogWarning("버튼이 설정되지 않았거나 버튼 배열이 비어있습니다.");
            return;
        }
        if (!System.Enum.IsDefined(typeof(CategotyQuestType), currentButtonIndex))
        {
            Debug.LogError($"유효하지 않은 카테고리 인덱스: {currentButtonIndex}");
            return;
        }

        string selectedTag = ((CategotyQuestType)currentButtonIndex).ToString();

        if (selectedTag == CategotyQuestType.전체퀘스트.ToString())
        {
            foreach (var category in categorizedQuest.Values)
            {
                foreach (var quest in category)
                {
                    CreateItemUI(quest);
                }
            }
        }
        else if (selectedTag == CategotyQuestType.완료퀘스트.ToString())
        {
            foreach (var completedQuest in QuestManager.CompletedQuests)
            {
                CreateItemUI(completedQuest);
            }
        }
        else if (categorizedQuest.ContainsKey(selectedTag))
        {
            foreach (var quest in categorizedQuest[selectedTag])
            {
                CreateItemUI(quest);
            }
        }
        else
        {
            Debug.LogWarning($"선택된 태그 '{selectedTag}'에 해당하는 퀘스트가 없습니다.");
        }
    }

    private void ClearUI()
    {
        foreach (Transform child in questParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateItemUI(Quest quest)
    {
        var questItem = Instantiate(questPrefab, questParent);
        
        // 완료 가능한 퀘스트에 특별한 텍스트 추가
        string questTitle = quest.ToStringTMProforList();
        if (quest.isCompletable)
        {
            questTitle = $"<color=green>[완료 가능]</color> {questTitle}";
        }
        
        questItem.GetComponentsInChildren<TextMeshProUGUI>()[0].text = questTitle;
        questItem.GetComponentsInChildren<TextMeshProUGUI>()[1].text = $"의뢰인 : {quest.questGiver}";
        questItem.GetComponentsInChildren<TextMeshProUGUI>()[2].text = $"{quest.progress.Count} / {quest.requiredConditions.Count}";
        Button button = questItem.GetComponent<Button>();
        button.onClick.AddListener(() => OuestTitleClick(quest));
    }

    public void AddButtonListeners()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    public void RemoveButtonListeners()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.RemoveAllListeners();
        }
    }
    
    private void OnButtonClick(int buttonIndex)
    {
        currentButtonIndex = buttonIndex;
        Debug.LogWarning("클릭번호 : " + currentButtonIndex);
        if (currentButtonIndex < 3)
        {
            if (questInfo.gameObject.activeSelf) questInfo.gameObject.SetActive(false);
            UpdateUI();
        }
        else if (currentButtonIndex == 3)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
        }
    }

    private void OuestTitleClick(Quest quest)
    {
        if (preQuest == quest)
        {
            questInfo.gameObject.SetActive(!questInfo.gameObject.activeSelf);
            return;
        }
        else
        {
            preQuest = quest;
            if (!questInfo.gameObject.activeSelf) questInfo.gameObject.SetActive(true);
        }

        commentText[4].text = quest.name;
        string giver = quest.questType == "메인퀘스트" ? "메인퀘스트" : quest.targetID.ToString();
        commentText[5].text = $"의뢰인 : {giver}";
        commentText[6].text = quest.description;

        // 퀘스트가 완료 가능하면 추가 정보 표시
        if (quest.isCompletable)
        {
            commentText[4].text = $"<color=green>[완료 가능]</color> {quest.name}";
            commentText[6].text = $"{quest.description}\n\n<color=green>퀘스트 조건을 모두 충족했습니다. 퀘스트 제공자에게 돌아가 보상을 받으세요.</color>";
        }

        // 기존 조건 텍스트 비활성화
        foreach (TextMeshProUGUI condText in conditionDisplayText)
        {
            if (condText != null) condText.gameObject.SetActive(false);
        }

        int conditionCount = 0;
        foreach (var condition in quest.requiredConditions)
        {
            if (conditionCount >= conditionDisplayText.Length)
            {
                Debug.LogWarning("conditionDisplayText 길이를 초과하는 조건이 있습니다.");
                break;
            }

            conditionDisplayText[conditionCount].gameObject.SetActive(true);
            string keyWord = condition.Key;
            string status = condition.Value.isCompleted ? "<color=red>완료</color>" : "<color=green>진행</color>";
            float distance = 0;

            switch (condition.Value.type)
            {
                case QuestConditionType.Collect:
                    // 진행 상황이 있는지 확인
                    if (!quest.progress.ContainsKey(keyWord))
                    {
                        quest.progress[keyWord] = 0;
                    }
                    conditionDisplayText[conditionCount].text = $" {status}   {condition.Value.targetName}를 <color=green>{condition.Value.requiredQuantity}</color>개 모으기   ({quest.progress[keyWord]} / {condition.Value.requiredQuantity})";
                    break;
                case QuestConditionType.Meet:
                    // Transform이 없는 경우 처리
                    Transform meetTransform = QuestManager.GetQuestConditionPoint(keyWord);
                    if (meetTransform != null)
                    {
                        distance = Vector3.Distance(GameManager.playerTransform.position, meetTransform.position);
                        conditionDisplayText[conditionCount].text = $" {status}   {condition.Value.targetName}를 찾아가기   {distance:F1}m";
                    }
                    else
                    {
                        conditionDisplayText[conditionCount].text = $" {status}   {condition.Value.targetName}를 찾아가기";
                    }
                    break;
                case QuestConditionType.Kill:
                    // 진행 상황이 없는 경우 초기화
                    if (!quest.progress.ContainsKey(keyWord))
                    {
                        quest.progress[keyWord] = 0;
                    }
                    conditionDisplayText[conditionCount].text = $" {status}   {condition.Value.targetName}를 <color=green>{condition.Value.requiredQuantity}</color> 마리 처치하세요   ({quest.progress[keyWord]} / {condition.Value.requiredQuantity})";
                    
                    // Transform이 있는 경우에만 거리 표시
                    Transform killTransform = QuestManager.GetQuestConditionPoint(keyWord);
                    if (killTransform != null)
                    {
                        float killDistance = Vector3.Distance(GameManager.playerTransform.position, killTransform.position);
                        string killColor = QuestManager.GetDistanceColor(killDistance);
                        conditionDisplayText[conditionCount].text += $"  <color={killColor}>{killDistance:F1}m</color>";
                    }
                    break;
                case QuestConditionType.Explore:
                    Transform exploreTransform = QuestManager.GetQuestConditionPoint(keyWord);
                    if (exploreTransform != null)
                    {
                        distance = Vector3.Distance(GameManager.playerTransform.position, exploreTransform.position);
                        string color = QuestManager.GetDistanceColor(distance);
                        string distanceColor = $" <color={color}>{distance:F1}m</color>";
                        conditionDisplayText[conditionCount].text = $" {status}   {condition.Value.targetName}를 찾아가기     {distanceColor}";
                    }
                    else
                    {
                        conditionDisplayText[conditionCount].text = $" {status}   {condition.Value.targetName}를 찾아가기";
                    }
                    break;
            }
            conditionCount++;
        }

        Vector2Int totalRewardGoldExp = Vector2Int.zero;
        foreach (Transform tr in rewardItemView)
        {
            if (tr != null) { Destroy(tr.gameObject); }
        }

        foreach (var reward in quest.rewards)
        {
            if (reward.experience > 0) totalRewardGoldExp.y += reward.experience;
            if (reward.gold > 0) totalRewardGoldExp.x += reward.gold;

            Item rewardItem = ItemManager.Instance.GetItemById(reward.itemId);
            if (rewardItem == null)
            {
                Debug.LogWarning($"보상 아이템 '{reward.itemId}'을(를) 찾을 수 없습니다.");
                continue;
            }

            string amount = rewardItem.isStackable ? rewardItem.questId.ToString() : "";
            CreatQuestRewardItemInfo(2, rewardItem.sprite, amount);
        }

        if (totalRewardGoldExp.x > 0)
        {
            CreatQuestRewardItemInfo(0, ItemManager.Instance.GetItemSprite("금화"), totalRewardGoldExp.x.ToString("N0"));
        }
        if (totalRewardGoldExp.y > 0)
        {
            CreatQuestRewardItemInfo(1, ItemManager.Instance.GetItemSprite("경험치"), totalRewardGoldExp.y.ToString("N0"));
        }

        Canvas.ForceUpdateCanvases();
    }

    private void CreatQuestRewardItemInfo(int index, Sprite sprite, string msg = "")
    {
        GameObject rewardGob = Instantiate(rewardItemViewPrefab, rewardItemView);
        rewardGob.transform.SetSiblingIndex(index);
        Image rewardImage = rewardGob.GetComponent<Image>();
        TextMeshProUGUI rewardItext = rewardGob.transform.GetComponentInChildren<TextMeshProUGUI>();
        rewardImage.sprite = sprite;
        rewardItext.text = msg;
        if ( msg == "" ) rewardItext.gameObject.SetActive(false);
    }
}

public enum CategotyQuestType
{
    메인퀘스트,   // 메인스토리 퀘스트
    서브퀘스트,   // 서브 퀘스트
    완료퀘스트,   // 완료된 퀘스트
    전체퀘스트
}
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestUI : MonoBehaviour
{
    [SerializeField] private GameObject basePrefab;  // 퀘스트 슬롯 프리팹
    [SerializeField] private Transform baseParent;  // 슬롯을 담을 부모
    private Button[] buttons;

    private List<QuestTable> questList;  // 전체 퀘스트 리스트
    private Dictionary<QuestType, List<QuestTable>> categorizedQuests = new Dictionary<QuestType, List<QuestTable>>();
    private int currentButtonIndex = 0;  // 현재 선택된 버튼의 인덱스

    [SerializeField] private TestQuestList testQuestList;  // 퀘스트 데이터를 가져올 소스

    private void Awake()
    {
        buttons = transform.GetComponentsInChildren<Button>();
    }

    private void OnEnable()
    {
        AddButtonListeners();
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
    }

    private void Start()
    {
        GetQuestData();
    }

    private void GetQuestData()
    {
        // TestQuestList에서 데이터 가져오기
        if (testQuestList != null)
        {
            questList = testQuestList.GetQuestList();  // 데이터 가져오기
            Debug.Log($"퀘스트 리스트 연결확인, {questList.Count}개의 퀘스트 소지 중.");
        }
        else
        {
            Debug.LogError("TestQuestList 연결 안 됨.");
        }
    }

    private void CategorizeQuests()
    {
        categorizedQuests.Clear();

        foreach (var quest in questList)
        {
            QuestType category = GetQuestType(quest);
            if (!categorizedQuests.ContainsKey(category))
            {
                categorizedQuests[category] = new List<QuestTable>();
            }
            categorizedQuests[category].Add(quest);
        }
    }

    private QuestType GetQuestType(QuestTable quest)
    {
        if (quest.quest_chapter == 1)
            return QuestType.Main;
        else if (quest.quest_chapter == 2)
            return QuestType.Sub;
        else
            return QuestType.History;
    }

    private void OnButtonClick(int buttonIndex)
    {
        currentButtonIndex = buttonIndex;
        UpdateUI();
        Debug.Log($"버튼 클릭: {buttonIndex}");
    }

    public void UpdateUI()
    {
        CategorizeQuests();
        ClearUI();
        Debug.Log("ClearUI 완료");

        // 기본 검증
        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogWarning("버튼이 설정되지 않았거나 버튼 배열이 비어있습니다.");
            return;
        }

        if (!System.Enum.IsDefined(typeof(QuestType), currentButtonIndex))
        {
            Debug.LogError($"유효하지 않은 카테고리 인덱스: {currentButtonIndex}");
            return;
        }

        QuestType selectedCategory = (QuestType)currentButtonIndex;

        Debug.Log($"선택된 카테고리 '{selectedCategory}'에 포함된 퀘스트 개수: {categorizedQuests[selectedCategory].Count}");
        foreach (var quest in categorizedQuests[selectedCategory])
        {
            CreateQuestUI(quest);
        }

        Debug.Log("UpdateUI 완료");
    }

    private void CreateQuestUI(QuestTable quest)
    {
        var questUI = Instantiate(basePrefab, baseParent);
        var texts = questUI.GetComponentsInChildren<TextMeshProUGUI>();

        texts[0].text = $"퀘스트 이름: {GetQuestName(quest.quest_stringIdx)}";  // 퀘스트 이름
        texts[1].text = $"퀘스트 설명: {GetQuestDescription(quest.quest_stringIdx)}";  // 퀘스트 설명
    }

    private string GetQuestName(int stringIdx)
    {
        return stringIdx switch
        {
            1001 => "마을 방어",
            1002 => "숲 속 탐험",
            1003 => "고대 유물",
            _ => "알 수 없는 퀘스트"
        };
    }

    private string GetQuestDescription(int stringIdx)
    {
        return stringIdx switch
        {
            1001 => "마을을 습격하는 몬스터를 처치하세요.",
            1002 => "숲에서 희귀한 약초를 찾아오세요.",
            1003 => "고대 유적에서 유물을 찾아오세요.",
            _ => "설명이 없습니다."
        };
    }

    private void ClearUI()
    {
        foreach (Transform child in baseParent)
        {
            Destroy(child.gameObject);
        }
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
        foreach (var button in buttons)
        {
            button.onClick.RemoveAllListeners();
        }
    }
}

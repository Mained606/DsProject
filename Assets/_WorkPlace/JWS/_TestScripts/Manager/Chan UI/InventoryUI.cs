using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemsParent;

    #region 툴팁 관련
    [SerializeField] private GameObject tooltip;
    [SerializeField] private Transform tooltipPanel;
    #endregion

    private Button[] buttons;
    private int currentButtonIndex = 0;
    private Dictionary<string, List<Item>> categorizedItems = new Dictionary<string, List<Item>>();

   

    private void Awake()
    {
        buttons = transform.GetComponentsInChildren<Button>();
        if (buttons != null && buttons.Length > 0)
        {
            Debug.Log($"버튼이 {buttons.Length}개 연결되었습니다.");
        }
        else
        {
            Debug.LogWarning("버튼이 연결되지 않았거나 배열이 비어있습니다.");
        }
        #region 툴팁 관련 초기화 추가
       
        #endregion
    }

  

    private void OnEnable()
    {
        AddButtonListeners();
        UpdateUI();
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
    }

    private void CategorizeItems()
    {
        categorizedItems.Clear();

        foreach (var item in InventoryManager.InventoryList)
        {
            string itemtag = item.type.ToString();
            if (!categorizedItems.ContainsKey(itemtag))
            {
                categorizedItems[itemtag] = new List<Item>();
            }
            categorizedItems[itemtag].Add(item);
        }
    }

    public void UpdateUI()
    {
        CategorizeItems();
        ClearUI();
        // 기본 검증
        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogWarning("버튼이 설정되지 않았거나 버튼 배열이 비어있습니다.");
            return;
        }
        if (InventoryManager.InventoryList.Count == 0)
        {
            Debug.LogWarning("인벤토리가 비어있습니다.");
            return;
        }
        if (!System.Enum.IsDefined(typeof(CategotyItemType), currentButtonIndex))
        {
            Debug.LogError($"유효하지 않은 카테고리 인덱스: {currentButtonIndex}");
            return;
        }

        string selectedTag = ((CategotyItemType)currentButtonIndex).ToString();
        if (selectedTag == CategotyItemType.전체아이템.ToString())
        {
            foreach (var category in categorizedItems.Values)
            {
                foreach (var item in category)
                {
                    CreateItemUI(item);
                }
            }
        }
        else if (categorizedItems.ContainsKey(selectedTag))
        {
            foreach (var item in categorizedItems[selectedTag])
            {
                CreateItemUI(item);
            }
        }
    }

    private void ClearUI()
    {
        foreach (Transform child in itemsParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void CreateItemUI(Item item)
    {
        var inventoryItem = Instantiate(itemPrefab, itemsParent);
        // -> 툴팁으로 따로 빼려고 임시로 주석 처리. 슬롯 자체에서 전부 보여줄거면 살리기
         inventoryItem.GetComponentInChildren<TextMeshProUGUI>().text = item.ToStringTMPro();
        if (item.sprite != null) inventoryItem.GetComponentsInChildren<Image>()[1].sprite = item.sprite;
        Debug.Log(item.ToStringTMPro());

        #region 툴팁 관련 추가부분
       
        #endregion

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

    private void OnButtonClick(int buttonIndex)
    {
        currentButtonIndex = buttonIndex;
        UpdateUI();
    }
}

public enum CategotyItemType
{
    전체아이템,
    소비아이템,   // 소비형 (예: 포션류)
    재료아이템,   // 재료형 (예: 비늘, 꽃 등)
    무기아이템,    // 장비형 (예: 무기류)
    방어아이템,    // 장비형 (예: 방어구류)
    퀘스트아이템,    // 퀘스트용
    특수아이템    // 버프증가, 스킬효과등 특수효과템들.
}
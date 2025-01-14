using System.Collections.Generic;

using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject basePrefab;
    [SerializeField] private Transform baseParent;
    private Button[] buttons;

    private List<Item> itemList;
    private Dictionary<ItemType, List<Item>> categorizedItems = new Dictionary<ItemType, List<Item>>();
    private int currentButtonIndex = 0; // 현재 선택된 버튼의 인덱스를 저장하는 변수

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
        GetItemData();
    }

    [SerializeField] ItemDatabase testItemList;
    private void GetItemData()
    {
        // TestItemList에서 데이터 가져오기
        if (testItemList != null)
        {
            //itemList = testItemList.GetTestItems(); // 데이터 가져오기
            Debug.Log($"아이템 리스트 연결확인, {itemList.Count}개의 소지중.");
        }
        else
        {
            Debug.LogError("TestItemList 연결안됨.");
        }
    }
    public void Update()
    {
        UpdateUI();
    }

    private void CategorizeItems()
    {
        categorizedItems.Clear();

        foreach (Item item in itemList)
        {
            ItemType category = item.itemType;
            if (!categorizedItems.ContainsKey(category))
            {
                categorizedItems[category] = new List<Item>();
            }
            categorizedItems[category].Add(item);
        }
    }

    private void OnButtonClick(int buttonIndex)
    {
        currentButtonIndex = buttonIndex;
        UpdateUI();
        Debug.Log(buttonIndex);
    }

    public void UpdateUI()
    {
        CategorizeItems();
        ClearUI();
        Debug.Log("ClearUI 완료");

        // 기본 검증
        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogWarning("버튼이 설정되지 않았거나 버튼 배열이 비어있습니다.");
            return;
        }

        if (!System.Enum.IsDefined(typeof(ItemType), currentButtonIndex))
        {
            Debug.LogError($"유효하지 않은 카테고리 인덱스: {currentButtonIndex}");
            return;
        }

        ItemType selectedCategory = (ItemType)currentButtonIndex;

        Debug.Log($"선택된 카테고리 '{selectedCategory}'에 포함된 아이템 개수: {categorizedItems[selectedCategory].Count}");
        foreach (var item in categorizedItems[selectedCategory])
        {
            CreateItemUI(item);
        }

        Debug.Log("UpdateUI 완료");
    }
    private void CreateItemUI(Item item)
    {
        var itemUI = Instantiate(basePrefab, baseParent);
        itemUI.GetComponentsInChildren<TextMeshProUGUI>()[0].text = item.itemName;
        itemUI.GetComponentsInChildren<TextMeshProUGUI>()[1].text = item.itemDescription;
    }

    private void ClearUI()
    {
        foreach (Transform child in baseParent.transform)
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






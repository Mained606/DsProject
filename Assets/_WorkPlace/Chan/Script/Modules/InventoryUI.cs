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


    private void GetItemData()
    {
        // TestItemList에서 데이터 가져오기
        itemList = InventoryManager.Instance.items; // 데이터 가져오기
        Debug.Log($"아이템 리스트 연결확인, {itemList.Count}개의 소지중.");

        if (itemList == null)
        {
            Debug.LogError("ItemList 연결안됨.");
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
        //  Debug.Log("ClearUI 완료");

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

        // 카테고리가 비어있으면 리턴
        if (!categorizedItems.ContainsKey(selectedCategory) || categorizedItems[selectedCategory].Count == 0)
        {
            //   Debug.Log($"선택된 카테고리 '{selectedCategory}'에 포함된 아이템이 없습니다.");
            return;
        }

        //   Debug.Log($"선택된 카테고리 '{selectedCategory}'에 포함된 아이템 개수: {categorizedItems[selectedCategory].Count}");
        foreach (var item in categorizedItems[selectedCategory])
        {
            CreateItemUI(item);
        }

        //   Debug.Log("UpdateUI 완료");
    }
    private void CreateItemUI(Item item)
    {
        var itemUI = Instantiate(basePrefab, baseParent);
        itemUI.GetComponentsInChildren<TextMeshProUGUI>()[0].text = item.itemName;
        itemUI.GetComponentsInChildren<TextMeshProUGUI>()[1].text = item.itemDescription;

        // 이미지 설정
        var images = itemUI.GetComponentsInChildren<Image>();
        if (images.Length > 1)
        {
            // 아이템 이미지가 있으면 설정, 없으면 NONE 상태로 유지
            images[1].sprite = item.itemImage; // item.itemImage가 null이면 이미지가 NONE 상태로 표시됨
        }
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






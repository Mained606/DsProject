using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using Unity.VisualScripting;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemsParent;

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
        UpdateUI();
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
        switch (currentButtonIndex)
        {
            case 5:
                Debug.Log("제거 처리로직 필요");
                break;

            case 6:
                GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
                break;

            case 7:
                Debug.Log("장착 처리로직 필요");
                break;

            default:
                if (currentButtonIndex >= 0 && currentButtonIndex < 5)
                {
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
                break;
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
       //  inventoryItem.GetComponentInChildren<TextMeshProUGUI>().text = item.ToStringTMPro();
        if (item.sprite != null) inventoryItem.GetComponentsInChildren<Image>()[1].sprite = item.sprite;
        Debug.Log(item.ToStringTMPro());
        inventoryItem.AddComponent<InventoryTooltip>();
        inventoryItem.GetComponent<InventoryTooltip>().currentitem = item;

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
    무기아이템,    // 장비형 (예: 무기류)
    방어아이템,    // 장비형 (예: 방어구류)
    소비아이템,   // 소비형 (예: 포션류)
    재료아이템,   // 재료형 (예: 비늘, 꽃 등)
}
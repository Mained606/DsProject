using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemsParent;
    [SerializeField] private GameObject itemInfoObject;
    [SerializeField] private TextMeshProUGUI[] itemInfoTextField;
    [SerializeField] private Image itemInfoImageField;

    private Button[] buttons;
    private int currentButtonIndex = 6;
    private Dictionary<string, List<Item>> categorizedItems = new Dictionary<string, List<Item>>();

    #region 골드 인벤으로 이전
    [SerializeField]private TextMeshProUGUI playerGold;
    #endregion

    private void Awake()
    {
        buttons = transform.GetComponentsInChildren<Button>();
    }

    private void OnEnable()
    {
        AddButtonListeners();
        OnButtonClick(currentButtonIndex);
    }

    private void OnDisable()
    {

        #region !버튼 애니 초기화!
        ResetButtonAnimations(); // 버튼 애니메이션 초기화
        #endregion

        RemoveButtonListeners();
        currentButtonIndex = 6;
    }

    private void Start()
    {

    }


    #region !버튼 애니 초기화 함수! 
    private void ResetButtonAnimations()
    {
        foreach (var button in buttons)
        {
            Animator buttonAnimator = button.GetComponent<Animator>();
            if (buttonAnimator != null)
            {
                buttonAnimator.Rebind(); // 애니메이터 내부 상태를 리셋
                buttonAnimator.Update(0f); // 즉시 반영
            }
        }
    }
    #endregion


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

        #region 골드 인벤토리로 이전
        playerGold.text = CharacterManager.PlayerCharacterData.gold.ToString();
        #endregion

        if (buttons == null || buttons.Length == 0)
        {
            Debug.LogWarning("버튼이 설정되지 않았거나 버튼 배열이 비어있습니다.");
            return;
        }
        switch (currentButtonIndex)
        {
            case 7:
                Debug.Log("제거 처리로직 필요");
                break;

            case 8:
                GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
                break;

            case 9:
                Debug.Log("장착 처리로직 필요");
                break;

            default:
                if (currentButtonIndex >= 0 && currentButtonIndex < 7)
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
        inventoryItem.GetComponent<InventoryTooltip>().currentItem = item;
        inventoryItem.GetComponent<InventoryTooltip>().InventorytooltipWindow = itemInfoObject;
        inventoryItem.GetComponent<InventoryTooltip>().textPoint = itemInfoTextField;
        inventoryItem.GetComponent<InventoryTooltip>().ItemImage = itemInfoImageField;
    }

    public void AddButtonListeners()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            #region 버튼 애니 사용시/비사용시    비활성화 /활성화
            /* if (index >= 0 && index < 7)
             {
                 Animator buttonAnimator = buttons[index].animator;
                 buttonAnimator.SetTrigger("Idle");
             }*/
            #endregion
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

        #region 버튼 애니 사용시/비사용시    비활성화 /활성화
        /* if (currentButtonIndex >= 0 && currentButtonIndex < 7)
         {
             Animator buttonAnimator = buttons[buttonIndex].animator;
             buttonAnimator.SetTrigger("Selected");
         }*/
        #endregion

        UpdateUI();
    }
}

public enum CategotyItemType
{
    무기,         // Weapon
    방어구,       // Armor
    소모품,       // Consumable
    퀘스트,       // QuestItem
    제작재료,     // Material
    장신구,        // Accessory
    전체아이템
}

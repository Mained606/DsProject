using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using UnityEngine.EventSystems;

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

    private IReadOnlyList<Item> preInventory;

    #region 골드 인벤으로 이전
    [SerializeField]private TextMeshProUGUI playerGold;
    #endregion

    private void Awake()
    {
        preInventory = new List<Item>();
        buttons = transform.GetComponentsInChildren<Button>();
    }

    private void OnEnable()
    {
        AddButtonListeners();
        OnButtonClick((int)CategotyItemType.전체아이템);
    }

    private void OnDisable()
    {
        //강화 테스트용
        //03.12 Hj 추가
        //InventoryManager.Instance.ResetSelectedItem();

        RemoveButtonListeners();
    }

    private void CategorizeItems()
    {
        if (InventoryManager.Instance == null) return;
        if (preInventory.SequenceEqual(InventoryManager.InventoryList)) { return; } // 내부 요소 비교

        preInventory = new List<Item>(InventoryManager.InventoryList); // 리스트 복사
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
                    transform.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;
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
        inventoryItem.GetComponent<InventorySlotTooltip>().currentItem = item;
        inventoryItem.GetComponent<InventorySlotTooltip>().InventorytooltipWindow = itemInfoObject;
        inventoryItem.GetComponent<InventorySlotTooltip>().textPoint = itemInfoTextField;
        inventoryItem.GetComponent<InventorySlotTooltip>().ItemImage = itemInfoImageField;
    }

    public void AddButtonListeners()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
           
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
            if (i == currentButtonIndex) continue;
            Animator animator = buttons[i].GetComponent<Animator>();
            if (animator != null)
            {
                animator.StopPlayback();
                animator.SetTrigger("Idle");
            }
        }
    }

    public void RemoveButtonListeners()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.RemoveAllListeners();
            Animator animator = buttons[i].GetComponent<Animator>();
            if (animator != null) ButtonReset(animator);
        }
    }

    private void OnButtonClick(int buttonIndex)
    {
        if (buttons[currentButtonIndex].animator != null) buttons[currentButtonIndex].animator.CrossFade("Idle", 0f);
        currentButtonIndex = buttonIndex;
        UpdateUI();
    }

    private void ButtonReset(Animator animator)
    {
        Image image1 = animator.transform.GetChild(0).GetComponent<Image>();
        Color color1 = image1.color;
        color1.a = 0f;
        image1.color = color1;

        Image image2 = animator.transform.GetChild(3).GetComponent<Image>();
        Color color2 = image2.color;
        color2.a = 0f;
        image2.color = color2;
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

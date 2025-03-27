using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnhanceUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemParent;
    [SerializeField] private GameObject itemInfoObject;


    [SerializeField] private TextMeshProUGUI[] itemInfoTextField;
    [SerializeField] private Image itemInfoImageField;

    [SerializeField] private Button enhanceButton;
    [SerializeField] private Button cancelButton;

    [SerializeField] private Image enhanceSlotImage;

    [SerializeField] private GameObject CurrentItem;
    [SerializeField] private GameObject AfterItem;

    private List<Item> targetItems = new(); // 강화 가능한 장비 목록
    private List<Item> previousItems = new(); // 변경 감지용
    private Item selectedItem = null; // 강화 슬롯에 들어간 아이템

    private const string ENHANCE_MATERIAL_ID = "강화석";

    private void OnEnable()
    {
        AddButtonListeners();
        LoadTargetItems(); // 무기/방어구 필터링해서 불러옴
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
    }

    private void LoadTargetItems()
    {
        if (InventoryManager.Instance == null) return;
        targetItems.Clear();

        foreach (var item in InventoryManager.InventoryList)
        {
            if (item.type == ItemType.무기 || item.type == ItemType.방어구)
            {
                targetItems.Add(item);
            }
        }

        if (!AreListsEqual(previousItems, targetItems))
        {
            UpdateUI();
            previousItems = new List<Item>(targetItems);
        }
    }

    private void UpdateUI()
    {
        ClearUI();

        // 인벤토리에 실제 존재하는 아이템만 유지
        targetItems = targetItems.FindAll(item => InventoryManager.Instance.HasItem(item.id));

        foreach (var item in targetItems)
        {
            CreateItemUI(item);
        }
    }

    private void CreateItemUI(Item item)
    {
        var go = Instantiate(itemPrefab, itemParent);
        var tooltip = go.GetComponent<InventoryTooltip>();
        tooltip.currentItem = item;
        tooltip.InventorytooltipWindow = itemInfoObject;
        tooltip.textPoint = itemInfoTextField;
        tooltip.ItemImage = itemInfoImageField;
    }

    private void ClearUI()
    {
        foreach (Transform child in itemParent)
        {
            Destroy(child.gameObject);
        }
    }

    private void AddButtonListeners()
    {
        enhanceButton.onClick.RemoveAllListeners();
        enhanceButton.onClick.AddListener(() =>
        {
            TryEnhance();
        });

        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            ClearEnhanceSlot();
            LoadTargetItems();
        });
    }

    private void RemoveButtonListeners()
    {
        enhanceButton.onClick.RemoveAllListeners();
        cancelButton.onClick.RemoveAllListeners();
    }

    private void TryEnhance()
    {
        if (selectedItem == null)
        {
            UIManager.SystemMessage("강화할 아이템이 없습니다.");
            return;
        }

        int qty = InventoryManager.Instance.GetItemQuantity(ENHANCE_MATERIAL_ID);
        if (qty <= 0)
        {
            UIManager.SystemMessage("강화석이 부족합니다.");
            return;
        }

        var mat = ItemManager.Instance.GetItemById(ENHANCE_MATERIAL_ID);
        EnhanceManager.Instance.Enhance(selectedItem, mat);

        ClearEnhanceSlot();
        LoadTargetItems();
    }

    private void ClearEnhanceSlot()
    {
        selectedItem = null;
        enhanceSlotImage.sprite = null;
        enhanceSlotImage.enabled = false; // 슬롯 비우면 안 보이게
    }

    private bool AreListsEqual(List<Item> a, List<Item> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].id != b[i].id || a[i].quantity != b[i].quantity)
                return false;
        }
        return true;
    }
    public void SetSelectedItem(Item item)
    {
        selectedItem = item;
        enhanceSlotImage.sprite = item.sprite; // ✅ 드랍된 장비 이미지 표시
        enhanceSlotImage.enabled = true;       // 혹시 꺼져 있을 수도 있으니까

    }
}
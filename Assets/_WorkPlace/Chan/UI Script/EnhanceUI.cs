using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EnhanceUI : MonoBehaviour
{
    [Header("아이템 리스트")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Transform itemParent;

    [Header("툴팁 패널")]
    [SerializeField] private GameObject itemInfoObject;
    [SerializeField] private TextMeshProUGUI[] itemInfoTextField;
    [SerializeField] private Image itemInfoImageField;
    [SerializeField] private Image elementIconField;
    [SerializeField] private TextMeshProUGUI itemLevel;

    [Header("버튼")]
    [SerializeField] private Button enhanceButton;
    [SerializeField] private Button cancelButton;

    [Header("슬롯")]
    [SerializeField] private Image enhanceSlotImage;

    [Header("강화 비교 패널")]
    [SerializeField] private GameObject infoPanelPrefab;

    [SerializeField] private Transform currentPanelParent;
    [SerializeField] private Transform afterPanelParent;

    private GameObject currentPanel;
    private GameObject afterPanel;

    [SerializeField] private Image enhanceStoneImage;
    [SerializeField] private TextMeshProUGUI enhanceStoneQuantityText;

    private List<Item> targetItems = new();
    private List<Item> previousItems = new();
    private Item selectedItem = null;

    private void OnEnable()
    {
        AddButtonListeners();
        LoadTargetItems();
        UpdateEnhanceStoneUI();
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
                targetItems.Add(item);
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
        targetItems = targetItems.FindAll(item => InventoryManager.Instance.HasItem(item.id));

        foreach (var item in targetItems)
            CreateItemUI(item);
    }

    private void CreateItemUI(Item item)
    {
        var go = Instantiate(itemPrefab, itemParent);
        var tooltip = go.GetComponent<InventorySlotTooltip2>();

        tooltip.currentItem = item;

        // 툴팁용 바인딩
        tooltip.InventorytooltipWindow = itemInfoObject;
        tooltip.textPoint = itemInfoTextField; // 최소 4개짜리 배열 (1:이름, 2:설명, 3:레벨)
        tooltip.ItemImage = itemInfoImageField;
        tooltip.ElementIcon = elementIconField;
        tooltip.ItemLevel = itemLevel;
    }

    private void ClearUI()
    {
        foreach (Transform child in itemParent)
            Destroy(child.gameObject);
    }

    private void AddButtonListeners()
    {
        enhanceButton.onClick.RemoveAllListeners();
        enhanceButton.onClick.AddListener(() => TryEnhance());

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

        if (selectedItem.isEquired) 
        {
            UIManager.SystemMessage("장착 중인 아이템은 강화할 수 없습니다.");
            return;
        }

        string materialId = EnhanceManager.Instance.enhancementItemIds[0];
        if (string.IsNullOrEmpty(materialId))
        {
            UIManager.SystemMessage("강화 재료가 설정되어 있지 않습니다.");
            return;
        }

        int qty = InventoryManager.Instance.GetItemQuantity(materialId);
        if (qty <= 0)
        {
            UIManager.SystemMessage("강화 재료가 부족합니다.");
            return;
        }

        var mat = ItemManager.Instance.GetItemById(materialId);
        EnhanceManager.Instance.Enhance(selectedItem, mat);

        ClearEnhanceSlot();
        LoadTargetItems();
        UpdateEnhanceStoneUI();
    }

    private void ClearEnhanceSlot()
    {
        selectedItem = null;
        enhanceSlotImage.sprite = null;
        enhanceSlotImage.enabled = false;

        if (currentPanel != null) Destroy(currentPanel);
        if (afterPanel != null) Destroy(afterPanel);
    }

   public void SetSelectedItem(Item item)
{
    selectedItem = item;
    enhanceSlotImage.sprite = item.sprite;
    enhanceSlotImage.enabled = true;

    // 기존 패널 삭제
    if (currentPanel != null) Destroy(currentPanel);
    if (afterPanel != null) Destroy(afterPanel);

        // Current Panel 생성 (빈 오브젝트 currentPanelParent 위치)
        currentPanel = Instantiate(infoPanelPrefab, currentPanelParent);
        currentPanel.transform.localPosition = Vector3.zero;
        currentPanel.transform.localScale = Vector3.one;
        currentPanel.transform.localRotation = Quaternion.identity;

        // After Panel 생성 (빈 오브젝트 afterPanelParent 위치)
        afterPanel = Instantiate(infoPanelPrefab, afterPanelParent);
        afterPanel.transform.localPosition = Vector3.zero;
        afterPanel.transform.localScale = Vector3.one;
        afterPanel.transform.localRotation = Quaternion.identity;

        // 텍스트, 이미지 바인딩
        var currentTexts = currentPanel.GetComponentsInChildren<TextMeshProUGUI>();
        var currentImage = currentPanel.GetComponentsInChildren<Image>();
        currentTexts[0].text = item.id;
        currentTexts[1].text = $"+{item.itemSkill.Level}";
        currentTexts[2].text = item.ToStringTMPro();
        currentImage[4].sprite = null;
        currentImage[5].sprite = item.sprite;

        // 속성 아이콘 - Current Panel
        ElementalAttribute attr = item.itemSkill.element;
        Addressables.LoadAssetAsync<Sprite>(attr.ToString()).Completed += (handle) =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                currentImage[4].sprite = handle.Result;
                currentImage[4].gameObject.SetActive(true);
            }
        };
        // 프리뷰 아이템 생성 (강화된 아이템 정보)
        Item previewItem = EnhanceManager.Instance.PreviewEnhance(item);

        var afterTexts = afterPanel.GetComponentsInChildren<TextMeshProUGUI>();
        var afterImage = afterPanel.GetComponentsInChildren<Image>();
        afterTexts[0].text = previewItem.id;
        afterTexts[1].text = $"+{previewItem.itemSkill.Level}";
        afterTexts[2].text = previewItem.ToStringTMPro();
        afterImage[4].sprite = null;
        afterImage[5].sprite = previewItem.sprite;

        afterTexts[4].text = item.itemSkill.element.ToString();
        afterPanel.GetComponentInChildren<TextMeshProUGUI>().text = $"Lv. {item.itemSkill.Level}";

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

    private void UpdateEnhanceStoneUI()
    {
        string materialId = EnhanceManager.Instance.enhancementItemIds[0];

        Item enhanceStone = ItemManager.Instance.GetItemById(materialId);

        if (enhanceStone != null)
        {
            enhanceStoneImage.sprite = enhanceStone.sprite;
            enhanceStoneImage.enabled = true;

            int quantity = InventoryManager.Instance.GetItemQuantity(materialId);
            enhanceStoneQuantityText.text = quantity.ToString();
        }
        else
        {
            enhanceStoneImage.enabled = false;
            enhanceStoneQuantityText.text = "0";
        }
    }
}

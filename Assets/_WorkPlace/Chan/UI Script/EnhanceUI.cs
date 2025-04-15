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

    [Header("경고창")]
    [SerializeField] private GameObject warningPopup;             // 전체 팝업 오브젝트
    [SerializeField] private TextMeshProUGUI warningText;         // 메시지 출력용
    [SerializeField] private Button warningButton;

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
        warningButton.onClick.AddListener(() => warningPopup.SetActive(false));
        LoadTargetItems();
        ClearEnhanceSlot();
        UpdateEnhanceStoneUI();
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
        itemInfoObject.SetActive(false);
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
            ShowWarning("강화할 아이템이 없습니다.");
            return;
        }

        if (selectedItem.isEquired)
        {
            ShowWarning("장착 중인 아이템은 강화할 수 없습니다.");
            return;
        }

        string materialId = EnhanceManager.Instance.enhancementItemIds[0];
        if (string.IsNullOrEmpty(materialId))
        {
            ShowWarning("강화 재료가 설정되어 있지 않습니다.");
            return;
        }

        int qty = InventoryManager.Instance.GetItemQuantity(materialId);
        if (qty <= 0)
        {
            ShowWarning("강화 재료가 부족합니다.");
            return;
        }

        var mat = ItemManager.Instance.GetItemById(materialId);
        int previousLevel = selectedItem.itemSkill.Level; 
        string deletedItemId = selectedItem.id;
        EnhanceManager.Instance.Enhance(selectedItem, mat);

        // 인벤토리 갱신
        previousItems.Clear();
        LoadTargetItems();
        UpdateEnhanceStoneUI();

        if (!InventoryManager.Instance.HasItem(deletedItemId))
        {
            ShowWarning("장비가 파괴되었습니다!");
            ClearEnhanceSlot();
            if (currentPanel != null) Destroy(currentPanel);
            if (afterPanel != null) Destroy(afterPanel);
        }
        else
        {
            int newLevel = selectedItem.itemSkill.Level;

            if (newLevel > previousLevel)
                ShowWarning("장비 강화 성공!");
            else if (newLevel < previousLevel)
                ShowWarning("장비 강화 실패!\n레벨이 하락했습니다.");
            else
                ShowWarning("장비 강화 실패!");

            SetSelectedItem(selectedItem);
        }
    }

    private void ClearEnhanceSlot()
    {
        selectedItem = null;
        enhanceSlotImage.sprite = null;
        enhanceSlotImage.enabled = false;
        Color color = enhanceSlotImage.color;
        color.a = 0f;
        enhanceSlotImage.color = color;

        if (currentPanel != null) Destroy(currentPanel);
        if (afterPanel != null) Destroy(afterPanel);
    }

   public void SetSelectedItem(Item item)
{
    selectedItem = item;
    enhanceSlotImage.sprite = item.sprite;
    enhanceSlotImage.enabled = true;
    Color color = enhanceSlotImage.color;
    color.a = 1f;
    enhanceSlotImage.color = color;


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

        // 속성 아이콘
        ElementalAttribute attr = item.itemSkill.element;
        if (attr == ElementalAttribute.None)
        {
            currentImage[4].gameObject.SetActive(false);
        }
        else
        {
            Addressables.LoadAssetAsync<Sprite>(attr.ToString()).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    currentImage[4].sprite = handle.Result;
                    currentImage[4].gameObject.SetActive(true);
                }
            };
        }

        // 프리뷰 아이템 생성 (강화된 아이템 정보)
        Item previewItem = EnhanceManager.Instance.PreviewEnhance(item);

        var afterTexts = afterPanel.GetComponentsInChildren<TextMeshProUGUI>();
        var afterImage = afterPanel.GetComponentsInChildren<Image>();
        afterTexts[0].text = previewItem.id;
        afterTexts[1].text = $"+{previewItem.itemSkill.Level}";
        afterTexts[2].text = previewItem.ToStringTMPro();
        afterImage[4].sprite = null;
        afterImage[5].sprite = previewItem.sprite;

        ElementalAttribute previewAttr = previewItem.itemSkill.element;
        if (attr == ElementalAttribute.None)
        {
            afterImage[4].gameObject.SetActive(false);
        }
        else
        {
            Addressables.LoadAssetAsync<Sprite>(attr.ToString()).Completed += (handle) =>
            {
                if (handle.Status == AsyncOperationStatus.Succeeded)
                {
                    afterImage[4].sprite = handle.Result;
                    afterImage[4].gameObject.SetActive(true);
                }
            };
        }
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

     public void ShowWarning(string message)
    {
        warningText.text = message;
        warningPopup.SetActive(true);
    }

}

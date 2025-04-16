using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab; // 아이템슬롯 프리팹
    [SerializeField] private Transform itemParent; // 슬롯창
    [SerializeField] private Transform cookedItemParent;

    [SerializeField] private GameObject itemInfoObject; // 인포판넬
    [SerializeField] private TextMeshProUGUI[] itemInfoTextField; // 텍스트바인딩
    [SerializeField] private Image itemInfoImageField; // 아이템 이미지 바인딩 
    [SerializeField] private Image elementIconField;
    [SerializeField] private TextMeshProUGUI itemLevel;

    //==========================================================================

    //  [SerializeField] private CookingManager cookingManager; // CookingManager 참조
    [SerializeField] private Button cancelButton; // 취소 버튼
    [SerializeField] private Button cookButton; // 요리 시작 버튼

    [SerializeField] private GameObject warningPopup;
    [SerializeField] private TextMeshProUGUI warningText;
    [SerializeField] private Button warningButton;

    //==========================================================================
    private Image iconImage;
    private TextMeshProUGUI itemInfoText;
    private List<Item> craftingMaterials = new(); // 제작재료 리스트
    private List<Item> previousMaterials = new(); // 이전 UI 상태 비교용 리스트

    //==========================================================================
    [SerializeField] private Transform potSlotParent;
    [SerializeField] private GameObject potSlotPrefab;
    private void OnEnable()
    {
        CookingManager.Instance.ClearIngredients();
        AddButtonListeners();
        warningButton.onClick.AddListener(() => warningPopup.SetActive(false));
        LoadCraftingMaterials();
        UpdatePotSlotUI();
    }
    private void OnDisable()
    {
        RemoveButtonListeners();
        itemInfoObject.SetActive(false);
        warningPopup.SetActive(false);
    }

    // 인벤토리에서 제작재료(ItemType.제작재료)만 필터링해서 불러오기
    private void LoadCraftingMaterials()
    {
        if (InventoryManager.Instance == null) return;

        craftingMaterials.Clear();

        foreach (var item in InventoryManager.InventoryList)
        {
            if (item.type == ItemType.요리재료)
                craftingMaterials.Add(item);
        }

        UpdateUI();
        previousMaterials = new List<Item>(craftingMaterials);
    }

    public void UpdateUI()
    {
        ClearUI();

        craftingMaterials = craftingMaterials.FindAll(item => InventoryManager.Instance.HasItem(item.id));

        foreach (var item in InventoryManager.InventoryList)
        {
            if (item.type == ItemType.요리재료)
                CreateItemUI(item, itemParent);
            else if (item.type == ItemType.요리)
                CreateItemUI(item, cookedItemParent);
        }
    }
    
    // 제작재료 슬롯 생성
    private void CreateItemUI(Item item, Transform parent)
    {
        var slot = Instantiate(itemPrefab, parent);
        var tooltip = slot.GetComponent<InventorySlotTooltip2>();

        tooltip.currentItem = item;
        tooltip.InventorytooltipWindow = itemInfoObject;
        tooltip.textPoint = itemInfoTextField;
        tooltip.ItemImage = itemInfoImageField;
        tooltip.ElementIcon = elementIconField;
        tooltip.ItemLevel = itemLevel;
    }

    // 기존 UI 초기화 (제작재료 슬롯 제거)
    private void ClearUI()
    {
        foreach (Transform child in itemParent.transform)
            Destroy(child.gameObject);

        foreach (Transform child in cookedItemParent.transform)
            Destroy(child.gameObject); //
    }

    private void AddButtonListeners()
    {
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            CookingManager.Instance.ClearIngredients();
            LoadCraftingMaterials();
            UpdatePotSlotUI();
        });

        cookButton.onClick.RemoveAllListeners();
        cookButton.onClick.AddListener(() =>
        {
            if (CookingManager.Instance.SelectedIngredients.Count == 0)
            {
                ShowWarning("재료를 먼저 넣어주세요!");
                return;
            }

            CookingManager.Instance.Craft();
            RefreshCookingUI();

            string result = CookingManager.Instance.LastCraftedItemName;
            ShowWarning(!string.IsNullOrEmpty(result)
                ? $"'{result}'이(가) 만들어졌습니다!"
                : "요리에 실패했습니다...");
        });

        warningButton.onClick.RemoveAllListeners();
        warningButton.onClick.AddListener(() => warningPopup.SetActive(false));
    }

    private void RemoveButtonListeners()
    {
        cancelButton.onClick.RemoveAllListeners();
        cookButton.onClick.RemoveAllListeners();
    }

    // 두 리스트의 아이템을 비교하여 변경된 사항이 있는지 확인
    /*private bool AreListsEqual(List<Item> listA, List<Item> listB)
    {
        if (listA.Count != listB.Count) return false;
        for (int i = 0; i < listA.Count; i++)
        {
            if (listA[i].id != listB[i].id || listA[i].quantity != listB[i].quantity)
                return false;
        }
        return true;
    }*/

    public void UpdatePotSlotUI()
    {
        ClearPotSlotUI();

        foreach (var item in CookingManager.Instance.SelectedIngredients)
        {
            var slot = Instantiate(potSlotPrefab, potSlotParent);
            var icon = slot.GetComponentInChildren<Image>();
            var countText = slot.GetComponentInChildren<TextMeshProUGUI>();

            // 원본 아이템에서 스프라이트만 참조
            var original = ItemManager.Instance.GetItemById(item.id);
            if (original != null)
            {
                icon.sprite = original.sprite;
            }
            else
            {
                icon.sprite = item.sprite; // fallback
            }

            countText.text = $"x{item.quantity}";
        }
    }

    private void ClearPotSlotUI()
    {
        foreach (Transform child in potSlotParent)
        {
            Destroy(child.gameObject);
        }
    }
    public void RefreshCookingUI()
    {
        ClearPotSlotUI();   // 냄비 비우기
        ClearUI();          // 슬롯 비우기
        LoadCraftingMaterials();  // 재료 & 요리 아이템 다시 로드
        UpdatePotSlotUI();  // 냄비 슬롯 다시 그리기
    }

    private void ShowWarning(string message)
    {
        warningText.text = message;
        warningPopup.SetActive(true);
    }
}
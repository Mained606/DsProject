using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CookingUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab; // 아이템슬롯 프리팹
    [SerializeField] private Transform itemParent; // 슬롯창
    [SerializeField] private GameObject itemInfoObject; // 인포판넬
    [SerializeField] private TextMeshProUGUI[] itemInfoTextField; // 텍스트바인딩
    [SerializeField] private Image itemInfoImageField; // 아이템 이미지 바인딩 
    [SerializeField] private Image elementIconField;
    [SerializeField] private TextMeshProUGUI itemLevel;

    //==========================================================================

    //  [SerializeField] private CookingManager cookingManager; // CookingManager 참조
    [SerializeField] private Button cancelButton; // 취소 버튼
    [SerializeField] private Button cookButton; // 요리 시작 버튼



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
        LoadCraftingMaterials();
        UpdatePotSlotUI();
    }
    private void OnDisable()
    {
        RemoveButtonListeners();
        itemInfoObject.SetActive(false);
    }
   
    // 인벤토리에서 제작재료(ItemType.제작재료)만 필터링해서 불러오기
    private void LoadCraftingMaterials()
    {
        if (InventoryManager.Instance == null) return;
        craftingMaterials.Clear();

        // InventoryManager의 InventoryList에서 제작재료만 가져오기
        foreach (var item in InventoryManager.InventoryList)
        {
            if (item.type == ItemType.요리재료)
            {
                craftingMaterials.Add(item);
            }
        }
        // 데이터 변경 확인 후 UI 업데이트
        if (!AreListsEqual(previousMaterials, craftingMaterials))
        {
            UpdateUI();
            previousMaterials = new List<Item>(craftingMaterials);
        }
    }

    // UI 업데이트 (제작재료만 표시)
    public void UpdateUI()
    {

        ClearUI();

        // 현재 인벤토리에 있는 아이템만 craftingMaterials 리스트에 유지
        craftingMaterials = craftingMaterials.FindAll(item => InventoryManager.Instance.HasItem(item.id));

        foreach (var item in craftingMaterials)
        {
            CreateItemUI(item);
        }
    }

    // 제작재료 슬롯 생성
    private void CreateItemUI(Item item)
    {
        var inventoryItem = Instantiate(itemPrefab, itemParent);
        inventoryItem.GetComponent<InventorySlotTooltip2>().currentItem = item;
        inventoryItem.GetComponent<InventorySlotTooltip2>().InventorytooltipWindow = itemInfoObject;
        inventoryItem.GetComponent<InventorySlotTooltip2>().textPoint = itemInfoTextField;
        inventoryItem.GetComponent<InventorySlotTooltip2>().ItemImage = itemInfoImageField;
        inventoryItem.GetComponent<InventorySlotTooltip2>().ElementIcon = elementIconField;
        inventoryItem.GetComponent<InventorySlotTooltip2>().ItemLevel = itemLevel;
    }

    // 기존 UI 초기화 (제작재료 슬롯 제거)
    private void ClearUI()
    {
        foreach (Transform child in itemParent.transform)
        {
            Destroy(child.gameObject);
        }
    }

    private void AddButtonListeners()
    {
        cancelButton.onClick.RemoveAllListeners();
        cancelButton.onClick.AddListener(() =>
        {
            CookingManager.Instance.ClearIngredients();
            LoadCraftingMaterials(); //  요리 재료 목록 다시 불러오기
            UpdatePotSlotUI();
        });

        cookButton.onClick.RemoveAllListeners();
        cookButton.onClick.AddListener(() =>
        {
            CookingManager.Instance.Craft();
            LoadCraftingMaterials(); // 요리 완료 후 재료 갱신
            UpdatePotSlotUI();
        });
    }

    private void RemoveButtonListeners()
    {
        cancelButton.onClick.RemoveAllListeners();
        cookButton.onClick.RemoveAllListeners();
    }

    // 두 리스트의 아이템을 비교하여 변경된 사항이 있는지 확인
    private bool AreListsEqual(List<Item> listA, List<Item> listB)
    {
        if (listA.Count != listB.Count) return false;
        for (int i = 0; i < listA.Count; i++)
        {
            if (listA[i].id != listB[i].id || listA[i].quantity != listB[i].quantity)
                return false;
        }
        return true;
    }

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

}
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

    //==========================================================================

  //  [SerializeField] private CookingManager cookingManager; // CookingManager 참조
    [SerializeField] private Button cancelButton; // 취소 버튼
    [SerializeField] private Button cookButton; // 요리 시작 버튼



    //==========================================================================
    private Image iconImage;
    private TextMeshProUGUI itemInfoText;
    private List<Item> craftingMaterials = new(); // 제작재료 리스트


    private void OnEnable()
    {
        AddButtonListeners();
        LoadCraftingMaterials();
    }
    private void OnDisable()
    {
        RemoveButtonListeners();
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

        UpdateUI();
    }

    // UI 업데이트 (제작재료만 표시)
    private void UpdateUI()
    {
        ClearUI();

        foreach (var item in craftingMaterials)
        {
            CreateItemUI(item);
        }
    }

    // 제작재료 슬롯 생성
    private void CreateItemUI(Item item)
    {
        var inventoryItem = Instantiate(itemPrefab, itemParent);
        inventoryItem.GetComponent<InventoryTooltip>().currentItem = item;
        inventoryItem.GetComponent<InventoryTooltip>().InventorytooltipWindow = itemInfoObject;
        inventoryItem.GetComponent<InventoryTooltip>().textPoint = itemInfoTextField;
        inventoryItem.GetComponent<InventoryTooltip>().ItemImage = itemInfoImageField;
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
            //  UpdatePotUI();
        });

        cookButton.onClick.RemoveAllListeners();
        cookButton.onClick.AddListener(() =>
        {
            CookingManager.Instance.Craft();
            //   UpdatePotUI();
        });
    }

    private void RemoveButtonListeners()
    {
        cancelButton.onClick.RemoveAllListeners();
        cookButton.onClick.RemoveAllListeners();
    }
}
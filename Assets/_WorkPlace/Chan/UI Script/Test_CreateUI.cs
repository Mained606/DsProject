using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test_CreateUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab; // 아이템 슬롯 프리팹 (제작재료 전용)
    [SerializeField] private Transform ItemPanel;   // 인벤토리에서 제작재료가 표시될 패널
    [SerializeField] private Transform CreatePanel; // 우측 제작 패널
    [SerializeField] private GameObject InfoPanel;  // 결과 아이템 인포 패널

    private Image iconImage;
    private TextMeshProUGUI itemInfoText;
    private List<Item> craftingMaterials = new(); // 제작재료 리스트

    private void Awake()
    {
        iconImage = CreatePanel.GetComponentInChildren<Image>();
        itemInfoText = InfoPanel.GetComponentInChildren<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        LoadCraftingMaterials(); 
    }

    // 인벤토리에서 제작재료(ItemType.제작재료)만 필터링해서 불러오기
    private void LoadCraftingMaterials()
    {
        craftingMaterials.Clear();

        // InventoryManager의 InventoryList에서 제작재료만 가져오기
        foreach (var item in InventoryManager.InventoryList)
        {
            if (item.type == ItemType.제작재료)
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
        var inventoryItem = Instantiate(itemPrefab, ItemPanel);
       /* var itemUI = inventoryItem.GetComponent<CraftingSlot>(); // 제작 슬롯 UI 스크립트 필요

        if (itemUI != null)
        {
            itemUI.SetItem(item);
        }*/
    }

    // 기존 UI 초기화 (제작재료 슬롯 제거)
    private void ClearUI()
    {
        foreach (Transform child in ItemPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }
}

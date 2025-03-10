using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test_CreateUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab; // 아이템 슬롯 프리팹
    [SerializeField] private Transform ItemPanel;   // 인벤토리에 재료 아이템 판넬
    [SerializeField] private Transform CreatePanel; // 우측 제작 창 판넬
    [SerializeField] private GameObject InfoPanel;   // 결과 아이템 인포 판넬

    private Image iconImage;
    private TextMeshProUGUI[] itemInfoText;         // 결과 아이템 인포창 텍스트 

    private Button[] buttons;

    private IReadOnlyList<Item> itemList;
    private Dictionary<ItemType, List<Item>> craftingMaterials = new();

    private void Start()
    {
        itemList = new List<Item>();

        ItemPanel = transform.GetComponent<Transform>();
        CreatePanel = transform.GetComponent<Transform>();

        iconImage = CreatePanel.GetComponentInChildren<Image>();
        itemInfoText = InfoPanel.GetComponentsInChildren<TextMeshProUGUI>();

        buttons = transform.GetComponentsInChildren<Button>();

    }

    private void OnEnable()
    {
        AddButtonListeners();
        LoadCraftingMaterials();  // 제작 재료 아이템만 불러오기
    }

    private void OnDisable()
    {
        
    }

    private void OnButtonClick(int buttonIndex)
    {
     
    }

    public void AddButtonListeners()
    {
    }

    private void LoadCraftingMaterials()
    {
        craftingMaterials.Clear();

        foreach (var item in itemList)
        {
            if (item.type == ItemType.제작재료)
            {
                if (!craftingMaterials.ContainsKey(item.type))
                {
                    craftingMaterials[item.type] = new List<Item>();
                }
                craftingMaterials[item.type].Add(item);
            }
        }

       // UpdateUI(craftingMaterials[ItemType.제작재료]);
    }
    private void CreateItemUI(Item item)
    {
        var inventoryItem = Instantiate(itemPrefab, ItemPanel);
        inventoryItem.GetComponent<InventoryTooltip>().currentItem = item;
        inventoryItem.GetComponent<InventoryTooltip>().InventorytooltipWindow = InfoPanel;
        inventoryItem.GetComponent<InventoryTooltip>().textPoint = itemInfoText;
        inventoryItem.GetComponent<InventoryTooltip>().ItemImage = iconImage;
    }

    private void UpdateUI()
   {

   }

}

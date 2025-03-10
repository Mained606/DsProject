using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test_CreateUI : MonoBehaviour
{
    [SerializeField] private GameObject itemPrefab; // 아이템 슬롯 프리팹 -> 제작재료
    [SerializeField] private Transform ItemPanel;   // 인벤토리에 재료 아이템 판넬
    [SerializeField] private Transform CreatePanel; // 우측 제작 창 판넬
    [SerializeField] private GameObject InfoPanel;   // 결과 아이템 인포 판넬 -> 완성 아이템 인포

    private Image iconImage;
    private TextMeshProUGUI itemInfoText;    // 결과 아이템 인포창 텍스트 

    private Button[] buttons;

    // 아이템 리스트 받아오기 
    private IReadOnlyList<Item> itemList;
    private Dictionary<ItemType, List<Item>> craftingMaterials = new();

    private void Awake()
    {
        itemList = new List<Item>();

        ItemPanel = transform.GetComponent<Transform>();
        CreatePanel = transform.GetComponent<Transform>();

        iconImage = CreatePanel.GetComponentInChildren<Image>();
        itemInfoText = InfoPanel.GetComponentInChildren<TextMeshProUGUI>();

        buttons = transform.GetComponentsInChildren<Button>();

    }
    // 활성화 시 제작재료 카테고리 아이템만 보여주기
    private void OnEnable()
    {
        AddButtonListeners();
        LoadCraftingMaterials();  // 제작 재료 아이템만 불러오기
    }
    // 비활성화 시 
    private void OnDisable()
    {
        RemoveButtonListeners();
    }
    // 클릭시 작동 
    private void OnButtonClick(int buttonIndex)
    {
        UpdateUI();
    }
    public void AddButtonListeners()
    {
        
    }

    public void RemoveButtonListeners()
    {
      /*  for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.RemoveAllListeners();
            Animator animator = buttons[i].GetComponent<Animator>();
            if (animator != null) ButtonReset(animator);
        }*/
    }
    // 켜질때 제작재료 카테고리 아이템만 보이기
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

        UpdateUI();
    }
    
    // 좌측에 아이템 슬롯 생성 
    private void CreateItemUI(Item item)
    {
        var inventoryItem = Instantiate(itemPrefab, ItemPanel);
        /* inventoryItem.GetComponent<InventoryTooltip>().currentItem = item;
         inventoryItem.GetComponent<InventoryTooltip>().InventorytooltipWindow = InfoPanel;
         inventoryItem.GetComponent<InventoryTooltip>().textPoint = itemInfoText;
         inventoryItem.GetComponent<InventoryTooltip>().ItemImage = iconImage;*/
        //  -> 제작창의 아이템 프리탭 스크립트 추가 작성 후에 연결 
    }

    private void UpdateUI()
   {
        ClearUI();

        if (craftingMaterials.ContainsKey(ItemType.제작재료))
        {
            foreach (var item in craftingMaterials[ItemType.제작재료])
            {
                CreateItemUI(item);
            }
        }
    }

    // 좌측 아이템판넬 초기화 함수 
    private void ClearUI()
    {
        foreach (Transform child in ItemPanel.transform)
        {
            Destroy(child.gameObject);
        }
    }
}

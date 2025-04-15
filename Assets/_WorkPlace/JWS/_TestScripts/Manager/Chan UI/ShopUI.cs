using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    [SerializeField] private Transform shopItemListParent;
    [SerializeField] private GameObject shopItemListPrefab;
    [SerializeField] private Transform shopItemInfoPosition;
    [SerializeField] private NPCData shopNpcData;

    [Header("알림창")]
    [SerializeField] private GameObject warningPopup;             // 전체 팝업 오브젝트
    [SerializeField] private TextMeshProUGUI warningText;         // 메시지 출력용
    [SerializeField] private Button warningButton;
    [SerializeField] private Button PlusButton;
    [SerializeField] private Button MinusButton;

    private int currentShopType = 0;
    private TextMeshProUGUI[] uiText;
    private Button[] buttons;
    private Item currentInfoItem;
    private int sellQuantity = 1;

    private void Awake()
    {
        buttons = transform.GetComponentsInChildren<Button>(true);
        uiText = transform.GetComponentsInChildren<TextMeshProUGUI>(true);
        shopItemInfoPosition.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        AddButtonListeners();
        PlusButton.onClick.AddListener(() =>
        {
            if (sellQuantity < currentInfoItem.quantity)
                sellQuantity++;
            warningText.text = $"{sellQuantity}";
        });

        MinusButton.onClick.AddListener(() =>
        {
            if (sellQuantity > 1)
                sellQuantity--;
            warningText.text = $"{sellQuantity}";
        });

        warningButton.onClick.AddListener(() =>
        {
            warningPopup.SetActive(false);
            ItemManager.Instance.SellItem(currentInfoItem, shopNpcData.shopData.valueReductionRate, sellQuantity);
            UpdateUI();
        });
    }

    private void OnDisable()
    {
        RemoveButtonListeners();
    }

    public void UpdateUI()
    {
        ShowItemInfoClear();
        uiText[5].text = CharacterManager.PlayerCharacterData.gold.ToString();
        if (shopNpcData != null)
        {
            uiText[2].text = shopNpcData.name;
            ClearShopList();
            switch (currentShopType)
            {
                case 0:
                    for (int i = 0; i < shopNpcData.shopData.availableItems.Count; i++)
                    {
                        Item item = shopNpcData.shopData.availableItems[i];
                        GameObject gob = Instantiate(shopItemListPrefab, shopItemListParent);
                        gob.transform.GetComponent<ShopitemUI>().SetItemInfo(item, this, i, currentShopType);
                    }
                    transform.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;
                    break;
                case 1:
                    for (int i = 0; i < InventoryManager.InventoryList.Count; i++)
                    {
                        Item item = InventoryManager.InventoryList[i];
                        if (item.isEquired) continue;
                        GameObject gob = Instantiate(shopItemListPrefab, shopItemListParent);
                        gob.transform.GetComponent<ShopitemUI>().SetItemInfo(item, this, i, currentShopType);
                    }
                    transform.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;
                    break;
            }
            transform.GetComponentInChildren<ScrollRect>().verticalNormalizedPosition = 1f;
        }
    }

    public void AddButtonListeners()
    {
        for (int i = 0; i < buttons.Length; i++)
        {
            int index = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].onClick.AddListener(() => OnButtonClick(index));
        }
    }

    public void RemoveButtonListeners()
    {
        foreach (var button in buttons)
        {
            button.onClick.RemoveAllListeners();
        }
    }

    private void OnButtonClick(int buttonIndex)
    {
        if (buttonIndex < 2)
        {
            currentShopType = buttonIndex;
        }
        else
        {
            switch (currentShopType)
            {
                case 0:
                    if (shopItemInfoPosition.gameObject.activeSelf)
                    {
                        if (CharacterManager.PlayerCharacterData.gold < currentInfoItem.costValue * currentInfoItem.quantity)
                        {
                            GameStateMachine.Instance.ChangeState(GameSystemState.InfoMessage, "보유한 금액이 부족합니다.", true);
                            return;
                        }
                        ItemManager.Instance.PurchaseItem(currentInfoItem, currentInfoItem.quantity);
                    }
                    break;

                case 1:
                    if (shopItemInfoPosition.gameObject.activeSelf)
                    {
                        // 스택형이고 2개 이상이면 수량 선택 팝업 띄움
                        if (currentInfoItem.isStackable && currentInfoItem.quantity > 1)
                        {
                            sellQuantity = 1;
                            warningText.text = $"{sellQuantity}";
                            warningPopup.SetActive(true);
                            return;
                        }
                        else
                        {
                            // 스택 불가 or 수량 1개면 바로 판매
                            ItemManager.Instance.SellItem(currentInfoItem, shopNpcData.shopData.valueReductionRate, 1);
                        }
                    }
                    break;
            }
        }
        UpdateUI();
    }

    private void ClearShopList()
    {
        foreach (Transform t in shopItemListParent)
        {
            Destroy(t.gameObject);
        }
    }

    public void SetShopInfo(NPCData nPCData)
    {
        shopNpcData = nPCData;
        OnButtonClick(0);
    }

    private void ShowItemInfoClear()
    {
        currentInfoItem = null;
        shopItemInfoPosition.GetComponentInChildren<Image>().sprite = null;
        uiText[0].text = "";
        uiText[1].text = "";
        shopItemInfoPosition.gameObject.SetActive(false);
        buttons[2].gameObject.SetActive(false);
    }

    public void ShowItemInfo(int index, int currentType)
    {
        shopItemInfoPosition.gameObject.SetActive(true);
        currentInfoItem = currentType == 0 ? shopNpcData.shopData.availableItems[index] : InventoryManager.InventoryList[index];
        shopItemInfoPosition.GetComponentInChildren<Image>().sprite = currentInfoItem.sprite;
        uiText[0].text = currentInfoItem.name;
        uiText[1].text = currentInfoItem.ToStringTMPro();
        buttons[2].gameObject.SetActive(true);
        buttons[2].transform.GetComponentInChildren<TextMeshProUGUI>().text = currentType == 0 ? "구입" : "판매";
    }
   
}

using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static UnityEditor.MaterialProperty;

public class ShopitemUI : MonoBehaviour,IPointerClickHandler
{
    private Item thisItem;
    private ShopUI shopUI;
    private int currentItemIndex;
    private int currentShopType;

    private void ShowItem()
    {
        TextMeshProUGUI[] text = transform.GetComponentsInChildren<TextMeshProUGUI>();
        transform.GetChild(2).GetComponent<Image>().sprite = thisItem.sprite;
        text[0].text = thisItem.id;
        text[1].text = currentShopType == 0
      ? thisItem.costValue.ToString() // 구매 가격
      : Mathf.RoundToInt(thisItem.costValue * 0.7f).ToString();
    }

    public void SetItemInfo(Item item, ShopUI shopUI, int index, int shopType)
    {
        this.thisItem = item;
        this.shopUI = shopUI;
        this.currentItemIndex = index;
        this.currentShopType = shopType;
        ShowItem();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        shopUI.ShowItemInfo(currentItemIndex, currentShopType);
    }
}

using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [NonSerialized] public Item currentItem;
    [NonSerialized] public GameObject InventorytooltipWindow;
    [NonSerialized] public TextMeshProUGUI[] textPoint = new TextMeshProUGUI[3];
    [NonSerialized] public Image ItemImage;
    [NonSerialized] public TextMeshProUGUI[] amountCount;
    private int preAmountCount = 0;

    private string[] condition = { "Consumable001", "Consumable002" };

    private void Start()
    {
        InventorytooltipWindow.SetActive(false);
        amountCount = transform.GetComponentsInChildren<TextMeshProUGUI>();
        if (currentItem != null && currentItem.sprite != null)
        {
            this.transform.GetComponentsInChildren<Image>(true)[1].sprite = currentItem.sprite;
        }
        if (currentItem.isStackable)
        {
            preAmountCount = currentItem.quantity;
            amountCount[0].enabled = true;
            amountCount[0].text = $"{currentItem.quantity}";
        }
        else
        {
            amountCount[0].enabled = false;
        }
    }

    private void Update()
    {
        if (preAmountCount !=0 && preAmountCount != currentItem.quantity)
        {
            preAmountCount = currentItem.quantity;
            amountCount[0].text = $"{currentItem.quantity}";
        }

        if ((currentItem.id == condition[0] && InventoryManager.QuickSlotsUI.GetQuicSlot(0)) || 
            ( currentItem.id == condition[1] && InventoryManager.QuickSlotsUI.GetQuicSlot(1)))
        {

            amountCount[1].enabled = true;
            amountCount[1].text = "S";
        }
        else
        {
            amountCount[1].enabled = false;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem.type == ItemType.소모품 && (currentItem.id == condition[0] || currentItem.id == condition[1]))
        {
            var ditem = transform.GetComponent<DraggableItem>();
            if (ditem == null) { ditem = transform.AddComponent<DraggableItem>(); }
        }
        InventorytooltipWindow.SetActive(true);
        ItemImage.sprite = currentItem.sprite;
        textPoint[1].text = currentItem.name;
        textPoint[2].text = currentItem.ToStringTMPro();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventorytooltipWindow.SetActive(false);
    }

    public Item GetItem() => currentItem;
}

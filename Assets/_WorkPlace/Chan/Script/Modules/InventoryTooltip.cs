using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Item currentItem;
    [NonSerialized] public GameObject InventorytooltipWindow;
    [NonSerialized] public TextMeshProUGUI[] textPoint = new TextMeshProUGUI[3];
    [NonSerialized] public Image ItemImage;

    private void Start()
    {
        InventorytooltipWindow.SetActive(false);
        if (currentItem != null && currentItem.sprite != null)
        {
            this.transform.GetComponentsInChildren<Image>(true)[1].sprite = currentItem.sprite;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        InventorytooltipWindow.SetActive(true);
        ItemImage.sprite = currentItem.sprite;
        Debug.Log("rottt : " + textPoint.Length);
        textPoint[1].text = currentItem.name;
        textPoint[2].text = currentItem.ToStringTMPro();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        InventorytooltipWindow.SetActive(false);
    }
}

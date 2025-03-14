using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [NonSerialized] public Item currentItem;
    [NonSerialized] public GameObject InventorytooltipWindow;
    [NonSerialized] public TextMeshProUGUI[] textPoint = new TextMeshProUGUI[3];
    [NonSerialized] public Image ItemImage;
    [NonSerialized] public TextMeshProUGUI[] amountCount;
    [NonSerialized] public bool isEquireSlot = false;
    private int preAmountCount = 0;
    private string[] condition = { "소형 체력포션", "소형 마나포션" };
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;

    private void Start()
    {
        if (!isEquireSlot) InventorytooltipWindow.SetActive(false);
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
        else if (currentItem.isEquired)
        {
            amountCount[1].enabled = true;
            amountCount[1].text = "E";
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
            if (ditem == null) { ditem = transform.AddComponent<DraggableItem>(); } // 소형 포션 2종류일때 드래그어블 컴포넌트 추가 
        }
        if (!isEquireSlot)
        {
            InventorytooltipWindow.SetActive(true);
            ItemImage.sprite = currentItem.sprite;
            textPoint[1].text = currentItem.name;
            textPoint[2].text = currentItem.ToStringTMPro();
        }
    }

   /* #region 아이템 인포 위치 이동 버전

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isEquireSlot && !InventorytooltipWindow.activeSelf)
        {
            InventorytooltipWindow.SetActive(true);
            ItemImage.sprite = currentItem.sprite;
            textPoint[1].text = currentItem.name;
            textPoint[2].text = currentItem.ToStringTMPro();
            //  아이템 아이콘 위치 기준으로 툴팁 실행
            RectTransform itemRect = GetComponent<RectTransform>(); // 현재 아이템 슬롯 위치
            RectTransform tooltipRect = InventorytooltipWindow.GetComponent<RectTransform>(); //  툴팁 위치

            float offsetX = itemRect.sizeDelta.x + 10f;
            float offsetY = 0f; // 

            Vector2 tooltipPosition = new Vector2(itemRect.position.x + offsetX, itemRect.position.y + offsetY);

            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                InventorytooltipWindow.transform.parent as RectTransform,
                tooltipPosition,
                eventData.pressEventCamera,
                out anchoredPos
            );

            tooltipRect.anchoredPosition = anchoredPos;
        }
    }

    #endregion*/

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isEquireSlot) InventorytooltipWindow.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if(eventData.button == PointerEventData.InputButton.Right)
        {
            HandleRightClick(eventData);
            return;
        }

        float currentTime = Time.time; // 현재 시간
        if (currentTime - lastClickTime <= doubleClickThreshold)
        {
            // 더블 클릭 처리
            HandleDoubleClick(eventData);
        }
        else
        {
            // 싱글 클릭 처리
            HandleSingleClick(eventData);
        }

        lastClickTime = currentTime; // 클릭 시간 갱신
    }

    private void HandleSingleClick(PointerEventData eventData)
    {
        if (currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구 || currentItem.type == ItemType.장신구)
        {
            Debug.Log($"'{currentItem.name}' 아이템을 클릭했습니다.");
        }

        //03.12 HJ 추가
        InventoryManager.Instance.SetSelectedItem(currentItem);
    }

    private void HandleDoubleClick(PointerEventData eventData)
    {
        if (currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구 || currentItem.type == ItemType.장신구)
        {
            if (!currentItem.isEquired) ItemEffectManager.Instance.ApplyItemEffect(currentItem);
            else ItemEffectManager.Instance.UnequipmentEffect(currentItem);
        }
    }

    private void HandleRightClick(PointerEventData eventData)
    {
        if (currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구)
        {
            Debug.Log($"마우스 우클릭, 아이템 타입: {currentItem.type}");
            EnhanceManager.Instance.Enhance(currentItem, InventoryManager.Instance.selectedItem);
        }        
    }

    public Item GetItem() => currentItem;
}

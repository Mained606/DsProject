using UnityEngine;
using UnityEngine.EventSystems;

public class EnhanceDropZone : MonoBehaviour, IDropHandler
{
    [SerializeField] private EnhanceUI enhanceUI;

    public void OnDrop(PointerEventData eventData)
    {
        var tooltip = eventData.pointerDrag?.GetComponent<InventorySlotTooltip2>();
        var draggable = eventData.pointerDrag?.GetComponent<DraggableItem>();
        if (tooltip == null || draggable == null) return;

        Item droppedItem = tooltip.currentItem;
        if (droppedItem == null) return;

        if (droppedItem.isEquired) 
        {
            UIManager.SystemMessage("장착 중인 아이템은 강화할 수 없습니다.");
            return;
        }

        if (droppedItem.type != ItemType.무기 && droppedItem.type != ItemType.방어구)
        {
            UIManager.SystemMessage("무기 또는 방어구만 강화할 수 있습니다.");
            return;
        }

        enhanceUI.SetSelectedItem(droppedItem);
        draggable.DestroyDragImage(); 
        UIManager.SystemMessage($"'{droppedItem.name}' 강화 슬롯에 등록됨.");
    }
}

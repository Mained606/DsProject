using UnityEngine;
using UnityEngine.EventSystems;

public class PotDropZone : MonoBehaviour, IDropHandler
{
  //  [SerializeField] private CookingManager cookingManager; // CookingManager 참조

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggableItem == null) return;

        Item droppedItem = draggableItem.GetComponent<InventorySlotTooltip2>().currentItem;
        if (droppedItem == null || droppedItem.type != ItemType.요리재료) return;

        // 드랍될 때, 1개만 넘기기 위해 quantity를 1로 설정
        Item itemToAdd = droppedItem.Clone();
        itemToAdd.quantity = 1;  // 1개만 추가되도록 설정

        CookingManager.Instance.AddIngredient(itemToAdd); // CookingManager에 재료 추가
        draggableItem.DestroyDragImage();
        UIManager.CookingUI.UpdateUI();
        UIManager.CookingUI.UpdatePotSlotUI();
    }
}
using UnityEngine;
using UnityEngine.EventSystems;

public class PotDropZone : MonoBehaviour, IDropHandler
{
  //  [SerializeField] private CookingManager cookingManager; // CookingManager 참조

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggableItem == null) return;

        Item droppedItem = draggableItem.GetComponent<InventoryTooltip>().currentItem;
        if (droppedItem == null || droppedItem.type != ItemType.요리재료) return;

        CookingManager.Instance.AddIngredient(droppedItem); // CookingManager에 재료 추가

    }
}
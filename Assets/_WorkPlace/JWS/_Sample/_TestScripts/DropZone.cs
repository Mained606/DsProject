using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler
{
    public GameObject Menu;

    public void OnDrop(PointerEventData eventData)
    {
        GameObject droppedObject = eventData.pointerDrag;

        if (droppedObject != null && droppedObject.GetComponent<DraggableItem>() != null)
        {
            RectTransform rect = droppedObject.GetComponent<RectTransform>();
            if (rect != null)
            {
                droppedObject.transform.SetParent(Menu.transform, false);
                LayoutRebuilder.ForceRebuildLayoutImmediate(Menu.GetComponent<RectTransform>());
                rect.anchoredPosition = Vector2.zero;
                droppedObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
                droppedObject.SetActive(false);
            }
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DeDropZone : MonoBehaviour, IDropHandler
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
                droppedObject.GetComponent<CanvasGroup>().blocksRaycasts = true;
                rect.anchoredPosition = Vector2.zero;
            }
        }
    }
}

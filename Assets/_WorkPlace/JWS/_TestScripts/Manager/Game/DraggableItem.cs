using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;
    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;
    private GameObject dragImageObject;
    private Image dragImage;
    private Vector2 originalPosition;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas.renderMode != RenderMode.ScreenSpaceOverlay && mainCanvas.worldCamera == null)
        {
            mainCanvas.worldCamera = Camera.main;
        }
        canvasRectTransform = mainCanvas.GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (mainCanvas == null) return;
        originalPosition = rectTransform.anchoredPosition;
        canvasGroup.blocksRaycasts = false;
        CreateDragImage();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (mainCanvas == null || dragImageObject == null) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mainCanvas.transform as RectTransform,
            eventData.position,
            mainCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCanvas.worldCamera,
            out Vector2 localPoint
        );
        dragImageObject.GetComponent<RectTransform>().anchoredPosition = localPoint;
    }


    public void OnEndDrag(PointerEventData eventData)
    {
        if (mainCanvas == null) return;

        canvasGroup.blocksRaycasts = true;
        DestroyDragImage();
        if (!eventData.pointerEnter || eventData.pointerEnter.GetComponent<DropZone>() == null)
        {
            rectTransform.anchoredPosition = originalPosition;
        }
    }

    private void CreateDragImage()
    {
        dragImageObject = new GameObject("Drag Image", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        dragImageObject.transform.SetParent(mainCanvas.transform, false);
        RectTransform dragRect = dragImageObject.GetComponent<RectTransform>();
        dragRect.sizeDelta = rectTransform.sizeDelta;
        dragRect.anchorMin = Vector2.zero;
        dragRect.anchorMax = Vector2.zero;
        dragRect.pivot = rectTransform.pivot;
        dragRect.localScale = rectTransform.lossyScale;
        dragImage = dragImageObject.GetComponent<Image>();
        dragImage.sprite = transform.GetChild(0).GetChild(0).GetComponent<Image>().sprite;
        dragImage.color = new Color(1, 1, 1, 0.6f);
        CanvasGroup dragCanvasGroup = dragImageObject.GetComponent<CanvasGroup>();
        dragCanvasGroup.blocksRaycasts = false;
    }

    private void DestroyDragImage()
    {
        if (dragImageObject != null)
        {
            Destroy(dragImageObject);
            dragImageObject = null;
        }
    }
}

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Skills SkillData { get; private set; }
    public Sprite Icon { get; private set; }

    private CanvasGroup canvasGroup;
    private Canvas mainCanvas;

    private GameObject dragImageObject;
    private Image dragImage;

    public void Initialize(Skills skillData, Sprite icon)
    {
        SkillData = skillData;
        Icon = icon;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }

    private void Awake()
    {
        mainCanvas = GetComponentInParent<Canvas>();
        if (mainCanvas.renderMode != RenderMode.ScreenSpaceOverlay && mainCanvas.worldCamera == null)
        {
            mainCanvas.worldCamera = Camera.main;
        }

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (mainCanvas == null) return;

        canvasGroup.blocksRaycasts = false;
        CreateDragImage();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragImageObject != null)
        {
            dragImageObject.GetComponent<RectTransform>().position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        DestroyDragImage();
    }

    private void CreateDragImage()
    {
        dragImageObject = new GameObject("SkillDragImage", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        dragImageObject.transform.SetParent(mainCanvas.transform, false);

        dragImage = dragImageObject.GetComponent<Image>();
        dragImage.sprite = Icon;
        dragImage.raycastTarget = false;
        dragImage.color = new Color(1, 1, 1, 0.6f);

        CanvasGroup dragCanvasGroup = dragImageObject.GetComponent<CanvasGroup>();
        dragCanvasGroup.blocksRaycasts = false;

        // 🔥 핵심: 드래그 정보 복사
        SkillDrag clone = dragImageObject.AddComponent<SkillDrag>();
        clone.SkillData = this.SkillData;
        clone.Icon = this.Icon;
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

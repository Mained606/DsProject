using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class SkillDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Skills SkillData { get; private set; }
    public Sprite Icon { get; private set; }

    private Image image;
    private CanvasGroup canvasGroup;
    private Transform originalParent;
    private Vector2 originalPosition;

    public void Initialize(Skills skillData, Sprite icon)
    {
        SkillData = skillData;
        Icon = icon;

        if (image == null) image = GetComponent<Image>();
        image.sprite = icon;
        image.enabled = true;
    }

    private void Awake()
    {
        image = GetComponent<Image>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalPosition = transform.position;
        transform.SetParent(transform.root); // 맨 위로 올림
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        transform.position = originalPosition;
        canvasGroup.blocksRaycasts = true;
    }
}
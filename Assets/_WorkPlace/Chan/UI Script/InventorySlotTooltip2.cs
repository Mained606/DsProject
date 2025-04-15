using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class InventorySlotTooltip2 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [NonSerialized] public Item currentItem;
    [NonSerialized] public GameObject InventorytooltipWindow;
    [NonSerialized] public TextMeshProUGUI[] textPoint = new TextMeshProUGUI[2];
    [NonSerialized] public Image ItemImage;
    [NonSerialized] public Image ElementIcon;
    [NonSerialized] public TextMeshProUGUI ItemLevel;
    [NonSerialized] public TextMeshProUGUI ItemLevelPreview;

    [NonSerialized] public TextMeshProUGUI[] amountCount;
    [NonSerialized] public bool isEquireSlot = false;
    private int preAmountCount = 0;
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;
    private Image ElenmtSlotIcon;

    private void Start()
    {
        if (!isEquireSlot) InventorytooltipWindow.SetActive(false);
        amountCount = transform.GetComponentsInChildren<TextMeshProUGUI>();

        if (currentItem != null && currentItem.sprite != null)
        {
            this.transform.GetComponentsInChildren<Image>(true)[2].sprite = currentItem.sprite;
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

        string nameColor = currentItem.GetGradeColor(currentItem.grade);
        amountCount[2].text = $"<color={nameColor}>{currentItem.name}</color>";

        ElenmtSlotIcon = GetComponentsInChildren<Image>()[3];
        if (ElenmtSlotIcon != null)
        {
            // 장비인지, 그리고 속성이 있는지 확인
            if ((currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구) &&
                currentItem.itemSkill != null && currentItem.itemSkill.element != ElementalAttribute.None)
            {
                ElenmtSlotIcon.gameObject.SetActive(false); // 로드 중에는 일단 꺼둠

                Addressables.LoadAssetAsync<Sprite>(currentItem.itemSkill.element.ToString()).Completed += (handle) =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        ElenmtSlotIcon.sprite = handle.Result;
                        ElenmtSlotIcon.gameObject.SetActive(true);
                    }
                    else
                    {
                        ElenmtSlotIcon.gameObject.SetActive(false);
                    }
                };
            }
            else
            {
                ElenmtSlotIcon.gameObject.SetActive(false);
            }
        }

        UpdateSlotLevel();
    }

    private void Update()
    {
        if (preAmountCount != 0 && preAmountCount != currentItem.quantity)
        {
            preAmountCount = currentItem.quantity;
            amountCount[0].text = $"{currentItem.quantity}";
        }

        bool isInQuickSlot = false;

        var quickSlots = InventoryManager.QuickSlotsUI?.GetSlots();
        if (quickSlots != null)
        {
            foreach (var slot in quickSlots)
            {
                if (slot.GetItem() == currentItem)
                {
                    isInQuickSlot = true;
                    break;
                }
            }
        }

        bool isEquipped = currentItem.isEquired;

        if (isInQuickSlot)
        {
            amountCount[1].enabled = true;
            amountCount[1].text = "S";
        }
        else if (isEquipped)
        {
            amountCount[1].enabled = true;
            amountCount[1].text = "E";
        }
        else
        {
            amountCount[1].enabled = false;
        }
    }
    private void UpdateSlotLevel()
    {
        if ((currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구) && currentItem.itemSkill != null)
        {
            int level = currentItem.itemSkill.Level;
            amountCount[3].enabled = true;
            amountCount[3].text = $"+{level}";

            Color parsedColor;
            if (level <= 2)
                amountCount[3].color = Color.white;
            else if (level <= 5 && ColorUtility.TryParseHtmlString("#A6D8F1", out parsedColor))
                amountCount[3].color = parsedColor;
            else if (level <= 9 && ColorUtility.TryParseHtmlString("#D4A6F1", out parsedColor))
                amountCount[3].color = parsedColor;
            else if (level >= 10 && ColorUtility.TryParseHtmlString("#FFD700", out parsedColor))
                amountCount[3].color = parsedColor;
        }
        else
        {
            amountCount[3].enabled = false;
        }
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isEquireSlot && !InventorytooltipWindow.activeSelf)
        {
            // 드래그 가능 판단
            if (currentItem.type == ItemType.소모품 ||
                currentItem.type == ItemType.요리재료 ||
                currentItem.type == ItemType.무기 ||
                currentItem.type == ItemType.요리||
                currentItem.type == ItemType.방어구)
            {
                if (!TryGetComponent(out DraggableItem ditem))
                    gameObject.AddComponent<DraggableItem>();
            }

            // 기본 인포 표시
            InventorytooltipWindow.SetActive(true);
            ItemImage.sprite = currentItem.sprite;

            string nameColor = currentItem.GetGradeColor(currentItem.grade);
            string coloredName = $"<color={nameColor}>{currentItem.name}</color>";
            textPoint[1].text = coloredName;
            textPoint[2].text = currentItem.ToStringTMPro();

            // 속성 아이콘 처리
            if (currentItem.itemSkill != null)
            {
                ElementalAttribute attr = currentItem.itemSkill.element;
                if (attr == ElementalAttribute.None)
                {
                    ElementIcon.gameObject.SetActive(false);
                }
                else
                {
                    ElementIcon.gameObject.SetActive(false);
                    Addressables.LoadAssetAsync<Sprite>(attr.ToString()).Completed += (handle) =>
                    {
                        if (handle.Status == AsyncOperationStatus.Succeeded)
                        {
                            Sprite icon = handle.Result;
                            ElementIcon.sprite = icon;
                            ElementIcon.gameObject.SetActive(true);
                        }
                        else
                        {
                            Debug.LogWarning($"[툴팁] '{attr}' 속성 아이콘 로드 실패 (Address: {attr})");
                        }
                    };
                }
            }
            else
            {
                ElementIcon.gameObject.SetActive(false);
            }

            // 강화 레벨 처리
            if ((currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구) && currentItem.itemSkill != null)
            {
                int level = currentItem.itemSkill.Level;
                ItemLevel.gameObject.SetActive(true);
                ItemLevel.text = $"+{level}";

                Color parsedColor;
                if (level <= 2)
                    ItemLevel.color = Color.white;
                else if (level <= 5 && ColorUtility.TryParseHtmlString("#A6D8F1", out parsedColor))
                    ItemLevel.color = parsedColor;
                else if (level <= 9 && ColorUtility.TryParseHtmlString("#D4A6F1", out parsedColor))
                    ItemLevel.color = parsedColor;
                else if (level >= 10 && ColorUtility.TryParseHtmlString("#FFD700", out parsedColor))
                    ItemLevel.color = parsedColor;
            }
            else
            {
                ItemLevel.gameObject.SetActive(false);
            }

            RectTransform itemRect = GetComponent<RectTransform>();
            RectTransform tooltipRect = InventorytooltipWindow.GetComponent<RectTransform>();
            RectTransform canvasRect = tooltipRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

            tooltipRect.position = itemRect.position;

            Vector3[] tooltipCorners = new Vector3[4];
            tooltipRect.GetWorldCorners(tooltipCorners);

            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);

            float tooltipBottomY = tooltipCorners[0].y;
            float canvasBottomY = canvasCorners[0].y;

            if (tooltipBottomY < canvasBottomY)
            {
                Vector3 newPosition = tooltipRect.position;
                float offset = canvasBottomY - tooltipBottomY;
                newPosition.y += offset;
                tooltipRect.position = newPosition;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isEquireSlot) InventorytooltipWindow.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        float currentTime = Time.time;
        if (currentTime - lastClickTime <= doubleClickThreshold)
        {
            HandleDoubleClick(eventData);
        }
        else
        {
            HandleSingleClick(eventData);
        }

        lastClickTime = currentTime;
    }

    private void HandleSingleClick(PointerEventData eventData)
    {
        if (currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구 || currentItem.type == ItemType.장신구)
        {
            Debug.Log($"'{currentItem.name}' 아이템을 클릭했습니다.");
        }
    }

    private void HandleDoubleClick(PointerEventData eventData)
    {
        if (currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구 || currentItem.type == ItemType.장신구)
        {
            if (!currentItem.isEquired) ItemEffectManager.Instance.ApplyItemEffect(currentItem);
            else ItemEffectManager.Instance.UnequipmentEffect(currentItem);
        }
    }

    public Item GetItem() => currentItem;
}

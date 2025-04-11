using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class InventorySlotTooltip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [NonSerialized] public Item currentItem;
    [NonSerialized] public GameObject InventorytooltipWindow;
    [NonSerialized] public TextMeshProUGUI[] textPoint = new TextMeshProUGUI[2];
    [NonSerialized] public Image ItemImage;
    [NonSerialized] public Image ElementIcon;
    [NonSerialized] public TextMeshProUGUI ItemLevel;

    [NonSerialized] public TextMeshProUGUI[] amountCount;
    [NonSerialized] public bool isEquireSlot = false;
    private int preAmountCount = 0;
    private string[] condition = { "소형 체력포션", "소형 마나포션" };

    private float lastClickTime = -1f;
    private Item lastClickedItem = null;
    private const float doubleClickThreshold = 0.3f;

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
    }

    private void Update()
    {
        if (preAmountCount != 0 && preAmountCount != currentItem.quantity)
        {
            preAmountCount = currentItem.quantity;
            amountCount[0].text = $"{currentItem.quantity}";
        }

        bool isInQuickSlot =
            (currentItem.id == condition[0] && InventoryManager.QuickSlotsUI.GetQuicSlot(0)) ||
            (currentItem.id == condition[1] && InventoryManager.QuickSlotsUI.GetQuicSlot(1));

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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!IsEquipable(currentItem)) return;

        float currentTime = Time.time;
        float timeSinceLastClick = currentTime - lastClickTime;

        bool isSameItem = lastClickedItem == currentItem;

        if (isSameItem && timeSinceLastClick <= doubleClickThreshold)
        {
            HandleDoubleClick(eventData);
            ResetClickState();
        }
        else
        {
            lastClickedItem = currentItem;
            lastClickTime = currentTime;
        }
    }

    private void ResetClickState()
    {
        lastClickTime = -1f;
        lastClickedItem = null;
    }

    private bool IsEquipable(Item item)
    {
        return item.type == ItemType.무기 || item.type == ItemType.방어구 || item.type == ItemType.장신구;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem.type == ItemType.소모품 && (currentItem.id == condition[0] || currentItem.id == condition[1]))
        {
            if (!TryGetComponent(out DraggableItem ditem))
            {
                gameObject.AddComponent<DraggableItem>();
            }
        }

        if (!isEquireSlot)
        {
            InventorytooltipWindow.SetActive(true);
            ItemImage.sprite = currentItem.sprite;
            textPoint[1].text = currentItem.id;
            textPoint[2].text = currentItem.ToStringTMPro();
        }

        if (currentItem.type == ItemType.요리재료 ||
            currentItem.type == ItemType.무기 ||
            currentItem.type == ItemType.방어구)
        {
            if (!TryGetComponent(out DraggableItem ditem))
            {
                gameObject.AddComponent<DraggableItem>();
            }
        }

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

        InventorytooltipWindow.SetActive(true);
        ItemImage.sprite = currentItem.sprite;

        string nameColor = currentItem.GetGradeColor(currentItem.grade);
        string coloredName = $"<color={nameColor}>{currentItem.name}</color>";
        textPoint[1].text = coloredName;
        textPoint[2].text = currentItem.ToStringTMPro();

        if ((currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구) && currentItem.itemSkill != null)
        {
            int level = currentItem.itemSkill.Level;

            ItemLevel.gameObject.SetActive(true);
            ItemLevel.text = $"LV.{level}";

            Color parsedColor;
            if (level <= 2)
            {
                ItemLevel.color = Color.white;
            }
            else if (level <= 5)
            {
                if (ColorUtility.TryParseHtmlString("#A6D8F1", out parsedColor))
                    ItemLevel.color = parsedColor;
            }
            else if (level <= 9)
            {
                if (ColorUtility.TryParseHtmlString("#D4A6F1", out parsedColor))
                    ItemLevel.color = parsedColor;
            }
            else if (level >= 10)
            {
                if (ColorUtility.TryParseHtmlString("#FFD700", out parsedColor))
                    ItemLevel.color = parsedColor;
            }
        }
        else
        {
            ItemLevel.gameObject.SetActive(false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isEquireSlot) InventorytooltipWindow.SetActive(false);
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

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
    [NonSerialized] public TextMeshProUGUI ItemSlotLevel;
    [NonSerialized] public TextMeshProUGUI[] amountCount;
    [NonSerialized] public bool isEquireSlot = false;

    private Image ElenmtSlotIcon;
    private int preAmountCount = 0;

    private void Start()
    {
        if (!isEquireSlot) InventorytooltipWindow.SetActive(false);

        amountCount = transform.GetComponentsInChildren<TextMeshProUGUI>();

        if (currentItem != null && currentItem.sprite != null)
        {
            GetComponentsInChildren<Image>(true)[2].sprite = currentItem.sprite;
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
        if ((currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구) &&
            currentItem.itemSkill != null && currentItem.itemSkill.element != ElementalAttribute.None)
        {
            ElenmtSlotIcon.gameObject.SetActive(false);
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

        foreach (var slot in InventoryManager.QuickSlotsUI.GetSlots())
        {
            if (slot.GetItem() == currentItem)
            {
                isInQuickSlot = true;
                break;
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
        if (currentItem.type == ItemType.소모품 ||
            currentItem.type == ItemType.요리재료 ||
            currentItem.type == ItemType.무기 ||
            currentItem.type == ItemType.요리 ||
            currentItem.type == ItemType.방어구)
        {
            if (!TryGetComponent(out DraggableItem ditem))
                gameObject.AddComponent<DraggableItem>();
        }

        if (!isEquireSlot)
        {
            InventorytooltipWindow.SetActive(true);
            
            if (currentItem == null)
            {
                InventorytooltipWindow.SetActive(false);
                return;
            }
            
            ItemImage.sprite = currentItem.sprite;

            string nameColor = currentItem.GetGradeColor(currentItem.grade);
            textPoint[1].text = $"<color={nameColor}>{currentItem.name}</color>";
            
            if (currentItem.type == ItemType.요리 && 
                (currentItem.itemStat == null || (currentItem.itemStat.HealHp <= 0 && currentItem.itemStat.HealMp <= 0)))
            {
                Item originalItem = ItemManager.Instance?.GetItemById(currentItem.id);
                if (originalItem != null && originalItem.itemStat != null)
                {
                    Item tempItem = currentItem.Clone();
                    tempItem.itemStat = originalItem.itemStat.Clone();
                    tempItem.effect = originalItem.effect?.Clone() ?? tempItem.effect;
                    textPoint[2].text = tempItem.ToStringTMPro();
                }
                else
                {
                    textPoint[2].text = currentItem.ToStringTMPro();
                }
            }
            else
            {
                textPoint[2].text = currentItem.ToStringTMPro();
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
                        ElementIcon.sprite = handle.Result;
                        ElementIcon.gameObject.SetActive(true);
                    }
                };
            }
        }
        else
        {
            ElementIcon.gameObject.SetActive(false);
        }

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
            else if (level <= 8 && ColorUtility.TryParseHtmlString("#D4A6F1", out parsedColor))
                ItemLevel.color = parsedColor;
            else if (level >= 9 && ColorUtility.TryParseHtmlString("#FFD700", out parsedColor))
                ItemLevel.color = parsedColor;
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

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Right) return;

        if (currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구 || currentItem.type == ItemType.장신구)
        {
            if (!currentItem.isEquired)
                ItemEffectManager.Instance.ApplyItemEffect(currentItem);
            else
                ItemEffectManager.Instance.UnequipmentEffect(currentItem);
        }
    }

    public Item GetItem() => currentItem;
}

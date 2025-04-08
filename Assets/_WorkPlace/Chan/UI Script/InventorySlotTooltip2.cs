using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class InventorySlotTooltip2 : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [NonSerialized] public Item currentItem;

    [Header("툴팁 패널 바인딩")]
    [SerializeField] private GameObject tooltipWindow;
    [SerializeField] private TextMeshProUGUI[] textPoint; // [1] 이름, [2] 설명
    [SerializeField] private Image itemImage;
    [SerializeField] private Image elementIcon;
    [SerializeField] private TextMeshProUGUI itemLevel;

    private string[] quickSlotCondition = { "소형 체력포션", "소형 마나포션" };

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem == null) return;

        // 툴팁 활성화
        tooltipWindow.SetActive(true);

        // 아이템 아이콘
        if (itemImage != null)
            itemImage.sprite = currentItem.sprite;

        // 이름 + 등급 컬러
        string nameColor = currentItem.GetGradeColor(currentItem.grade);
        string coloredName = $"<color={nameColor}>{currentItem.name}</color>";
        if (textPoint.Length > 1)
            textPoint[1].text = coloredName;

        // 설명
        if (textPoint.Length > 2)
            textPoint[2].text = currentItem.ToStringTMPro();

        // 속성 아이콘 처리
        if (currentItem.itemSkill != null)
        {
            ElementalAttribute attr = currentItem.itemSkill.element;

            if (attr == ElementalAttribute.None)
            {
                elementIcon.gameObject.SetActive(false);
            }
            else
            {
                elementIcon.gameObject.SetActive(false);
                Addressables.LoadAssetAsync<Sprite>(attr.ToString()).Completed += (handle) =>
                {
                    if (handle.Status == AsyncOperationStatus.Succeeded)
                    {
                        elementIcon.sprite = handle.Result;
                        elementIcon.gameObject.SetActive(true);
                    }
                };
            }
        }
        else
        {
            elementIcon.gameObject.SetActive(false);
        }

        // 강화 레벨 표시
        if ((currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구) && currentItem.itemSkill != null)
        {
            int level = currentItem.itemSkill.Level;
            itemLevel.gameObject.SetActive(true);
            itemLevel.text = $"LV.{level}";

            Color parsedColor;
            if (level <= 2)
                itemLevel.color = Color.white;
            else if (level <= 5 && ColorUtility.TryParseHtmlString("#A6D8F1", out parsedColor))
                itemLevel.color = parsedColor;
            else if (level <= 9 && ColorUtility.TryParseHtmlString("#D4A6F1", out parsedColor))
                itemLevel.color = parsedColor;
            else if (level >= 10 && ColorUtility.TryParseHtmlString("#FFD700", out parsedColor))
                itemLevel.color = parsedColor;
        }
        else
        {
            itemLevel.gameObject.SetActive(false);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipWindow != null)
            tooltipWindow.SetActive(false);
    }
}

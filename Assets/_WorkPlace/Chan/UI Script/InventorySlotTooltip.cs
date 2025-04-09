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
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;

    private void Start()
    {
        if (!isEquireSlot) InventorytooltipWindow.SetActive(false);
        amountCount = transform.GetComponentsInChildren<TextMeshProUGUI>();
        if (currentItem != null && currentItem.sprite != null)
        {
            this.transform.GetComponentsInChildren<Image>(true)[3].sprite = currentItem.sprite;
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

        if ((currentItem.id == condition[0] && InventoryManager.QuickSlotsUI.GetQuicSlot(0)) ||
            (currentItem.id == condition[1] && InventoryManager.QuickSlotsUI.GetQuicSlot(1)))
        {
            amountCount[1].enabled = true;
            amountCount[1].text = "S";
        }
        else if (currentItem.isEquired)
        {
            amountCount[1].enabled = true;
            amountCount[1].text = "E";
        }
        else
        {
            amountCount[1].enabled = false;
        }
    }


    // 기존 인포판넬 로직
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 특정 아이템(포션 등)에 드래그 가능 컴포넌트 추가
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
            textPoint[1].text = currentItem.name;
            textPoint[2].text = currentItem.ToStringTMPro();
        }

        // 요리재료, 무기, 방어구도 드래그 가능
        if (currentItem.type == ItemType.요리재료 ||
            currentItem.type == ItemType.무기 ||
            currentItem.type == ItemType.방어구)
        {
            if (!TryGetComponent(out DraggableItem ditem))
            {
                gameObject.AddComponent<DraggableItem>();
            }
        }

        // 아이템 속성 아이콘 처리 (Addressables 방식)
        if (currentItem.itemSkill != null)
        {
            ElementalAttribute attr = currentItem.itemSkill.element;

            if (attr == ElementalAttribute.None)
            {
                ElementIcon.gameObject.SetActive(false);
            }
            else
            {
                ElementIcon.gameObject.SetActive(false); // 기본으로 꺼두고
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

        // 툴팁창 기본 정보
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

    #region 아이템 인포 위치 이동 버전

    /*public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isEquireSlot && !InventorytooltipWindow.activeSelf)
        {
            // 특정 아이템(포션 등)에 드래그 가능 컴포넌트 추가
            if (currentItem.type == ItemType.소모품 && (currentItem.id == condition[0] || currentItem.id == condition[1]))
            {
                if (!TryGetComponent(out DraggableItem ditem))
                {
                    gameObject.AddComponent<DraggableItem>();
                }
            }
            // 요리재료도 추가
            if (currentItem.type == ItemType.요리재료)
            {
                if (!TryGetComponent(out DraggableItem ditem))
                {
                    gameObject.AddComponent<DraggableItem>();
                }
            }
            if (currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구)
            {
                if (!TryGetComponent(out DraggableItem ditem))
                {
                    gameObject.AddComponent<DraggableItem>();
                }
            }
            // 툴팁 창 활성화 및 정보 업데이트
            InventorytooltipWindow.SetActive(true);
            ItemImage.sprite = currentItem.sprite;
            textPoint[1].text = currentItem.id;
            textPoint[2].text = currentItem.ToStringTMPro();

            RectTransform itemRect = GetComponent<RectTransform>();
            RectTransform tooltipRect = InventorytooltipWindow.GetComponent<RectTransform>();
            RectTransform canvasRect = tooltipRect.GetComponentInParent<Canvas>().GetComponent<RectTransform>();

            // 기본 위치 적용
            tooltipRect.position = itemRect.position;

            // 화면 밖으로 나가는지 체크 후 조정
            Vector3[] tooltipCorners = new Vector3[4];
            tooltipRect.GetWorldCorners(tooltipCorners);

            Vector3[] canvasCorners = new Vector3[4];
            canvasRect.GetWorldCorners(canvasCorners);

            float tooltipBottomY = tooltipCorners[0].y; // 툴팁 아래쪽 모서리 Y 좌표
            float canvasBottomY = canvasCorners[0].y; // 캔버스 아래쪽 경계 Y 좌표

            if (tooltipBottomY < canvasBottomY)
            {
                Vector3 newPosition = tooltipRect.position;
                float offset = canvasBottomY - tooltipBottomY; // 부족한 거리 계산
                newPosition.y += offset; // 캔버스 아래줄과 맞춰서 올리기
                tooltipRect.position = newPosition;
            }
        }
    }*/

    #endregion

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isEquireSlot) InventorytooltipWindow.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ////강화 테스트용
        //if(eventData.button == PointerEventData.InputButton.Right)
        //{
        //    HandleRightClick(eventData);
        //    return;
        //}

        float currentTime = Time.time; // 현재 시간
        if (currentTime - lastClickTime <= doubleClickThreshold)
        {
            // 더블 클릭 처리
            HandleDoubleClick(eventData);
        }
        else
        {
            // 싱글 클릭 처리
            HandleSingleClick(eventData);
        }

        lastClickTime = currentTime; // 클릭 시간 갱신
    }

    private void HandleSingleClick(PointerEventData eventData)
    {
        if (currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구 || currentItem.type == ItemType.장신구)
        {
            Debug.Log($"'{currentItem.name}' 아이템을 클릭했습니다.");
        }

        //03.12 HJ 추가
        //  InventoryManager.Instance.SetSelectedItem(currentItem);
    }

    private void HandleDoubleClick(PointerEventData eventData)
    {
        if (currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구 || currentItem.type == ItemType.장신구)
        {
            if (!currentItem.isEquired) ItemEffectManager.Instance.ApplyItemEffect(currentItem);
            else ItemEffectManager.Instance.UnequipmentEffect(currentItem);
        }
    }

    //강화 테스트용
    //private void HandleRightClick(PointerEventData eventData)
    //{
    //    if (currentItem.type == ItemType.무기 || currentItem.type == ItemType.방어구)
    //    {
    //        Debug.Log($"마우스 우클릭, 아이템 타입: {currentItem.type}");
    //        EnhanceManager.Instance.Enhance(currentItem, InventoryManager.Instance.selectedItem);
    //    }        
    //}

    public Item GetItem() => currentItem;
}

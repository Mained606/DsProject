using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler
{
    [Header("UI 바인딩")]
    [SerializeField] private Image iconImage;
    [SerializeField] private Image cooldownOverlay;
    [SerializeField] private TextMeshProUGUI quantityText;

    [Header("사용 키 설정")]
    [SerializeField] private KeyCode useKey = KeyCode.F1;

    private Item currentItem;
    private BasicTimer cooldown;

    private void Awake()
    {
        ResetSlot();
    }

    private void Update()
    {
        // 쿨타임 시각화
        if (cooldown?.IsRunning == true && cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = cooldown.RemainingPercent;
        }
        if (iconImage != null)
        {
            iconImage.color = cooldown?.IsRunning == true
                ? new Color(0.5f, 0.5f, 0.5f, 1f) // 어두운 회색
                : new Color(1f, 1f, 1f, 1f);      // 원래 색
        }
        // 단축키 입력
        if (Input.GetKeyDown(useKey))
        {
            UseItem();
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        // 슬롯툴팁1 또는 2 중 하나라도 있으면 통과
        var tooltip = eventData.pointerDrag?.GetComponent<InventorySlotTooltip>();
        var tooltip2 = eventData.pointerDrag?.GetComponent<InventorySlotTooltip2>();

        Item item = null;

        if (tooltip != null)
            item = tooltip.GetItem();
        else if (tooltip2 != null)
            item = tooltip2.currentItem;
        else
            return;

        if (item == null || (item.type != ItemType.소모품 && item.type != ItemType.요리)) return;

        // 중복 제거
        foreach (var slot in InventoryManager.QuickSlotsUI.GetSlots())
        {
            if (slot != this && slot.GetItem() == item)
            {
                slot.ResetSlot();
            }
        }

        SetItem(item);
    }

    public void SetItem(Item item)
    {
        currentItem = item;

        cooldown ??= new BasicTimer(10f);

        // 아이콘 표시
        iconImage.sprite = item.sprite;
        iconImage.enabled = true;
        iconImage.preserveAspect = true;
        var c = iconImage.color; c.a = 1f; iconImage.color = c;

        // 쿨타임 오버레이 초기화
        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.enabled = true;
        }

        // 수량 표시
        quantityText.text = InventoryManager.Instance.GetItemQuantity(item.id).ToString();
        quantityText.transform.parent.gameObject.SetActive(true);
    }

    private void UseItem()
    {
        if (currentItem == null || cooldown?.IsRunning == true) return;

        ItemManager.Instance.UseItem(currentItem);
        TimerManager.Instance.StartTimer(cooldown);

        int qty = InventoryManager.Instance.GetItemQuantity(currentItem.id);
        quantityText.text = qty.ToString();

        // 인벤토리에 없으면 즉시 리셋, 아니면 유지
        if (!InventoryManager.Instance.HasItem(currentItem.id))
        {
            ResetSlot();
        }
    }

    public void ResetSlot()
    {
        currentItem = null;

        iconImage.sprite = null;
        iconImage.enabled = false;
        var c = iconImage.color; c.a = 0f; iconImage.color = c;

        if (cooldownOverlay != null)
        {
            cooldownOverlay.fillAmount = 0f;
            cooldownOverlay.enabled = false;
        }

        quantityText.text = string.Empty;
        quantityText.transform.parent.gameObject.SetActive(false);

        cooldown = null;
    }

    public Item GetItem() => currentItem;

    // 외부에서 수량 텍스트만 업데이트할 수 있는 메서드 추가
    public void UpdateQuantityText(int quantity)
    {
        if (currentItem == null) return;
        
        quantityText.text = quantity.ToString();
        
        // 수량이 0인 경우 슬롯 리셋
        if (quantity <= 0)
        {
            ResetSlot();
        }
    }
}

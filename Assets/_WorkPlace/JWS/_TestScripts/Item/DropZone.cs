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
        var tooltip = eventData.pointerDrag?.GetComponent<InventorySlotTooltip>();
        if (tooltip == null) return;

        var item = tooltip.GetItem();
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
}

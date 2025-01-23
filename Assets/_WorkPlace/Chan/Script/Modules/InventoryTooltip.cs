using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InventoryTooltip : MonoBehaviour
{
    public Item currentitem;
    private Button Button;

    private void OnEnable()
    {
        Button = GetComponent<Button>();

        // 버튼 호버 이벤트 등록
        EventTrigger trigger = Button.gameObject.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = Button.gameObject.AddComponent<EventTrigger>();
        }
        trigger.triggers.Clear();

        // PointerEnter (마우스 오버)
        EventTrigger.Entry pointerEnter = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        pointerEnter.callback.AddListener((data) => UIManager.Instance.ToggleInventorytooltipWindow(currentitem, true));
        trigger.triggers.Add(pointerEnter);

        // PointerExit (마우스 벗어남)
        EventTrigger.Entry pointerExit = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        pointerExit.callback.AddListener((data) => UIManager.Instance.ToggleInventorytooltipWindow(currentitem, false));
        trigger.triggers.Add(pointerExit);
    }
}

using UnityEngine;
using UnityEngine.EventSystems;

public class MouseOverDisplayToolTip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public string itemInfo;  // 아이템 정보 텍스트
    private ToolTipWindow tooltip; // 툴팁 스크립트 연결

    void Start()
    {
        tooltip = FindObjectOfType<ToolTipWindow>();  // Tooltip 스크립트 찾기
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltip.ShowTooltip(itemInfo);  // 마우스가 아이템에 들어오면 툴팁 표시
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltip.HideTooltip();  // 마우스가 아이템에서 나가면 툴팁 숨기기
    }
}

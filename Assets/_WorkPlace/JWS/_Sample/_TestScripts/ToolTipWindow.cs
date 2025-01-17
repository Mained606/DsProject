using UnityEngine;
using TMPro;

public class ToolTipWindow : MonoBehaviour
{
    public GameObject tooltipPanel;   // 툴팁 패널 오브젝트
    public TextMeshProUGUI tooltipText;          // 툴팁에 표시할 텍스트
    public RectTransform backgroundRect; // 툴팁 패널 RectTransform
    public float toolTipWindowSpacing = 15f;
    public string toolTipContents;
    public Vector3 offset;


    private void Start()
    {
        tooltipPanel.SetActive(false);
        tooltipPanel.transform.localScale = Vector3.one;  // 기본 스케일로 설정
        offset = Vector3.zero;
        ShowTooltip(toolTipContents);
    }

    void Update()
    {
        FollowMousePoint();
    }

    public void FollowMousePoint()
    {
        Vector2 newSize = new Vector2(tooltipText.preferredWidth + toolTipWindowSpacing, tooltipText.preferredHeight + toolTipWindowSpacing);
        backgroundRect.sizeDelta = newSize;

        if (tooltipPanel.activeSelf)
        {
            Vector3 newPos = Input.mousePosition + offset;
            // 툴팁의 크기를 얻어와서 경계 체크
            RectTransform rect = tooltipPanel.GetComponent<RectTransform>();
            float tooltipWidth = rect.rect.width;
            float tooltipHeight = rect.rect.height;
            // 화면 경계 체크
            newPos.x = Mathf.Clamp(newPos.x, tooltipWidth / 2, Screen.width - tooltipWidth / 2);
            newPos.y = Mathf.Clamp(newPos.y, tooltipHeight / 2, Screen.height - tooltipHeight / 2);

            tooltipPanel.transform.position = newPos;
        }
    }

    public void ShowTooltip(string info)
    {
        tooltipPanel.SetActive(true);
        tooltipText.text = info;
    }

    public void HideTooltip()
    {
        tooltipPanel.SetActive(false);
    }
}

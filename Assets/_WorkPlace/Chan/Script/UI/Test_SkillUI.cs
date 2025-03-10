using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test_SkillUI : MonoBehaviour
{
    [SerializeField] private Transform ADPanelParent;
    [SerializeField] private Transform APPanelParent;
    [SerializeField] private GameObject SKInfoPanel;
 
    [SerializeField] private GameObject skillSlotPrefab;

    private TextMeshProUGUI[] skInfoText;
    private Image[] skIcon;
    private Image[] skInfoIcon;
    private Button[] buttons;

    private void Awake()
    {
        ADPanelParent = transform.GetComponent<Transform>();
        APPanelParent = transform.GetComponent<Transform>();
        skInfoIcon = SKInfoPanel.GetComponentsInChildren<Image>();

        skInfoText = skillSlotPrefab.GetComponentsInChildren<TextMeshProUGUI>();
        skIcon = skillSlotPrefab.GetComponentsInChildren<Image>();

        buttons = transform.GetComponentsInChildren<Button>();
    }

    private void OnEnable()
    {
        
    }

    private void OnDisable()
    {
        
    }

    private void UpdateSkillInfo()
    {
       
    }

}

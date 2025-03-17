using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test_SkillUI : MonoBehaviour
{
    [SerializeField] private Transform ADPanelParent;   // ad스킬 목록
    [SerializeField] private Transform APPanelParent;   // ap스킬 목록
    [SerializeField] private GameObject InfoPanel;      // 우측 인포 판넬
 
    [SerializeField] private GameObject ADskillPrefab;  // ad스킬 프리팹
    [SerializeField] private GameObject APskillPrefab;  // ap스킬 프리팹

    private TextMeshProUGUI[] InfoText;
    private Image[] skIcon;
    private Image[] skInfoIcon;
    private Button[] buttons;

    private void Awake()
    {
       
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

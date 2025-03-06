using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test_SkillUI : MonoBehaviour
{
    [SerializeField] private Transform skPanelParent;
    [SerializeField] private GameObject skPrefab;

    private TextMeshProUGUI[] skLevel;
    private Image[] skIcon;

    private void Awake()
    {
        skLevel = skPrefab.GetComponentsInChildren<TextMeshProUGUI>();
        skIcon = skPrefab.GetComponentsInChildren<Image>();


    }






}

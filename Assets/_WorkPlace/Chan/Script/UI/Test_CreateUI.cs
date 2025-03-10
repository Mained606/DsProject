using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Test_CreateUI : MonoBehaviour
{
    [SerializeField] private Transform ItemPanel;   // 인벤토리에 재료 아이템 판넬
    [SerializeField] private Transform CreatePanel; // 우측 제작 창 판넬
    [SerializeField] private Transform InfoPanel;   // 결과 아이템 인포 판넬

    private Image[] iconImage;
    private TextMeshProUGUI[] itemValueText;        // 재료 아이템 개수 
    private TextMeshProUGUI[] itemInfoText;         // 결과 아이템 인포창 텍스트 

    private Button[] buttons;

    private IReadOnlyList<Item> CreaftingMaterial;

    private void Start()
    {
        CreaftingMaterial = new List<Item>();

        ItemPanel = transform.GetComponent<Transform>();
        CreatePanel = transform.GetComponent<Transform>();

        iconImage = CreatePanel.GetComponentsInChildren<Image>();
        itemValueText = CreatePanel.GetComponentsInChildren<TextMeshProUGUI>();
        itemInfoText = InfoPanel.GetComponentsInChildren<TextMeshProUGUI>();

        buttons = transform.GetComponentsInChildren<Button>();

    }

    private void OnEnable()
    {
        AddButtonListeners();
        
    }

    private void OnDisable()
    {
        
    }
    private void OnButtonClick(int buttonIndex)
    {
     
    }
    public void AddButtonListeners()
    {
    }
}

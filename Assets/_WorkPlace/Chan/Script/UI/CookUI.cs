using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class CookUI : MonoBehaviour
{
    [SerializeField] private Transform itemPanel;

    private Button[] buttons;

    private Dictionary<string, List<Item>> categorizedItems = new Dictionary<string, List<Item>>();
    private IReadOnlyList<Item> preInventory;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        preInventory = new List<Item>();
        buttons = transform.GetComponentsInChildren<Button>();
    }

    private void OnEnable()
    {
        
    }


}

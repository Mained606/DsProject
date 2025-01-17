using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialMenu : MonoBehaviour
{
    public GameObject menuItemPrefab;
    public RectTransform centerPoint;
    public float radius = 400f;
    public int itemCount = 5;
    public float startAngle = -90f;
    public float endAngle = 90f;
    public float highlightScale = 1.2f;

    public List<GameObject> menu3DPrefabs; // 메뉴 항목에 해당하는 3D 프리팹 리스트
    public Transform previewPoint; // 3D 프리팹을 보여줄 지점
    public TextMeshProUGUI descriptionText; // 설명 텍스트

    private List<GameObject> menuItems = new List<GameObject>();
    private int highlightedIndex = 0;
    private GameObject currentPreview; // 현재 보여지고 있는 3D 프리팹

    void Start()
    {
        GenerateMenuItems();
        HighlightItem(highlightedIndex);
    }

    void GenerateMenuItems()
    {
        float angleStep = radius / (itemCount - 1);
        for (int i = 0; i < itemCount; i++)
        {
            float angle = (-radius / 2) + (i * angleStep);
            Vector3 position = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * radius;

            GameObject menuItem = Instantiate(menuItemPrefab, centerPoint.position, Quaternion.identity, transform);
            menuItem.gameObject.transform.localPosition = position;
            menuItems.Add(menuItem);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            ChangeHighlight(-1);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            ChangeHighlight(1);
        }
    }

    void ChangeHighlight(int direction)
    {
        highlightedIndex = (highlightedIndex + direction + itemCount) % itemCount;
        RotateMenuItems(direction);
        HighlightItem(highlightedIndex);
    }

    void RotateMenuItems(int direction)
    {
        float angleStep = radius / (itemCount - 1);
        for (int i = 0; i < menuItems.Count; i++)
        {
            float angle = (-radius / 2) + ((i - direction + itemCount) % itemCount) * angleStep;
            Vector3 position = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * radius;
            menuItems[i].transform.localPosition = position;
        }
    }


    void HighlightItem(int index)
    {
        for (int i = 0; i < menuItems.Count; i++)
        {
            if (i == index)
            {
                menuItems[i].transform.localScale = Vector3.one * highlightScale;
                ShowPreview(i);
            }
            else
            {
                menuItems[i].transform.localScale = Vector3.one;
            }
        }
    }

    void ShowPreview(int index)
    {
        if (currentPreview != null)
        {
            Destroy(currentPreview);
        }
        currentPreview = Instantiate(menu3DPrefabs[index], previewPoint.position, Quaternion.identity);
        descriptionText.text = "설명: " + menu3DPrefabs[index].name;
    }
}

using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class RadialMenu : MonoBehaviour
{
    public GameObject menuListObject;
    private GameObject[] menuItems;
    public float radius = 800f;
    public Transform menuCenter;    // 메뉴 중앙 기준점
    public float radiusArch = 180f;
    private int itemCount;
    public int highlightedIndex = 1;
    public float expandedSize = 2.0f;
    public Vector2 basePosition = new Vector2(1850f, -1000f);
    public float spacing = 50f;
    public float animationSpeed = 1f;
    public float fadeDuration = 0.5f; // 메뉴가 사라질 때 걸리는 시간
    public float spacingMultiplier = 1.2f;
    private int countMenuList;
    private string callingFunction;

    bool isArc = false;
    bool isRadius = false;
    bool isArange = false;
    void Start()
    {
        CheckChildList();
        HighlightCurrentItem();
    }

    void Update()
    {
        HandleMouseWheel();
        CheckFunctionBefore();
    }

    private void CheckChildList()
    {
        menuItems = new GameObject[menuListObject.transform.childCount];
        countMenuList = menuListObject.transform.childCount;
        for (int i = 0; i < countMenuList; i++)
        {
            menuItems[i] = menuListObject.transform.GetChild(i).gameObject;
        }
        itemCount = menuItems.Length;
    }
    
    private void CheckFunctionBefore()
    {
        if (menuListObject.transform.childCount != countMenuList)
        {
            CheckChildList();
            ResetFlags();
            // 각 함수에 맞춰 호출
            switch (callingFunction)
            {
                case "ArrangeMenuVertical":
                    ArrangeMenuVertical();
                    break;
                case "ArrangeMenuHorizontal":
                    ArrangeMenuHorizontal();
                    break;
                case "ArrangeRadialMenu":
                    ArrangeRadialMenu();
                    break;
                case "ArrangeArchMenu":
                    ArrangeArchMenu();
                    break;
                default:
                    break;
            }
        }
    }
    // 마우스 휠로 하이라이트 인덱스 변경
    void HandleMouseWheel()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll > 0f)
        {
            highlightedIndex++;
            if (highlightedIndex >= itemCount)
                highlightedIndex = 0;
        }
        else if (scroll < 0f)
        {
            highlightedIndex--;
            if (highlightedIndex < 0)
                highlightedIndex = itemCount - 1;
        }

        HighlightCurrentItem();  // 하이라이트 갱신
    }

    void HighlightCurrentItem()
    {
        for (int i = menuItems.Length - 1 ; i >= 0; i--)
        {
            RectTransform item = menuItems[i].GetComponent<RectTransform>();
            Button button = menuItems[i].GetComponent<Button>();

            if (i == highlightedIndex)
            {
                item.localScale = Vector3.one * expandedSize;
                button.Select();
            }
            else
            {
                item.localScale = Vector3.one;
            }
        }
    }

    public void HideMenu()
    {
        StartCoroutine(HideMenuCoroutine());
    }

    IEnumerator HideMenuCoroutine()
    {
        for (int i = menuItems.Length - 1; i >= 0; i--)
        {
            RectTransform item = menuItems[i].GetComponent<RectTransform>();
            CanvasGroup canvasGroup = menuItems[i].GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = menuItems[i].AddComponent<CanvasGroup>();
            }

            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration)
            {
                // 서서히 사라지는 애니메이션
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            canvasGroup.gameObject.SetActive(false);
            canvasGroup.alpha = 1f;
        }
        ResetFlags();
    }

    public void HideMenuWithSlide()
    {
        StartCoroutine(HideMenuWithSlideCoroutine());
    }

    IEnumerator HideMenuWithSlideCoroutine()
    {
        callingFunction = string.Empty;
        for (int i = menuItems.Length - 1; i >= 0; i--)
        {
            RectTransform item = menuItems[i].GetComponent<RectTransform>();
            CanvasGroup canvasGroup = menuItems[i].GetComponent<CanvasGroup>();

            if (canvasGroup == null)
            {
                canvasGroup = menuItems[i].AddComponent<CanvasGroup>();
            }

            float elapsedTime = 0f;

            Vector3 startPosition = item.localPosition;
            Vector3 endPosition = new Vector3(basePosition.x, basePosition.y, 0);

            while (elapsedTime < fadeDuration)
            {
                item.localPosition = Vector3.Lerp(startPosition, endPosition, elapsedTime / fadeDuration);
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            canvasGroup.gameObject.SetActive(false);
            canvasGroup.alpha = 1f;
        }
        ResetFlags();
    }

    public void ArrangeMenuVertical()
    {
        callingFunction = "ArrangeMenuVertical";
        if (isArange)
        {
            HideMenu();
            return;
        }

        isArange = true;
        for (int i = menuItems.Length - 1; i >= 0; i--)
        {
            RectTransform item = menuItems[i].GetComponent<RectTransform>();

            if (i == highlightedIndex)
            {
                item.localScale = Vector3.one * expandedSize;
                item.anchoredPosition = basePosition;
            }
            else
            {
                item.localScale = Vector3.one;
                int relativeIndex = (i - highlightedIndex + itemCount) % itemCount;
                float yPos = basePosition.y + (relativeIndex * (item.sizeDelta.y + spacing));
                item.anchoredPosition = new Vector2(basePosition.x, yPos);
            }

            item.gameObject.SetActive(true);
        }
    }

    public void ArrangeMenuHorizontal()
    {
        callingFunction = "ArrangeMenuHorizontal";
        if (isArange)
        {
            HideMenu();
            return;
        }

        isArange = true;
        for (int i = 0; i < menuItems.Length; i++)
        {
            RectTransform item = menuItems[i].GetComponent<RectTransform>();

            if (i == highlightedIndex)
            {
                item.localScale = Vector3.one * expandedSize;
                item.anchoredPosition = basePosition;  // 고정된 위치
            }
            else
            {
                item.localScale = Vector3.one;
                int relativeIndex = (i - highlightedIndex + itemCount) % itemCount;
                float xPos = basePosition.x + (relativeIndex * (item.sizeDelta.x + spacing));
                item.anchoredPosition = new Vector2(xPos, basePosition.y);
            }

            item.gameObject.SetActive(true);
        }
    }

    public void ArrangeRadialMenu()
    {
        callingFunction = "ArrangeRadialMenu";
        if (isRadius)
        {
            HideMenu();
            return;
        }

        isRadius = true;
        float totalWidth = 0f;
        for (int i = 0; i < itemCount; i++)
        {
            RectTransform rect = menuItems[i].GetComponent<RectTransform>();
            totalWidth += rect.sizeDelta.x * spacingMultiplier;
        }

        float angleStep = 360f / itemCount;

        for (int i = 0; i < itemCount; i++)
        {
            // 각도 계산
            float angle = i * angleStep * Mathf.Deg2Rad;
            RectTransform itemRect = menuItems[i].GetComponent<RectTransform>();

            Vector3 position = new Vector3(
                Mathf.Cos(angle) * (radius + itemRect.sizeDelta.x / 2),
                Mathf.Sin(angle) * (radius + itemRect.sizeDelta.y / 2),
                0);

            itemRect.localPosition = position;
            itemRect.gameObject.SetActive(true);
        }
    }

    public void ArrangeArchMenu()
    {
        callingFunction = "ArrangeArchMenu";
        if (isArc)
        {
            HideMenu();
            return;
        }

        isArc = true;
        float angleStep = radiusArch / (itemCount - 1);
        for (int i = 0; i < itemCount; i++)
        {
            float angle = (-radiusArch / 2) + (i * angleStep);
            Vector3 position = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad), 0) * radius;
            menuItems[i].GetComponent<RectTransform>().localPosition = position;
            menuItems[i].GetComponent<RectTransform>().gameObject.SetActive(true);
        }
    }

    public void NewArrangeMenu()
    {
        float angleStep = radiusArch / (menuItems.Length - 1);
        float startingAngle = radiusArch / 2;  // 왼쪽으로 시작하게 설정

        for (int i = 0; i < menuItems.Length; i++)
        {
            GameObject item = menuItems[i];
            RectTransform rect = item.GetComponent<RectTransform>();

            // 각도 계산 (왼쪽으로 향하게 설정)
            float angle = startingAngle - i * angleStep;
            float radians = angle * Mathf.Deg2Rad;

            // X축과 Z축을 활용해 3D 공간에 아이템 배치
            Vector3 position = new Vector3(
                Mathf.Sin(radians) * radius,
                0,  // Y축은 고정 (수평 아치형)
                Mathf.Cos(radians) * radius
            );

            // 메뉴 아이템 위치와 회전 설정
            item.transform.localPosition = menuCenter.position + position;
            item.transform.LookAt(menuCenter);  // 중앙을 바라보도록 설정
            item.transform.localScale = Vector3.one;  // 기본 크기로 설정
        }
    }

    void ResetFlags()
    {
        isArc = false;
        isRadius = false;
        isArange = false;
    }
}

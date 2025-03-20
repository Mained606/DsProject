using UnityEngine;

public class CursorImage : MonoBehaviour
{
    public static CursorImage Instance { get; private set; }

    [SerializeField] private Texture2D idleCursorImage;
    [SerializeField] private Texture2D ClickCursorImage;
    [SerializeField] private Vector2 Point = Vector2.zero;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        SetCursorIdle();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetCursorClick();
        }
        if (Input.GetMouseButtonUp(0))
        {
            SetCursorIdle();
        }
    }

    public void SetCursorIdle()
    {
        Cursor.SetCursor(idleCursorImage,Point, CursorMode.Auto);
    }

    public void SetCursorClick()
    {
        Cursor.SetCursor(ClickCursorImage, Point, CursorMode.Auto);
    }
}

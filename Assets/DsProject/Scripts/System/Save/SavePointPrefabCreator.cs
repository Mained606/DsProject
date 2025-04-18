#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using TMPro;

public class SavePointPrefabCreator : Editor
{
    [MenuItem("GameObject/DsProject/저장 포인트", false, 10)]
    static void CreateSavePointPrefab()
    {
        // 세이브 포인트 오브젝트 생성
        GameObject savePointObj = new GameObject("SavePoint");
        SavePoint savePoint = savePointObj.AddComponent<SavePoint>();
        
        // 콜라이더 추가
        SphereCollider sphereCollider = savePointObj.AddComponent<SphereCollider>();
        sphereCollider.radius = 2.0f;
        sphereCollider.isTrigger = true;
        
        // 시각적 표현을 위한 메시 추가
        GameObject visualObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        visualObj.transform.SetParent(savePointObj.transform);
        visualObj.transform.localPosition = Vector3.zero;
        visualObj.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        
        // 메테리얼 설정
        Renderer renderer = visualObj.GetComponent<Renderer>();
        if (renderer != null)
        {
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.2f, 0.6f, 1.0f, 0.7f);
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(0.2f, 0.6f, 1.0f, 0.7f));
            renderer.material = material;
        }
        
        // 콜라이더 제거 (시각적 오브젝트에서는 필요 없음)
        Collider visualCollider = visualObj.GetComponent<Collider>();
        if (visualCollider != null)
        {
            DestroyImmediate(visualCollider);
        }
        
        // UI 캔버스 생성
        GameObject canvasObj = new GameObject("SavePointCanvas");
        canvasObj.transform.SetParent(savePointObj.transform);
        canvasObj.transform.localPosition = new Vector3(0, 2.0f, 0);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<UnityEngine.UI.GraphicRaycaster>();
        
        // UI 패널 생성
        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform);
        panelObj.transform.localPosition = Vector3.zero;
        panelObj.transform.localRotation = Quaternion.identity;
        panelObj.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        UnityEngine.UI.Image panel = panelObj.AddComponent<UnityEngine.UI.Image>();
        panel.color = new Color(0, 0, 0, 0.7f);
        panelObj.GetComponent<RectTransform>().sizeDelta = new Vector2(200, 50);
        
        // 텍스트 생성
        GameObject textObj = new GameObject("SaveText");
        textObj.transform.SetParent(panelObj.transform);
        textObj.transform.localPosition = Vector3.zero;
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale = Vector3.one;
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "F키를 눌러 저장";
        text.color = Color.white;
        text.fontSize = 16;
        text.alignment = TextAlignmentOptions.Center;
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(180, 40);
        
        // 캔버스를 항상 카메라를 향하도록 빌보드 스크립트 추가
        canvasObj.AddComponent<Billboard>();
        
        // SavePoint 컴포넌트에 UI 참조 설정
        savePoint.messageText = text;
        savePoint.uiObject = canvasObj;
        
        // 생성된 오브젝트 선택
        Selection.activeGameObject = savePointObj;
        
        Debug.Log("저장 포인트가 생성되었습니다.");
    }
}

// 빌보드 스크립트 (UI가 항상 카메라를 향하게 함)
public class Billboard : MonoBehaviour
{
    private Transform mainCameraTransform;
    
    private void Start()
    {
        mainCameraTransform = Camera.main.transform;
    }
    
    private void LateUpdate()
    {
        if (mainCameraTransform != null)
        {
            transform.LookAt(transform.position + mainCameraTransform.rotation * Vector3.forward,
                mainCameraTransform.rotation * Vector3.up);
        }
    }
}
#endif 
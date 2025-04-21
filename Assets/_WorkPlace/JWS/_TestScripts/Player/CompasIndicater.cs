using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CompassIndicater : MonoBehaviour
{
    public static CompassIndicater Instance { get; private set; }
    [SerializeField] private RectTransform compassBar;
    [SerializeField] private RectTransform markerPrefab;
    [SerializeField] private List<Transform> targets;
    [SerializeField] private List<RectTransform> activeMarkers = new List<RectTransform>();
    private Transform player;
    private PlayerController controller;
    private RectTransform[] compassMarkers;
    private float compassWidth = 1600f;

    private float maxVisibleDistance = 200f;
    private float minDistanceForScaling = 50f;
    private Color nearColor = Color.green;
    private Color farColor = Color.red;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
        compassBar.transform.parent.gameObject.SetActive(true);
        compassMarkers = new RectTransform[compassBar.childCount];
        for (int i = 0; i < compassBar.childCount; i++)
        {
            compassMarkers[i] = compassBar.GetChild(i).GetComponent<RectTransform>();
        }
        targets = new List<Transform> ();
    }

    void Start()
    {
        player = GameManager.playerTransform;
        controller = player.GetComponent<PlayerController>();
        SetupMarkers();
        InitializeTargetMarkers();
    }

    void Update()
    {
        //if (targets == null || targets.Count == 0) return;
        UpdateMarkers();
        UpdateTargetMarkers();
    }

    void SetupMarkers()
    {
        for (int i = 0; i < compassMarkers.Length; i++)
        {
            float positionX = (i - 6) * (compassWidth / 12);
            compassMarkers[i].anchoredPosition = new Vector2(positionX, -3.73f);
        }
    }

    void InitializeTargetMarkers()
    {
        if (targets == null || targets.Count == 0) return;

        foreach (Transform target in targets)
        {
            RectTransform newMarker = Instantiate(markerPrefab, compassBar);
            activeMarkers.Add(newMarker);
        }
    }

    void UpdateMarkers()
    {
        //float playerRotationY = player.eulerAngles.y;
        float cameraRotationY = controller.cameraTransform.eulerAngles.y;

        for (int i = 0; i < compassMarkers.Length; i++)
        {
            float angleOffset = (i * (360f / compassMarkers.Length)) - cameraRotationY;
            float normalizedOffset = angleOffset / 360f;
            float markerX = normalizedOffset * compassWidth;
            markerX = Mathf.Repeat(markerX + compassWidth / 2, compassWidth) - compassWidth / 2;
            compassMarkers[i].anchoredPosition = new Vector2(markerX, compassMarkers[i].anchoredPosition.y);
        }
    }


    void UpdateTargetMarkers()
    {
        for (int i = 0; i < targets.Count; i++)
        {
            Transform target = targets[i];
            RectTransform marker = activeMarkers[i];

            //Vector3 directionToTarget = target.position - player.position;
            Vector3 directionToTarget = (target.position - player.position).normalized;
            float distance = Vector3.Distance(target.position, player.position);
            float targetAngle = Mathf.Atan2(directionToTarget.x, directionToTarget.z) * Mathf.Rad2Deg;
            float relativeAngle = Mathf.DeltaAngle(controller.cameraTransform.eulerAngles.y, targetAngle);

            float markerX = (relativeAngle / 360f) * compassWidth;

            markerX = Mathf.Repeat(markerX + compassWidth / 2, compassWidth) - compassWidth / 2;
            marker.anchoredPosition = new Vector2(markerX, marker.anchoredPosition.y);

            TextMeshProUGUI[] distanceText = marker.GetComponentsInChildren<TextMeshProUGUI>();

            Color markerColor = Color.Lerp(nearColor, farColor, Mathf.Clamp01(distance / maxVisibleDistance));
            marker.GetChild(0).GetComponent<UnityEngine.UI.Image>().color = markerColor;
            if (distanceText != null)
            {
                distanceText[1].color = markerColor;
                distanceText[1].text = distance.ToString("N0") + "m";
                distanceText[0].faceColor = markerColor;
                distanceText[0].text = target.name;
            }

            float scale = Mathf.Clamp(1f - (distance - minDistanceForScaling) / (maxVisibleDistance - minDistanceForScaling), 0.5f, 1f);
            marker.GetChild(0).localScale = Vector3.one * scale;

            marker.gameObject.SetActive(distance <= maxVisibleDistance);
        }
    }
    
    public static void AddTarget(Transform newTarget)
    {
        if (Instance == null)
        {
            Debug.LogError("[CompassIndicater] Instance가 null입니다. AddTarget을 호출할 수 없습니다.");
            return;
        }

        if (Instance.targets.Contains(newTarget))
        {
            // //Debug.Log($"[CompassIndicater] 타겟 '{newTarget.name}'는 이미 콤파스에 등록되어 있습니다.");
            return;
        }

        Instance.targets.Add(newTarget);
        RectTransform newMarker = Instantiate(Instance.markerPrefab, Instance.compassBar);
        Instance.activeMarkers.Add(newMarker);
        //Debug.Log($"[CompassIndicater] 새 타겟 '{newTarget.name}'를 콤파스에 추가했습니다.");
    }

    public static void RemoveTarget(Transform target)
    {
        if (Instance == null)
        {
            Debug.LogError("[CompassIndicater] Instance가 null입니다. RemoveTarget을 호출할 수 없습니다.");
            return;
        }

        int targetIndex = Instance.targets.IndexOf(target);
        if (targetIndex != -1)
        {
            // 타겟 리스트에서 제거
            Instance.targets.RemoveAt(targetIndex);
            
            // 해당 마커 제거
            if (targetIndex < Instance.activeMarkers.Count)
            {
                RectTransform marker = Instance.activeMarkers[targetIndex];
                if (marker != null)
                {
                    Destroy(marker.gameObject);
                }
                Instance.activeMarkers.RemoveAt(targetIndex);
                //Debug.Log($"[CompassIndicater] 타겟 '{target.name}'를 콤파스에서 제거했습니다.");
            }
            else
            {
                Debug.LogWarning($"[CompassIndicater] 타겟 '{target.name}'는 제거되었지만, 해당하는 마커를 찾을 수 없습니다.");
                ResetMarkers();
            }
        }
        else
        {
            //Debug.Log($"[CompassIndicater] 타겟 '{target.name}'는 콤파스에 등록되어 있지 않습니다.");
        }
    }
    
    private static void ResetMarkers()
    {
        if (Instance == null) return;
        
        // 기존 모든 마커 제거
        foreach (RectTransform marker in Instance.activeMarkers)
        {
            if (marker != null)
            {
                Destroy(marker.gameObject);
            }
        }
        Instance.activeMarkers.Clear();
        
        // 모든 타겟에 대해 새 마커 생성
        foreach (Transform target in Instance.targets)
        {
            RectTransform newMarker = Instantiate(Instance.markerPrefab, Instance.compassBar);
            Instance.activeMarkers.Add(newMarker);
        }
        
        //Debug.Log("[CompassIndicater] 모든 마커를 재설정했습니다.");
    }

    public static void ClearAllTargets()
    {
        if (Instance == null) return;
        
        foreach (var marker in Instance.activeMarkers)
        {
            if (marker != null)
            {
                Destroy(marker.gameObject);
            }
        }
        
        Instance.activeMarkers.Clear();
        Instance.targets.Clear();
    }
}

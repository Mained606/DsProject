using UnityEngine;

public class ZonesManager : MonoBehaviour
{
    [SerializeField] private Transform[] objectZones;
    [SerializeField] private float visibleDistance = 300f;
    private Transform player;
    private Camera mainCamera;

    private void Awake()
    {
        // 모든 Zone 비활성화
        for (int i = 0; i < objectZones.Length; i++)
        {
            objectZones[i].gameObject.SetActive(false);
        }
        mainCamera = Camera.main;
    }

    void Update()
    {
        if (player == null) player = GameManager.playerTransform;
        if (objectZones == null || objectZones.Length == 0) return;

        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(mainCamera);

        for (int i = 0; i < objectZones.Length; i++)
        {
            Transform zone = objectZones[i];
            float distanceToPlayer = Vector3.Distance(zone.position, player.position);

            Collider zoneCollider = zone.GetComponent<Collider>();
            Bounds zoneBounds = zoneCollider != null ? zoneCollider.bounds : new Bounds(zone.position, Vector3.one);
            bool isInFrustum = GeometryUtility.TestPlanesAABB(frustumPlanes, zoneBounds);

            if (distanceToPlayer < visibleDistance && isInFrustum)
            {
                if (!zone.gameObject.activeSelf) zone.gameObject.SetActive(true);
            }
            else
            {
                if (zone.gameObject.activeSelf) zone.gameObject.SetActive(false);
            }
        }
    }
}

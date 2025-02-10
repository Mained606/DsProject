using UnityEngine;

public class ZonesManager : MonoBehaviour
{
    [SerializeField] private Transform[] objectZones;
    [SerializeField] private float visibleDistance = 300f;

    private void Awake()
    {
        if (objectZones == null || objectZones.Length == 0) return;

        foreach (Transform zone in objectZones)
        {
            zone.gameObject.SetActive(false);
            if (zone.name == "용의둥지" && zone.childCount > 3)
            {
                if (zone.GetChild(3).gameObject.activeSelf) zone.GetChild(3).gameObject.SetActive(false);
            }
        }
    }

    private void Update()
    {
        BaseCheck();
    }

    private void BaseCheck()
    {
        if (objectZones == null || objectZones.Length == 0) return;

        Transform player = GameManager.playerTransform;

        foreach (Transform zone in objectZones)
        {
            float distanceToPlayer = Vector3.Distance(zone.position, player.position);
            float maxDistance = visibleDistance;
            bool isInFrustum = CameraManager.IsInFrustum(zone);

            switch (zone.name)
            {
                case "용의둥지":
                    if (QuestManager.CurrentMainQuestIndex != 2) continue;
                    maxDistance = 40f;
                    bool eggActive = distanceToPlayer < 1f;
                    if (zone.GetChild(3).gameObject.activeSelf != eggActive)
                    {
                        zone.GetChild(3).gameObject.SetActive(eggActive);
                    }
                    break;

                case "최고위험지역":
                    if (QuestManager.CurrentMainQuestIndex != 9) continue;
                    break;
            }

            bool shouldBeActive = distanceToPlayer < maxDistance;
            if (zone.gameObject.activeSelf != shouldBeActive)
            {
                zone.gameObject.SetActive(shouldBeActive);
            }
        }
    }
}

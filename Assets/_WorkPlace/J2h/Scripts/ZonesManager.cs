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
            switch (zone.name)
            {
                case "용의둥지":
                    if (zone.GetChild(3).gameObject.activeSelf) zone.GetChild(3).gameObject.SetActive(false);
                    break;
                case "보스루인":
                    break;
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
            if (zone == null) continue;
            float distanceToPlayer = Vector3.Distance(zone.position, player.position);
            float maxDistance = visibleDistance;

            int currentQuestIndex = QuestManager.CurrentMainQuestIndex;

            if (zone.name == "용의둥지")
            {
                if (currentQuestIndex != 2) continue;
                maxDistance = 1f;

                if (zone.childCount > 3)
                {
                    bool eggActive = distanceToPlayer < 1f;
                    if (zone.GetChild(3).gameObject.activeSelf != eggActive)
                    {
                        zone.GetChild(3).gameObject.SetActive(eggActive);
                    }
                }
            }
            else if (zone.name == "보스루인" && currentQuestIndex == 9)
            {
                maxDistance = 40f;
            }
            else if (zone.name == "최고위험지역" && currentQuestIndex == 7)
            {
                maxDistance = 40f;
            }
            else if (zone.name == "Egg" && currentQuestIndex == 2)
            {
                maxDistance = 10f;
            }
            else if (zone.name == "Strawberry" && currentQuestIndex == 2)
            {
                maxDistance = 30f;
            }
            else if (zone.name == "나뭇가지" && currentQuestIndex == 4)
            {
                maxDistance = 100f;
            }
            else if ((zone.name == "돌" || zone.name == "나무") && currentQuestIndex == 6)
            {
                maxDistance = 100f;
            }
            else
            {
                continue;
            }

            bool shouldBeActive = distanceToPlayer < maxDistance;
            if (zone.gameObject.activeSelf != shouldBeActive)
            {
                zone.gameObject.SetActive(shouldBeActive);
            }
        }
    }
}

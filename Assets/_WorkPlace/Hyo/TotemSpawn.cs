using UnityEngine;

public class TotemSpawn : MonoBehaviour
{
    // 트리거에 플레이어 무기 태그가 인식될 경우 특정 컴포넌트를 엑티브 시킴
    [SerializeField] private GameObject Spawner;
    private NPCSpawner npcSpawner;

    void Start()
    {
        npcSpawner = Spawner.gameObject.GetComponent<NPCSpawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("PlayerWeapon"))
        {
            QuestManager.Instance.UpdateQuestProgress(QuestConditionType.Kill, "Totem", 1);
            npcSpawner.enabled = true;
            Destroy(this.gameObject, 0.1f);
        }
    }
}

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
        if (other.CompareTag("Player"))
        {
            Debug.Log("트리거엔터");
            npcSpawner.enabled = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Destroy(this.gameObject, 1f);
    }
}

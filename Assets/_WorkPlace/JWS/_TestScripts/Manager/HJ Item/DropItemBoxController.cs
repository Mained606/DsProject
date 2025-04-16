using UnityEngine;
using System.Collections.Generic;

public class DropItemBoxController : MonoBehaviour
{
    public List<string> dropItemIds = new List<string>(); // 고정 드롭 아이템 ID 리스트
    public List<MonsterData.DropItemChance> randomDropItems = new List<MonsterData.DropItemChance>(); // 랜덤 드롭 아이템과 확률
    [SerializeField] private float detectionDistance = 2f;
    public bool isRandomDrop = true;

    private void Start()
    {
        Destroy(this.gameObject, DsConstValue.DROP_ITEM_DESTROY_INTERVAL);
    }

    private void Update()
    {
        if (InputManager.InputActions.actions["Interact"].triggered)
        {
            OpenBox();
        }
    }

    public void OpenBox()
    {
        if (!IsNearPlayer())
            return;

        if (isRandomDrop)
        {
            // 랜덤 드롭 - 확률에 따라 아이템 드롭
            foreach (var dropItem in randomDropItems)
            {
                // 0-100 범위의 확률을 0-1 범위로 변환하여 비교
                float randomValue = Random.Range(0f, 100f);
                if (randomValue <= dropItem.dropChance)
                {
                    // 확률에 성공하면 아이템 추가
                    ItemManager.Instance.AddItemLogic(dropItem.itemId);
                    Debug.Log($"랜덤 드롭: {dropItem.itemId} 획득! (확률: {dropItem.dropChance}%, 주사위: {randomValue})");
                }
                else
                {
                    Debug.Log($"랜덤 드롭 실패: {dropItem.itemId} (확률: {dropItem.dropChance}%, 주사위: {randomValue})");
                }
            }
        }
        else
        {
            // 고정 드롭 - 지정된 모든 아이템 드롭
            foreach (var itemId in dropItemIds)
            {
                ItemManager.Instance.AddItemLogic(itemId);
                Debug.Log($"고정 드롭: {itemId} 획득!");
            }
        }

        Destroy(gameObject); // 박스 삭제
    }

    private bool IsNearPlayer()
    {
        float disatance = Vector3.Distance(transform.position, GameManager.playerTransform.position);
        return disatance <= detectionDistance;
    }
}

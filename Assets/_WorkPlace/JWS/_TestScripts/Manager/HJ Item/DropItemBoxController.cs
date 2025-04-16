using UnityEngine;
using System.Collections.Generic;

public class DropItemBoxController : MonoBehaviour
{
    public List<string> dropItemIds = new List<string>(); // 고정 드롭 아이템 ID 리스트
    public List<MonsterData.DropItemChance> randomDropItems = new List<MonsterData.DropItemChance>(); // 랜덤 드롭 아이템과 확률
    [SerializeField] private float detectionDistance = 2f;
    public bool isRandomDrop = true;
    
    // 실제로 드롭될 아이템들을 저장할 리스트
    private List<string> actualDropItemIds = new List<string>();

    private void Start()
    {
        // 랜덤 드롭일 경우 확률 계산을 미리 수행
        if (isRandomDrop)
        {
            CalculateRandomDrops();
            
            // 드롭될 아이템이 없으면 아이템 박스 삭제
            if (actualDropItemIds.Count == 0)
            {
                Debug.Log("확률 계산 결과 드롭될 아이템이 없어 아이템 박스를 제거합니다.");
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            // 고정 드롭일 경우 그대로 사용
            actualDropItemIds.AddRange(dropItemIds);
        }
        
        Destroy(this.gameObject, DsConstValue.DROP_ITEM_DESTROY_INTERVAL);
    }

    private void Update()
    {
        if (InputManager.InputActions.actions["Interact"].triggered)
        {
            OpenBox();
        }
    }
    
    // 랜덤 드롭 아이템 확률 계산
    private void CalculateRandomDrops()
    {
        actualDropItemIds.Clear();
        
        foreach (var dropItem in randomDropItems)
        {
            // 0-100 범위의 확률로 계산
            float randomValue = Random.Range(0f, 100f);
            if (randomValue <= dropItem.dropChance)
            {
                // 확률에 성공하면 드롭될 아이템 리스트에 추가
                actualDropItemIds.Add(dropItem.itemId);
                Debug.Log($"랜덤 드롭 계산: {dropItem.itemId} 획득 성공! (확률: {dropItem.dropChance}%, 주사위: {randomValue})");
            }
            else
            {
                Debug.Log($"랜덤 드롭 계산: {dropItem.itemId} 획득 실패 (확률: {dropItem.dropChance}%, 주사위: {randomValue})");
            }
        }
    }

    public void OpenBox()
    {
        if (!IsNearPlayer())
            return;

        // 실제 드롭될 아이템 리스트를 기반으로 아이템 추가
        foreach (var itemId in actualDropItemIds)
        {
            ItemManager.Instance.AddItemLogic(itemId);
            Debug.Log($"아이템 박스에서 {itemId} 획득!");
        }

        Destroy(gameObject); // 박스 삭제
    }

    private bool IsNearPlayer()
    {
        float disatance = Vector3.Distance(transform.position, GameManager.playerTransform.position);
        return disatance <= detectionDistance;
    }
}

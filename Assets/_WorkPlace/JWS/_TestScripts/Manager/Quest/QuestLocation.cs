using UnityEngine;

[RequireComponent(typeof(SphereCollider))]
public class QuestLocation : MonoBehaviour
{
    private bool isQuestUpdated = false;

    private void Awake()
    {
        SphereCollider collider = GetComponent<SphereCollider>();
        collider.radius = 5f;
        collider.isTrigger = true;
    }

    private void Start()
    {
        if (!QuestManager.QuestConditionPoint.ContainsKey(this.gameObject.name))
        {
            QuestManager.QuestConditionPoint.Add(this.gameObject.name, this.transform);
        }
        else
        {
            Debug.LogError($"이미 등록된 퀘스트 포인트입니다: {this.gameObject.name}");
            return;
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Vector3 target = new Vector3(transform.position.x, 0, transform.position.z);
            Vector3 player = new Vector3(other.transform.position.x, 0, other.transform.position.z);
            float distance = Vector3.Distance(target, player);

            if (distance <= 2f && !isQuestUpdated)
            {
                isQuestUpdated = true;
                CompassIndicater.RemoveTarget(QuestManager.GetQuestConditionPoint(this.gameObject.name));
                QuestManager.Instance.UpdateQuestProgress(QuestConditionType.Explore, this.gameObject.name);
            }
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            isQuestUpdated = false;
        }
    }
}

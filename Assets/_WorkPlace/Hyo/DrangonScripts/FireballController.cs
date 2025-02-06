using UnityEngine;

public class FireballController : MonoBehaviour
{
    private Vector3 targetPosition;
    private Transform targetTransform; // 🔹 추가: 타겟 Transform 저장
    private DragonData dragonData;
    private CharacterData targetData;
    public float speed = 10f;
    public float skillMultiplier = 1.2f;

    // 초기화 함수 (Vector3 위치 + Transform 전달)
    public void Initialize(Vector3 targetPosition, DragonData dragonData, Vector3 dragonPosition, CharacterData targetData, Transform targetTransform)
    {
        this.targetPosition = targetPosition;
        this.dragonData = dragonData;
        this.targetData = targetData;
        this.targetTransform = targetTransform; // 🔹 추가 저장

        // 용의 위치에서 타겟 중심으로 향하는 방향 계산
        Vector3 directionToTarget = targetPosition - dragonPosition;

        // 보정된 방향을 기준으로 회전 설정
        transform.rotation = Quaternion.LookRotation(directionToTarget.normalized);
    }

    void Update()
    {
        // 타겟 위치로 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);

        // 타겟에 도달 시
        if (Vector3.Distance(transform.position, targetPosition) < 0.3f)
        {
            if (targetTransform != null)
            {
                // Transform 전달하여 문제 해결!
                CombatManager.Instance.ProcessDragonAttack(dragonData, targetData, targetTransform, true, skillMultiplier);
            }

            // VFX 파괴 (필요시, 파티클 효과 등 추가 가능)
            Destroy(gameObject);
        }
    }
}
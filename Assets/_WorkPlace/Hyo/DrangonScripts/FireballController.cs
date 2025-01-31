using UnityEngine;

public class FireballController : MonoBehaviour
{
    private Transform targetPosition;
    private DragonData dragonData; // 공격자 데이터
    private CharacterData targetData; // 타겟 데이터
    public float speed = 10f;

    // 초기화 함수
    public void Initialize(Transform targetPosition, DragonData dragonData, Vector3 dragonPosition, CharacterData targetData)
    {
        this.targetPosition = targetPosition;
        this.dragonData = dragonData;
        this.targetData = targetData;

        // 용의 위치에서 몬스터로 향하는 방향 계산
        Vector3 directionToTarget = targetPosition.position - dragonPosition;

        // 용의 높이를 고려해서 타겟을 약간 위로 보정
        directionToTarget.y = targetPosition.position.y - dragonPosition.y;

        // 보정된 방향을 기준으로 회전 설정
        transform.rotation = Quaternion.LookRotation(directionToTarget.normalized);
    }

    void Update()
    {
        // 타겟 위치로 이동
        transform.position = Vector3.MoveTowards(transform.position, targetPosition.position, speed * Time.deltaTime);

        // 타겟에 도달 시
        if (Vector3.Distance(transform.position, targetPosition.position) < 0.1f)
        {
            CombatManager.Instance.ProcessDragonAttack(dragonData, targetData, targetPosition, true);

            // VFX 파괴 (필요시, 파티클 효과 등 추가 가능)
            Destroy(gameObject);
        }
    }
}
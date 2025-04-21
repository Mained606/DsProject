using UnityEngine;

public class LightningBlast : MonoBehaviour
{
    private float height;
    private Skills skills;
    [SerializeField] private LayerMask layer;

    private bool down;
    private bool up;

    private Vector3 spawnPosition;
    [SerializeField] private float effectHeightOffset;

    private void Start()
    {
        skills = SkillManager.Instance.GetSkill(EntityType.Player, "LightningBlast");
        spawnPosition = transform.position;

        // 위치 보정
        CalculationHeight();
    }

    private void Update()
    {
        //CalculationHeight();
    }

    private void CalculationHeight()
    {
        //down = Physics.Raycast(spawnPosition, Vector3.down, out RaycastHit downHit, 10f, layer);
        if (Physics.Raycast(spawnPosition, -transform.up, out RaycastHit downHit, layer))
        {
            //Debug.Log("downHit 위치 보정");
            spawnPosition.y = downHit.point.y; // offset 더하기
            transform.position = spawnPosition;
            return;
        }
        //up = Physics.Raycast(spawnPosition, Vector3.up, out RaycastHit upHit, 10f, layer);
        if (Physics.Raycast(spawnPosition, transform.up, out RaycastHit upHit, layer))
        {
            //Debug.Log("upHit 위치 보정");
            spawnPosition.y = upHit.point.y;
            transform.position = spawnPosition;
            return;
        }
    }
}

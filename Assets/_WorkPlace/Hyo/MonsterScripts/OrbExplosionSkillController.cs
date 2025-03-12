using System.Collections;
using UnityEngine;

public class OrbExplosionSkillController : MonoBehaviour
{
    public BossData bossData;
    public float skillMultiplier = 3f;
    private bool hasAttacked = false;
    private Collider triggerCollider;
    private Skills skills;
    
    private void OnEnable()
    {
        // 이펙트가 활성화될 때 초기화 작업이 필요하면 여기서 처리합니다.
        hasAttacked = false;
        StartCoroutine(ActivateTriggerAfterDelay(3f));
    }
    
    private void Awake()
    {
        triggerCollider = GetComponent<Collider>();
        if (triggerCollider == null)
        {
            Debug.LogError("Collider가 이 오브젝트에 존재하지 않습니다!");
            return;
        }

        triggerCollider.enabled = false; // 초기에는 트리거 비활성화
    }

    private void Start()
    {
        skills = SkillManager.SkillDatabase.bossSkills[0];
    }
    
    private IEnumerator ActivateTriggerAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        triggerCollider.enabled = true; // 지연 후 트리거 활성화
        yield return new WaitForSeconds(0.5f);
        triggerCollider.enabled = false; // 지연 후 트리거 활성화
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (hasAttacked)
            return;

        // 플레이어 태그("Player")를 가진 오브젝트인지 확인합니다.
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.name + "플레이어에게 닿음");
            // 플레이어의 CharacterData를 가져옵니다.
            CharacterData playerData = CharacterManager.PlayerCharacterData;
            if (playerData == null)
            {
                Debug.LogWarning("OrbExplosionSkillController: Player의 CharacterData가 null입니다.");
                return;
            }

            // 보스 데이터가 할당되어 있는지 확인합니다.
            if (bossData == null)
            {
                Debug.LogWarning("OrbExplosionSkillController: 보스 데이터가 할당되어 있지 않습니다.");
                return;
            }

            // CombatManager의 ProcessAttack을 호출하여 공격 처리를 실행합니다.
            // 매개변수: playerData, bossData, 플레이어의 Transform, 
            // isPlayerAttacking: false (보스가 공격하므로), isMagicAttack: false (물리 공격 예시), skillMultiplier
            
            CombatManager.Instance.ProcessAttack(
                playerData,
                bossData,
                other.transform,
                false,
                true,
                skills, 
                true
            );

            hasAttacked = true;
        }
    }
}

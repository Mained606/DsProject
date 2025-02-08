using System.Collections;
using UnityEngine;

public class OrbExplosionSkillController : MonoBehaviour
{
    public BossData bossData;
    public float skillMultiplier = 3f;
    private bool hasAttacked = false;
    
    private void OnEnable()
    {
        // 이펙트가 활성화될 때 초기화 작업이 필요하면 여기서 처리합니다.
        hasAttacked = false;
    }
    
    private IEnumerator OnTriggerEnter(Collider other)
    {
        if (hasAttacked)
            yield break;

        // 플레이어 태그("Player")를 가진 오브젝트인지 확인합니다.
        if (other.CompareTag("Player"))
        {
            Debug.Log(other.name + "플레이어에게 닿음");
            // 플레이어의 CharacterData를 가져옵니다.
            CharacterData playerData = CharacterManager.PlayerCharacterData;
            if (playerData == null)
            {
                Debug.LogWarning("OrbExplosionSkillController: Player의 CharacterData가 null입니다.");
                yield break;
            }

            // 보스 데이터가 할당되어 있는지 확인합니다.
            if (bossData == null)
            {
                Debug.LogWarning("OrbExplosionSkillController: 보스 데이터가 할당되어 있지 않습니다.");
                yield break;
            }

            // CombatManager의 ProcessAttack을 호출하여 공격 처리를 실행합니다.
            // 매개변수: playerData, bossData, 플레이어의 Transform, 
            // isPlayerAttacking: false (보스가 공격하므로), isMagicAttack: false (물리 공격 예시), skillMultiplier
            yield return new WaitForSeconds(3f);
            
            CombatManager.Instance.ProcessAttack(
                playerData,
                bossData,
                other.transform,
                false,
                true,
                1f * skillMultiplier, 
                true
            );

            hasAttacked = true;

            // 공격 후 이펙트를 제거하거나 비활성화합니다.
            Destroy(gameObject, 5f);
        }
    }
}

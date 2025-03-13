using System.Collections.Generic;
using UnityEngine;

public class WeaponAttack : MonoBehaviour
{
    private Collider weaponCollider;
    public string weaponId;

    public string toolTag;

    public HashSet<GameObject> DamagedTargets { get; set; } = new HashSet<GameObject>();

    //03.13 HJ 추가
    public Vector3 effectScale = new Vector3();

    private void Start()
    {
        weaponCollider = GetComponentInParent<Collider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        InteractableOb interactable = other.GetComponent<InteractableOb>();
        if (interactable != null)
        {
            interactable.Interact(toolTag); // 상호작용 호출
        }
        if (!weaponCollider.enabled) return;
        if (!DamagedTargets.Contains(other.gameObject))
        {
            DamagedTargets.Add(other.gameObject);
            
            // ================ 2025-02-07 09:18 HYO 코드 추가 ====================================================================================================================================
            if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
            {
                // BaseMonsterData에서 monsterOrBossData를 가져오고, 타입에 맞게 처리
                BaseMonsterData baseMonsterData = other.GetComponent<BaseMonsterData>();
                if (baseMonsterData != null)
                {
                    // monsterOrBossData가 MonsterData일 경우 처리
                    MonsterData enemyMonsterData = baseMonsterData.monsterOrBossData as MonsterData;
                    if (enemyMonsterData != null)
                    {
                        CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyMonsterData, other.transform, true, false);
                        return;  // MonsterData 처리 완료 후 반환
                    }

                    // monsterOrBossData가 BossData일 경우 처리
                    BossData enemyBossData = baseMonsterData.monsterOrBossData as BossData;
                    if (enemyBossData != null)
                    {
                        CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyBossData, other.transform, true, false);
                    }
                }
            }
            // ===================================================================================================================================================================================
            else
            {
                //Debug.Log("암튼 뭔갈 침");
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        DamagedTargets.Remove(other.gameObject);
    }



}

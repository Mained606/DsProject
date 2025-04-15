using System.Collections.Generic;
using UnityEngine;

public class BlazeScatter : MonoBehaviour
{
    private Skills skills;

    [SerializeField] private LayerMask layer;
    private Dictionary<Collider, int> enemyDamageCount = new Dictionary<Collider, int>();
    public int maxHits = 3;
    private bool playSound = false;

    private void Start()
    {
        skills = SkillManager.Instance.GetSkill(EntityType.Player, "BlazeScatter");

        Destroy(gameObject, skills.effectDuration);
    }

    private void OnTriggerStay(Collider other)
    {
        if (enemyDamageCount.ContainsKey(other))
        {
            if (enemyDamageCount[other] >= maxHits)
            {
                return;
            }

            enemyDamageCount[other]++;
        }
        else
        {
            enemyDamageCount.Add(other, 1);
        }

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
                    CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyMonsterData, other.transform, true, true, skills, false, skills.attribute, skills.debuffDuration, skills.debuffValue);
                    return;  // MonsterData 처리 완료 후 반환
                }

                // monsterOrBossData가 BossData일 경우 처리
                BossData enemyBossData = baseMonsterData.monsterOrBossData as BossData;
                if (enemyBossData != null)
                {
                    CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyBossData, other.transform, true, true, skills, false, skills.attribute, skills.debuffDuration, skills.debuffValue);
                }
            }
        }
    }
}


using System.Collections.Generic;
using UnityEngine;

public class TerraSurge : MonoBehaviour
{
    private Skills skills;

    private Dictionary<Collider, int> enemyDamageCount = new Dictionary<Collider, int>();
    [SerializeField] private int maxHits = 3;

    private Collider col;
    [SerializeField] private float colliderEnabledTime = 1f;
    private float timer = 0f;

    private void Start()
    {
        skills = SkillManager.Instance.GetSkill(EntityType.Player, "TerraSurge");
        col = GetComponent<BoxCollider>();
        if(col.enabled == false)
        {
            col.enabled = true;
        }
    }

    private void Update()
    {
        if(timer < colliderEnabledTime)
        {
            timer += Time.deltaTime;
        }
        else
        {
            col.enabled = false;
        }
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

        if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        {
            BaseMonsterData baseMonsterData = other.GetComponent<BaseMonsterData>();
            if (baseMonsterData != null)
            {
                MonsterData enemyMonsterData = baseMonsterData.monsterOrBossData as MonsterData;
                if (enemyMonsterData != null)
                {
                    CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyMonsterData, other.transform, true, true, skills);
                    return;
                }
                BossData enemyBossData = baseMonsterData.monsterOrBossData as BossData;
                if (enemyBossData != null)
                {
                    CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyBossData, other.transform, true, true, skills);
                }
            }
        }
    }
}

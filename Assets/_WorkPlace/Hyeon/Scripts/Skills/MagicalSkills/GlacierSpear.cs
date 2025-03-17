using System.Collections.Generic;
using UnityEngine;

public class GlacierSpear : MonoBehaviour
{
    private Skills skills;

    private Dictionary<Collider, int> enemyDamageCount = new Dictionary<Collider, int>();
    [SerializeField] private int maxHits = 3;

    [SerializeField] private Collider min_col;
    [SerializeField] private Collider max_col;
    [SerializeField] private float colliderEnabledTime = 0.5f;
    [SerializeField] private float colliderDisabledTime = 1f;
    private float enabledTimer = 0f;
    private float disabledTimer = 0f;

    private bool flag = false;

    private void Start()
    {
        skills = SkillManager.Instance.GetSkill(EntityType.Player, "GlacierSpear");
        if (!min_col.enabled)
        {
            min_col.enabled = true;
        }
        if(max_col.enabled)
        {
            max_col.enabled = false;
        }
    }

    private void Update()
    {
        if (enabledTimer >= colliderEnabledTime && !flag)
        {
            Debug.Log("큰 콜라이더 켜짐");
            max_col.enabled = true;
            flag = true;
        }
        else
        {
            enabledTimer += Time.deltaTime;
        }

        if(disabledTimer >= colliderDisabledTime)
        {
            Debug.Log("콜라이더 모두 꺼짐");
            min_col.enabled = false;
            max_col.enabled = false;
        }
        else
        {
            disabledTimer += Time.deltaTime;
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

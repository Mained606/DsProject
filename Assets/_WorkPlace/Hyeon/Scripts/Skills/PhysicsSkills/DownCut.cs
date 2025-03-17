using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DownCut : MonoBehaviour
{
    private Skills skills;
    private float damage;

    [SerializeField] private float attackRange = 5f;
    [SerializeField] private int maxTargets = 5;
    [SerializeField] private LayerMask layer;
    private float timer = 0f;
    private bool alreadyAttack = false;

    private void Start()
    {
        skills = SkillManager.Instance.GetSkill(EntityType.Player, "DownCut");
    }
    private void Update()
    {
        Attack();
    }

    private void Attack()
    {
        if(timer >= skills.particleDelay)
        {
            if (!alreadyAttack)
            {
                Collider[] hitColliders = Physics.OverlapSphere(transform.position, attackRange, layer)
                .OrderBy(hit => Vector3.Distance(transform.position, hit.transform.position))
                .Take(maxTargets)
                .ToArray();


                foreach (var hit in hitColliders)
                {
                    Debug.Log($"{hit.transform.name}");
                    alreadyAttack = true;
                    BaseMonsterData baseMonsterData = hit.transform.GetComponent<BaseMonsterData>();
                    if (baseMonsterData != null)
                    {
                        // monsterOrBossData가 MonsterData일 경우 처리
                        MonsterData enemyMonsterData = baseMonsterData.monsterOrBossData as MonsterData;
                        if (enemyMonsterData != null)
                        {
                            CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyMonsterData, hit.transform, true, true, skills, false, skills.attribute, skills.debuffDuration, skills.debuffValue);
                        }

                        // monsterOrBossData가 BossData일 경우 처리
                        BossData enemyBossData = baseMonsterData.monsterOrBossData as BossData;
                        if (enemyBossData != null)
                        {
                            CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyBossData, hit.transform, true, true, skills, false, skills.attribute, skills.debuffDuration, skills.debuffValue);
                        }


                    }
                }
            }
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

    }
}

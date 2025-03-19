using TreeEditor;
using UnityEngine;
using UnityEngine.VFX;

public class ElectroComet : MonoBehaviour
{
    private Skills skills;
    private VisualEffect vfx;
    [SerializeField] private LayerMask layer;

    private float attackRange;
    [SerializeField] private float attackInterval = 1f;
    private float timer = 0f;
    private Vector3 spawnPosition;

    private bool elementalAttack = false;

    private void Start()
    {
        skills = SkillManager.Instance.GetSkill(EntityType.Player, "ElectroComet");
        vfx = GetComponentInChildren<VisualEffect>();
        attackRange = vfx.GetFloat("Radius");
        spawnPosition = transform.position;
    }

    private void Update()
    {
        Attack();
    }

    private void Attack()
    {
        if(timer < attackInterval)
        {
            timer += Time.deltaTime;
            return;
        }

        Collider[] hitColliders = Physics.OverlapSphere(spawnPosition, attackRange, layer);
        foreach(Collider hit in hitColliders)
        {
            BaseMonsterData baseMonsterData = hit.transform.GetComponent<BaseMonsterData>();
            if (baseMonsterData != null)
            {
                // monsterOrBossData가 MonsterData일 경우 처리
                MonsterData enemyMonsterData = baseMonsterData.monsterOrBossData as MonsterData;
                if (enemyMonsterData != null)
                {
                    if (!elementalAttack)
                    {
                        CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyMonsterData, hit.transform, true, true, skills, false, skills.attribute, skills.debuffDuration, skills.debuffValue);
                        elementalAttack = true;
                    }
                    else
                    {
                        CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyMonsterData, hit.transform, true, true, skills, false, skills.attribute, 0f);
                    }
                }

                // monsterOrBossData가 BossData일 경우 처리
                BossData enemyBossData = baseMonsterData.monsterOrBossData as BossData;
                if (enemyBossData != null)
                {
                    if (!elementalAttack)
                    {
                        CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyBossData, hit.transform, true, true, skills, false, skills.attribute, skills.debuffDuration, skills.debuffValue);
                        elementalAttack = true;
                    }
                    else
                    {
                        CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyBossData, hit.transform, true, true, skills, false, skills.attribute, 0f);
                    }
                }


            }
        }
        timer = 0f;
    }
}

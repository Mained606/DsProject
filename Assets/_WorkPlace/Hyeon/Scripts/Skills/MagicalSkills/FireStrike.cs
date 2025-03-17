using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FireStrike : MonoBehaviour
{
    //private ParticleSystem _particleSystem;
    //private List<GameObject> attackedEnemy = new List<GameObject>();
    private Dictionary<GameObject, int> enemyDamageCount = new Dictionary<GameObject, int>();
    public int maxHits = 3;

    [SerializeField] private float multiplier = 0.1f;

    private float damage;
    private ElementalAttribute attribute;
    private Skills skills;

    private void Start()
    {
        //_particleSystem = GetComponent<ParticleSystem>();
        skills = SkillManager.Instance.GetSkill(EntityType.Player, "FireStrike");
        attribute = skills.attribute;
        damage = skills.currentDamage;
    }

    private void OnParticleCollision(GameObject other)
    {
        if (enemyDamageCount.ContainsKey(other))
        {
            if(enemyDamageCount[other] >= maxHits)
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
                    CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyMonsterData, other.transform, true, true, skills);
                    return;  // MonsterData 처리 완료 후 반환
                }

                // monsterOrBossData가 BossData일 경우 처리
                BossData enemyBossData = baseMonsterData.monsterOrBossData as BossData;
                if (enemyBossData != null)
                {
                    CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, enemyBossData, other.transform, true, true, skills);
                }
            }
        }
        // ===================================================================================================================================================================================
        // if (other.gameObject.layer == LayerMask.NameToLayer("Enemy"))
        // {
        //     MonsterData monster = other.GetComponent<Test1>().monster;
        //     BossData boss = other.GetComponent<Test1>().bossData;
        //     int monsterHP;
        //     if (monster != null)
        //     {
        //         monsterHP = monster.currentHp;
        //         // monster.TakeDamage(damage);
        //         // 2025-01-27 HYO ProcessAttack 로직 변경으로 매개변수에 물리 공격인지 마법 공격인지 확인 추가 및 스킬 배율 추가 true = 마법데미지, false = 물리데미지, 2f = 스킬 데미지 배율 --------------
        //         CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, monster, other.transform, true, true, 1f + multiplier);
        //         // ----------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //         Debug.LogWarning($"{monster.characterName}이 스킬맞음");
        //     }
        //     else if (boss != null)
        //     {
        //         monsterHP = boss.currentHp;
        //         CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, boss, other.transform, true, true, 1f + multiplier);
        //         Debug.LogWarning($"{boss.characterName}이 스킬맞음");
        //     }
        // }
        Debug.Log($"Enter : {other.name}");
    }
}

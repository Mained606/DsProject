using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class FireStrike : MonoBehaviour
{
    //private ParticleSystem _particleSystem;
    //private List<GameObject> attackedEnemy = new List<GameObject>();
    private Dictionary<GameObject, int> enemyDamageCount = new Dictionary<GameObject, int>();
    public int maxHits = 3;

    private Skills skills;

    private string[] skillSound = { "Fire_Heavy_Throw", "Mgc_Fire_Impact_01" };
    private float soundTimer = 0f;
    private bool playSound = false;

    private void Start()
    {
        //_particleSystem = GetComponent<ParticleSystem>();
        skills = SkillManager.Instance.GetSkill(EntityType.Player, "FireStrike");
        
    }

    private void Update()
    {
        PlaySound();
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

    private void PlaySound()
    {
        if(soundTimer >= skills.particleDelay)
        {
            if (!playSound)
            {
                SoundManager.Instance.PlayClipAtPoint(skillSound[0], transform.position, 0.4f, false);
                SoundManager.Instance.PlayClipAtPoint(skillSound[1], transform.position, 0.4f, false);
                playSound = true;
            }
        }
        else
        {
            soundTimer += Time.deltaTime;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UpperAttack : MonoBehaviour
{
    public float speed = 30;
    public float slowDownRate = 0.01f;
    public float detectingDistance = 0.1f;
    public float destroyDelay = 5;
    [SerializeField] private LayerMask layer;
    private Skills skills;
    private float timer = 0f;

    private Rigidbody rb;
    private bool stopped;

    private Dictionary<Collider, int> enemyDamageCount = new Dictionary<Collider, int>();
    public int maxHits = 3;
    private bool playSound = false;

    void Start()
    {
        //transform.position = new Vector3(transform.position.x, 0, transform.position.z);

        if (GetComponent<Rigidbody>() != null)
        {
            rb = GetComponent<Rigidbody>();
            StartCoroutine(SlowDown());
        }
        else
            //Debug.Log("No Rigidbody");

        skills = SkillManager.Instance.GetSkill(EntityType.Player, "UpperAttack");
        SoundManager.Instance.PlayClipAtPoint("Sword_Scratch", transform.position, 0.1f, false);

        Destroy(gameObject, skills.effectDuration);
    }

    private void FixedUpdate()
    {
        Shooting();
        if (!stopped)
        {
            RaycastHit hit;
            Vector3 distance = new Vector3(transform.position.x, transform.position.y + 2f, transform.position.z);
            if (Physics.Raycast(distance, transform.TransformDirection(-Vector3.up), out hit, 4f))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y, transform.position.z);
            }
            else
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z);
            }
            Debug.DrawRay(distance, transform.TransformDirection(-Vector3.up * detectingDistance), Color.red);
        }
    }

    IEnumerator SlowDown()
    {
        float t = 1;
        while (t > 0)
        {
            rb.linearVelocity = Vector3.Lerp(Vector3.zero, rb.linearVelocity, t);
            t -= slowDownRate;
            yield return new WaitForSeconds(0.1f);
        }

        stopped = true;
    }

    private void Shooting()
    {
        if (timer >= skills.particleDelay)
        {
            rb.linearVelocity = transform.forward * speed;
            if (!playSound)
            {
                SoundManager.Instance.PlayClipAtPoint("Water_Splash", transform.position, 0.1f, false);
                playSound = true;
            }
        }
        else
        {
            timer += Time.deltaTime;
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

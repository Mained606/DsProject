using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class UltimateDragon : MonoBehaviour
{
    public bool followGround = true;
    public float speed = 30;
    public float slowDownRate = 0.01f;
    public float detectingDistance = 7f;
    public float objectsToDetachDelay = 2;
    public List<GameObject> objectsToDetach = new List<GameObject>();
    [Space]
    public float erodeInRate = 0.06f;
    public float erodeOutRate = 0.03f;
    public float erodeRefreshRate = 0.01f;
    public float erodeAwayDelay = 1.25f;
    public List<SkinnedMeshRenderer> objectsToErode = new List<SkinnedMeshRenderer>();

    private Rigidbody rb;
    [SerializeField] private LayerMask groundLayer;
    private bool stopped;

    private Skills skills;
    private float timer = 0f;
    [SerializeField] private float groundHeightOffset = 2.5f;

    private Dictionary<Collider, int> enemyDamageCount = new Dictionary<Collider, int>();
    public int maxHits = 3;
    private bool playSound = false;

    private SkinnedMeshRenderer meshRenderer;
    [SerializeField] private List<GameObject> additionalEffects = new List<GameObject>();    // 필요에 따라 리스트로 바뀔 수 있음

    void Start()
    {
        //if (followGround)
        transform.position = new Vector3(transform.position.x, transform.position.y + groundHeightOffset, transform.position.z);

        if (GetComponent<Rigidbody>() != null)
        {
            rb = GetComponent<Rigidbody>();
            StartCoroutine(SlowDown());
        }
        else
            Debug.Log("No Rigidbody");

        if (objectsToDetach != null)
            StartCoroutine(DetachObjects());

        if (objectsToErode != null)
            StartCoroutine(ErodeObjects());

        skills = SkillManager.Instance.GetSkill(EntityType.Player, "UltimateDragon");
        meshRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
        if (meshRenderer.enabled)
        {
            meshRenderer.enabled = false;
        }

        Destroy(gameObject, skills.effectDuration);
    }

    private void FixedUpdate()
    {
        Shooting();
        if (!stopped && followGround)
        {
            RaycastHit hit;
            Vector3 distance = new Vector3(transform.position.x, transform.position.y + 1, transform.position.z);
            if (Physics.Raycast(distance, transform.TransformDirection(-Vector3.up), out hit, detectingDistance, groundLayer))
            {
                transform.position = new Vector3(transform.position.x, hit.point.y + groundHeightOffset, transform.position.z);
            }
            else
            {
                //transform.position = new Vector3(transform.position.x, 0, transform.position.z);
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

    IEnumerator DetachObjects()
    {
        yield return new WaitForSeconds(objectsToDetachDelay);

        for (int i = 0; i < objectsToDetach.Count; i++)
        {
            objectsToDetach[i].transform.parent = null;
            Destroy(objectsToDetach[i], objectsToDetachDelay);
        }
    }

    IEnumerator ErodeObjects()
    {
        for (int i = 0; i < objectsToErode.Count; i++)
        {
            float t = 1;
            while (t > 0)
            {
                t -= erodeInRate;
                for (int j = 0; j < objectsToErode[i].materials.Length; j++)
                {
                    objectsToErode[i].materials[j].SetFloat("_Erode", t);
                }
                yield return new WaitForSeconds(erodeRefreshRate);
            }
        }

        yield return new WaitForSeconds(erodeAwayDelay);

        for (int i = 0; i < objectsToErode.Count; i++)
        {
            float t = 0;
            while (t < 1)
            {
                t += erodeOutRate;
                for (int j = 0; j < objectsToErode[i].materials.Length; j++)
                {
                    objectsToErode[i].materials[j].SetFloat("_Erode", t);
                }
                yield return new WaitForSeconds(erodeRefreshRate);
            }
        }
    }

    private void Shooting()
    {
        if (timer >= skills.particleDelay)
        {
            rb.linearVelocity = transform.forward * speed;
            AdditionalEffectOn();
            if (!playSound)
            {
                //SoundManager.Instance.PlayClipAtPoint("Water_Splash", transform.position, 0.1f, false);
                playSound = true;
            }
            if (!meshRenderer.enabled)
            {
                meshRenderer.enabled = true;
            }
        }
        else
        {
            timer += Time.deltaTime;
        }
    }

    private void AdditionalEffectOn()
    {
        if(additionalEffects.Count > 0)
        {
            foreach(var effect in additionalEffects)
            {
                effect.SetActive(true);
            }
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
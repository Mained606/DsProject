using UnityEngine;
using static UnityEngine.ParticleSystem;

public class RapidFireballSkillController : MonoBehaviour
{
    public BossData bossData;
    public float skillMultiplier = 1f;
    private int maxHit = 4;
    private int currentHit = 0;
    private Skills skills;
    // private ParticleSystem particleSystem;

    private void Start()
    {
        // particleSystem = GetComponent<ParticleSystem>();
        skills = SkillManager.SkillDatabase.bossSkills[1];
    }
    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("보스 래피드 파이어 시작");
        
        if(other.gameObject.layer == LayerMask.NameToLayer("Ds Player"))
        {
            if (currentHit >= maxHit) return;
            
            CombatManager.Instance.ProcessAttack(CharacterManager.PlayerCharacterData, bossData, other.transform, false, true, skills, true );
            currentHit++;
            Debug.Log("보스 래피드 파이어 Hit" + currentHit);
        }
    }
    
}

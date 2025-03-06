using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// ===== 2025-02-01 11:08 HYO 코드 추가 ====
public enum EntityType
{
    Player,
    Dragon,
    Boss
}

public class BuffInfo {
    public Coroutine coroutine;
    public float startTime;  // 버프가 시작된 시간
    public float duration;   // 버프 지속시간
}

public class SkillManager : BaseManager<SkillManager>
{
    [SerializeField] private SkillList skillDatabase;
    public static SkillList SkillDatabase => Instance.skillDatabase;

    // <(엔티티 타입, 스킬 이름), 스킬 정보> 형태로 모든 스킬 저장
    private Dictionary<(EntityType, string), Skills> skillList = new Dictionary<(EntityType, string), Skills>();
    public static Dictionary<(EntityType, string), Skills> SkillList => Instance.skillList;

    private Dictionary<Skills, Image> activeSkill = new Dictionary<Skills, Image>();
    private Dictionary<string, BuffInfo> activeBuffs = new Dictionary<string, BuffInfo>();
    private Dictionary<Skills, Coroutine> blinkCoroutines = new Dictionary<Skills, Coroutine>();
    private Dictionary<Skills, bool> skillBlinkState = new Dictionary<Skills, bool>();

    [SerializeField] private List<Skills> currentUsedSkills = new List<Skills>();

    private Animator animator;
    [SerializeField] private GameObject[] skillImage;
    [SerializeField] private Transform[] skillPanel;
    [Header ("✅ 버프깜박임관련 알파값")]
    [SerializeField] private float blinkInterval = 0.075f;
    [Header("✅ 디버그용")]
    public bool isActivating;
    private int currentMp;

    protected override void Awake()
    {
        base.Awake();
        InitializeSkillDictionary();
    }

    protected override void Start()
    {
        base.Start();
        animator = GameManager.playerTransform.GetComponent<PlayerController>().PlayerAnimator;
        UpdateCurrentMana();
    }

    private void Update()
    {
        CheckSkillCoolTime();
    }

    private void InitializeSkillDictionary()
    {
        RegisterSkills(EntityType.Player, skillDatabase.playerSkills);
        RegisterSkills(EntityType.Dragon, skillDatabase.dragonSkills);
        RegisterSkills(EntityType.Boss, skillDatabase.bossSkills);
    }

    private void RegisterSkills(EntityType entityType, List<Skills> skills)
    {
        foreach (var skill in skills)
        {
            var key = (entityType, skill.skillName);
            if (!skillList.ContainsKey(key))
            {
                skillList[key] = skill;
                skill.Initialize();
            }
        }
    }

    public Skills GetSkill(EntityType entityType, string skillName)
    {
        return skillList.TryGetValue((entityType, skillName), out var skill) ? skill : null;
    }

    public bool CanActivateSkill(EntityType entityType, string skillName)
    {
        if (isActivating) return false;

        Skills skill = GetSkill(entityType, skillName);
        if (skill == null || skill.cooldownTimer.IsRunning) return false;

        return true;
    }

    public void ActivateSkillForEntity(EntityType entityType, string skillName, GameObject target = null, Transform useTransform = null)
    {
        if (isActivating)
        {
            Debug.Log("현재 스킬이 활성화 중이므로 새로운 스킬을 사용할 수 없습니다.");
            return;
        }

        Skills skill = GetSkill(entityType, skillName);
        if (skill == null)
        {
            Debug.LogError($"[ActivateSkill] 스킬을 찾을 수 없음: {skillName}");
            return;
        }

        if (skill.cooldownTimer.IsRunning)
        {
            Debug.Log($"{skill.skillName} is on cooldown! (남은 시간: {skill.cooldownTimer.RemainingTime})");
            return;
        }

        if (entityType == EntityType.Player)
        {
            PlayerController playerController = GameManager.playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.isUseSkill = true;
                target = GameManager.playerTransform.gameObject;
            }
        }

        if (target == null)
        {
            target = GameManager.playerTransform.gameObject;
        }

        isActivating = true;
        CharacterManager.PlayerCharacterData.UseMp(skill.energyCost);
        TimerManager.Instance.StartTimer(skill.cooldownTimer);
        StartCoroutine(ExecuteSkill(entityType, skill, target, useTransform));
    }

    private IEnumerator ExecuteSkill(EntityType entityType, Skills skill, GameObject target, Transform useTransform = null)
    {

        Vector3 spawnPosition = target.transform.position;

        Animator entityAnimator = GetEntityAnimator(entityType);
        if (entityAnimator != null)
        {
            entityAnimator.SetTrigger(skill.activeTriggerName);
        }

        if (skill.effectPrefab != null)
        {
            Quaternion entityRotation = Quaternion.LookRotation(target.transform.forward);
            var effect = Instantiate(skill.effectPrefab, spawnPosition, entityRotation);
            switch (effect.name)
            {
                case "OrbExplosion(Clone)":
                    OrbExplosionSkillController obrctrl = effect.GetComponentInChildren<OrbExplosionSkillController>();
                    if (useTransform != null)
                    {
                        obrctrl.bossData = useTransform.transform.GetComponent<BaseMonsterData>().GetBossData();
                    }
                    break;
                case "RapidFireball(Clone)":
                    RapidFireballSkillController rapidf = effect.GetComponentInChildren<RapidFireballSkillController>();
                    if (useTransform != null)
                    {
                        rapidf.bossData = useTransform.transform.GetComponent<BaseMonsterData>().GetBossData();
                    }
                    break;
                case "JumpSkill(Clone)":
                    JumpSkillController jumpSkillCtrl = effect.GetComponentInChildren<JumpSkillController>();
                    if (useTransform != null)
                    {
                        jumpSkillCtrl.bossData = useTransform.transform.GetComponent<BaseMonsterData>().GetBossData();
                    }
                    break;
            }
            if (skill.particleDelay > 0)
            {
                var particleEffect = effect.GetComponent<ParticleSystem>().main;
                particleEffect.startDelay = skill.particleDelay;
            }
            Destroy(effect, 5f);
        }

        float animationLength = GetAnimationClipLength(entityAnimator, skill.activeTriggerName);
        yield return new WaitForSeconds(animationLength + 0.5f);

        isActivating = false;

        if (entityType == EntityType.Player)
        {
            PlayerController playerController = GameManager.playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.isUseSkill = false;
            }
        }
    }

    private float GetAnimationClipLength(Animator animator, string triggerName)
    {
        if (animator == null) return 0;
        return animator.runtimeAnimatorController.animationClips
            .FirstOrDefault(clip => clip.name == triggerName)?.length ?? 0.5f;
    }

    private Animator GetEntityAnimator(EntityType entityType)
    {
        return entityType switch
        {
            EntityType.Player => GameManager.playerTransform?.GetComponentInChildren<Animator>(),
            EntityType.Dragon => GameManager.DragonTransform?.GetComponent<Animator>(),
            // EntityType.Boss => GameManager.BossTransform?.GetComponent<Animator>(),
            _ => null
        };
    }

    public void CheckSkillCoolTime()
    {
        List<Skills> skillsToRemove = new List<Skills>();

        foreach (var skill in skillList.Values)
        {
            if (!skill.cooldownTimer.IsRunning) continue;

            if (skill.cooldownTimer.RemainingTime <= 0.1f)
            {
                if (activeSkill.ContainsKey(skill))
                {
                    Destroy(activeSkill[skill].transform.parent.gameObject);
                    activeSkill.Remove(skill);
                }
                skillsToRemove.Add(skill);
                skill.cooldownTimer.Stop();
            }
            else
            {
                if (skill.entityType != EntityType.Player) UpdateSkillCooldownUI(skill);
            }
        }

        foreach (var skill in skillsToRemove)
        {
            currentUsedSkills.Remove(skill);
            CharacterData targetCharacter = skill.entityType switch
            {
                EntityType.Dragon => CharacterManager.PlayerCharacterData,
                //EntityType.Player => CharacterManager.PlayerCharacterData,
                //EntityType.Boss => CharacterManager.BossCharacterData,
                _ => null
            };

            if (targetCharacter == null) return;

            if (activeBuffs.ContainsKey(skill.skillName))
            {
                BuffInfo existingBuff = activeBuffs[skill.skillName];
                RemoveBuffEffect(skill.skillName, targetCharacter);
                StopCoroutine(existingBuff.coroutine);
                activeBuffs.Remove(skill.skillName);
                targetCharacter.UpdateDerivedStats();
                if (targetCharacter.maxHp < targetCharacter.currentHp)
                {
                    targetCharacter.currentHp = targetCharacter.maxHp;
                }
            }
        }
    }

    private void UpdateSkillCooldownUI(Skills skill)
    {
        if (!activeSkill.ContainsKey(skill))
        {
            var skillImg = Instantiate(skillImage[skill.skillType == SkillType.Support ? 0 : 1],
                                       skill.entityType != EntityType.Boss ? skillPanel[0] : skillPanel[1]);
            skillImg.AddComponent<CanvasGroup>();
            activeSkill[skill] = skillImg.transform.GetChild(1).GetComponent<Image>();
            activeSkill[skill].sprite = ItemManager.Instance.GetSkillSprite(skill.skillName);
        }

        if (skill.cooldownTimer.RemainingTime > 1f && skill.cooldownTimer.RemainingTime < 10f)
        {
            if (blinkCoroutines.ContainsKey(skill) && blinkCoroutines[skill] != null)
            {
                StopCoroutine(blinkCoroutines[skill]);
                blinkCoroutines.Remove(skill);
            }
            blinkCoroutines[skill] = StartCoroutine(BlinkSkillIcon(skill));
        }
    }

    private IEnumerator BlinkSkillIcon(Skills skill)
    {
        if (!activeSkill.ContainsKey(skill))
        {
            Debug.LogError($"Skill {skill.skillName} is missing in activeSkill dictionary.");
            yield break;
        }

        CanvasGroup canvasGroup = activeSkill[skill].transform.parent.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError($"CanvasGroup is missing for skill {skill.skillName}");
            yield break;
        }

        if (!skillBlinkState.ContainsKey(skill))
            skillBlinkState[skill] = true;

        while (skill.cooldownTimer.RemainingTime > 0f && skill.cooldownTimer.RemainingTime < 10f)
        {
            if (canvasGroup == null) yield break;

            if (skillBlinkState[skill])
            {
                canvasGroup.alpha = Mathf.Max(0f, canvasGroup.alpha - blinkInterval);
                if (Mathf.Approximately(canvasGroup.alpha, 0f)) skillBlinkState[skill] = false;
            }
            else
            {
                canvasGroup.alpha = Mathf.Min(1f, canvasGroup.alpha + blinkInterval);
                if (Mathf.Approximately(canvasGroup.alpha, 1f)) skillBlinkState[skill] = true;
            }
            yield return null;
        }
        blinkCoroutines.Remove(skill);
    }


    public List<string> GetAvailableSkills(EntityType entityType)
    {
        return skillList.Keys.Where(k => k.Item1 == entityType).Select(k => k.Item2).ToList();
    }

    public List<string> GetAvailableBuffs(EntityType entityType)
    {
        return skillList
            .Where(kv => kv.Key.Item1 == entityType && kv.Value.skillType == SkillType.Support)
            .Select(kv => kv.Key.Item2)
            .ToList();
    }

    private void UpdateCurrentMana()
    {
        currentMp = CharacterManager.PlayerCharacterData.currentMp;
    }
    
    public float GetCooldownForSkill(EntityType entityType, string skillName)
    {
        if (!skillList.TryGetValue((entityType, skillName), out Skills skill))
        {
            Debug.LogError($"[GetCooldownForSkill] 스킬을 찾을 수 없음: {skillName}");
            return 60f;
        }
        return skill.cooldown;
    }

    public bool CheckMana(EntityType entity, string skillName)
    {
        UpdateCurrentMana();

        Skills skill = GetSkill(entity, skillName);
        if (skill == null)
        {
            Debug.LogError($"CheckMana 오류: {skillName} 스킬이 SkillManager에 등록되지 않았습니다!");
            return false;
        }

        if (currentMp < skill.energyCost)
        {
            Debug.Log($"MP 부족: {skillName} 사용 불가 (현재 MP: {currentMp}, 필요 MP: {skill.energyCost})");
            return false;
        }

        return true;
    }

    // ========================== 🛠️ 버프 기능 추가 ==========================
    public void ApplyBuff(EntityType entityType, string skillName)
    {
        Skills skill = GetSkill(entityType, skillName);
        if (skill == null) return;

        CharacterData targetCharacter = entityType switch
        {
            EntityType.Dragon => CharacterManager.PlayerCharacterData,
            //EntityType.Player => CharacterManager.PlayerCharacterData,
            //EntityType.Boss => CharacterManager.BossCharacterData,
            _ => null
        };

        if (targetCharacter == null) return;

        if (activeBuffs.ContainsKey(skillName))
        {
            BuffInfo existingBuff = activeBuffs[skillName];
            RemoveBuffEffect(skillName, targetCharacter);
            StopCoroutine(existingBuff.coroutine);
            activeBuffs.Remove(skillName);
        }

        BuffInfo newBuffInfo = new BuffInfo
        {
            startTime = Time.time,
            duration = skill.buffDuration,
            coroutine = StartCoroutine(RemoveBuff(skillName, skill.buffDuration, targetCharacter))
        };

        activeBuffs[skillName] = newBuffInfo;
        ApplyBuffEffect(skillName, targetCharacter);
        targetCharacter.UpdateDerivedStats();
        if (skill.effectPrefab != null)
        {
            InstantiateBuffEffect(skill.effectPrefab, GameManager.playerTransform.position);
        }

        TimerManager.Instance.StartTimer(skill.cooldownTimer);
    }

    private void ApplyBuffEffect(string skillName, CharacterData targetCharacter)
    {
        switch (skillName)
        {
            case "PlayerBuffPhysical":
                targetCharacter.physicalDamageBuffMultiplier *= 1.2f;
                break;
            case "PlayerBuffMagic":
                targetCharacter.magicDamageBuffMultiplier *= 1.2f;
                break;
            case "PlayerBuffHP":
                targetCharacter.hpBuffBonus += 100;
                targetCharacter.currentHp = Mathf.Min(targetCharacter.currentHp + 100, targetCharacter.maxHp);
                break;
        }
    }

    private void RemoveBuffEffect(string skillName, CharacterData targetCharacter)
    {
        switch (skillName)
        {
            case "PlayerBuffPhysical":
                targetCharacter.physicalDamageBuffMultiplier /= 1.2f;
                break;
            case "PlayerBuffMagic":
                targetCharacter.magicDamageBuffMultiplier /= 1.2f;
                break;
            case "PlayerBuffHP":
                targetCharacter.hpBuffBonus -= 100;
                break;
        }
    }

    private void InstantiateBuffEffect(GameObject effectPrefab, Vector3 targetPosition)
    {
        var effect = Instantiate(effectPrefab, targetPosition, Quaternion.identity);
        effect.transform.SetParent(GameManager.playerTransform);
        Destroy(effect, 3f);
    }

    private IEnumerator RemoveBuff(string skillName, float duration, CharacterData targetCharacter)
    {
        yield return new WaitForSeconds(duration);
        RemoveBuffEffect(skillName, targetCharacter);
        if (targetCharacter.maxHp < targetCharacter.currentHp)
        {
            targetCharacter.currentHp = targetCharacter.maxHp;
        }
        activeBuffs.Remove(skillName);
        Debug.Log(skillName + " 버프 종료");
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

    }
}

using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EntityType
{
    Player,
    Dragon,
    Boss
}

public class SkillManager : BaseManager<SkillManager>
{
    [SerializeField] private SkillList skillDatabase;
    
    // 2025-02-01 11:08 HYO 코드 추가 -----------------------------------------------------------------------
    [SerializeField] private SkillList dragonSkillDatabase;
    [SerializeField] private SkillList bossSkillDatabase;

    private Dictionary<string, Skills> dragonSkillDictionary = new Dictionary<string, Skills>();
    private Dictionary<string, Skills> bossSkillDictionary = new Dictionary<string, Skills>();
    // -----------------------------------------------------------------------------------------------------
    
    private Dictionary<string, Skills> skillDictionary = new Dictionary<string, Skills>();
    [SerializeField] private List<Skills> currentUsedSkills = new List<Skills>();

    private Animator animator;
    [SerializeField] private GameObject skillImage;
    [SerializeField] private Transform skillPanel;
    public bool isActivating;
    private int currentMp;

    protected override void OnEnable()
    {
        base.OnEnable();

    }
    protected override void Awake()
    {
        base.Awake();
        
        // 2025-02-01 11:08 HYO 코드 추가 -----------------------------------------
        InitializeSkillDictionary(skillDatabase.playerSkills, skillDictionary);
        InitializeSkillDictionary(skillDatabase.dragonSkills, dragonSkillDictionary);
        InitializeSkillDictionary(skillDatabase.bossSkills, bossSkillDictionary);
        //-----------------------------------------------------------------------
        
        // 예전 코드 ---------------------
        // InitializeSkillDictionary();
        // -----------------------------
        
        Debug.Log($"PlayerSkill_1 존재 여부: {skillDictionary.ContainsKey("PlayerSkill_1")}");
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
    
    // 2025-02-01 11:08 HYO 코드 추가 ----------------------------------------------------------------------------------
    // 스킬을 Dictionary에 저장하는 함수
    private void InitializeSkillDictionary(List<Skills> skills, Dictionary<string, Skills> skillDictionary)
    {
        foreach (var skill in skills)
        {
            if (!skillDictionary.ContainsKey(skill.skillName))
            {
                skillDictionary.Add(skill.skillName, skill);
                skill.Initialize();
            }
        }
    }

    // -----------------------------------------------------------------------------------------------------
    
    // -------------------  예전 코드 -----------------------------------------------------------------------
    // private void InitializeSkillDictionary()
    // {
    //     foreach(var skill in skillDatabase.skillList)
    //     {
    //         if (!skillDictionary.ContainsKey(skill.skillName))
    //         {
    //             skillDictionary.Add(skill.skillName, skill);
    //             skill.Initialize();
    //             Debug.Log($"skillDatabase: {skillDictionary.ContainsKey(skill.skillName)}");
    //         }
    //         else
    //         {
    //             Debug.Log($"중복된 스킬 이름 : {skill.skillName}이(가) 데이터베이스에 이미 존재합니다.");
    //         }
    //     }
    //     
    // }
    // ---------------------------------------------------------------------------------------------------
    
    // -----------------------2025-02-01 11:08 HYO 코드 추가 ----------------------------------------------
    public Skills GetSkill(EntityType entityType, string skillName)
    {
        Dictionary<string, Skills> selectedDictionary = entityType switch
        {
            EntityType.Player => skillDictionary,
            EntityType.Dragon => dragonSkillDictionary,
            EntityType.Boss => bossSkillDictionary,
            _ => null
        };

        if (selectedDictionary != null && selectedDictionary.TryGetValue(skillName, out var skill))
        {
            return skill;
        }

        return null;
    }
    // -----------------------------------------------------------------------------------------------------

    // -------------------  예전 코드 -----------------------------------------------------------------------
    // public Skills GetSkill(string skillName)
    // {
    //     if(skillDictionary.TryGetValue(skillName, out var skill))
    //     {
    //         return skill;
    //     }
    //
    //     return null;
    // }
    // -----------------------------------------------------------------------------------------------------
    
    public void ActivateSkill(string skillName, GameObject target = null)
    {
        if(target == null)
        {
            target = GameManager.playerTransform.gameObject;
        }
        Skills skill = GetSkill(EntityType.Player, skillName);
        isActivating = activeSkill.ContainsKey(skill);
        UpdateCurrentMana();
        if (isActivating)
        {
            Debug.Log("isActivating true");
            return;
        }
        else
        {
            CharacterManager.PlayerCharacterData.UseMp(skill.energyCost);
            UpdateCurrentMana();
            TimerManager.Instance.StartTimer(skill.cooldownTimer);
            animator.SetTrigger(skill.activeTriggerName);
            if (skill.effectPrefab != null)
            {
                Quaternion playerRotation = Quaternion.LookRotation(GameManager.playerTransform.forward);
                var effect = Instantiate(skill.effectPrefab, target.transform.position, playerRotation);
                if (skill.particleDelay > 0)
                {
                    var particleEffect = effect.GetComponent<ParticleSystem>().main;
                    particleEffect.startDelay = skill.particleDelay;
                }

                Destroy(effect, 5f);

            }
            //Debug.Log($"ActivateSkill: {skill.skillName}");
            currentUsedSkills.Add(skill);
        }
    }

    private Dictionary<Skills, Image> activeSkill = new Dictionary<Skills, Image>();
    
    public void CheckSkillCoolTime()
    {
        List<Dictionary<string, Skills>> allDictionaries = new()
        {
            skillDictionary, dragonSkillDictionary, bossSkillDictionary
        };

        foreach (var dict in allDictionaries)
        {
            foreach (var skill in dict.Values)
            {
                if (skill.cooldownTimer.RemainingPercent <= 0)
                {
                    if (activeSkill.ContainsKey(skill))
                    {
                        Destroy(activeSkill[skill].gameObject);
                        activeSkill.Remove(skill);
                    }
                }
                else
                {
                    Image skillCoolImage;
                    TextMeshProUGUI skillCoolName;

                    if (!activeSkill.ContainsKey(skill))
                    {
                        var _skillImage = Instantiate(skillImage, skillPanel);
                        skillCoolImage = _skillImage.GetComponent<Image>();
                        skillCoolName = _skillImage.GetComponentInChildren<TextMeshProUGUI>();
                        activeSkill.Add(skill, skillCoolImage);
                    }
                    else
                    {
                        skillCoolImage = activeSkill[skill];
                        skillCoolName = activeSkill[skill].GetComponentInChildren<TextMeshProUGUI>();
                    }

                    skillCoolImage.fillAmount = skill.cooldownTimer.RemainingPercent;
                    skillCoolName.text = skill.skillName;
                }
            }
        }
    }

    // ----------- 이전 코드 ---------------------------------------------------------------------------------
    // public void CheckSkillCoolTime()
    // {
    //     if (currentUsedSkills.Count > 0)
    //     {
    //         for(int i = currentUsedSkills.Count - 1; i>=0; i--)
    //         {
    //             var skill = currentUsedSkills[i];
    //             if (skill.cooldownTimer.RemainingPercent <= 0)
    //             {
    //                 if (activeSkill.ContainsKey(skill))
    //                 {
    //                     Destroy(activeSkill[skill].gameObject);
    //                     activeSkill.Remove(skill);
    //                     currentUsedSkills.Remove(skill);
    //                 }
    //
    //             }
    //             else
    //             {
    //                 Image skillCoolImage = null;
    //                 TextMeshProUGUI skillCoolName = null;
    //                 if (!activeSkill.ContainsKey(skill))
    //                 {
    //                     var _skillImage = Instantiate(skillImage, skillPanel);
    //                     skillCoolImage = _skillImage.GetComponent<Image>();
    //                     skillCoolName = _skillImage.GetComponentInChildren<TextMeshProUGUI>();
    //                     activeSkill.Add(skill, skillCoolImage);
    //                 }
    //                 else
    //                 {
    //                     skillCoolImage = activeSkill[skill];
    //                     skillCoolName = activeSkill[skill].GetComponentInChildren<TextMeshProUGUI>();
    //                 }
    //
    //                 skillCoolImage.fillAmount = skill.cooldownTimer.RemainingPercent;
    //                 skillCoolName.text = skill.skillName;
    //             }
    //         }
    //     }
    // }
    // ----------------------------------------------------------------------------------------------------------
    
    public bool CheckMana(string skillName)
    {
        Skills skill = GetSkill(EntityType.Player, skillName);
            
        if (skill == null)  // 🚨 skill이 null이면 바로 로그 출력 후 리턴
        {
            Debug.LogError($"CheckMana 오류: {skillName} 스킬이 SkillManager에 등록되지 않았습니다!");
            return false;
        }
        
        if (currentMp < skill.energyCost)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void UpdateCurrentMana()
    {
        currentMp = CharacterManager.PlayerCharacterData.currentMp;
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

    }
    
    // ----------------- 2025-02-01 10:44 HYO 용 및 보스 전용 코드 추가 ----------------------------------------------------
    public void ActivateSkillForEntity(EntityType entityType, string skillName, GameObject target = null)
    {
        Skills skill = GetSkill(entityType, skillName);
        if (skill == null) return;

        ExecuteSkill(entityType, skill, target);
    }
    
    // 공통 스킬 실행 로직 (애니메이션, 효과, 쿨다운 포함)
    private void ExecuteSkill(EntityType entityType, Skills skill, GameObject target)
    {
        if (target == null)
        {
            target = GameManager.playerTransform.gameObject;
        }
        
        if (skill.cooldownTimer.RemainingTime > 0)
        {
            Debug.Log($"{skill.skillName} is on cooldown!");
            return;
        }

        // 애니메이션 실행
        if (entityType == EntityType.Player)
        {
            animator.SetTrigger(skill.activeTriggerName);
        }
        else if (entityType == EntityType.Dragon || entityType == EntityType.Boss)
        {
            Animator entityAnimator = GetEntityAnimator(entityType);
            if (entityAnimator != null)
            {
                entityAnimator.SetTrigger(skill.activeTriggerName);
            }
        }

        // 스킬 효과 적용
        if (skill.effectPrefab != null)
        {
            Quaternion entityRotation = Quaternion.LookRotation(target.transform.forward);
            var effect = Instantiate(skill.effectPrefab, target.transform.position, entityRotation);
            Destroy(effect, 5f);
        }

        // 스킬 쿨다운 시작
        TimerManager.Instance.StartTimer(skill.cooldownTimer);
    }
    
    private Animator GetEntityAnimator(EntityType entityType)
    {
        if (entityType == EntityType.Dragon)
        {
            return GameManager.DragonTransform?.GetComponent<Animator>();
        }
        
        else if (entityType == EntityType.Boss)
        {
            // return GameManager.BossTransform?.GetComponent<Animator>();
        }
        
        return null;
    }
    
    private Dictionary<string, Coroutine> activeBuffs = new Dictionary<string, Coroutine>();

    public void ApplyBuff(EntityType entityType, string skillName)
    {
        Skills skill = GetSkill(entityType, skillName);
        if (skill == null) return;
        
        // 🛠️ 버프 적용 대상 설정 (용 → 플레이어, 보스 → 보스 자신)
        CharacterData targetCharacter = entityType switch
        {
            EntityType.Dragon => CharacterManager.PlayerCharacterData,
            // EntityType.Boss => CharacterManager.BossCharacterData, // 보스 데이터 추가
            _ => null
        };

        if (targetCharacter == null) return;
        
        if (activeBuffs.ContainsKey(skillName))
        {
            StopCoroutine(activeBuffs[skillName]);
        }

        // 버프 지속시간을 연장하는 방식으로 수정
        activeBuffs[skillName] = StartCoroutine(RemoveBuffAfterDuration(skillName, skill.buffDuration, targetCharacter));

        // 버프 효과 적용
        switch (skillName)
        {
            case "DragonBuffAttack":
                targetCharacter.physicalDamage *= 1.2f; // 20% 증가
                break;

            case "DragonBuffDefense":
                targetCharacter.physicalDefense *= 1.2f; // 20% 증가
                break;

            case "DragonBuffHP":
                targetCharacter.maxHp += 100;
                if (targetCharacter.currentHp > targetCharacter.maxHp)
                    targetCharacter.currentHp = targetCharacter.maxHp;
                break;
        }
        
        // 버프 해제 코루틴 실행 & 등록
        Coroutine buffCoroutine = StartCoroutine(RemoveBuffAfterDuration(skillName, skill.buffDuration, targetCharacter));
        activeBuffs[skillName] = buffCoroutine;
    }

    // 🔄 일정 시간이 지나면 버프 해제
    private IEnumerator RemoveBuffAfterDuration(string skillName, float duration, CharacterData targetCharacter)
    {
        yield return new WaitForSeconds(duration);

        switch (skillName)
        {
            case "DragonBuffAttack":
                targetCharacter.physicalDamage /= 1.2f;
                break;

            case "DragonBuffDefense":
                targetCharacter.physicalDefense /= 1.2f;
                break;

            case "DragonBuffHP":
                targetCharacter.maxHp -= 100;
                if (targetCharacter.currentHp > targetCharacter.maxHp)
                    targetCharacter.currentHp = targetCharacter.maxHp;
                break;
        }

        // 🛑 버프가 해제되었으므로 딕셔너리에서 제거
        activeBuffs.Remove(skillName);
    }

// ---------------------------------------------------------------------------------------------------------------------
}

using System.Collections;
using System.Collections.Generic;
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
// ========================================
public class SkillManager : BaseManager<SkillManager>
{
    [SerializeField] private SkillList skillDatabase;
    
    // ==========================2025-02-01 11:08 HYO 코드 추가 =============================================
    private Dictionary<string, Skills> playerSkillDictionary = new Dictionary<string, Skills>();
    private Dictionary<string, Skills> dragonSkillDictionary = new Dictionary<string, Skills>();
    private Dictionary<string, Skills> bossSkillDictionary = new Dictionary<string, Skills>();
    
    private Dictionary<Skills, Image> activeSkill = new Dictionary<Skills, Image>();
    
    // =====================================================================================================
    
    // ============================= 예전 코드 ==================================================
    // private Dictionary<string, Skills> skillDictionary = new Dictionary<string, Skills>();
    // ========================================================================================

    [SerializeField] private List<Skills> currentUsedSkills = new List<Skills>();

    private Animator animator;
    [SerializeField] private GameObject skillImage;
    [SerializeField] private Transform skillPanel;
    public bool isActivating;
    private int currentMp;

    protected override void Awake()
    {
        base.Awake();
        
        // ================= 2025-02-01 11:08 HYO 코드 추가 ===========================
        // 플레이어, 드래곤, 보스 스킬을 각각 Dictionary에 저장
        InitializeSkillDictionary(skillDatabase.playerSkills, playerSkillDictionary);
        InitializeSkillDictionary(skillDatabase.dragonSkills, dragonSkillDictionary);
        InitializeSkillDictionary(skillDatabase.bossSkills, bossSkillDictionary);
        //===========================================================================
        
        // ======== 예전 코드 ============
        // InitializeSkillDictionary();
        // ==============================
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
    
    // ========================== 2025-02-01 11:08 HYO 코드 추가 ==============================================
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
    // =======================================================================================================
    
    // ==================== 예전 코드 =========================================================================
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
    // ======================================================================================================
    
    // ========================= 2025-02-01 11:08 HYO 코드 추가 ====================================================
    public Skills GetSkill(EntityType entityType, string skillName)
    {
        return entityType switch
        {
            EntityType.Player => playerSkillDictionary.TryGetValue(skillName, out var skill) ? skill : null,
            EntityType.Dragon => dragonSkillDictionary.TryGetValue(skillName, out var skill) ? skill : null,
            EntityType.Boss => bossSkillDictionary.TryGetValue(skillName, out var skill) ? skill : null,
            _ => null
        };
    }
    // ===========================================================================================================

    // ====================== 예전 코드 =================================
    // public Skills GetSkill(string skillName)
    // {
    //     if(skillDictionary.TryGetValue(skillName, out var skill))
    //     {
    //         return skill;
    //     }
    //
    //     return null;
    // }
    // ================================================================
    
    // ===================== 2025-02-01 10:44 HYO 코드 추가 =======================================================
    public void ActivateSkillForEntity(EntityType entityType, string skillName, GameObject target = null)
    {
        Debug.Log($"[ActivateSkillForEntity] {entityType}가 {skillName} 스킬을 사용하려 합니다.");
        
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
        
        Debug.Log($"[ActivateSkillForEntity] {skillName} 스킬을 정상적으로 찾았습니다.");
        
        if (skill.cooldownTimer.IsRunning)
        {
            Debug.Log($"{skill.skillName} is on cooldown! (남은 시간: {skill.cooldownTimer.RemainingTime})");
            return;
        }

        Debug.Log($"[ActivateSkillForEntity] {skillName} 스킬을 실행합니다.");
        
        isActivating = true; // 스킬 사용 중 플래그 설정
        TimerManager.Instance.StartTimer(skill.cooldownTimer); // 쿨다운 시작
        StartCoroutine(ExecuteSkill(entityType, skill, target)); // 코루틴 실행
    }
    
    // 공통 스킬 실행 로직 (애니메이션, 효과, 쿨다운 포함)
    private IEnumerator ExecuteSkill(EntityType entityType, Skills skill, GameObject target)
    {
        if (target == null)
        {
            target = GameManager.playerTransform.gameObject;
        }

        Animator entityAnimator = GetEntityAnimator(entityType);
        if (entityAnimator != null)
        {
            entityAnimator.SetTrigger(skill.activeTriggerName);
        }

        if (skill.effectPrefab != null)
        {
            Quaternion entityRotation = Quaternion.LookRotation(target.transform.forward);
            var effect = Instantiate(skill.effectPrefab, target.transform.position, entityRotation);
            Destroy(effect, 5f);
        }

        // 현재 실행 중인 애니메이션의 길이만큼 대기 (애니메이션이 끝날 때까지)
        float animationLength = GetAnimationClipLength(entityAnimator, skill.activeTriggerName);
        yield return new WaitForSeconds(animationLength);

        isActivating = false; // 스킬 사용 완료 후 다시 입력 가능
    }
    
    // 애니메이션 길이 가져오기
    private float GetAnimationClipLength(Animator animator, string triggerName)
    {
        if (animator == null) return 0;

        foreach (var clip in animator.runtimeAnimatorController.animationClips)
        {
            if (clip.name == triggerName)
            {
                return clip.length;
            }
        }
        return 0.5f; // 기본값 (애니메이션이 없을 경우)
    }
    
    private Animator GetEntityAnimator(EntityType entityType)
    {
        if (entityType == EntityType.Player)
        {
            return GameManager.playerTransform?.GetComponentInChildren<Animator>();
        }
        else if (entityType == EntityType.Dragon)
        {
            return GameManager.DragonTransform?.GetComponent<Animator>();
        }
        else if (entityType == EntityType.Boss)
        {
            // return GameManager.BossTransform?.GetComponent<Animator>();
        }
        
        return null;
    }
    //============================================================================================================
    
    // ====================== 예전 코드 ===========================================================================
    // public void ActivateSkill(string skillName, GameObject target = null)
    // {
    //     if(target == null)
    //     {
    //         target = GameManager.playerTransform.gameObject;
    //     }
    //     Skills skill = GetSkill(EntityType.Player, skillName);
    //     isActivating = activeSkill.ContainsKey(skill);
    //     UpdateCurrentMana();
    //     if (isActivating)
    //     {
    //         Debug.Log("isActivating true");
    //         return;
    //     }
    //     else
    //     {
    //         CharacterManager.PlayerCharacterData.UseMp(skill.energyCost);
    //         UpdateCurrentMana();
    //         TimerManager.Instance.StartTimer(skill.cooldownTimer);
    //         animator.SetTrigger(skill.activeTriggerName);
    //         if (skill.effectPrefab != null)
    //         {
    //             Quaternion playerRotation = Quaternion.LookRotation(GameManager.playerTransform.forward);
    //             var effect = Instantiate(skill.effectPrefab, target.transform.position, playerRotation);
    //             if (skill.particleDelay > 0)
    //             {
    //                 var particleEffect = effect.GetComponent<ParticleSystem>().main;
    //                 particleEffect.startDelay = skill.particleDelay;
    //             }
    //
    //             Destroy(effect, 5f);
    //
    //         }
    //         //Debug.Log($"ActivateSkill: {skill.skillName}");
    //         currentUsedSkills.Add(skill);
    //     }
    // }
    // ==========================================================================================================
    
    // ===================== 2025-02-01 10:44 HYO 코드 추가 ======================================================
    public void CheckSkillCoolTime()
    {
        List<Dictionary<string, Skills>> allDictionaries = new()
        {
            playerSkillDictionary, dragonSkillDictionary, bossSkillDictionary
        };
        
        List<Skills> skillsToRemove = new List<Skills>();

        foreach (var dict in allDictionaries)
        {
            foreach (var skill in dict.Values)
            {
                if (!skill.cooldownTimer.IsRunning) continue; // 동작 중인 타이머만 체크

                if (skill.cooldownTimer.RemainingPercent <= 0)
                {
                    if (activeSkill.ContainsKey(skill))
                    {
                        Destroy(activeSkill[skill].gameObject);
                        activeSkill.Remove(skill);
                    }
                    
                    skillsToRemove.Add(skill);
                    skill.cooldownTimer.Stop(); // 쿨타임 종료
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
        // 스킬 리스트에서 제거 (리스트를 수정할 때는 별도 리스트에 저장 후 제거)
        foreach (var skill in skillsToRemove)
        {
            currentUsedSkills.Remove(skill);
        }
    }
    // ========================================================================================================
    
    // ====================================== 이전 코드 ========================================================
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
    // ======================================================================================================
    
    // ===================== 2025-02-01 10:44 HYO 코드 수정 =======================================================
    public bool CheckMana(string skillName)
    {
        UpdateCurrentMana(); // MP 정보 최신화
        
        Skills skill = GetSkill(EntityType.Player, skillName);
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
    // ==========================================================================================================

    private void UpdateCurrentMana()
    {
        currentMp = CharacterManager.PlayerCharacterData.currentMp;
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

    }
    
    // ===================== 2025-02-01 1:33 HYO 코드 추가 ==============================================================
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
    // =================================================================================================================
}

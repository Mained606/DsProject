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

/*
// ========================================
public class SkillManager : BaseManager<SkillManager>
{
    [SerializeField] private SkillList skillDatabase;
    private Dictionary<(EntityType, string), Skills> skillList = new Dictionary<(EntityType, string), Skills>();


    // ==========================2025-02-01 11:08 HYO 코드 추가 =============================================
    private Dictionary<string, Skills> playerSkillDictionary = new Dictionary<string, Skills>();
    private Dictionary<string, Skills> dragonSkillDictionary = new Dictionary<string, Skills>();
    private Dictionary<string, Skills> bossSkillDictionary = new Dictionary<string, Skills>();
    
    private Dictionary<Skills, Image> activeSkill = new Dictionary<Skills, Image>();
    private Dictionary<string, BuffInfo> activeBuffs = new Dictionary<string, BuffInfo>();
  
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
    private float animationDuration;

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

    private void InitializeSkillDictionary()
    {
        for (int i = 0; i < 3; i++)
        {
            foreach (var skill in SkillManager.Instance.GetSkills((EntityType)i))
            {
                if (!skillList.ContainsKey(((EntityType)i, skill.skillName)))
                {
                    skillList[((EntityType)i, skill.skillName)] = skill;
                }
            }
        }
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
    public void ActivateSkillForEntity(EntityType entityType, string skillName, GameObject target = null, Vector3? overridePosition = null)
    {
        // Debug.Log($"[ActivateSkillForEntity] {entityType}가 {skillName} 스킬을 사용하려 합니다.");
        
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
        // Debug.Log($"[ActivateSkillForEntity] {skillName} 스킬을 정상적으로 찾았습니다.");
        
        if (skill.cooldownTimer.IsRunning)
        {
            Debug.Log($"{skill.skillName} is on cooldown! (남은 시간: {skill.cooldownTimer.RemainingTime})");
            return;
        }
        
        // 만약 플레이어의 스킬 사용이라면, 스킬 실행 전에 플레이어 상태를 먼저 변경
        if (entityType == EntityType.Player)
        {
            PlayerController playerController = GameManager.playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.isUseSkill = true;
            }
        }

        // Debug.Log($"[ActivateSkillForEntity] {skillName} 스킬을 실행합니다.");
        
        isActivating = true; // 스킬 사용 중 플래그 설정
        CharacterManager.PlayerCharacterData.UseMp(skill.energyCost);
        TimerManager.Instance.StartTimer(skill.cooldownTimer); // 쿨다운 시작
        StartCoroutine(ExecuteSkill(entityType, skill, target, overridePosition)); // 코루틴 실행
    }
    
    // 공통 스킬 실행 로직 (애니메이션, 효과, 쿨다운 포함)
    private IEnumerator ExecuteSkill(EntityType entityType, Skills skill, GameObject target, Vector3? overridePosition = null)
    {
        if (target == null)
        {
            target = GameManager.playerTransform.gameObject;
        }
        
        Vector3 spawnPosition = overridePosition ?? target.transform.position; // 전달된 위치가 없으면 기본 위치 사용
        
        Animator entityAnimator = GetEntityAnimator(entityType);
        if (entityAnimator != null)
        {
            entityAnimator.SetTrigger(skill.activeTriggerName);
        }

        if (skill.effectPrefab != null)
        {
            Quaternion entityRotation = Quaternion.LookRotation(target.transform.forward);
            var effect = Instantiate(skill.effectPrefab, spawnPosition, entityRotation);
            if (skill.particleDelay > 0)
            {
                var particleEffect = effect.GetComponent<ParticleSystem>().main;
                particleEffect.startDelay = skill.particleDelay;
            }
            Destroy(effect, 5f);
        }

        // 현재 실행 중인 애니메이션의 길이만큼 대기 (애니메이션이 끝날 때까지)
        float animationLength = GetAnimationClipLength(entityAnimator, skill.activeTriggerName);
        yield return new WaitForSeconds(animationLength + 0.5f);

        isActivating = false; // 스킬 사용 완료 후 다시 입력 가능
        
        // 스킬 애니메이션 종료 후, 플레이어 상태 리셋
        if (entityType == EntityType.Player)
        {
            PlayerController playerController = GameManager.playerTransform.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.isUseSkill = false;
            }
        }
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
    
    public bool CanActivateSkill(EntityType entityType, string skillName)
    {
        // 다른 스킬이 실행 중이면 안 됨
        if (isActivating)
        {
            return false;
        }
    
        Skills skill = GetSkill(entityType, skillName);
        if (skill == null)
        {
            return false;
        }
    
        // 해당 스킬이 쿨타임 중이면 사용 불가
        if (skill.cooldownTimer.IsRunning)
        {
            return false;
        }
    
        return true;
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
                // 쿨타임이 실행되고 있는지 확인
                if (!skill.cooldownTimer.IsRunning)
                {
                    continue;
                }
                
                // Debug.Log($"[쿨타임 진행] {skill.skillName} 남은 퍼센트: {skill.cooldownTimer.RemainingPercent * 100}%");
                
                if (skill.cooldownTimer.RemainingTime <= 0.1f)
                {
                    // Debug.Log($"[쿨타임 종료] {skill.skillName}의 쿨타임이 끝났습니다.");
                    
                    if (activeSkill.ContainsKey(skill))
                    {
                        // Debug.Log($"[쿨타임 종료] {skill.skillName} 쿨타임이 끝났습니다. UI 제거 시도.");
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
                    
                    // 기본적으로 쿨타임 남은 시간을 표시
                    string uiText = $"CD: {skill.cooldownTimer.RemainingTime:0.0}s";
                    
                    // 버프 스킬인 경우, 현재 활성화된 버프의 남은 지속시간을 추가로 표시
                    if (skill.buffDuration > 0 && activeBuffs.ContainsKey(skill.skillName))
                    {
                        BuffInfo buffInfo = activeBuffs[skill.skillName];
                        float buffRemaining = Mathf.Max(0, buffInfo.duration - (Time.time - buffInfo.startTime));
                        uiText = $"Buff: {buffRemaining:0.0}s\n" + uiText;
                    }

                    skillCoolImage.fillAmount = skill.cooldownTimer.RemainingPercent;
                    skillCoolName.text = skill.skillName;
                }
            }
        }
        // 스킬 리스트에서 제거 (리스트를 수정할 때는 별도 리스트에 저장 후 제거)
        foreach (var skill in skillsToRemove)
        {
            if (currentUsedSkills.Contains(skill))
            {
                // Debug.Log($"[쿨타임 종료] {skill.skillName}을 currentUsedSkills에서 제거.");
                currentUsedSkills.Remove(skill);
            }
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
    public void ApplyBuff(EntityType entityType, string skillName)
    {
        Skills skill = GetSkill(entityType, skillName);
        if (skill == null) return;
        
        // 🛠️ 버프 적용 대상 설정 (용 → 플레이어, 보스 → 보스 자신)
        CharacterData targetCharacter = entityType switch
        {
            EntityType.Dragon => CharacterManager.PlayerCharacterData,
            // EntityType.Boss => CharacterManager.BossCharacterData, // 보스 데이터 추가
            // EntityType.Player => CharacterManager.PlayerCharacterData,
            _ => null
        };

        if (targetCharacter == null) return;
        
        // 이미 해당 버프가 적용 중이면 기존 코루틴 중단 후 지속시간만 갱신
        if (activeBuffs.ContainsKey(skillName))
        {
            // 버프가 이미 활성화되어 있으면 기존 코루틴을 멈추고 지속시간만 갱신
            StopCoroutine(activeBuffs[skillName].coroutine);
            activeBuffs.Remove(skillName);
            
            // 기존 버프 정보를 새로 갱신
            BuffInfo buffInfo = new BuffInfo();
            buffInfo.startTime = Time.time;
            buffInfo.duration = skill.buffDuration;
            buffInfo.coroutine = StartCoroutine(RemoveBuffAfterDuration(skillName, skill.buffDuration, targetCharacter));
            activeBuffs[skillName] = buffInfo;
            
            Debug.Log(skillName + "이미 중복되어 실행 중지");
    
            // 기존 버프 효과가 이미 적용되어 있으면 추가 효과를 적용하지 않음
            return;
        }

        // 버프 정보를 새로 생성하여 저장 (현재 시간과 지속시간)
        BuffInfo newBuffInfo = new BuffInfo();
        newBuffInfo.startTime = Time.time;
        newBuffInfo.duration = skill.buffDuration;
        newBuffInfo.coroutine = StartCoroutine(RemoveBuffAfterDuration(skillName, skill.buffDuration, targetCharacter));
        activeBuffs[skillName] = newBuffInfo;

        // 버프 효과 적용
        switch (skillName)
        {
            case "PlayerBuffPhysical":
                targetCharacter.physicalDamageBuffMultiplier *= 1.2f; // 20% 증가
                targetCharacter.UpdateDerivedStats();
                break;

            case "PlayerBuffMagic":
                targetCharacter.magicDamageBuffMultiplier *= 1.2f; // 20% 증가
                targetCharacter.UpdateDerivedStats();
                break;

            case "PlayerBuffHP":
                targetCharacter.hpBuffBonus += 100;
                targetCharacter.currentHp += 100; // 즉시 회복 효과
                targetCharacter.UpdateDerivedStats();
                break;
            default:
                Debug.LogWarning("알 수 없는 버프 스킬: " + skillName);
                break;
        }
        
        if (skill.effectPrefab != null)
        {
            // 용이 플레이어에게 버프를 사용하므로, 플레이어 근처에 이팩트를 표시
            InstantiateBuffEffect(skill.effectPrefab, GameManager.playerTransform.position);
        }
        
        TimerManager.Instance.StartTimer(skill.cooldownTimer);
    }
    
    private void InstantiateBuffEffect(GameObject effectPrefab, Vector3 targetPosition)
    {
        // 이펙트를 플레이어 위치에 생성 (부모를 플레이어로 설정)
        var effect = Instantiate(effectPrefab, targetPosition, Quaternion.identity);
    
        // 이펙트를 플레이어의 자식 오브젝트로 설정
        effect.transform.SetParent(GameManager.playerTransform);

        // 이펙트가 일정 시간 후에 제거되도록 설정
        Destroy(effect, 3f); // 3초 후 이펙트 삭제
    }
    
    // 🔄 일정 시간이 지나면 버프 해제
    private IEnumerator RemoveBuffAfterDuration(string skillName, float duration, CharacterData targetCharacter)
    {
        yield return new WaitForSeconds(duration);

        switch (skillName)
        {
            case "PlayerBuffPhysical":
                targetCharacter.physicalDamageBuffMultiplier /= 1.2f;
                targetCharacter.UpdateDerivedStats();
                break;

            case "PlayerBuffMagic":
                targetCharacter.magicDamageBuffMultiplier /= 1.2f;
                targetCharacter.UpdateDerivedStats();
                break;

            case "PlayerBuffHP":
                targetCharacter.hpBuffBonus -= 100;
                targetCharacter.UpdateDerivedStats();
                break;
        }
        
        // 버프 해제 후 최대 체력(maxHp)과 현재 체력(currentHp) 처리
        if (targetCharacter.maxHp < targetCharacter.currentHp)
        {
            targetCharacter.currentHp = targetCharacter.maxHp; // 최대 체력에 맞게 현재 체력 조정
        }

        // 버프가 해제되었으므로 딕셔너리에서 제거
        Debug.Log(skillName + "버프 시간 종료");
        activeBuffs.Remove(skillName);
    }
    
    public float GetCooldownForSkill(EntityType entityType, string skillName)
    {
        // 각 엔티티 타입별로 적합한 스킬 목록을 선택
        List<Skills> skillsList = entityType switch
        {
            EntityType.Player => skillDatabase.playerSkills,
            EntityType.Dragon => skillDatabase.dragonSkills,
            EntityType.Boss => skillDatabase.bossSkills,
            _ => null
        };

        // 유효한 스킬 목록이 없거나, 해당 스킬이 목록에 없으면 기본 쿨다운 반환
        if (skillsList == null)
        {
            Debug.LogError($"[GetCooldownForSkill] 유효하지 않은 엔티티 타입: {entityType}");
            return 60f; // 기본값 60초
        }

        // 스킬 목록에서 해당 스킬 찾기
        var skill = skillsList.FirstOrDefault(s => s.skillName == skillName);

        // 해당 스킬이 없으면 기본 쿨다운 반환
        if (skill == null)
        {
            Debug.LogError($"[GetCooldownForSkill] 스킬을 찾을 수 없음: {skillName}");
            return 60f; // 기본값 60초
        }

        return skill.cooldown;
    }
    
    // 사용할 수 있는 모든 버프 스킬 목록 가져오기
    public List<string> GetAvailableBuffs(EntityType entityType)
    {
        List<string> buffSkills = new List<string>();

        Dictionary<string, Skills> skillDictionary = entityType switch
        {
            EntityType.Player => playerSkillDictionary,
            EntityType.Dragon => dragonSkillDictionary,
            EntityType.Boss => bossSkillDictionary,
            _ => null
        };

        if (skillDictionary == null)
        {
            Debug.LogError($"[GetAvailableBuffs] 유효하지 않은 엔티티 타입: {entityType}");
            return buffSkills;
        }

        foreach (var skill in skillDictionary.Values)
        {
            if (skill.skillType == SkillType.Support)
            {
                buffSkills.Add(skill.skillName);
            }
        }

        return buffSkills;
    }

    public List<string> GetAvailableSkills(EntityType entityType)
    {
        Dictionary<string, Skills> skillDictionary = entityType switch
        {
            EntityType.Player => playerSkillDictionary,
            EntityType.Dragon => dragonSkillDictionary,
            EntityType.Boss => bossSkillDictionary,
            _ => null
        };

        if (skillDictionary == null) return new List<string>();

        return skillDictionary.Keys.ToList();


    }

    // =================================================================================================================


    public List<Skills> GetSkills(EntityType entityType)
    {
        List<Skills> tempList = entityType switch
        {
            EntityType.Player => skillDatabase.playerSkills,
            EntityType.Dragon => skillDatabase.dragonSkills,
            EntityType.Boss => skillDatabase.bossSkills,
            _ => null
        };

        if(tempList == null) return new List<Skills>();

        return tempList;
    }
}
*/

public class SkillManager : BaseManager<SkillManager>
{
    [SerializeField] private SkillList skillDatabase;
    public static SkillList SkillDatabase => Instance.skillDatabase;

    // <(엔티티 타입, 스킬 이름), 스킬 정보> 형태로 모든 스킬 저장
    private Dictionary<(EntityType, string), Skills> skillList = new Dictionary<(EntityType, string), Skills>();
    public static Dictionary<(EntityType, string), Skills> SkillList => Instance.skillList;

    private Dictionary<Skills, Image> activeSkill = new Dictionary<Skills, Image>();
    private Dictionary<string, BuffInfo> activeBuffs = new Dictionary<string, BuffInfo>();

    [SerializeField] private List<Skills> currentUsedSkills = new List<Skills>();

    private Animator animator;
    [SerializeField] private GameObject[] skillImage;
    [SerializeField] private Transform skillPanel;

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
        }
    }

    private void UpdateSkillCooldownUI(Skills skill)
    {
        if (!activeSkill.ContainsKey(skill))
        {
            var skillImg = Instantiate(skillImage[skill.skillType == SkillType.Support ? 0 : 1], skillPanel);
            activeSkill[skill] = skillImg.transform.GetChild(1).GetComponent<Image>();
            activeSkill[skill].sprite = ItemManager.Instance.GetSkillSprite(skill.skillName);
        }
        activeSkill[skill].transform.GetChild(1).GetComponent<Image>().fillAmount = skill.cooldownTimer.RemainingPercent;
        activeSkill[skill].transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = skill.cooldownTimer.RemainingTime.ToString("N0") + "s";
        activeSkill[skill].transform.GetChild(2).GetComponent<TextMeshProUGUI>().text = skill.skillName;
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

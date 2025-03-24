using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.VFX;

// ===== 2025-02-01 11:08 HYO 코드 추가 ====
public enum EntityType
{
    Player,
    Dragon,
    Boss
}

[System.Serializable]
public class BuffInfo {
    [System.NonSerialized]
    public Coroutine coroutine;
    public float startTime;  // 버프가 시작된 시간
    public float duration;   // 버프 지속시간
    public float originalPhysicalMultiplier; // 원래 물리 데미지 배율 저장
    public float originalMagicMultiplier;    // 원래 마법 데미지 배율 저장
    public int originalHpBonus;              // 원래 HP 버프 보너스 저장
    
    // 버프 남은 시간 계산
    public float GetRemainingTime()
    {
        float elapsedTime = Time.time - startTime;
        return Mathf.Max(0, duration - elapsedTime);
    }
}

public class SkillManager : BaseManager<SkillManager>
{
    [SerializeField] private SkillList skillDatabase;
    public static SkillList SkillDatabase => Instance.skillDatabase;

    //========== 250320 SH 추가 ==========
    [SerializeField] private SkillWeightList skillWeightDatabase;
    public static SkillWeightList SkillWeightDatabase => Instance.skillWeightDatabase;
    //========== 250320 SH 추가 ==========

    // <(엔티티 타입, 스킬 이름), 스킬 정보> 형태로 모든 스킬 저장
    [System.NonSerialized]
    private Dictionary<(EntityType, string), Skills> skillList = new Dictionary<(EntityType, string), Skills>();
    public static Dictionary<(EntityType, string), Skills> SkillList => Instance.skillList;

    // <(엔티티 타입, 스킬 이름), 스킬 정보> 형태로 모든 스킬 가중치 저장 ========== 250320 SH 추가 ==========
    [System.NonSerialized]
    private Dictionary<(EntityType, string), SkillWeights> skillWeightList = new Dictionary<(EntityType, string), SkillWeights>();
    public static Dictionary<(EntityType, string), SkillWeights> SkillWeightList => Instance.skillWeightList;
    // ========== 250320 SH 추가 ==========

    [System.NonSerialized]
    private Dictionary<Skills, Image> activeSkill = new Dictionary<Skills, Image>();
    [System.NonSerialized]
    private Dictionary<string, BuffInfo> activeBuffs = new Dictionary<string, BuffInfo>();
    [System.NonSerialized]
    private Dictionary<Skills, Coroutine> blinkCoroutines = new Dictionary<Skills, Coroutine>();
    [System.NonSerialized]
    private Dictionary<Skills, bool> skillBlinkState = new Dictionary<Skills, bool>();

    [SerializeField] private List<Skills> currentUsedSkills = new List<Skills>();

    [System.NonSerialized]
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
        // 직렬화되지 않는 변수들 초기화
        skillList = new Dictionary<(EntityType, string), Skills>();
        skillWeightList = new Dictionary<(EntityType, string), SkillWeights>();
        activeSkill = new Dictionary<Skills, Image>();
        activeBuffs = new Dictionary<string, BuffInfo>();
        blinkCoroutines = new Dictionary<Skills, Coroutine>();
        skillBlinkState = new Dictionary<Skills, bool>();
        
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
        RegisterSkillWeights(EntityType.Player, skillWeightDatabase.playerSkillWeights);
        RegisterSkills(EntityType.Dragon, skillDatabase.dragonSkills);
        RegisterSkillWeights(EntityType.Dragon, skillWeightDatabase.dragonSkillWeights);
        RegisterSkills(EntityType.Boss, skillDatabase.bossSkills);
        RegisterSkillWeights(EntityType.Boss, skillWeightDatabase.bossSkillWeights);
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
                Debug.Log($"[SkillManager] 스킬 등록: {entityType}-{skill.skillName}, 지속시간: {skill.buffDuration}초, 쿨타임: {skill.cooldown}초");
            }
        }
    }

    private void RegisterSkillWeights(EntityType entityType, List<SkillWeights> skillWeights)
    {
        foreach (var skillWeight in skillWeights)
        {
            Debug.Log($"스킬가중치 등록 {skillWeight.skillName}");
            var key = (entityType, skillWeight.skillName);
            if (!skillWeightList.ContainsKey(key))
            {
                skillWeightList[key] = skillWeight;
            }
        }
    }

    public Skills GetSkill(EntityType entityType, string skillName)
    {
        return skillList.TryGetValue((entityType, skillName), out var skill) ? skill : null;
    }

    public SkillWeights GetSkillWeights(EntityType entityType, string skillName)
    {
        return skillWeightList.TryGetValue((entityType, skillName), out var skillWeight) ? skillWeight : null;
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
                // =========== 250317 SH 추가 ==========
                // target이 null로 들어왔을 때만 player target을 player로 설정하도록
                if(target == null)
                {
                    target = GameManager.playerTransform.gameObject;
                }
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
            if(entityType == EntityType.Player)
            {
                entityAnimator.CrossFade(skill.skillName, 0f);
            }
            else
            {
                entityAnimator.SetTrigger(skill.activeTriggerName);
            }  
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
                ApplyEffect(effect, skill.particleDelay);
            }

            if(skill.effectDuration > 0)    // effectDuration 있는 경우 Destroy 시간 변경
            {
                Destroy(effect, skill.effectDuration);
            }
            else
            {
                Destroy(effect, 5f);
            }
                
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

    private void CheckSkillCoolTime()
    {
        List<Skills> skillsToRemove = new List<Skills>();

        foreach (var skill in currentUsedSkills)
        {
            if (skill.cooldownTimer.RemainingTime <= 0.1f)
            {
                skillsToRemove.Add(skill);
                skill.cooldownTimer.Stop();
            }
            else if (skill.entityType != EntityType.Player && skill.skillType != SkillType.Support)
            {
                // 버프 스킬(Support 타입)이 아닌 스킬만 쿨다운 UI 업데이트
                UpdateSkillCooldownUI(skill);
            }
        }

        // 쿨다운이 끝난 스킬 처리
        foreach (var skill in skillsToRemove)
        {
            currentUsedSkills.Remove(skill);
            Debug.Log($"[CheckSkillCoolTime] {skill.skillName} 스킬 쿨다운 완료");
        }
        
        // 모든 활성 버프에 대해 UI 업데이트 - 지속시간 기반
        UpdateAllBuffsUI();
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

    // 모든 활성 버프의 UI를 업데이트하는 메서드
    private void UpdateAllBuffsUI()
    {
        if (activeBuffs == null) return;
        
        foreach (var buffEntry in activeBuffs)
        {
            string buffName = buffEntry.Key;
            BuffInfo buffInfo = buffEntry.Value;
            
            // 버프 스킬 정보 가져오기
            Skills buffSkill = null;
            
            // 엔티티 타입을 확인하여 스킬 검색
            if (skillList != null && skillList.TryGetValue((EntityType.Dragon, buffName), out var dragonSkill))
            {
                buffSkill = dragonSkill;
            }
            else if (skillList != null && skillList.TryGetValue((EntityType.Player, buffName), out var playerSkill))
            {
                buffSkill = playerSkill;
            }
            
            if (buffSkill != null)
            {
                // 버프 UI 업데이트
                UpdateBuffUI(buffSkill, buffInfo);
            }
        }
    }
    
    // 버프 UI를 업데이트하는 메서드 (지속시간 기반)
    private void UpdateBuffUI(Skills skill, BuffInfo buffInfo)
    {
        if (activeSkill == null) return;

        if (!activeSkill.ContainsKey(skill))
        {
            // 새 버프 아이콘 생성
            var skillImg = Instantiate(skillImage[0], // 버프는 항상 Support 타입 (0번 인덱스)
                                      skill.entityType != EntityType.Boss ? skillPanel[0] : skillPanel[1]);
            skillImg.AddComponent<CanvasGroup>();
            activeSkill[skill] = skillImg.transform.GetChild(1).GetComponent<Image>();
            activeSkill[skill].sprite = ItemManager.Instance.GetSkillSprite(skill.skillName);
            
            // 제목 설정 (옵션)
            var titleText = skillImg.GetComponentInChildren<TextMeshProUGUI>();
            if (titleText != null)
            {
                if (skill.skillName.Contains("Health") || skill.skillName.Contains("HP"))
                {
                    titleText.text = "체력 버프";
                }
                else if (skill.skillName.Contains("Physical") || skill.skillName.Contains("물리"))
                {
                    titleText.text = "물리 버프";
                }
                else if (skill.skillName.Contains("Magic") || skill.skillName.Contains("Magical") || skill.skillName.Contains("마법"))
                {
                    titleText.text = "마법 버프";
                }
                else
                {
                    titleText.text = skill.skillName;
                }
            }
        }
        
        // 버프 아이콘의 쿨다운 UI는 지속시간 표시로 사용
        if (activeSkill.TryGetValue(skill, out var image))
        {
            // 남은 시간 계산
            float remainingTime = buffInfo.GetRemainingTime();
            
            // 지속시간 표시 (원형 필 이미지가 줄어들게)
            float remainingPercentage = remainingTime / buffInfo.duration;
            
            // 자식 이미지(필)의 fillAmount 업데이트
            Image fillImage = image;
            if (fillImage != null)
            {
                fillImage.fillAmount = remainingPercentage;
            }
            
            // 남은 시간 텍스트 표시 (있다면)
            TextMeshProUGUI timeText = image.transform.parent.GetComponentInChildren<TextMeshProUGUI>();
            if (timeText != null)
            {
                timeText.text = $"{Mathf.CeilToInt(remainingTime)}";
            }
            
            // 남은 시간이 10초 미만일 때 깜빡임 효과 추가
            if (remainingTime > 0f && remainingTime < 10f)
            {
                // 이미 깜빡임 코루틴이 실행 중인지 확인
                if (blinkCoroutines != null && !blinkCoroutines.ContainsKey(skill))
                {
                    blinkCoroutines[skill] = StartCoroutine(BlinkBuffIcon(skill, buffInfo));
                }
            }
            else if (blinkCoroutines != null && blinkCoroutines.ContainsKey(skill))
            {
                // 남은 시간이 10초 이상이면 깜빡임 코루틴 정리
                StopCoroutine(blinkCoroutines[skill]);
                blinkCoroutines.Remove(skill);
                
                // 깜빡임이 중지된 경우 알파값을 1로 복원
                CanvasGroup canvasGroup = activeSkill[skill].transform.parent.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                }
            }
        }
    }
    
    // 버프 아이콘 깜빡임 효과 (지속시간 기준)
    private IEnumerator BlinkBuffIcon(Skills skill, BuffInfo buffInfo)
    {
        if (activeSkill == null || !activeSkill.ContainsKey(skill))
        {
            Debug.LogError($"Skill {skill.skillName} is missing in activeSkill dictionary.");
            yield break;
        }

        CanvasGroup canvasGroup = activeSkill[skill].transform.parent.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            Debug.LogError($"CanvasGroup is missing for buff {skill.skillName}");
            yield break;
        }

        if (skillBlinkState == null)
        {
            skillBlinkState = new Dictionary<Skills, bool>();
        }
        
        if (!skillBlinkState.ContainsKey(skill))
            skillBlinkState[skill] = true;

        // 버프 지속시간이 10초 미만인 동안 깜빡임
        while (buffInfo.GetRemainingTime() > 0f && buffInfo.GetRemainingTime() < 10f)
        {
            if (canvasGroup == null) yield break;
            
            // 남은 시간에 따라 깜빡임 속도 계산
            // 10초에 가까울수록 느리게, 0초에 가까울수록 빠르게
            float remainingTime = buffInfo.GetRemainingTime();
            float dynamicBlinkInterval = blinkInterval * (2.0f - (remainingTime / 10f) * 1.7f);
            
            if (skillBlinkState[skill])
            {
                canvasGroup.alpha = Mathf.Max(0.5f, canvasGroup.alpha - dynamicBlinkInterval); // 최소 0.5 이상으로 유지
                if (Mathf.Approximately(canvasGroup.alpha, 0.5f)) skillBlinkState[skill] = false;
            }
            else
            {
                canvasGroup.alpha = Mathf.Min(1f, canvasGroup.alpha + dynamicBlinkInterval);
                if (Mathf.Approximately(canvasGroup.alpha, 1f)) skillBlinkState[skill] = true;
            }
            yield return null;
        }
        
        // 깜빡임이 끝났을 때 alpha를 1로 복원
        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
            
        if (blinkCoroutines != null)
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

    // ========== 250313 SH 추가 ==========
    public bool CheckWeaponType(string skillName)
    {
        Skills skill = GetSkill(EntityType.Player, skillName);
        Item weaponItem = ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.손);
        if (skill.skillType == SkillType.Physical && (weaponItem.weaponType == WeaponType.한손무기 || weaponItem.weaponType == WeaponType.양손무기))
        {
            return true;
        }
        else if(skill.skillType == SkillType.Magic && weaponItem.weaponType == WeaponType.완드)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void ApplyEffect(GameObject effect, float delay)
    {
        if (effect == null) return;

        ParticleSystem particle = effect.GetComponent<ParticleSystem>();
        if(particle != null)
        {
            var particleEffect = particle.main;
            particleEffect.startDelay = delay;
        }

        VisualEffect visualEffect = effect.GetComponentInChildren<VisualEffect>();
        if(visualEffect != null)
        {
            visualEffect.Stop();
            StartCoroutine(PlayVisualEffectWithDelay(visualEffect, delay));
        }
    }

    private IEnumerator PlayVisualEffectWithDelay(VisualEffect vfx, float delay)
    {
        yield return new WaitForSeconds(delay);
        vfx.Play();
    }

    // ========================== 🛠️ 버프 기능 추가 ==========================
    public void ApplyBuff(EntityType entityType, string skillName, CharacterData targetCharacter = null)
    {
        // 타겟 캐릭터가 지정되지 않은 경우, 기본값 설정
        if (targetCharacter == null)
        {
            targetCharacter = entityType switch
            {
                EntityType.Dragon => CharacterManager.PlayerCharacterData,
                //EntityType.Player => CharacterManager.PlayerCharacterData,
                //EntityType.Boss => CharacterManager.BossCharacterData,
                _ => null
            };
        }
        
        if (targetCharacter == null)
        {
            Debug.LogError("[ApplyBuff] 타겟 캐릭터가 null입니다!");
            return;
        }
        
        // 이미 적용된 버프가 있다면 제거
        if (activeBuffs.ContainsKey(skillName))
        {
            BuffInfo existingBuff = activeBuffs[skillName];
            RemoveBuffEffect(entityType, skillName, targetCharacter);
            if (existingBuff.coroutine != null)
            {
                StopCoroutine(existingBuff.coroutine);
            }
            activeBuffs.Remove(skillName);
            // 체력 업데이트 여부 결정 (체력 버프만 현재 체력 업데이트)
            bool updateHp = skillName.Contains("Health") || skillName.Contains("HP");
            targetCharacter.UpdateDerivedStats(updateHp);
        }
        
        // 엔티티 타입으로 스킬 가져오기
        Skills skill = GetSkill(entityType, skillName);
        
        if (skill == null)
        {
            Debug.LogError($"[ApplyBuff] {entityType} 엔티티의 {skillName} 스킬을 찾을 수 없습니다.");
            return;
        }
        
        // 각 스킬의 버프 지속시간은 스킬 정의에 따름
        float buffDuration = skill.buffDuration;

        // 원본 값 저장
        BuffInfo newBuffInfo = new BuffInfo
        {
            startTime = Time.time,
            duration = buffDuration, 
            originalPhysicalMultiplier = targetCharacter.physicalDamageBuffMultiplier,
            originalMagicMultiplier = targetCharacter.magicDamageBuffMultiplier,
            originalHpBonus = targetCharacter.hpBuffBonus
        };

        // BuffInfo를 저장
        activeBuffs[skillName] = newBuffInfo;
        
        // 버프 효과 적용
        ApplyBuffEffect(entityType, skillName, targetCharacter);
        
        // 버프 적용 후 스탯 업데이트 (체력 버프만 현재 체력 업데이트)
        bool updateCurrentHp = skillName.Contains("Health") || skillName.Contains("HP");
        targetCharacter.UpdateDerivedStats(updateCurrentHp);
        
        // 버프 종료 코루틴 시작 - buffDuration을 전달
        newBuffInfo.coroutine = StartCoroutine(RemoveBuff(entityType, skillName, buffDuration, targetCharacter));
        
        // 이펙트 있으면 생성
        if (skill.effectPrefab != null)
        {
            InstantiateBuffEffect(skill.effectPrefab, GameManager.playerTransform.position);
        }
        
        // 쿨다운 타이머 시작 (스킬 자체의 쿨다운)
        TimerManager.Instance.StartTimer(skill.cooldownTimer);
    }

    private void ApplyBuffEffect(EntityType entityType, string skillName, CharacterData targetCharacter)
    {
        if (targetCharacter == null) return;

        // 엔티티 타입으로 스킬 가져오기
        Skills skill = GetSkill(entityType, skillName);
        
        if (skill == null)
        {
            Debug.LogError($"[ApplyBuffEffect] {entityType} 엔티티의 {skillName} 스킬을 찾을 수 없습니다.");
            return;
        }
        
        // 스킬 이름이 아닌 스킬 내용에 따라 버프 적용
        // 스킬 이름에 특정 키워드가 포함된 경우 해당 유형의 버프로 판단
        if (skillName.Contains("Health") || skillName.Contains("HP"))
        {
            // 체력 버프
            int healthBonus = activeBuffs[skillName].originalHpBonus + (int)skill.buffValue;
            targetCharacter.hpBuffBonus = healthBonus;
            // 현재 체력은 UpdateDerivedStats에서 처리하므로 여기서는 수정하지 않음
        }
        else if (skillName.Contains("Physical") || skillName.Contains("물리"))
        {
            // 물리 공격력 버프
            float physicalMultiplier = activeBuffs[skillName].originalPhysicalMultiplier * (1 + skill.buffValue / 100f);
            targetCharacter.physicalDamageBuffMultiplier = physicalMultiplier;
        }
        else if (skillName.Contains("Magic") || skillName.Contains("Magical") || skillName.Contains("마법"))
        {
            // 마법 공격력 버프
            float magicMultiplier = activeBuffs[skillName].originalMagicMultiplier * (1 + skill.buffValue / 100f);
            targetCharacter.magicDamageBuffMultiplier = magicMultiplier;
        }
        else
        {
            Debug.LogWarning($"[ApplyBuffEffect] 알 수 없는 버프 유형: {skillName}");
        }
    }

    private void RemoveBuffEffect(EntityType entityType, string skillName, CharacterData targetCharacter)
    {
        if (!activeBuffs.ContainsKey(skillName))
        {
            Debug.LogWarning($"[RemoveBuffEffect] 버프 '{skillName}'를 제거하려고 했으나 active 버프 목록에 없습니다.");
            return;
        }

        Skills skill = GetSkill(entityType, skillName);
        if (skill == null)
        {
            Debug.LogError($"[RemoveBuffEffect] {entityType} 엔티티의 {skillName} 스킬을 찾을 수 없습니다.");
            return;
        }

        // 스킬 이름 대신 스킬 내용에 따라 처리
        if (skillName.Contains("Health") || skillName.Contains("HP"))
        {
            // 체력 버프 제거
            int originalHp = activeBuffs[skillName].originalHpBonus;
            targetCharacter.hpBuffBonus = originalHp;
        }
        else if (skillName.Contains("Physical") || skillName.Contains("물리"))
        {
            // 물리 공격력 버프 제거
            float originalPhysical = activeBuffs[skillName].originalPhysicalMultiplier;
            targetCharacter.physicalDamageBuffMultiplier = originalPhysical;
        }
        else if (skillName.Contains("Magic") || skillName.Contains("Magical") || skillName.Contains("마법"))
        {
            // 마법 공격력 버프 제거
            float originalMagic = activeBuffs[skillName].originalMagicMultiplier;
            targetCharacter.magicDamageBuffMultiplier = originalMagic;
        }
        else
        {
            Debug.LogWarning($"[RemoveBuffEffect] 알 수 없는 버프 유형: {skillName}");
        }
    }

    private void InstantiateBuffEffect(GameObject effectPrefab, Vector3 targetPosition)
    {
        var effect = Instantiate(effectPrefab, targetPosition, Quaternion.identity);
        effect.transform.SetParent(GameManager.playerTransform);
        Destroy(effect, 3f);
    }

    // 버프 종료 코루틴을 시작하는 메서드 (이름+엔티티 기반)
    private IEnumerator RemoveBuff(EntityType entityType, string skillName, float duration, CharacterData targetCharacter)
    {
        // 지정된 시간만큼 대기 후 버프 제거
        yield return new WaitForSeconds(duration);
        
        // 버프 효과 제거 및 스탯 업데이트
        RemoveBuffEffect(entityType, skillName, targetCharacter);
        
        // 체력 버프만 현재 체력 업데이트 (다른 버프에서는 체력 회복이 안되게)
        bool updateCurrentHp = skillName.Contains("Health") || skillName.Contains("HP");
        targetCharacter.UpdateDerivedStats(updateCurrentHp);
        
        // 체력 제한 - 체력 버프일 경우에만 체력 제한 적용
        if (skillName.Contains("Health") || skillName.Contains("HP"))
        {
            if (targetCharacter.maxHp < targetCharacter.currentHp)
            {
                targetCharacter.currentHp = targetCharacter.maxHp;
            }
        }
        
        // 스킬 객체 찾기
        Skills buffSkill = GetSkill(entityType, skillName);
        if (buffSkill != null)
        {
            // 깜빡임 코루틴 정리
            if (blinkCoroutines != null && blinkCoroutines.ContainsKey(buffSkill))
            {
                StopCoroutine(blinkCoroutines[buffSkill]);
                blinkCoroutines.Remove(buffSkill);
            }
            
            // UI 제거
            if (activeSkill != null && activeSkill.ContainsKey(buffSkill))
            {
                Destroy(activeSkill[buffSkill].transform.parent.gameObject);
                activeSkill.Remove(buffSkill);
            }
        }
        
        // 활성 버프 목록에서 제거
        if (activeBuffs != null && activeBuffs.ContainsKey(skillName))
        {
            activeBuffs.Remove(skillName);
        }
    }

    // 버프가 현재 활성화되어 있는지 확인
    public bool IsBuffActive(string skillName)
    {
        return activeBuffs.ContainsKey(skillName);
    }

    // 버프의 남은 시간을 가져오는 메소드
    public float GetBuffRemainingTime(string skillName)
    {
        if (!activeBuffs.ContainsKey(skillName))
        {
            return 0f;
        }
        
        return activeBuffs[skillName].GetRemainingTime();
    }
    
    // 버프의 총 지속시간을 가져오는 메소드
    public float GetBuffTotalDuration(string skillName)
    {
        if (!activeBuffs.ContainsKey(skillName))
        {
            return 0f;
        }
        
        return activeBuffs[skillName].duration;
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

    }
}

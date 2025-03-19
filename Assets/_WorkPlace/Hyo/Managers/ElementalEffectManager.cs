using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 모든 속성 효과(버프, 디버프)를 관리하는 매니저 클래스
/// 기존 DebuffManager를 대체합니다.
/// </summary>
public class ElementalEffectManager : BaseManager<ElementalEffectManager>
{
    [SerializeField] 
    private List<ElementalEffect> activeEffects = new List<ElementalEffect>();
    private Dictionary<ElementalEffect, Coroutine> effectCoroutines = new Dictionary<ElementalEffect, Coroutine>();
    
    // 마지막 isActive 확인 시간 (매 프레임 체크 방지)
    private float lastCheckTime = 0f;
    private const float CHECK_INTERVAL = 0.5f; // 0.5초마다 체크

    protected override void Awake()
    {
        base.Awake(); // 부모 구현을 호출하여 Instance 설정
        Debug.Log("ElementalEffectManager 초기화됨");
        Debug.Log($"ElementalEffectManager Awake 호출됨, Instance: {Instance != null}");
    }
    
    // 다른 스크립트에서 ElementalEffectManager 얻기 위한 헬퍼 메서드
    public static void EnsureExists()
    {
        if (Instance == null)
        {
            GameObject managerObject = new GameObject("ElementalEffectManager");
            managerObject.AddComponent<ElementalEffectManager>();
            Debug.Log("ElementalEffectManager 자동 생성됨");
            Debug.Log($"ElementalEffectManager.EnsureExists 호출됨, Instance 이전: {Instance != null}");
            DontDestroyOnLoad(managerObject);
            Debug.Log($"ElementalEffectManager.EnsureExists 객체 생성 후, Instance: {Instance != null}");
        }
    }
    
    private void Update()
    {
        // 일정 간격으로 땅 속성 효과의 무기 활성화 상태 체크
        if (Time.time - lastCheckTime > CHECK_INTERVAL)
        {
            lastCheckTime = Time.time;
            CheckEarthEffectWeaponState();
        }
    }
    
    // 땅 속성 효과가 적용된 상태에서 무기 효과가 비활성화된 경우 처리
    private void CheckEarthEffectWeaponState()
    {
        // 플레이어에게 적용된 땅 속성 효과 찾기
        EarthDamageEffect earthEffect = GetEarthDamageEffect(CharacterManager.PlayerCharacterData);
        
        // 땅 속성 효과가 있고 ItemSkillManager가 존재하는 경우
        if (earthEffect != null && ItemSkillManager.Instance != null)
        {
            // 무기 효과가 비활성화된 상태인지 확인
            if (!ItemSkillManager.Instance.IsActive)
            {
                // 무기 효과가 비활성화되었으므로 땅 속성 효과 제거
                RemoveEffect(earthEffect);
                Debug.Log("무기 효과 비활성화로 인해 땅 속성 효과가 자동 제거되었습니다.");
            }
        }
    }
    
    public void AddEffect(ElementalEffect newEffect)
    {
        // 자동 생성 확인
        EnsureExists();
        
        // 동일한 캐릭터에 같은 유형의 효과가 있는지 확인
        ElementalEffect existingEffect = activeEffects.FirstOrDefault(e => 
            e.Target == newEffect.Target && 
            e.GetType() == newEffect.GetType());
            
        // 이미 같은 유형의 효과가 적용되어 있다면 제거 후 새로 적용
        if (existingEffect != null)
        {
            RemoveEffect(existingEffect);
        }
        
        // 새 효과 적용
        Coroutine newCoroutine = StartCoroutine(newEffect.ProcessEffect());
        effectCoroutines[newEffect] = newCoroutine;
        activeEffects.Add(newEffect);
    }
    
    public void RemoveEffect(ElementalEffect effect)
    {
        // 코루틴 중지 (필요시)
        if (effectCoroutines.TryGetValue(effect, out Coroutine coroutine))
        {
            StopCoroutine(coroutine);
            effectCoroutines.Remove(effect);
        }
        
        activeEffects.Remove(effect);
    }
    
    public void ClearAllEffects()
    {
        foreach (var effect in activeEffects.ToList())
        {
            if (effectCoroutines.TryGetValue(effect, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
            }
            effect.Remove();
        }
        
        effectCoroutines.Clear();
        activeEffects.Clear();
    }
    
    public void ClearCharacterEffects(CharacterData character)
    {
        var characterEffects = activeEffects.Where(e => e.Target == character).ToList();
        foreach (var effect in characterEffects)
        {
            RemoveEffect(effect);
        }
    }
    
    // 특정 타입의 효과를 가져오는 메서드
    public T GetActiveEffect<T>(CharacterData character) where T : ElementalEffect
    {
        return activeEffects.FirstOrDefault(e => e.Target == character && e is T) as T;
    }
    
    // 땅 속성 데미지 효과를 가져오는 편의 메서드
    public EarthDamageEffect GetEarthDamageEffect(CharacterData character)
    {
        return GetActiveEffect<EarthDamageEffect>(character);
    }
    
    // 게임 상태 변경 시 처리
    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        // 게임 상태 변화에 따른 효과 처리 로직
        // 예: 게임이 일시정지 상태로 변경되면 모든 효과 일시정지
    }
} 
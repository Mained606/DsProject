using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DebuffManager : BaseManager<DebuffManager>
{
    [SerializeField] public List<Debuff> activeDebuffs = new List<Debuff>();
    private Dictionary<Debuff, Coroutine> debuffCoroutines = new Dictionary<Debuff, Coroutine>();

    public void ApplyDebuff(Debuff newDebuff)
    {
        // 동일한 캐릭터에 같은 유형의 디버프가 있는지 확인
        Debuff existingDebuff = activeDebuffs.FirstOrDefault(d => 
            d.Character == newDebuff.Character && 
            d.GetType() == newDebuff.GetType());

        // 이미 같은 유형의 디버프가 적용되어 있다면 새 디버프를 무시
        if (existingDebuff != null)
        {
            Debug.Log($"{existingDebuff.Character.characterName}에게 이미 {existingDebuff.GetType().Name} 디버프가 적용 중입니다. 새 디버프는 무시됩니다.");
            return;
        }

        // 새 디버프 적용
        Debug.Log($"적용된 버프: {newDebuff.GetType().Name} 버프 대상: {newDebuff.Character.characterName}");
        Coroutine newCoroutine = StartCoroutine(newDebuff.ApplyEffect());
        debuffCoroutines[newDebuff] = newCoroutine;
        activeDebuffs.Add(newDebuff);
    }

    public void RemoveDebuff(Debuff debuff)
    {
        // 코루틴 중지 (필요시)
        if (debuffCoroutines.TryGetValue(debuff, out Coroutine coroutine))
        {
            StopCoroutine(coroutine);
            debuffCoroutines.Remove(debuff);
        }
        
        activeDebuffs.Remove(debuff);
    }

    public void ClearAllDebuffs()
    {
        foreach (var debuff in activeDebuffs)
        {
            if (debuffCoroutines.TryGetValue(debuff, out Coroutine coroutine))
            {
                StopCoroutine(coroutine);
            }
        }
        
        debuffCoroutines.Clear();
        activeDebuffs.Clear();
    }

    // HandleGameStateChange 메서드 구현
    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        // 게임 상태 변화에 따른 디버프 처리 로직을 여기에 추가
        // 예: 게임이 일시정지 상태로 변경되면 모든 디버프 일시정지
        // if (newState == GameSystemState.Paused)
        // {
        //     // 모든 디버프 일시정지 로직
        // }
        // else if (newState == GameSystemState.Running)
        // {
        //     // 모든 디버프 재개 로직
        // }
    }
}

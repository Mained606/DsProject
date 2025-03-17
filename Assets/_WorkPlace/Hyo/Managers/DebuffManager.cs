using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebuffManager : BaseManager<DebuffManager>
{
    [SerializeField] public List<Debuff> activeDebuffs = new List<Debuff>();

    public void ApplyDebuff(Debuff debuff)
    {
        Debug.Log($"적용된 버프: {debuff.GetType().Name} 버프 대상: {debuff.Character.characterName}");
        StartCoroutine(debuff.ApplyEffect());
        activeDebuffs.Add(debuff);
    }

    public void RemoveDebuff(Debuff debuff)
    {
        activeDebuffs.Remove(debuff);
    }

    public void ClearAllDebuffs()
    {
        foreach (var debuff in activeDebuffs)
        {
            StopCoroutine(debuff.ApplyEffect());
        }
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

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 드래곤 상태 머신 클래스
/// 드래곤의 상태를 관리하고 상태 전환을 처리
/// </summary>
public class DragonStateMachine
{
    private IDragonState currentState; // 현재 드래곤 상태
    private DragonController dragon; // 드래곤 컨트롤러 참조
    private Animator animator; // 애니메이터 참조
    private Dictionary<System.Type, IDragonState> stateCache = new Dictionary<System.Type, IDragonState>(); // 상태 캐싱

    /// <summary>
    /// 상태 머신 생성자
    /// </summary>
    /// <param name="dragon">드래곤 컨트롤러 참조</param>
    /// <param name="animator">애니메이터 참조</param>
    public DragonStateMachine(DragonController dragon, Animator animator)
    {
        this.dragon = dragon;
        this.animator = animator;

        // 상태 객체 생성 및 캐싱
        stateCache[typeof(DragonIdleState)] = new DragonIdleState();
        stateCache[typeof(DragonMovingState)] = new DragonMovingState();
        stateCache[typeof(DragonMeleeAttackState)] = new DragonMeleeAttackState();
        stateCache[typeof(DragonRangedAttackState)] = new DragonRangedAttackState();
        stateCache[typeof(DragonSkillAttackState)] = new DragonSkillAttackState();
        stateCache[typeof(DragonUltimateAttackState)] = new DragonUltimateAttackState();
    }

    /// <summary>
    /// 현재 상태 가져오기
    /// </summary>
    public IDragonState CurrentState => currentState;

    /// <summary>
    /// 특정 상태로 전환
    /// </summary>
    /// <typeparam name="T">전환할 상태 타입</typeparam>
    public void SetState<T>() where T : IDragonState
    {
        if (stateCache.TryGetValue(typeof(T), out IDragonState newState))
        {
            // 이미 같은 상태이고 강제 전환이 아니면 무시
            if (currentState != null && currentState.GetType() == typeof(T))
                return;

            // 현재 상태 종료 처리
            currentState?.ExitState();
            
            // 새 상태로 전환
            currentState = newState;
            currentState.EnterState(dragon, this, animator);
            
            // 디버그 로그
            Debug.Log($"[DragonFSM] 상태 전환: {typeof(T).Name}");
        }
        else
        {
            Debug.LogError($"[DragonFSM] {typeof(T).Name} 상태가 캐시에 존재하지 않습니다!");
        }
    }

    /// <summary>
    /// 현재 상태 업데이트
    /// </summary>
    public void UpdateState()
    {
        currentState?.UpdateState();
    }
} 
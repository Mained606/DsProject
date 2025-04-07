using UnityEngine;

/// <summary>
/// 드래곤 FSM 상태 인터페이스
/// 모든 드래곤 상태는 이 인터페이스를 구현해야 함
/// </summary>
public interface IDragonState
{
    /// <summary>
    /// 상태 진입 시 호출되는 메서드
    /// </summary>
    /// <param name="dragon">드래곤 컨트롤러 참조</param>
    /// <param name="stateMachine">상태 머신 참조</param>
    /// <param name="animator">애니메이터 참조</param>
    void EnterState(DragonController dragon, DragonStateMachine stateMachine, Animator animator);
    
    /// <summary>
    /// 상태 업데이트 시 호출되는 메서드
    /// </summary>
    void UpdateState();
    
    /// <summary>
    /// 상태 종료 시 호출되는 메서드
    /// </summary>
    void ExitState();
} 
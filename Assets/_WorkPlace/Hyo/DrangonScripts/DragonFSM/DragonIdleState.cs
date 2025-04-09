using UnityEngine;

/// <summary>
/// 드래곤 대기 상태 클래스
/// </summary>
public class DragonIdleState : IDragonState
{
    private DragonController dragon;
    private DragonStateMachine stateMachine;
    private Animator animator;
    private float stateTimer;
    private float movingStateThreshold = 0.5f; // 이동 상태 전환을 위한 시간 임계값 (더 길게 설정)
    private float lastPlayerMovingCheck = 0f; // 마지막으로 플레이어 이동 상태를 확인한 시간
    private bool isPlayerMovingConsistent = false; // 플레이어 이동 상태의 일관성 확인
    private float playerIdleTime = 0f; // 플레이어가 정지한 상태로 유지된 시간
    private bool wasFollowingMovingPlayer = false; // 이전에 이동 중인 플레이어를 따라가고 있었는지

    /// <summary>
    /// 드래곤 대기 상태 진입
    /// </summary>
    public void EnterState(DragonController dragon, DragonStateMachine stateMachine, Animator animator)
    {
        this.dragon = dragon;
        this.stateMachine = stateMachine;
        this.animator = animator;
        
        // 애니메이터 null 체크 추가
        if (animator == null)
        {
            Debug.LogWarning("[DragonIdleState] 애니메이터가 null입니다. 애니메이션을 적용하지 않습니다.");
            return;
        }
        
        try
        {
            // 대기 애니메이션 설정
            animator.SetBool(DragonController.IsMoving, false);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DragonIdleState] 애니메이션 설정 중 오류 발생: {e.Message}");
        }
        
        // 상태 타이머 초기화
        stateTimer = 0f;
        lastPlayerMovingCheck = 0f;
        isPlayerMovingConsistent = false;
        playerIdleTime = 0f;
    }

    /// <summary>
    /// 드래곤 대기 상태 업데이트
    /// </summary>
    public void UpdateState()
    {
        stateTimer += Time.deltaTime;

        // 전투 상태인지 확인
        bool inCombat = GameStateMachine.Instance.CurrentState == GameSystemState.Combat || 
                         GameStateMachine.Instance.CurrentState == GameSystemState.BossBattle;

        if (inCombat)
        {
            // 전투 중 타겟이 있으면 전투 로직 실행
            if (dragon.ShouldFindTarget())
            {
                dragon.FindNearestTarget();
                
                // 타겟이 있고, 스킬 쿨다운이 끝났으면
                if (!dragon.IsSkillCooldown && dragon.HasTarget)
                {
                    stateMachine.SetState<DragonSkillAttackState>();
                    return;
                }
                
                // 타겟이 있고, 원거리 공격 쿨다운이 끝났으면
                if (!dragon.IsRangedCooldown && dragon.HasTarget && dragon.IsTargetInRange(dragon.DetectRange))
                {
                    stateMachine.SetState<DragonRangedAttackState>();
                    return;
                }
                
                // 타겟이 있고, 근접 공격 쿨다운이 끝났고, 타겟이 근접 범위 내에 있으면
                if (!dragon.IsMeleeCooldown && dragon.HasTarget && dragon.IsTargetInMeleeRange())
                {
                    stateMachine.SetState<DragonMeleeAttackState>();
                    return;
                }
                
                // 타겟이 있지만 거리가 멀면 이동 상태로 전환
                if (dragon.HasTarget && !dragon.IsTargetInMeleeRange())
                {
                    stateMachine.SetState<DragonMovingState>();
                    return;
                }
            }
        }
        else
        {
            // 일정 시간 간격으로 플레이어 이동 감지
            lastPlayerMovingCheck += Time.deltaTime;
            if (lastPlayerMovingCheck >= movingStateThreshold)
            {
                // 플레이어 이동 상태 확인
                bool currentPlayerMoving = dragon.IsPlayerMoving();
                
                // 이전 체크와 일관성이 있으면 상태 전환 고려
                if (currentPlayerMoving)
                {
                    if (!isPlayerMovingConsistent)
                    {
                        isPlayerMovingConsistent = true;
                        playerIdleTime = 0f;
                    }
                    else
                    {
                        // 플레이어가 일정 시간 이상 일관되게 이동 중이면 이동 상태로 전환
                        stateMachine.SetState<DragonMovingState>();
                        return;
                    }
                }
                else
                {
                    // 이동이 감지되지 않았으면 일관성 플래그 초기화
                    isPlayerMovingConsistent = false;
                }
                
                lastPlayerMovingCheck = 0f; // 타이머 리셋
            }
            
            // 플레이어와의 거리가 너무 멀면 이동 상태로 전환
            if (dragon.IsPlayerTooFar())
            {
                stateMachine.SetState<DragonMovingState>();
                return;
            }
            
            // 플레이어가 멈춰있을 때도 드래곤이 플레이어를 따라가야 함
            dragon.FollowPlayer();
        }
    }

    /// <summary>
    /// 드래곤 대기 상태 종료
    /// </summary>
    public void ExitState()
    {
        // 필요한 리소스 정리
        dragon = null;
        stateMachine = null;
        animator = null;
    }
}
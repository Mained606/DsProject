using UnityEngine;

/// <summary>
/// 드래곤 이동 상태 클래스
/// </summary>
public class DragonMovingState : IDragonState
{
    private DragonController dragon;
    private DragonStateMachine stateMachine;
    private Animator animator;
    private float stateTimer;
    private float idleStateThreshold = 0.5f; // 대기 상태 전환을 위한 시간 임계값
    private float lastPlayerMovingCheck = 0f; // 마지막으로 플레이어 이동 상태를 확인한 시간

    /// <summary>
    /// 드래곤 이동 상태 진입
    /// </summary>
    public void EnterState(DragonController dragon, DragonStateMachine stateMachine, Animator animator)
    {
        this.dragon = dragon;
        this.stateMachine = stateMachine;
        this.animator = animator;
        
        // 이동 애니메이션 설정
        animator.SetBool(DragonController.IsMoving, true);
        
        // 상태 타이머 초기화
        stateTimer = 0f;
        lastPlayerMovingCheck = 0f;
    }

    /// <summary>
    /// 드래곤 이동 상태 업데이트
    /// </summary>
    public void UpdateState()
    {
        stateTimer += Time.deltaTime;

        // 전투 상태인지 확인
        bool inCombat = GameStateMachine.Instance.CurrentState == GameSystemState.Combat || 
                         GameStateMachine.Instance.CurrentState == GameSystemState.BossBattle;

        if (inCombat)
        {
            // 전투 중 이동 로직
            if (dragon.HasTarget)
            {
                // 타겟을 향해 이동
                dragon.MoveTowardTarget();
                
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
                
                // 플레이어와 너무 멀어졌으면 플레이어 쫓아가기
                if (dragon.IsPlayerTooFar())
                {
                    dragon.ResetTarget();
                    dragon.FollowPlayer();
                }
            }
            else
            {
                // 타겟이 없으면 타겟 찾기
                dragon.FindNearestTarget();
                
                // 그래도 타겟이 없으면 플레이어 쫓아가기
                if (!dragon.HasTarget)
                {
                    dragon.FollowPlayer();
                    
                    // 플레이어가 멈췄으면 대기 상태로 전환
                    if (!dragon.IsPlayerMoving())
                    {
                        stateMachine.SetState<DragonIdleState>();
                        return;
                    }
                }
            }
        }
        else
        {
            // 탐색 모드에서 플레이어 따라가기
            dragon.FollowPlayer();
            
            // 일정 간격으로만 상태 전환 검사 (과도한 전환 방지)
            lastPlayerMovingCheck += Time.deltaTime;
            if (lastPlayerMovingCheck >= idleStateThreshold)
            {
                lastPlayerMovingCheck = 0f;
                
                // 플레이어가 멈췄으면 대기 상태로 전환
                if (!dragon.IsPlayerMoving())
                {
                    stateMachine.SetState<DragonIdleState>();
                    return;
                }
            }
        }
    }

    /// <summary>
    /// 드래곤 이동 상태 종료
    /// </summary>
    public void ExitState()
    {
        // 필요한 리소스 정리
        dragon = null;
        stateMachine = null;
        animator = null;
    }
} 
using UnityEngine;

/// <summary>
/// 드래곤 스킬 공격 상태 클래스
/// </summary>
public class DragonSkillAttackState : IDragonState
{
    private DragonController dragon;
    private DragonStateMachine stateMachine;
    private Animator animator;
    private float skillDuration;
    private float stateTimer;
    private bool hasAppliedSkill;

    /// <summary>
    /// 드래곤 스킬 공격 상태 진입
    /// </summary>
    public void EnterState(DragonController dragon, DragonStateMachine stateMachine, Animator animator)
    {
        this.dragon = dragon;
        this.stateMachine = stateMachine;
        this.animator = animator;
        
        // 스킬 애니메이션 설정
        animator.SetBool(DragonController.IsMoving, false);
        animator.SetTrigger(DragonController.IsUseSkill);
        
        // 스킬 상태 변수 초기화
        stateTimer = 0f;
        skillDuration = 1.5f; // 스킬 애니메이션 지속 시간
        hasAppliedSkill = false;
        
        // 드래곤이 공격 중 상태로 설정
        dragon.SetAttacking(true);
    }

    /// <summary>
    /// 드래곤 스킬 공격 상태 업데이트
    /// </summary>
    public void UpdateState()
    {
        stateTimer += Time.deltaTime;

        // 스킬 실행 타이밍 (애니메이션의 절반 시점)
        if (!hasAppliedSkill && stateTimer >= skillDuration * 0.5f)
        {
            hasAppliedSkill = true;
            
            // 버프 스킬 사용
            dragon.UseBuffSkill();
            
            // 스킬 쿨다운 시작
            dragon.StartSkillCooldown();
        }

        // 스킬 애니메이션 종료 후 다음 상태로 전환
        if (stateTimer >= skillDuration)
        {
            // 공격 상태 해제
            dragon.SetAttacking(false);
            
            // 전투 상태인지 확인
            bool inCombat = GameStateMachine.Instance.CurrentState == GameSystemState.Combat || 
                           GameStateMachine.Instance.CurrentState == GameSystemState.BossBattle;
            
            if (inCombat && dragon.HasTarget)
            {
                // 전투 중이고 타겟이 있으면 타겟 상태에 따라 다음 상태 결정
                if (dragon.IsTargetInMeleeRange() && !dragon.IsMeleeCooldown)
                {
                    // 타겟이 근접 범위 내에 있고 근접 공격이 가능하면
                    stateMachine.SetState<DragonMeleeAttackState>();
                }
                else if (!dragon.IsRangedCooldown && dragon.IsTargetInRange(dragon.DetectRange))
                {
                    // 원거리 공격이 가능하면
                    stateMachine.SetState<DragonRangedAttackState>();
                }
                else if (!dragon.IsTargetInMeleeRange())
                {
                    // 타겟이 근접 범위 밖에 있으면 이동
                    stateMachine.SetState<DragonMovingState>();
                }
                else
                {
                    // 모든 공격이 쿨다운 중이면 대기
                    stateMachine.SetState<DragonIdleState>();
                }
            }
            else
            {
                // 전투 중이 아니거나 타겟이 없으면 대기 상태로
                stateMachine.SetState<DragonIdleState>();
            }
        }
    }

    /// <summary>
    /// 드래곤 스킬 공격 상태 종료
    /// </summary>
    public void ExitState()
    {
        // 공격 상태 해제 (중복 해제 방지용)
        dragon.SetAttacking(false);
        
        // 필요한 리소스 정리
        dragon = null;
        stateMachine = null;
        animator = null;
    }
} 
using UnityEngine;

/// <summary>
/// 드래곤 궁극기 공격 상태 클래스
/// </summary>
public class DragonUltimateAttackState : IDragonState
{
    private DragonController dragon;
    private DragonStateMachine stateMachine;
    private Animator animator;
    private float ultimateDuration;
    private float stateTimer;
    private bool hasDealtDamage;

    /// <summary>
    /// 드래곤 궁극기 상태 진입
    /// </summary>
    public void EnterState(DragonController dragon, DragonStateMachine stateMachine, Animator animator)
    {
        this.dragon = dragon;
        this.stateMachine = stateMachine;
        this.animator = animator;
        
        // 궁극기 애니메이션 설정
        animator.SetBool(DragonController.IsMoving, false);
        animator.SetTrigger(DragonController.IsUltimate);
        
        // 궁극기 상태 변수 초기화
        stateTimer = 0f;
        ultimateDuration = 3.0f; // 궁극기 애니메이션 지속 시간
        hasDealtDamage = false;
        
        // 드래곤이 공격 중 상태로 설정
        dragon.SetAttacking(true);
    }

    /// <summary>
    /// 드래곤 궁극기 상태 업데이트
    /// </summary>
    public void UpdateState()
    {
        stateTimer += Time.deltaTime;

        // 궁극기 데미지 타이밍 (애니메이션의 2/3 시점)
        if (!hasDealtDamage && stateTimer >= ultimateDuration * 0.6f)
        {
            hasDealtDamage = true;
            
            // 궁극기 효과 실행 (광역 공격)
            dragon.PerformUltimateAttack();
            
            // 궁극기 쿨다운 시작
            dragon.StartUltimateCooldown();
        }

        // 궁극기 애니메이션 종료 후 다음 상태로 전환
        if (stateTimer >= ultimateDuration)
        {
            // 공격 상태 해제
            dragon.SetAttacking(false);
            
            // 전투 상태인지 확인
            bool inCombat = dragon.IsInCombat();
            
            // 다음 상태 결정
            if (inCombat && dragon.HasTarget)
            {
                if (dragon.IsTargetInMeleeRange())
                {
                    // 타겟이 근접 범위 내에 있으면 대기 상태로
                    stateMachine.SetState<DragonIdleState>();
                }
                else
                {
                    // 타겟이 범위 밖에 있으면 이동 상태로
                    stateMachine.SetState<DragonMovingState>();
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
    /// 드래곤 궁극기 상태 종료
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
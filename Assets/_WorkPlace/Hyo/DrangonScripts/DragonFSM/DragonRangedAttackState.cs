using System.Collections;
using UnityEngine;

/// <summary>
/// 드래곤 원거리 공격 상태 클래스
/// </summary>
public class DragonRangedAttackState : IDragonState
{
    private DragonController dragon;
    private DragonStateMachine stateMachine;
    private Animator animator;
    private float attackDuration;
    private float stateTimer;
    private bool hasLaunchedProjectile;

    /// <summary>
    /// 드래곤 원거리 공격 상태 진입
    /// </summary>
    public void EnterState(DragonController dragon, DragonStateMachine stateMachine, Animator animator)
    {
        this.dragon = dragon;
        this.stateMachine = stateMachine;
        this.animator = animator;
        
        // 공격 애니메이션 설정
        animator.SetBool(DragonController.IsMoving, false);
        animator.SetTrigger(DragonController.IsRangedAttack);
        
        // 공격 상태 변수 초기화
        stateTimer = 0f;
        attackDuration = 2.0f; // 원거리 공격 애니메이션 지속 시간
        hasLaunchedProjectile = false;
        
        // 드래곤이 공격 중 상태로 설정
        dragon.SetAttacking(true);
        
        // 타겟을 향해 회전
        if (dragon.HasTarget)
        {
            dragon.RotateToTarget();
        }
    }

    /// <summary>
    /// 드래곤 원거리 공격 상태 업데이트
    /// </summary>
    public void UpdateState()
    {
        stateTimer += Time.deltaTime;

        // 공격 준비 시간이 지났고 아직 발사하지 않았다면
        if (!hasLaunchedProjectile && stateTimer >= 0.5f)
        {
            hasLaunchedProjectile = true;
            
            // 발사체 생성 및 발사
            if (dragon.HasTarget)
            {
                dragon.LaunchProjectile();
            }
            
            // 공격 쿨다운 시작
            dragon.StartRangedCooldown();
        }

        // 공격 애니메이션 종료 후 다음 상태로 전환
        if (stateTimer >= attackDuration)
        {
            // 공격 상태 해제
            dragon.SetAttacking(false);
            
            // 타겟 상태에 따라 다음 상태 선택
            if (dragon.HasTarget)
            {
                if (dragon.IsTargetInMeleeRange())
                {
                    // 타겟이 근접 범위 내에 있고 근접 공격이 가능하면
                    if (!dragon.IsMeleeCooldown)
                    {
                        stateMachine.SetState<DragonMeleeAttackState>();
                    }
                    else
                    {
                        // 근접 공격이 불가능하면 대기
                        stateMachine.SetState<DragonIdleState>();
                    }
                }
                else
                {
                    // 타겟이 범위 밖에 있으면 이동 상태로
                    stateMachine.SetState<DragonMovingState>();
                }
            }
            else
            {
                // 타겟이 없으면 대기 상태로
                stateMachine.SetState<DragonIdleState>();
            }
        }
    }

    /// <summary>
    /// 드래곤 원거리 공격 상태 종료
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
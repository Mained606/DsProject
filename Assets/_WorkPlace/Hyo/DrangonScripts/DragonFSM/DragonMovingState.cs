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
    private float idleStateThreshold = 0.8f; // 대기 상태 전환을 위한 시간 임계값 (더 길게 설정)
    private float lastPlayerMovingCheck = 0f; // 마지막으로 플레이어 이동 상태를 확인한 시간
    private float idleCheckCountdown = 0f; // 플레이어가 멈춘 후 아이들 상태로 전환하기 전 카운트다운
    private float followDistance = 0f; // 드래곤이 일정 거리만큼 따라간 후 아이들 상태로 돌아가기 위한 변수
    private Vector3 lastPlayerPosition; // 플레이어의 마지막 위치
    private bool isTransitioningToIdle = false; // 아이들 상태로 전환 중인지 여부

    /// <summary>
    /// 드래곤 이동 상태 진입
    /// </summary>
    public void EnterState(DragonController dragon, DragonStateMachine stateMachine, Animator animator)
    {
        this.dragon = dragon;
        this.stateMachine = stateMachine;
        this.animator = animator;
        
        // 애니메이터 null 체크 추가
        if (animator == null)
        {
            Debug.LogWarning("[DragonMovingState] 애니메이터가 null입니다. 애니메이션을 적용하지 않습니다.");
            return;
        }
        
        try
        {
            // 이동 애니메이션 설정
            animator.SetBool(DragonController.IsMoving, true);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[DragonMovingState] 애니메이션 설정 중 오류 발생: {e.Message}");
        }
        
        // 상태 타이머 초기화
        stateTimer = 0f;
        lastPlayerMovingCheck = 0f;
        idleCheckCountdown = 0f;
        isTransitioningToIdle = false;
        
        // 플레이어 위치 저장
        if (GameManager.playerTransform != null)
        {
            lastPlayerPosition = GameManager.playerTransform.position;
            followDistance = 0f;
        }
    }

    /// <summary>
    /// 드래곤 이동 상태 업데이트
    /// </summary>
    public void UpdateState()
    {
        stateTimer += Time.deltaTime;

        // 전투 상태인지 확인
        bool inCombat = dragon.IsInCombat();

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
                
                // 플레이어 이동 상태 확인
                bool isPlayerMoving = dragon.IsPlayerMoving();
                
                // 플레이어가 멈췄을 때
                if (!isPlayerMoving)
                {
                    if (!isTransitioningToIdle)
                    {
                        // 플레이어가 멈췄지만 아직 전환 중이 아니라면
                        isTransitioningToIdle = true;
                        idleCheckCountdown = 2.0f; // 2초 후에 아이들 상태로 전환
                    }
                }
                else
                {
                    // 플레이어가 다시 움직이면 전환 취소
                    isTransitioningToIdle = false;
                    idleCheckCountdown = 0f;
                    
                    // 새로운 위치 저장
                    if (GameManager.playerTransform != null)
                    {
                        lastPlayerPosition = GameManager.playerTransform.position;
                        followDistance = 0f;
                    }
                }
            }
            
            // 아이들 상태로 전환 중이라면
            if (isTransitioningToIdle)
            {
                // 플레이어와 마지막 위치 사이의 거리 계산
                if (GameManager.playerTransform != null)
                {
                    followDistance += Vector3.Distance(lastPlayerPosition, GameManager.playerTransform.position);
                    lastPlayerPosition = GameManager.playerTransform.position;
                    
                    // 일정 거리 이상 따라갔거나 카운트다운이 끝났다면 아이들 상태로 전환
                    idleCheckCountdown -= Time.deltaTime;
                    if (idleCheckCountdown <= 0f)
                    {
                        stateMachine.SetState<DragonIdleState>();
                        return;
                    }
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
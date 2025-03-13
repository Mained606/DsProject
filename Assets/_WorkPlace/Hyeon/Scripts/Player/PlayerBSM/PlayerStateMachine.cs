using System.Collections.Generic;

using UnityEngine;

namespace JWSTEST
{
    public class PlayerStateMachine
    {
        private IPlayerState currentState; // 현재 플레이어 상태
        private PlayerController player; // 플레이어 컨트롤러
        private Animator animator; // 애니메이터
        private Dictionary<System.Type, IPlayerState> stateCache = new Dictionary<System.Type, IPlayerState>(); // 상태 캐싱

        public PlayerStateMachine(PlayerController player, Animator animator)
        {
            this.player = player;
            this.animator = animator;

            // 🔹 상태를 미리 생성하여 캐싱
            stateCache[typeof(IdleState)] = new IdleState();
            //stateCache[typeof(MoveState)] = new MoveState();
            //stateCache[typeof(JumpState)] = new JumpState();
            //stateCache[typeof(HitState)] = new HitState();
            //stateCache[typeof(DeathState)] = new DeathState();
        }

        public void SetState<T>() where T : IPlayerState
        {
            if (stateCache.TryGetValue(typeof(T), out IPlayerState newState))
            {
                currentState?.ExitState();
                currentState = newState;
                currentState.EnterState(player, this, animator);
            }
            else
            {
                Debug.LogError($"[오류] {typeof(T)} 상태가 캐시에 존재하지 않습니다!");
            }
        }

        public void UpdateState()
        {
            currentState?.UpdateState();
        }

        public void OnHit()
        {
            // SetState<HitState>();
        }
    }
}

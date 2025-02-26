using UnityEngine;

namespace JWSTEST
{
    public class IdleState : IPlayerState
    {
        private PlayerController player;
        private PlayerStateMachine stateMachine;
        private Animator animator;

        public void EnterState(PlayerController player, PlayerStateMachine stateMachine, Animator animator)
        {
            this.player = player;
            this.stateMachine = stateMachine;
            this.animator = animator;
            animator.SetBool("Idle", true);
        }

        public void UpdateState()
        {
            if (InputManager.InputActions.actions["Move"].ReadValue<Vector2>() != Vector2.zero)
            {
                // stateMachine.SetState<MoveState>(); // ✅ 상태 변경 시 캐싱된 객체 사용
            }
        }

        public void ExitState()
        {
            ResetState();
        }

        private void ResetState()
        {
            animator.SetBool("Idle", false);
            player = null;
            stateMachine = null;
            animator = null;
        }
    }
}

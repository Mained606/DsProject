using UnityEngine;

namespace JWSTEST
{
    public interface IPlayerState
    {
        void EnterState(PlayerController player, PlayerStateMachine stateMachine, Animator animator);
        void UpdateState();
        void ExitState();
    }
}
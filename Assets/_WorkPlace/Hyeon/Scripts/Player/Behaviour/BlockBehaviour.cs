using UnityEngine;
using static UnityEngine.InputSystem.OnScreen.OnScreenStick;

public class BlockBehaviour : IBehaviour
{
    private PlayerController controller;
    private Animator animator;

    public BlockBehaviour()
    {
        controller = GameManager.playerTransform.GetComponent<PlayerController>();
        animator = controller.PlayerAnimator;
    }

    public void Enter()
    {
        controller.playerCombat.isBlocking = false;
        animator.SetBool("Block", false);
    }
    public void Execute()
    {
        HandleBlockInput();
    }
    public void Exit()
    {
        controller.playerCombat.isBlocking = false;
        animator.SetBool("Block", false);
    }

    private void HandleBlockInput()
    {
        if (InputManager.InputActions.actions["Block"].IsPressed() && controller.playerCombat.hasWeapon)
        {
            PlayerBehaviourManager.Instance.CanBlock = true;    // 테스트때문에 넣어줌 bool수로 잘 제어되면 삭제할 것
            PlayerBehaviourManager.Instance.CanMove = false;
            PlayerBehaviourManager.Instance.CanAttack = false;
            PlayerBehaviourManager.Instance.CanParry = true;
            controller.playerCombat.isBlocking = true;
            
        }
        else
        {
            if (controller.playerCombat.isBlocking)
            {
                BlockFinished();
            }
        }
        animator.SetBool("Block", controller.playerCombat.isBlocking);
    }

    private void BlockFinished()
    {
        PlayerBehaviourManager.Instance.CanMove = true;
        PlayerBehaviourManager.Instance.CanAttack = true;
        PlayerBehaviourManager.Instance.CanParry = false;
        controller.playerCombat.isBlocking = false;
    }
}

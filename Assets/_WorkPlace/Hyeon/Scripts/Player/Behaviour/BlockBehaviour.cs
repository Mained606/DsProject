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
        Debug.Log("Block Exit");
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
            //PlayerBehaviourManager.Instance.CanBlock = false;    // 테스트때문에 넣어줌 bool수로 잘 제어되면 삭제할 것
            PlayerBehaviourManager.Instance.CanMove = true;
            PlayerBehaviourManager.Instance.CanAttack = true;
            controller.playerCombat.isBlocking = false;
            PlayerBehaviourManager.Instance.CanParry = false;
        }
        animator.SetBool("Block", controller.playerCombat.isBlocking);
    }
}

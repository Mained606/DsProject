using UnityEngine;
using UnityEngine.InputSystem;

// 베이스 커뮤니케이터 상속
public class InputManager : BaseCommunicator
{
    public Vector2 MoveInput { get; private set; }
    public bool IsJumping { get; private set; }
    // public bool IsAttacking { get; private set; } // 전투 상태에서 공격 여부

    private PlayerInput playerInput;
    
    protected override void OnEnable()
    {
        base.OnEnable();
        
        // PlayerInput 초기화
        playerInput = new PlayerInput();
        
        // 이동 및 점프 이벤트 등록
        playerInput.Player.Move.performed += OnMovePerformed;
        playerInput.Player.Move.canceled += OnMoveCanceled;
        playerInput.Player.Jump.performed += OnJumpPerformed;
        playerInput.Player.Jump.canceled += OnJumpCanceled;
        
        // 전투 상태 입력 설정
        // playerInput.Combat.Attack.performed += OnAttackPerformed;
        // playerInput.Combat.Attack.canceled += OnAttackCanceled;
        
        // 기본적으로 탐험 상태 활성화
        EnableExplorationInput();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        
        // 이벤트 해제
        playerInput.Player.Move.performed -= OnMovePerformed;
        playerInput.Player.Move.canceled -= OnMoveCanceled;
        playerInput.Player.Jump.performed -= OnJumpPerformed;
        playerInput.Player.Jump.canceled -= OnJumpCanceled;

        playerInput.Dispose();
    }
    
    private void OnGameStateChanged(GameSystemState newState)
    {
        if (newState == GameSystemState.Exploration)
        {
            EnableExplorationInput();
        }
        
        // 이후로 모드 별 키 세팅 메서드 추가
        // 인풋 액션에서 세팅 추가 필요(Combat)
        // else if (newState == GameSystemState.Combat)
        // {
        //     EnableCombatInput();
        // }
    }
    
    // 탐험 모드의 키 세팅
    private void EnableExplorationInput()
    {
        playerInput.Player.Enable();
        // playerInput.Combat.Disable(); // 전투 입력 비활성화
        Debug.Log("[InputManager] 탐험 상태 입력 활성화");
    }
    
    // 전투 모드의 키 세팅
    // private void EnableCombatInput()
    // {
    //     playerInput.Combat.Enable();
    //     playerInput.Player.Disable(); // 탐험 입력 비활성화
    // }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        MoveInput = context.ReadValue<Vector2>();
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        MoveInput = Vector2.zero;
    }

    private void OnJumpPerformed(InputAction.CallbackContext context)
    {
        IsJumping = true;
    }

    private void OnJumpCanceled(InputAction.CallbackContext context)
    {
        IsJumping = false;
    }
    
    // private void OnAttackPerformed(InputAction.CallbackContext context)
    // {
    //     IsAttacking = true;
    // }
    //
    // private void OnAttackCanceled(InputAction.CallbackContext context)
    // {
    //     IsAttacking = false;
    // }
}
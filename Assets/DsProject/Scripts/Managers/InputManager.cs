using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : BaseManager<InputManager>
{
    public static PlayerInput InputActions;
    protected override void Awake()
    {
        base.Awake();
        InputActions = GetComponent<PlayerInput>();
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
       
    }
    #region 테스트용 추가 메서드 불필요시 삭제
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("I키 입력 확인");
            ToggleInventoryState();
        }
        if(Input.GetKeyDown(KeyCode.Q))
        {
            ToggleInventoryState2();
        }
        if (Input.GetKeyDown(KeyCode.S))
        {
            ToggleInventoryState3();
        }
    }
    
    private void ToggleInventoryState()
    {
        var currentState = GameStateMachine.Instance.CurrentState;

        if (currentState != GameSystemState.Inventory)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Inventory, null);
        }
        else if (currentState == GameSystemState.Inventory)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu, null);
        }
    }
    private void ToggleInventoryState2()
    {
        var currentState = GameStateMachine.Instance.CurrentState;

        if (currentState != GameSystemState.Quest)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Quest, null);
        }
        else if (currentState == GameSystemState.Quest)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu, null);
        }
    }
    private void ToggleInventoryState3()
    {
        var currentState = GameStateMachine.Instance.CurrentState;

        if (currentState != GameSystemState.Status)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Status, null);
        }
        else if (currentState == GameSystemState.Status)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu, null);
        }
    }
    #endregion
}


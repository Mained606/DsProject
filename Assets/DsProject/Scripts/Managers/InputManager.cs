using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Linq;

public class InputManager : BaseManager<InputManager>
{
    public static PlayerInput InputActions;

    // 입력 상태 관리
    private static Dictionary<string, bool> inputStates = new Dictionary<string, bool>();
    private static Dictionary<string, InputAction> inputActions = new Dictionary<string, InputAction>();

    protected override void Awake()
    {
        base.Awake();
        InputActions = GetComponent<PlayerInput>();

        InputInitialize();  // 입력 초기화
    }

    private void InputInitialize()
    {
        foreach (var action in InputActions.actions)
        {
            string actionName = action.name;
            inputActions[actionName] = action;
            inputStates[actionName] = false;
            action.performed += ctx => UpdateInputState(actionName, true);
            action.canceled += ctx => UpdateInputState(actionName, false);
        }
    }

    private void UpdateInputState(string actionName, bool state)
    {
        if (inputStates.ContainsKey(actionName))
        {
            inputStates[actionName] = state;
        }
    }

    public static bool GetInputState(string actionName)
    {
        return inputStates.ContainsKey(actionName) && inputStates[actionName];
    }

    public void SetInputEnabled(bool enabled, string actionName)
    {
        if (inputActions.ContainsKey(actionName))
        {
            inputStates[actionName] = enabled;

            if (enabled)
                inputActions[actionName].Enable();
            else
                inputActions[actionName].Disable();
        }
        else
        {
            Debug.LogWarning($"[경고] 입력 액션 '{actionName}'을 찾을 수 없습니다.");
        }
    }

    public void SetMultipleInputsEnabled(bool enabled, params string[] actionNames)
    {
        foreach (var actionName in actionNames)
        {
            SetInputEnabled(enabled, actionName);
        }
    }

    public void SetAllInputs(bool enabled)
    {
        foreach (var actionName in inputActions.Keys)
        {
            SetInputEnabled(enabled, actionName);
        }
    }

    private void OnDisable()
    {
        foreach (var action in InputActions.actions)
        {
            string actionName = action.name;
            action.performed -= ctx => UpdateInputState(actionName, true);
            action.canceled -= ctx => UpdateInputState(actionName, false);
        }
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}

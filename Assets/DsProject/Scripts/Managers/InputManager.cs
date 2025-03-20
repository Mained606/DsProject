using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : BaseManager<InputManager>
{ 
    public static PlayerInput InputActions;

    // 모든 입력의 활성 상태를 관리하는 딕셔너리
    private Dictionary<string, bool> inputStates = new Dictionary<string, bool>();

    protected override void Awake()
    {
        base.Awake();
        InputActions = GetComponent<PlayerInput>();

        InputInitialize();

        #region Delete
        // ui키 연결
        InputActions.actions["Inventory"].performed += OnInventoryKey;
        InputActions.actions["Quest"].performed += OnQuestReview;
        InputActions.actions["StatusUI"].performed += OnStatKey;
        InputActions.actions["ESC"].performed += OnMainMenu;
        InputActions.actions["Craft"].performed += OnCrafting;
        InputActions.actions["Cook"].performed += OnCooking;
        InputActions.actions["Skill"].performed += OnSkills;
        #endregion
    }

    private void InputInitialize()
    {
        foreach(var action in InputActions.actions)
        {
            inputStates[action.name] = true;
        }
    }

    public void SetInputEnabled(bool enabled, string actionName)
    {
        var action = InputActions.actions.FindAction(actionName, false);
        if (action != null)
        {
            inputStates[actionName] = enabled;

            if (enabled)
            {
                InputActions.actions[actionName].Enable();
            }
            else
            {
                InputActions.actions[actionName].Disable();
                Debug.Log("InputAction Disable");
            }
        }
        else
        {
            Debug.LogWarning($"입력 액션 '{actionName}'을 찾을 수 없습니다.");
        }
    }

    // 여러 개의 입력을 한 번에 설정하는 함수
    public void SetMultipleInputsEnabled(bool enabled, params string[] actionNames)
    {
        foreach(var actionName in actionNames)
        {
            SetInputEnabled(enabled, actionName);
        }
    }

    public void SetAllInputs(bool enabled)
    {
        foreach(var action in InputActions.actions)
        {
            SetInputEnabled(enabled, action.name);
        }
    }

    #region Delete


    private void OnInventoryKey(InputAction.CallbackContext context)
    {

        // 상태를 Inventory로 전환
        if (GameStateMachine.Instance.CurrentState != GameSystemState.Inventory)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Inventory);
            Debug.Log("Inventory 상태로 전환됨.");
        }

        else
        {
            Debug.Log("I 1");

            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
            Debug.Log("MainMenu 상태로 복귀됨.");
        }

    }

    #region 03.17 C
    private void OnCrafting(InputAction.CallbackContext context)
    {
        // 상태를 Crafting으로 전환
        if (GameStateMachine.Instance.CurrentState != GameSystemState.Craft)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Craft);
            Debug.Log("Crafting 상태로 전환됨.");
        }

        else
        {
            Debug.Log("I 1");

            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
            Debug.Log("MainMenu 상태로 복귀됨.");
        }
    }
    private void OnCooking(InputAction.CallbackContext context)
    {
        if(GameStateMachine.Instance.CurrentState != GameSystemState.Cook)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Cook);
            Debug.Log("Cooking 상태로 전환됨.");
        }
        else
        {
            Debug.Log("I 1");

            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
            Debug.Log("MainMenu 상태로 복귀됨.");
        }
    }

    private void OnSkills(InputAction.CallbackContext context)
    {
        if (GameStateMachine.Instance.CurrentState != GameSystemState.Skill)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Skill);
            Debug.Log("Skill 상태로 전환됨.");
        }
        else
        {
            Debug.Log("I 1");

            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
            Debug.Log("MainMenu 상태로 복귀됨.");
        }
    }
    #endregion

    private void OnQuestReview(InputAction.CallbackContext context)
    {
        // 상태를 Inventory로 전환
        if (GameStateMachine.Instance.CurrentState != GameSystemState.QuestReview)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.QuestReview);
            Debug.Log("Quest 상태로 전환됨.");
        }
        else
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
            Debug.Log("MainMenu 상태로 복귀됨.");
        }
    }
    private void OnStatKey(InputAction.CallbackContext context)
    {
        // 상태를 Inventory로 전환
        if (GameStateMachine.Instance.CurrentState != GameSystemState.StatusUI)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.StatusUI);
            Debug.Log("StatusUI 상태로 전환됨.");
        }
        else
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
            Debug.Log("MainMenu 상태로 복귀됨.");
        }
    }

    private void OnMainMenu(InputAction.CallbackContext context)
    {
        if (GameStateMachine.Instance.CurrentState != GameSystemState.MainMenu)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
            Debug.Log("ESC로 MainMenu상태로 전환.");
        }
    }

    #endregion

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

    }
}

/* #region 테스트용 추가 메서드 불필요시 삭제

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            Debug.Log("I키 입력 확인");
            ToggleInventoryState();
        }
        if (Input.GetKeyDown(KeyCode.Q))
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

        if (currentState != GameSystemState.QuestReview)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.QuestReview, null);
        }
        else if (currentState == GameSystemState.QuestReview)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu, null);
        }
    }
    private void ToggleInventoryState3()
    {
        var currentState = GameStateMachine.Instance.CurrentState;

        if (currentState != GameSystemState.Stat)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Stat, null);
        }
        else if (currentState == GameSystemState.Stat)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu, null);
        }
    }
    #endregion*/
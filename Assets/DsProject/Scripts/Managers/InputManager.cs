using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : BaseManager<InputManager>
{ 
    public static PlayerInput InputActions;

    // 모든 입력의 활성 상태를 관리하는 딕셔너리
    private Dictionary<string, bool> inputStates = new Dictionary<string, bool>();

    // UI 관련 액션 이름 저장
    private readonly string[] uiActions = new string[] { "Inventory", "Quest", "StatusUI", "ESC", "Enhance", "Cook", "Skill" };
    
    // 플레이어 이동 및 전투 관련 액션 이름
    private readonly string[] gameplayActions = new string[] { 
        "Move", "Jump", "Attack", "Block", "Sprint", "Dodge", 
        "PlayerSkill_1", "PlayerSkill_2", "PlayerSkill_3","PlayerSkill_4", "Interact"
    };

    protected override void Awake()
    {
        base.Awake();
        InputActions = GetComponent<PlayerInput>();

        InputInitialize();
        RegisterUIActionCallbacks();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            var itemsToAdd = new[] {"양손검_물","양손검_불", "야채스프" };
            foreach (var item in itemsToAdd) ItemManager.Instance.AddItemLogic(item);

            ItemManager.Instance.AddItemLogic("버섯",10);
            ItemManager.Instance.AddItemLogic("비트", 10);
            ItemManager.Instance.AddItemLogic("무", 10);
            ItemManager.Instance.AddItemLogic("강화석", 10);
            ItemManager.Instance.AddItemLogic("체력물약(소)", 5);
            ItemManager.Instance.AddItemLogic("마나물약(소)", 5);
        }
    }

    private void InputInitialize()
    {
        foreach(var action in InputActions.actions)
        {
            inputStates[action.name] = true;
        }
    }

    private void RegisterUIActionCallbacks()
    {
        // UI 키 연결 - 딕셔너리와 델리게이트를 활용하여 중복 코드 제거
        var actionStateMap = new Dictionary<string, GameSystemState>
        {
            { "Inventory", GameSystemState.Inventory },
            { "Quest", GameSystemState.QuestReview },
            { "StatusUI", GameSystemState.StatusUI },
            { "Cook", GameSystemState.Cook },
            { "Skill", GameSystemState.Skill },
            {"Enhance", GameSystemState.Enhance}
        };

        foreach (var actionPair in actionStateMap)
        {
            string actionName = actionPair.Key;
            GameSystemState targetState = actionPair.Value;
            
            InputActions.actions[actionName].performed += context => ToggleUIState(targetState);
        }

        // ESC 키는 특별하게 처리
        InputActions.actions["ESC"].performed += OnMainMenu;
    }

    // UI 상태 전환을 처리하는 공통 메서드
    private void ToggleUIState(GameSystemState targetState)
    {
        if (GameStateMachine.Instance.CurrentState != targetState)
        {
            GameStateMachine.Instance.ChangeState(targetState);
            Debug.Log($"{targetState} 상태로 전환됨.");
        }
        else
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
            Debug.Log("MainMenu 상태로 복귀됨.");
        }
    }

    private void OnMainMenu(InputAction.CallbackContext context)
    {
        GameSystemState currentState = GameStateMachine.Instance.CurrentState;
        
        // UI 관련 상태인 경우 Play 상태로 복귀
        if (IsUIRelatedState(currentState))
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Play);
            Debug.Log("ESC로 Play 상태로 복귀.");
        }
        // 그 외의 상태에서는 MainMenu로 전환
        else if (currentState != GameSystemState.MainMenu)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
            Debug.Log("ESC로 MainMenu 상태로 전환.");
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
                // Debug.Log($"InputAction {actionName} Disabled");
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

    // 게임플레이 관련 입력 활성화/비활성화
    public void SetGameplayInputsEnabled(bool enabled)
    {
        SetMultipleInputsEnabled(enabled, gameplayActions);
    }

    // UI 관련 입력 활성화/비활성화
    public void SetUIInputsEnabled(bool enabled)
    {
        SetMultipleInputsEnabled(enabled, uiActions);
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        // UI 관련 상태인지 확인
        bool isUIState = IsUIRelatedState(newState);
        
        // 플레이어 이동 및 액션 관련 입력 활성화/비활성화
        SetGameplayInputsEnabled(!isUIState);
        
        // UI 상태에 따라 인터랙트 입력 특별 처리
        if (newState == GameSystemState.Play)
        {
            SetInputEnabled(true, "Interact");
        }
    }
    
    // UI 관련 상태인지 확인하는 메서드
    public bool IsUIRelatedState(GameSystemState state)
    {
        return state == GameSystemState.Inventory 
            || state == GameSystemState.StatusUI
            || state == GameSystemState.QuestReview
            || state == GameSystemState.Shopping
            || state == GameSystemState.Craft
            || state == GameSystemState.Cook
            || state == GameSystemState.Skill
              || state == GameSystemState.Enhance
            || state == GameSystemState.PetInteraction
            || state == GameSystemState.Event;
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
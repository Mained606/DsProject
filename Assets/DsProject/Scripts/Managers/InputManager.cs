using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class InputManager : BaseManager<InputManager>
{ 
    public static PlayerInput InputActions;

    // 모든 입력의 활성 상태를 관리하는 딕셔너리
    private Dictionary<string, bool> inputStates = new Dictionary<string, bool>();

    // UI 관련 액션 이름 저장
    private readonly string[] uiActions = new string[] { "Inventory", "Quest", "StatusUI", "ESC", "Enhance", "Cook", "Skill", "OptionUI" };
    
    // 플레이어 이동 및 전투 관련 액션 이름
    private readonly string[] gameplayActions = new string[] { 
        "Move", "Jump", "Attack", "Block", "Sprint", "Dodge", 
        "PlayerSkill_1", "PlayerSkill_2", "PlayerSkill_3","PlayerSkill_4", "Interact"
    };

    // UI 활성화 상태를 추적하는 딕셔너리
    private Dictionary<string, bool> uiActiveStates = new Dictionary<string, bool>();

    // UI GameObject 참조를 저장하는 딕셔너리
    private Dictionary<string, GameObject> uiObjects = new Dictionary<string, GameObject>();

    // UI 액션과 GameSystemState 매핑
    private Dictionary<string, GameSystemState> actionStateMap = new Dictionary<string, GameSystemState>
    {
        { "Inventory", GameSystemState.Inventory },
        { "Quest", GameSystemState.QuestReview },
        { "StatusUI", GameSystemState.StatusUI },
        { "Cook", GameSystemState.Cook },
        { "Skill", GameSystemState.Skill },
        { "Enhance", GameSystemState.Enhance },
        { "OptionUI", GameSystemState.Option }

    };

    protected override void Awake()
    {
        base.Awake();
        InputActions = GetComponent<PlayerInput>();

        InputInitialize();
        RegisterUIActionCallbacks();
        InitializeUIStates();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F8))
        {
            var itemsToAdd = new[] {"수련용검_일반","수련용검_물","한손검_불", "완드", "야채스프" };
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

    private void InitializeUIStates()
    {
        // UI 상태 초기화
        foreach (var actionName in actionStateMap.Keys)
        {
            uiActiveStates[actionName] = false;
        }

        // 씬 로드 시 초기화를 위해 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드 시 모든 UI 상태 초기화
        ResetUIStates();
    }

    private void ResetUIStates()
    {
        // 모든 UI 상태 초기화
        foreach (var actionName in actionStateMap.Keys)
        {
            uiActiveStates[actionName] = false;
        }
        
        // 게임 상태를 Play로 설정
        if (GameStateMachine.Instance != null)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
        }
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void RegisterUIActionCallbacks()
    {
        // UI 키 연결 - 토글 방식으로 변경
        foreach (var actionPair in actionStateMap)
        {
            string actionName = actionPair.Key;
            // 로컬 변수에 액션 이름 복사 (클로저 문제 방지)
            string currentAction = actionName;
            
            InputActions.actions[actionName].performed += context => ToggleUI(currentAction);
        }

        // ESC 키는 특별하게 처리
        InputActions.actions["ESC"].performed += OnMainMenu;
    }

    // 새로운 UI 토글 방식 구현
    private void ToggleUI(string actionName)
    {
        GameSystemState targetState = actionStateMap[actionName];
        bool isCurrentlyActive = uiActiveStates[actionName];

        if (!isCurrentlyActive)
        {
            // UI를 활성화
            uiActiveStates[actionName] = true;
            GameStateMachine.Instance.ChangeState(targetState);
            //Debug.Log($"{actionName} UI 활성화됨.");
        }
        else
        {
            // UI를 비활성화하고 Play 상태로 복귀
            uiActiveStates[actionName] = false;
            GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
            //Debug.Log($"{actionName} UI 비활성화됨.");
        }
    }

    private void OnMainMenu(InputAction.CallbackContext context)
    {
        GameSystemState currentState = GameStateMachine.Instance.CurrentState;
        
        // UI 관련 상태인 경우 Play 상태로 복귀
        if (IsUIRelatedState(currentState))
        {
            // 현재 활성화된 UI 상태 찾아서 비활성화
            foreach (var pair in actionStateMap)
            {
                if (pair.Value == currentState)
                {
                    uiActiveStates[pair.Key] = false;
                    break;
                }
            }
            
            GameStateMachine.Instance.ChangeState(GameSystemState.Exploration);
            //Debug.Log("ESC로 Play 상태로 복귀.");
        }
        // 그 외의 상태에서는 MainMenu로 전환
        else if (currentState != GameSystemState.MainMenu)
        {
            GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
            //Debug.Log("ESC로 MainMenu 상태로 전환.");
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
                // //Debug.Log($"InputAction {actionName} Disabled");
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
            || state == GameSystemState.Option
        //    || state == GameSystemState.Event
            || state == GameSystemState.GameOver;
    }
}

/* #region 테스트용 추가 메서드 불필요시 삭제

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            //Debug.Log("I키 입력 확인");
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
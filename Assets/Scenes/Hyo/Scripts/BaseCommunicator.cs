using UnityEngine;
using System.Linq;

public abstract class BaseCommunicator : MonoBehaviour
{
    [SerializeField] private string jsonFilePath = "Assets/Resources/GameStateData.json"; // JSON 파일 경로
    [SerializeField] private GameStateData gameStateData; 

    protected GameSystemState currentState;
    
    protected virtual void Start()
    {
        // JSON 데이터 로드
        gameStateData = GameStateDataLoader.LoadGameStateData(jsonFilePath);
        if (gameStateData == null)
        {
            Debug.LogError("GameStateData 로드에 실패했습니다.");
        }
    }

    protected virtual void OnEnable()
    {
        // 상태 변경 이벤트 구독
        GameStateMachine.GameStateChanged += OnGameStateChanged;
    }

    protected virtual void OnDisable()
    {
        // 상태 변경 이벤트 해제
        GameStateMachine.GameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameSystemState newState)
    {
        currentState = newState;    // 현재 상태 업데이트
        ExecuteSystemLogic();
    }

    private void ExecuteSystemLogic()
    {
        if (gameStateData == null) return;

        // 현재 상태에 해당하는 데이터 찾기
        var state = gameStateData.States.FirstOrDefault(s => s.StateName == currentState.ToString());
        if (state == null)
        {
            Debug.LogWarning($"현재 상태 {currentState}에 해당하는 데이터가 없습니다.");
            return;
        }

        // 각 로직 실행
        foreach (var systemLogic in state.SystemLogics)
        {
            if (systemLogic.SystemName == this.GetType().Name)
            {
                // 메서드 실행
                Invoke(systemLogic.Logic, 0f);
            }
        }
    }
}
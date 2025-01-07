using System;
using UnityEngine;

public enum GameSystemState
{
    Exploration,    // 모험 상태
    Pause,          // 일시정지 상태
    Menu            // 메뉴 상태
                    // 이후 추가...
}

public class GameStateMachine : MonoBehaviour
{
    public static GameStateMachine Instance { get; private set; }
    public static event Action<GameSystemState> GameStateChanged;

    [SerializeField] private GameSystemState currentGameState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void ChangeGameState(GameSystemState newState)
    {
        if (currentGameState == newState) return;

        currentGameState = newState;
        Debug.Log($"게임 상태 변경: {newState}");
        GameStateChanged?.Invoke(newState);
    }
}
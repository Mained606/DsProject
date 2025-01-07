using System;
using UnityEngine;

public enum GameState
{
    Exploration,
    Pause,
    Menu,
    Combat,

}

public class TestGameStateMachine : MonoBehaviour
{
    public static TestGameStateMachine Instance { get; private set; }
    public static event Action<GameState> OnGameStateChanged;

    private GameState currentState;

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

    public void ChangeState(GameState newState)
    {
        if (currentState == newState) return;

        currentState = newState;
        Debug.Log($"게임 상태 변경: {newState}");

        // 매니저들이 이벤트를 통해 새로운 상태에 반응
        OnGameStateChanged?.Invoke(newState);
    }

    public GameState GetCurrentState()
    {
        return currentState;
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public class TestUIManager : MonoBehaviour
{
    private Dictionary<GameState, Action> stateActions;

    private void Start()
    {
        stateActions = new Dictionary<GameState, Action>
        {
            { GameState.Menu, ShowMenuUI },
            { GameState.Pause, ShowPauseUI },
            { GameState.Exploration, HideAllUI }
        };
    }
    private void OnEnable()
    {
        TestGameStateMachine.OnGameStateChanged += HandleGameStateChanged;
    }

    private void OnDisable()
    {
        TestGameStateMachine.OnGameStateChanged -= HandleGameStateChanged;
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if(stateActions.TryGetValue(newState, out var action))
        {
            action?.Invoke();
        }
        else
        {
            Debug.Log($"UIManager의 상태 {newState}에 대한 처리가 존재하지 않음");
        }
    }

    private void ShowMenuUI()
    {
        Debug.Log($"ShowMenuUI {TestGameManager.Instance.CharacterManager.playerStats.characterHP}");
    }

    private void ShowPauseUI()
    {
        Debug.Log("ShowPauseUI");
    }

    private void HideAllUI()
    {
        Debug.Log("HideAllUI");
    }
}

using System;
using System.Collections.Generic;
using UnityEngine;

public abstract class TestBaseManager : MonoBehaviour
{
    protected Dictionary<GameState, Action> stateActions = new Dictionary<GameState, Action> { };

    protected void OnEnable()
    {
        TestGameStateMachine.OnGameStateChanged += HandleGameStateChanged;
    }

    protected void OnDisable()
    {
        TestGameStateMachine.OnGameStateChanged -= HandleGameStateChanged;
    }

    protected virtual void HandleGameStateChanged(GameState newState)
    {
        if (stateActions.TryGetValue(newState, out var action))
        {
            action?.Invoke();
        }
        else
        {
            Debug.Log($"{this}에 상태 {newState}에 대한 처리가 존재하지 않음");
        }
    }
}

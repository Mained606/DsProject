using System;
using System.Collections.Generic;
using UnityEngine;

public class TestUIManager : TestBaseManager
{
    //private Dictionary<GameState, Action> stateActions;

    private void Start()
    {
        stateActions = new Dictionary<GameState, Action>
        {
            { GameState.Menu, ShowMenuUI },
            { GameState.Pause, ShowPauseUI },
            { GameState.Exploration, HideAllUI }
        };

        //TestGameManager.Instance.RegisterManager(this);
    }

    private void ShowMenuUI()
    {
        Debug.Log($"ShowMenuUI {TestGameManager.Instance.GetManager<TestCharacterManager>("CharacterManager").CharacterStats.characterHP}");
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

using System;
using System.Collections.Generic;
using UnityEngine;

public class TestCharacterManager : TestBaseManager
{
    [SerializeField]
    private TestCharacterStats characterStats;

    public TestCharacterStats CharacterStats => characterStats;

    private void Start()
    {
        stateActions = new Dictionary<GameState, Action>
        {
            { GameState.Menu, CharacterPause },
            { GameState.Pause, CharacterPause },
            { GameState.Exploration, CharacterPlay },
            { GameState.Combat, CharacterOnCambatMode }
        };

        //TestGameManager.Instance.RegisterManager(this);
    }

    public void CharacterPause()
    {
        Debug.Log("CharacterPause");
    }

    public void CharacterPlay()
    {
        Debug.Log("CharacterPlay");
    }

    public void CharacterOnCambatMode()
    {
        Debug.Log("CharacterOnCambatMode");
    }
}

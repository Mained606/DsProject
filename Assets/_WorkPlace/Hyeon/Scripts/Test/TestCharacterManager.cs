using System;
using System.Collections.Generic;
using UnityEngine;

public class TestCharacterManager : BaseManager<TestCharacterManager>
{
    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

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

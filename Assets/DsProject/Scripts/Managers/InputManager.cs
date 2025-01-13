using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : BaseManager<InputManager>
{
    public static PlayerInput InputActions;
    protected override void Awake()
    {
        base.Awake();
        InputActions = GetComponent<PlayerInput>();
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

    }
}


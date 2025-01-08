using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TestInputManager : TestBaseManager
{
    public event Action<Vector2> OnMoveInput;
    public event Action OnJumpInput;

    private InputSystem_Actions inputActions;

    private void Awake()
    {
        inputActions = new InputSystem_Actions();

        inputActions.Player.Move.performed += ctx => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        inputActions.Player.Move.canceled += ctx => OnMoveInput?.Invoke(Vector2.zero);

        inputActions.Player.Jump.performed += ctx => OnJumpInput?.Invoke();
    }

    private void OnEnable() => inputActions.Player.Enable();
    private void OnDisable() => inputActions.Player.Disable();

    //private void Update()
    //{
    //    Vector2 moveInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    //    if(moveInput != Vector2.zero)
    //    {
    //        OnMoveInput?.Invoke(moveInput);
    //    }

    //    if (Input.GetButtonDown("Jump"))
    //    {
    //        OnJumpInput?.Invoke();
    //    }
    //}
}

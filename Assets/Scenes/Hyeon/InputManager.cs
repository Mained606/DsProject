using System;
using UnityEngine;

public class InputManager
{
    public Action keyaction = null;

    public void OnUpdate()
    {
        if (Input.anyKey == false) return;

        if(keyaction != null)
        {
            keyaction.Invoke();
        }
    }

}

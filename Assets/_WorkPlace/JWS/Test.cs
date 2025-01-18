using UnityEngine;

public class Test : MonoBehaviour
{
    void Update()
    {
        if (InputManager.InputActions.actions["Interact"].triggered)
            Debug.Log("인터액트 ");
    }
}

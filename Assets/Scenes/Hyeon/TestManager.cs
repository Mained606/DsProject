using UnityEngine;

public class TestManager : MonoBehaviour
{
    public static TestManager Instance;
    InputManager inputManager = new InputManager();
    public static InputManager Input { get { return Instance.inputManager; } }

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    private void Update()
    {
        inputManager.OnUpdate();
    }
}
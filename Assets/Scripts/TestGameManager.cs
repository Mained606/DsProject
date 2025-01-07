using UnityEngine;

public class TestGameManager : MonoBehaviour
{
    public static TestGameManager Instance { get; private set; }

    public TestCharacterManager CharacterManager { get; private set; }
    public TestUIManager UIManager { get; private set; }
    public TestInputManager InputManager { get; private set; }

    private void Awake()
    {
        if (Instance == null)
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
        if (Input.GetKeyDown(KeyCode.P))
        {
            TestGameStateMachine.Instance.ChangeState(GameState.Pause);
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            TestGameStateMachine.Instance.ChangeState(GameState.Menu);
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            TestGameStateMachine.Instance.ChangeState(GameState.Exploration);
        }
    }

    public void RegisterManager<T>(T manager) where T : MonoBehaviour
    {
        switch (manager)
        {
            case TestCharacterManager cm:
                CharacterManager = cm;
                break;
            case TestUIManager um:
                UIManager = um;
                break;
            case TestInputManager im:
                InputManager = im;
                break;
            default:
                Debug.LogWarning($"Unknown manager type: {typeof(T)}");
                break;
        }
    }
}

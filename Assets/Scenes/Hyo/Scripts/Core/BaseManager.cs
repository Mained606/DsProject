using UnityEngine;

public abstract class BaseManager<T> : MonoBehaviour where T : MonoBehaviour
{
    // 싱글톤
    public static T Instance { get; private set; }
    
    protected virtual void OnEnable()
    {
        GameStateMachine.OnGameStateChanged += HandleGameStateChange;
    }
    
    protected virtual void OnDisable()
    {
        GameStateMachine.OnGameStateChanged -= HandleGameStateChange;
    }

    protected virtual void Awake()
    {
        if (Instance == null)
        {
            Instance = this as T;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    protected abstract void HandleGameStateChange(GameSystemState state);
}
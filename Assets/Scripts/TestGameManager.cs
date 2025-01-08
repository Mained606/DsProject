using System;
using System.Collections.Generic;
using UnityEngine;

public class TestGameManager : MonoBehaviour
{
    public static TestGameManager Instance { get; private set; }

    private Dictionary<string, TestBaseManager> managers = new Dictionary<string, TestBaseManager>();

    //public TestCharacterManager CharacterManager { get; private set; }
    //public TestUIManager UIManager { get; private set; }
    //public TestInputManager InputManager { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            //DontDestroyOnLoad(this.gameObject);
            InitializeManagers();
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
        else if (Input.GetKeyDown(KeyCode.C))
        {
            TestGameStateMachine.Instance.ChangeState(GameState.Combat);
        }
    }

    private void InitializeManagers()
    {
        RegisterManager<TestInputManager>("InputManager");
        RegisterManager<TestUIManager>("UIManager");
        RegisterManager<TestCharacterManager>("CharacterManager");
    }

    // 매니저 등록 메서드
    private void RegisterManager<T>(string managerName) where T : TestBaseManager
    {
        GameObject managerObject = new GameObject(managerName);
        T manager = managerObject.AddComponent<T>();
        managers.Add(managerName, manager);
    }

    public T GetManager<T>(string managerName) where T : TestBaseManager
    {
        if(managers.TryGetValue(managerName, out var manager))
        {
            return manager as T;
        }
        return null;
    }



    //public void RegisterManager<T>(T manager) where T : MonoBehaviour
    //{
    //    switch (manager)
    //    {
    //        case TestCharacterManager cm:
    //            CharacterManager = cm;
    //            break;
    //        case TestUIManager um:
    //            UIManager = um;
    //            break;
    //        case TestInputManager im:
    //            InputManager = im;
    //            break;
    //        default:
    //            Debug.LogWarning($"Unknown manager type: {typeof(T)}");
    //            break;
    //    }
    //}
}

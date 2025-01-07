using UnityEngine;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private Dictionary<string, MonoBehaviour> managers = new Dictionary<string, MonoBehaviour>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeManagers()
    {
        RegisterManager<InputManager>("InputManager");
        // 이후 매니저 추가...
    }
    
    // 매니저 등록 메서드
    private void RegisterManager<T>(string managerName) where T : MonoBehaviour
    {
        if (managers.ContainsKey(managerName))
        {
            Debug.LogWarning($"매니저 이름: {managerName} 가 레지스터에 이미 등록되어 있습니다.");
            return;
        }

        // 새 매니저 생성
        GameObject managerObject = new GameObject(managerName);
        managerObject.transform.SetParent(this.transform); // GameManager의 자식으로 설정
        DontDestroyOnLoad(managerObject); // 매니저가 씬 전환 시 유지되도록 설정
        T manager = managerObject.AddComponent<T>();
        managers.Add(managerName, manager);
    }
    
    // 매니저 가져오기
    public T GetManager<T>(string managerName) where T : MonoBehaviour
    {
        if (managers.TryGetValue(managerName, out var manager))
        {
            return manager as T;
        }

        Debug.LogError($"가져올 매니저 이름: {managerName}을(를) 찾을 수 없습니다.");
        return null;
    }
}
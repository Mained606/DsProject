using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystemInitializer : MonoBehaviour
{
    private static SaveSystemInitializer instance;
    
    [Header("세이브 시스템 설정")]
    [Tooltip("타이틀 씬 이름")]
    public string titleSceneName = "TitleScene";
    
    [Tooltip("메인 게임 씬 이름")]
    public string mainGameSceneName = "Ds_ProjectTest";
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 씬 로드 이벤트 등록
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // 씬이 로드되었을 때 호출
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 현재 씬이 타이틀 씬인지 확인
        bool isTitleScene = scene.name == titleSceneName;
        
        // 타이틀 씬에서는 SaveManager만 활성화
        if (isTitleScene)
        {
            // 이미 SaveManager가 없다면 생성
            if (SaveManager.Instance == null)
            {
                GameObject saveManagerObj = new GameObject("SaveManager");
                saveManagerObj.AddComponent<SaveManager>();
                DontDestroyOnLoad(saveManagerObj);
            }
        }
        // 게임 씬에서는 필요에 따라 저장 데이터 로드
        else
        {
            LoadGameIfNeeded();
        }
    }
    
    // 필요한 경우 게임 데이터 로드
    private void LoadGameIfNeeded()
    {
        StartCoroutine(LoadGameAfterDelay());
    }
    
    private IEnumerator LoadGameAfterDelay()
    {
        // 플레이어 및 게임 매니저가 초기화될 시간을 줌
        yield return new WaitForSeconds(0.5f);
        
        // SaveManager가 초기화되어 있는지 확인
        if (SaveManager.Instance != null)
        {
            // 저장 데이터가 있다면 로드
            if (SaveManager.Instance.HasSaveData())
            {
                SaveManager.Instance.LoadGame();
            }
        }
    }
    
    // 게임 시작 (새 게임)
    public void StartNewGame()
    {
        // 저장 데이터 초기화
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetSaveData();
        }
        
        // 메인 게임 씬 로드
        SceneManager.LoadScene(mainGameSceneName);
    }
    
    // 게임 로드 (저장된 게임 불러오기)
    public void LoadSavedGame()
    {
        // 저장 데이터가 있는지 확인
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            // 메인 게임 씬 로드
            SceneManager.LoadScene(mainGameSceneName);
        }
    }
} 
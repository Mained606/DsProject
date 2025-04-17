using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class TitleSceneManager : MonoBehaviour
{
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button loadGameButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;
    
    [SerializeField] private GameObject loadGameUI;
    [SerializeField] private SaveLoadUI saveLoadUI;
    
    [SerializeField] private string newGameSceneName = "MainSceneTest";
    
    private void Awake()
    {
        // 버튼 이벤트 설정
        if (newGameButton != null) newGameButton.onClick.AddListener(StartNewGame);
        if (loadGameButton != null) loadGameButton.onClick.AddListener(ShowLoadGameUI);
        if (optionsButton != null) optionsButton.onClick.AddListener(ShowOptions);
        if (quitButton != null) quitButton.onClick.AddListener(QuitGame);
    }
    
    private void Start()
    {
        // SaveManager 초기화 확인
        if (SaveManager.Instance == null)
        {
            Debug.LogError("SaveManager가 초기화되지 않았습니다. SaveManager 오브젝트가 씬에 존재하는지 확인하세요.");
        }
    }
    
    private void StartNewGame()
    {
        // 새 게임 시작
        SceneManager.LoadScene(newGameSceneName);
    }
    
    private void ShowLoadGameUI()
    {
        // 로드 게임 UI 표시
        if (loadGameUI != null && saveLoadUI != null)
        {
            loadGameUI.SetActive(true);
            saveLoadUI.ShowUI();
        }
    }
    
    private void ShowOptions()
    {
        // 옵션 메뉴 표시
        // 여기에 옵션 UI 활성화 로직 구현
    }
    
    private void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
} 
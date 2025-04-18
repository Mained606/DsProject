using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 타이틀 씬에서 SaveManager를 초기화하고 타이틀 UI와 연결하는 역할을 담당합니다.
/// 이 스크립트는 타이틀 씬의 빈 게임 오브젝트에 추가해야 합니다.
/// </summary>
public class TitleSceneInitializer : MonoBehaviour
{
    [Header("세이브 시스템 설정")]
    [Tooltip("세이브 매니저 프리팹 (없을 경우 런타임에 생성)")]
    public GameObject saveManagerPrefab;
    
    [Tooltip("타이틀 씬 UI 컴포넌트")]
    public TitleScene titleSceneUI;
    
    private void Awake()
    {
        // SaveManager가 없으면 생성
        if (SaveManager.Instance == null)
        {
            if (saveManagerPrefab != null)
            {
                Instantiate(saveManagerPrefab);
            }
            else
            {
                GameObject saveManagerObj = new GameObject("SaveManager");
                saveManagerObj.AddComponent<SaveManager>();
                DontDestroyOnLoad(saveManagerObj);
            }
        }
        
        // SaveSystemInitializer가 없으면 생성
        if (FindObjectOfType<SaveSystemInitializer>() == null)
        {
            GameObject saveSystemObj = new GameObject("SaveSystemInitializer");
            SaveSystemInitializer saveSystem = saveSystemObj.AddComponent<SaveSystemInitializer>();
            
            // 씬 이름 설정
            saveSystem.titleSceneName = SceneManager.GetActiveScene().name;
            saveSystem.mainGameSceneName = "MainSceneTest"; // 메인 게임 씬 이름 설정
            
            DontDestroyOnLoad(saveSystemObj);
        }
    }
    
    private void Start()
    {
        // 타이틀 씬 UI를 찾고 초기화
        if (titleSceneUI == null)
        {
            titleSceneUI = FindObjectOfType<TitleScene>();
        }
    }
    
    /// <summary>
    /// 세이브 데이터 유무 상태를 확인하고 UI 버튼 상태를 업데이트합니다.
    /// </summary>
    /// <returns>저장된 게임이 있으면 true, 없으면 false</returns>
    public bool CheckSaveDataExists()
    {
        return SaveManager.Instance != null && SaveManager.Instance.HasSaveData();
    }
    
    /// <summary>
    /// 마지막으로 저장된 날짜/시간 정보를 가져옵니다.
    /// </summary>
    /// <returns>저장 날짜 문자열, 데이터가 없으면 빈 문자열</returns>
    public string GetLastSaveDate()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            SaveData saveData = SaveManager.Instance.GetCurrentSaveData();
            return saveData != null ? saveData.saveDate : "";
        }
        return "";
    }
    
    /// <summary>
    /// 저장 데이터 삭제 (모든 진행 초기화)
    /// </summary>
    public void DeleteSaveData()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetSaveData();
            
            // UI 업데이트
            if (titleSceneUI != null)
            {
                // TitleScene 클래스에 UpdateContinueButtonState 메서드가 있다고 가정
                titleSceneUI.UpdateContinueButtonState();
            }
        }
    }
} 
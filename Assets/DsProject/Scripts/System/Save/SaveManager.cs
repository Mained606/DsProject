using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    // 현재 저장 데이터
    private SaveData currentSaveData;
    
    // 저장 파일 이름
    private const string SAVE_FILE_NAME = "savegame.json";
    
    // 플레이어가 세이브 포인트 범위 내에 있는지 여부
    private bool isPlayerInSavePoint = false;
    private SavePoint currentSavePoint;
    
    // 저장/로드 이벤트
    public static event Action<SaveData> OnGameSaved;
    public static event Action<SaveData> OnGameLoaded;
    
    // 싱글톤 인스턴스 초기화
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 초기 데이터 로드 시도
            LoadGame();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 입력 처리
    private void Update()
    {
        // 플레이어가 세이브 포인트 내에 있고 F키가 눌렸을 때
        if (isPlayerInSavePoint && Input.GetKeyDown(KeyCode.F) && currentSavePoint != null)
        {
            SaveGame(currentSavePoint.savePointId);
            Debug.Log("게임 세이브 완료!");
        }
    }
    
    // 저장 지점 진입 감지
    public void OnPlayerEnterSavePoint(SavePoint savePoint)
    {
        isPlayerInSavePoint = true;
        currentSavePoint = savePoint;
    }
    
    // 저장 지점 이탈 감지
    public void OnPlayerExitSavePoint()
    {
        isPlayerInSavePoint = false;
        currentSavePoint = null;
    }
    
    // 게임 저장
    public void SaveGame(string savePointId = "")
    {
        try
        {
            // 새 SaveData 생성
            SaveData saveData = new SaveData();
            saveData.saveDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            saveData.savePointId = savePointId;
            
            // 플레이어 데이터 저장
            SavePlayerData(saveData);
            
            // 인벤토리 데이터 저장
            SaveInventoryData(saveData);
            
            // 스킬 데이터 저장
            SaveSkillData(saveData);
            
            // 퀘스트 데이터 저장
            SaveQuestData(saveData);
            
            // 용 데이터 저장
            SaveDragonData(saveData);
            
            // 데이터를 JSON으로 변환
            string jsonData = JsonUtility.ToJson(saveData, true);
            
            // 저장 경로 설정
            string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            
            // 파일에 저장
            File.WriteAllText(savePath, jsonData);
            
            // 현재 데이터 갱신
            currentSaveData = saveData;
            
            // 저장 이벤트 발생
            OnGameSaved?.Invoke(saveData);
            
            Debug.Log($"게임이 성공적으로 저장되었습니다: {savePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 저장 중 오류 발생: {e.Message}");
        }
    }
    
    // 게임 로드
    public bool LoadGame()
    {
        try
        {
            // 저장 경로 설정
            string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
            
            // 저장 파일이 존재하는지 확인
            if (!File.Exists(savePath))
            {
                Debug.Log("저장된 게임을 찾을 수 없습니다.");
                return false;
            }
            
            // 파일에서 데이터 읽기
            string jsonData = File.ReadAllText(savePath);
            
            // JSON을 SaveData로 변환
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
            
            // 데이터 검증
            if (saveData == null)
            {
                Debug.LogError("저장 데이터를 로드할 수 없습니다.");
                return false;
            }
            
            // 현재 저장 데이터 설정
            currentSaveData = saveData;
            
            // 플레이어 및 게임 상태 복원
            ApplySaveData(saveData);
            
            // 로드 이벤트 발생
            OnGameLoaded?.Invoke(saveData);
            
            Debug.Log($"게임이 성공적으로 로드되었습니다. (저장일시: {saveData.saveDate})");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 로드 중 오류 발생: {e.Message}");
            return false;
        }
    }
    
    // 저장 데이터 적용
    private void ApplySaveData(SaveData saveData)
    {
        // GameManager가 초기화되어 있는지 확인
        if (GameManager.playerTransform == null)
        {
            Debug.LogWarning("플레이어 오브젝트가 초기화되지 않았습니다. 일부 데이터가 적용되지 않을 수 있습니다.");
            return;
        }
        
        // 플레이어 데이터 적용
        ApplyPlayerData(saveData);
        
        // 인벤토리 데이터 적용
        ApplyInventoryData(saveData);
        
        // 스킬 데이터 적용
        ApplySkillData(saveData);
        
        // 퀘스트 데이터 적용
        ApplyQuestData(saveData);
        
        // 용 데이터 적용
        ApplyDragonData(saveData);
    }
    
    // 플레이어 데이터 저장
    private void SavePlayerData(SaveData saveData)
    {
        if (GameManager.playerTransform == null) return;
        
        // 플레이어 컨트롤러 가져오기
        PlayerController playerController = GameManager.playerTransform.GetComponent<PlayerController>();
        if (playerController == null) return;
        
        // 위치 및 회전 정보 저장
        saveData.playerData.position = GameManager.playerTransform.position;
        saveData.playerData.rotation = GameManager.playerTransform.rotation;
        
        // 플레이어 체력/마나/스테미나 저장
        // 실제 PlayerController의 구조에 맞게 아래 코드를 수정해야 합니다.
        // 예시: playerController.GetComponent<PlayerStats>()에서 값을 가져오는 등
        try {
            // 임시로 기본값 설정 (실제 구현 시 주석 해제하고 올바른 경로로 수정 필요)
            saveData.playerData.maxHealth = 100; // playerController.GetComponent<PlayerStats>().maxHealth;
            saveData.playerData.currentHealth = 100; // playerController.GetComponent<PlayerStats>().currentHealth;
            saveData.playerData.maxStamina = 100; // playerController.GetComponent<PlayerStats>().maxStamina;
            saveData.playerData.currentStamina = 100; // playerController.GetComponent<PlayerStats>().currentStamina;
            
            // 레벨 및 경험치 저장
            saveData.playerData.level = 1; // playerController.GetComponent<PlayerStats>().level;
            saveData.playerData.experience = 0; // playerController.GetComponent<PlayerStats>().exp;
        }
        catch (Exception e) {
            Debug.LogWarning($"플레이어 데이터 저장 중 오류: {e.Message}");
        }
    }
    
    // 인벤토리 데이터 저장
    private void SaveInventoryData(SaveData saveData)
    {
        // 인벤토리 시스템 참조 필요
        // 실제 인벤토리 시스템에 맞게 구현 필요
    }
    
    // 스킬 데이터 저장
    private void SaveSkillData(SaveData saveData)
    {
        // 스킬 시스템 참조 필요
        // 실제 스킬 시스템에 맞게 구현 필요
    }
    
    // 퀘스트 데이터 저장
    private void SaveQuestData(SaveData saveData)
    {
        // 퀘스트 시스템 참조 필요
        // 실제 퀘스트 시스템에 맞게 구현 필요
    }
    
    // 용 데이터 저장
    private void SaveDragonData(SaveData saveData)
    {
        if (GameManager.DragonTransform == null) return;
        
        // 용 컨트롤러 가져오기
        DragonController dragonController = GameManager.DragonTransform.GetComponent<DragonController>();
        if (dragonController == null) return;
        
        // 위치 및 회전 정보 저장
        saveData.dragonData.position = GameManager.DragonTransform.position;
        saveData.dragonData.rotation = GameManager.DragonTransform.rotation;
        
        // 용 상태 저장
        // 실제 DragonController의 구조에 맞게 아래 코드를 수정해야 합니다.
        try {
            saveData.dragonData.isUnlocked = true; // 실제 언락 상태에 따라 조정 필요
            saveData.dragonData.level = 1; // dragonController.GetComponent<DragonStats>().level;
            saveData.dragonData.currentHealth = 1000; // dragonController.GetComponent<DragonStats>().currentHealth;
            saveData.dragonData.maxHealth = 1000; // dragonController.GetComponent<DragonStats>().maxHealth;
        }
        catch (Exception e) {
            Debug.LogWarning($"용 데이터 저장 중 오류: {e.Message}");
        }
    }
    
    // 플레이어 데이터 적용
    private void ApplyPlayerData(SaveData saveData)
    {
        if (GameManager.playerTransform == null) return;
        
        // 플레이어 컨트롤러 가져오기
        PlayerController playerController = GameManager.playerTransform.GetComponent<PlayerController>();
        if (playerController == null) return;
        
        // 위치 및 회전 복원
        GameManager.playerTransform.position = saveData.playerData.position;
        GameManager.playerTransform.rotation = saveData.playerData.rotation;
        
        // 플레이어 스탯 복원
        // 실제 PlayerController의 구조에 맞게 아래 코드를 수정해야 합니다.
        try {
            // 임시로 주석 처리 (실제 구현 시 주석 해제하고 올바른 경로로 수정 필요)
            // var playerStats = playerController.GetComponent<PlayerStats>();
            // if (playerStats != null) {
            //     playerStats.maxHealth = saveData.playerData.maxHealth;
            //     playerStats.currentHealth = saveData.playerData.currentHealth;
            //     playerStats.maxStamina = saveData.playerData.maxStamina;
            //     playerStats.currentStamina = saveData.playerData.currentStamina;
            //     playerStats.level = saveData.playerData.level;
            //     playerStats.exp = saveData.playerData.experience;
            // }
        }
        catch (Exception e) {
            Debug.LogWarning($"플레이어 데이터 적용 중 오류: {e.Message}");
        }
    }
    
    // 인벤토리 데이터 적용
    private void ApplyInventoryData(SaveData saveData)
    {
        // 인벤토리 시스템 참조 필요
        // 실제 인벤토리 시스템에 맞게 구현 필요
    }
    
    // 스킬 데이터 적용
    private void ApplySkillData(SaveData saveData)
    {
        // 스킬 시스템 참조 필요
        // 실제 스킬 시스템에 맞게 구현 필요
    }
    
    // 퀘스트 데이터 적용
    private void ApplyQuestData(SaveData saveData)
    {
        // 퀘스트 시스템 참조 필요
        // 실제 퀘스트 시스템에 맞게 구현 필요
    }
    
    // 용 데이터 적용
    private void ApplyDragonData(SaveData saveData)
    {
        if (GameManager.DragonTransform == null) return;
        
        // 용 컨트롤러 가져오기
        DragonController dragonController = GameManager.DragonTransform.GetComponent<DragonController>();
        if (dragonController == null) return;
        
        // 위치 및 회전 복원
        GameManager.DragonTransform.position = saveData.dragonData.position;
        GameManager.DragonTransform.rotation = saveData.dragonData.rotation;
        
        // 용 상태 복원
        // 실제 DragonController의 구조에 맞게 아래 코드를 수정해야 합니다.
        try {
            // 임시로 주석 처리 (실제 구현 시 주석 해제하고 올바른 경로로 수정 필요)
            // var dragonStats = dragonController.GetComponent<DragonStats>();
            // if (dragonStats != null) {
            //     dragonStats.level = saveData.dragonData.level;
            //     dragonStats.currentHealth = saveData.dragonData.currentHealth;
            //     dragonStats.maxHealth = saveData.dragonData.maxHealth;
            // }
        }
        catch (Exception e) {
            Debug.LogWarning($"용 데이터 적용 중 오류: {e.Message}");
        }
    }
    
    // 저장 데이터 초기화
    public void ResetSaveData()
    {
        string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        
        if (File.Exists(savePath))
        {
            File.Delete(savePath);
            Debug.Log("저장 데이터가 초기화되었습니다.");
        }
        
        currentSaveData = null;
    }
    
    // 현재 저장 데이터 반환
    public SaveData GetCurrentSaveData()
    {
        return currentSaveData;
    }
    
    // 저장 데이터 존재 여부 확인
    public bool HasSaveData()
    {
        string savePath = Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);
        return File.Exists(savePath);
    }
} 
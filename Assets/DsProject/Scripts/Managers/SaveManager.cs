using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }
    
    [SerializeField] private int maxSaveSlots = 3; // 최대 세이브 슬롯 수
    
    private SaveData currentSaveData; // 현재 게임의 세이브 데이터
    private string saveFilePathTemplate; // 저장 경로 템플릿
    
    // 저장된 세이브 슬롯 정보를 저장할 딕셔너리
    private Dictionary<int, SaveSlotInfo> saveSlots = new Dictionary<int, SaveSlotInfo>();
    
    // 이벤트 정의
    public event Action<SaveData> OnGameSaved;
    public event Action<SaveData> OnGameLoaded;
    public event Action<int> OnSaveDeleted;
    public event Action OnSaveSlotsChanged;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // 저장 경로 설정
            saveFilePathTemplate = Path.Combine(Application.persistentDataPath, "SaveData_{0}.json");
            
            // 세이브 슬롯 정보 불러오기
            LoadSaveSlotInfo();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    // 세이브 슬롯 정보 불러오기
    private void LoadSaveSlotInfo()
    {
        saveSlots.Clear();
        
        for (int i = 1; i <= maxSaveSlots; i++)
        {
            string filePath = string.Format(saveFilePathTemplate, i);
            
            if (File.Exists(filePath))
            {
                try
                {
                    string jsonData = File.ReadAllText(filePath);
                    SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
                    
                    SaveSlotInfo slotInfo = new SaveSlotInfo
                    {
                        SlotNumber = i,
                        PlayerName = saveData.playerName,
                        SaveDateTime = saveData.saveDateTime,
                        PlayerLevel = saveData.playerData.level,
                        PlayTime = saveData.playTime
                    };
                    
                    saveSlots.Add(i, slotInfo);
                }
                catch (Exception e)
                {
                    Debug.LogError($"세이브 슬롯 {i} 정보 로드 중 오류 발생: {e.Message}");
                }
            }
        }
        
        OnSaveSlotsChanged?.Invoke();
    }
    
    // 게임 데이터 저장하기
    public void SaveGame(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"잘못된 세이브 슬롯 번호: {slotNumber}. 1부터 {maxSaveSlots} 사이의 값이어야 합니다.");
            return;
        }
        
        try
        {
            // 현재 게임 상태 수집
            SaveData saveData = CollectGameData();
            
            // 저장 슬롯에 관련 정보 업데이트
            SaveSlotInfo slotInfo = new SaveSlotInfo
            {
                SlotNumber = slotNumber,
                PlayerName = saveData.playerName,
                SaveDateTime = saveData.saveDateTime,
                PlayerLevel = saveData.playerData.level,
                PlayTime = saveData.playTime
            };
            
            saveSlots[slotNumber] = slotInfo;
            
            // 파일로 저장
            string jsonData = JsonUtility.ToJson(saveData, true);
            string filePath = string.Format(saveFilePathTemplate, slotNumber);
            File.WriteAllText(filePath, jsonData);
            
            Debug.Log($"게임 데이터가 슬롯 {slotNumber}에 성공적으로 저장되었습니다.");
            
            OnGameSaved?.Invoke(saveData);
            OnSaveSlotsChanged?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 데이터 저장 중 오류 발생: {e.Message}");
        }
    }
    
    // 게임 데이터 로드하기
    public void LoadGame(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"잘못된 세이브 슬롯 번호: {slotNumber}. 1부터 {maxSaveSlots} 사이의 값이어야 합니다.");
            return;
        }
        
        try
        {
            string filePath = string.Format(saveFilePathTemplate, slotNumber);
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"세이브 슬롯 {slotNumber}에 저장된 데이터가 없습니다.");
                return;
            }
            
            string jsonData = File.ReadAllText(filePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
            
            // 현재 세이브 데이터 설정
            currentSaveData = saveData;
            
            // 게임 데이터 적용
            ApplyGameData(saveData);
            
            Debug.Log($"슬롯 {slotNumber}에서 게임 데이터를 성공적으로 로드했습니다.");
            
            OnGameLoaded?.Invoke(saveData);
        }
        catch (Exception e)
        {
            Debug.LogError($"게임 데이터 로드 중 오류 발생: {e.Message}");
        }
    }
    
    // 세이브 데이터 삭제하기
    public void DeleteSave(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"잘못된 세이브 슬롯 번호: {slotNumber}. 1부터 {maxSaveSlots} 사이의 값이어야 합니다.");
            return;
        }
        
        try
        {
            string filePath = string.Format(saveFilePathTemplate, slotNumber);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                
                if (saveSlots.ContainsKey(slotNumber))
                {
                    saveSlots.Remove(slotNumber);
                }
                
                Debug.Log($"세이브 슬롯 {slotNumber}의 데이터가 성공적으로 삭제되었습니다.");
                
                OnSaveDeleted?.Invoke(slotNumber);
                OnSaveSlotsChanged?.Invoke();
            }
            else
            {
                Debug.LogWarning($"세이브 슬롯 {slotNumber}에 저장된 데이터가 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"세이브 데이터 삭제 중 오류 발생: {e.Message}");
        }
    }
    
    // 모든 세이브 슬롯 정보 가져오기
    public Dictionary<int, SaveSlotInfo> GetAllSaveSlots()
    {
        return new Dictionary<int, SaveSlotInfo>(saveSlots);
    }
    
    // 특정 슬롯의 세이브 정보 가져오기
    public SaveSlotInfo GetSaveSlotInfo(int slotNumber)
    {
        if (saveSlots.TryGetValue(slotNumber, out SaveSlotInfo slotInfo))
        {
            return slotInfo;
        }
        
        return null;
    }
    
    // 세이브 슬롯이 비어있는지 확인
    public bool IsSaveSlotEmpty(int slotNumber)
    {
        return !saveSlots.ContainsKey(slotNumber);
    }
    
    // 세이브 데이터 미리보기 로드
    public SaveData LoadSaveDataPreview(int slotNumber)
    {
        if (slotNumber < 1 || slotNumber > maxSaveSlots)
        {
            Debug.LogError($"잘못된 세이브 슬롯 번호: {slotNumber}. 1부터 {maxSaveSlots} 사이의 값이어야 합니다.");
            return null;
        }
        
        try
        {
            string filePath = string.Format(saveFilePathTemplate, slotNumber);
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"세이브 슬롯 {slotNumber}에 저장된 데이터가 없습니다.");
                return null;
            }
            
            string jsonData = File.ReadAllText(filePath);
            SaveData saveData = JsonUtility.FromJson<SaveData>(jsonData);
            
            return saveData;
        }
        catch (Exception e)
        {
            Debug.LogError($"세이브 데이터 미리보기 로드 중 오류 발생: {e.Message}");
            return null;
        }
    }
    
    // 현재 게임 상태 수집 메서드
    private SaveData CollectGameData()
    {
        SaveData saveData = new SaveData();
        
        // 플레이어 데이터 수집
        PlayerData playerData = GetPlayerData();
        if (playerData != null)
        {
            saveData.playerName = playerData.characterName;
            
            // PlayerSaveData 생성 및 설정
            PlayerSaveData playerSaveData = new PlayerSaveData
            {
                characterName = playerData.characterName,
                level = playerData.level,
                currentExperience = playerData.currentExperience,
                experienceToLevelUp = playerData.experienceToLevelUp,
                
                // 기본 스탯
                strength = playerData.strength,
                agility = playerData.agility,
                vitality = playerData.vitality,
                intelligence = playerData.intelligence,
                
                // 파생 스탯
                currentHp = playerData.currentHp,
                maxHp = playerData.maxHp,
                currentMp = playerData.currentMp,
                maxMp = playerData.maxMp,
                staminaCurrent = playerData.staminaCurrent,
                stamina = playerData.stamina,
                
                // 전투 관련 스탯
                physicalDamage = playerData.physicalDamage,
                magicDamage = playerData.magicDamage,
                physicalDefense = playerData.physicalDefense,
                magicDefense = playerData.magicDefense,
                criticalChance = playerData.criticalChance,
                criticalDamage = playerData.criticalDamage,
                dodgeChance = playerData.dodgeChance,
                blockChance = playerData.blockChance,
                attribute = playerData.attribute,
                
                // 스킬 및 포인트
                skills = new List<string>(playerData.skills),
                availableSkillPoints = playerData.availableSkillPoints,
                availableStatPoints = playerData.availableStatPoints,
                
                // 골드
                gold = playerData.gold,
                
                // 위치 정보 (플레이어 위치 가져오기)
                position = new Vector3SaveData(GameManager.playerTransform.position),
                rotation = new Vector3SaveData(GameManager.playerTransform.rotation.eulerAngles)
            };
            
            // 퀵슬롯 정보 수집
            // (퀵슬롯 관리자에서 정보 가져와 설정)
            
            // 장착 아이템 정보 수집
            // (인벤토리 매니저나 장비 매니저에서 정보 가져와 설정)
            
            saveData.playerData = playerSaveData;
        }
        
        // 용 데이터 수집
        DragonData dragonData = GetDragonData();
        if (dragonData != null)
        {
            DragonSaveData dragonSaveData = new DragonSaveData
            {
                characterName = dragonData.characterName,
                evolutionStage = dragonData.evolutionStage,
                bondLevel = dragonData.bondLevel,
                bondExperience = dragonData.bondExperience,
                
                // 용 스탯
                strength = dragonData.strength,
                agility = dragonData.agility,
                vitality = dragonData.vitality,
                intelligence = dragonData.intelligence,
                
                criticalChance = dragonData.criticalChance,
                criticalDamage = dragonData.criticalDamage,
                physicalDamage = dragonData.physicalDamage,
                magicDamage = dragonData.magicDamage,
                
                dragonAttribute = dragonData.dragonAttribute
            };
            
            saveData.dragonData = dragonSaveData;
        }
        
        // 퀘스트 데이터 수집
        // (퀘스트 매니저에서 데이터 가져와 설정)
        
        // 인벤토리 데이터 수집
        // (인벤토리 매니저에서 데이터 가져와 설정)
        
        // 게임 진행 상태 수집
        saveData.progressData = new GameProgressData
        {
            currentScene = SceneManager.GetActiveScene().name
            // 기타 게임 진행 상태 정보 설정
        };
        
        // 플레이 시간 설정
        // 여기에서는 임시로 0을 설정
        saveData.playTime = 0;
        
        return saveData;
    }
    
    // 저장된 게임 데이터 적용 메서드
    private void ApplyGameData(SaveData saveData)
    {
        // 씬 변경이 필요한 경우
        if (SceneManager.GetActiveScene().name != saveData.progressData.currentScene)
        {
            // 씬 로드 후 데이터 적용을 위해 데이터 임시 저장
            currentSaveData = saveData;
            
            // 씬 로드 시작
            SceneManager.LoadScene(saveData.progressData.currentScene);
            
            // SceneManager.sceneLoaded 이벤트에서 ApplyGameDataAfterSceneLoad 호출하도록 이벤트 등록
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            // 현재 씬이 저장된 씬과 동일한 경우 바로 데이터 적용
            ApplyGameDataInternal(saveData);
        }
    }
    
    // 씬 로드 후 데이터 적용
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentSaveData != null)
        {
            // 씬 로드 완료 후 데이터 적용
            ApplyGameDataInternal(currentSaveData);
        }
        
        // 이벤트 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    
    // 실제 게임 데이터 적용 내부 메서드
    private void ApplyGameDataInternal(SaveData saveData)
    {
        // 플레이어 데이터 적용
        ApplyPlayerData(saveData.playerData);
        
        // 용 데이터 적용
        ApplyDragonData(saveData.dragonData);
        
        // 퀘스트 데이터 적용
        // (퀘스트 매니저를 통해 데이터 적용)
        
        // 인벤토리 데이터 적용
        // (인벤토리 매니저를 통해 데이터 적용)
        
        // 게임 진행 상태 적용
        // (게임 매니저를 통해 데이터 적용)
    }
    
    // 플레이어 데이터 가져오기 (게임 내에서 구현 필요)
    private PlayerData GetPlayerData()
    {
        // GameManager를 통해 플레이어 데이터를 가져옵니다.
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.GetCurrentPlayerData();
        }
        return null;
    }
    
    // 용 데이터 가져오기 (게임 내에서 구현 필요)
    private DragonData GetDragonData()
    {
        // GameManager를 통해 용 데이터를 가져옵니다.
        if (GameManager.Instance != null)
        {
            return GameManager.Instance.GetCurrentDragonData();
        }
        return null;
    }
    
    // 플레이어 데이터 적용 (게임 내에서 구현 필요)
    private void ApplyPlayerData(PlayerSaveData playerSaveData)
    {
        // GameManager를 통해 플레이어 데이터를 적용합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyPlayerSaveData(playerSaveData);
        }
    }
    
    // 용 데이터 적용 (게임 내에서 구현 필요)
    private void ApplyDragonData(DragonSaveData dragonSaveData)
    {
        // GameManager를 통해 용 데이터를 적용합니다.
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ApplyDragonSaveData(dragonSaveData);
        }
    }
}

[System.Serializable]
public class SaveSlotInfo
{
    public int SlotNumber;
    public string PlayerName;
    public string SaveDateTime;
    public int PlayerLevel;
    public int PlayTime; // 초 단위
    
    public string GetFormattedPlayTime()
    {
        int hours = PlayTime / 3600;
        int minutes = (PlayTime % 3600) / 60;
        int seconds = PlayTime % 60;
        
        return $"{hours:D2}:{minutes:D2}:{seconds:D2}";
    }
} 
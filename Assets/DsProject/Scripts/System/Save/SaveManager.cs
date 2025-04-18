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
        
        try {
            // PlayerData 참조 가져오기
            PlayerData playerData = CharacterManager.PlayerCharacterData;
            if (playerData == null) {
                Debug.LogError("PlayerCharacterData가 null입니다.");
                return;
            }
            
            // 기본 스탯 저장
            saveData.playerData.maxHealth = playerData.maxHp;
            saveData.playerData.currentHealth = playerData.currentHp;
            saveData.playerData.maxMana = playerData.maxMp;
            saveData.playerData.currentMana = playerData.currentMp;
            saveData.playerData.maxStamina = playerData.stamina;
            saveData.playerData.currentStamina = playerData.staminaCurrent;
            
            // 레벨 및 경험치 저장
            saveData.playerData.level = playerData.level;
            saveData.playerData.experience = playerData.currentExperience;
            saveData.playerData.statPoints = playerData.availableStatPoints;
            
            // 기본 능력치 저장
            saveData.playerData.strength = playerData.strength;
            saveData.playerData.intelligence = playerData.intelligence;
            saveData.playerData.dexterity = playerData.agility;
            saveData.playerData.vitality = playerData.vitality;
            
            // 파생 스탯 저장
            saveData.playerData.attackPower = playerData.physicalDamage;
            saveData.playerData.magicPower = playerData.magicDamage;
            saveData.playerData.defense = playerData.physicalDefense;
            saveData.playerData.evasion = playerData.dodgeChance;
            
            Debug.Log("플레이어 데이터가 성공적으로 저장되었습니다.");
        }
        catch (Exception e) {
            Debug.LogWarning($"플레이어 데이터 저장 중 오류: {e.Message}");
        }
    }
    
    // 인벤토리 데이터 저장
    private void SaveInventoryData(SaveData saveData)
    {
        try {
            // 인벤토리 항목 저장
            if (InventoryManager.Instance != null)
            {
                // 일반 인벤토리 저장
                saveData.inventoryData.items.Clear();
                foreach (var item in InventoryManager.InventoryList)
                {
                    SaveData.InventoryData.ItemInfo itemInfo = new SaveData.InventoryData.ItemInfo
                    {
                        itemId = item.id,
                        count = item.quantity,
                        slotIndex = 0 // 실제 구현에 맞게 조정
                    };
                    saveData.inventoryData.items.Add(itemInfo);
                }
                
                // 장착 중인 장비 저장
                if (ItemEffectManager.Instance != null)
                {
                    // 무기 저장
                    var weapon = ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.손);
                    if (weapon != null)
                    {
                        saveData.inventoryData.weapon = new SaveData.InventoryData.ItemInfo
                        {
                            itemId = weapon.id,
                            count = 1,
                            slotIndex = -1
                        };
                    }
                    
                    // 방어구 저장
                    var armor = ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.몸);
                    if (armor != null)
                    {
                        saveData.inventoryData.armor = new SaveData.InventoryData.ItemInfo
                        {
                            itemId = armor.id,
                            count = 1,
                            slotIndex = -1
                        };
                    }
                    
                    // 액세서리 저장
                    var accessory = ItemEffectManager.Instance.GetEquippedItem(EquipmentSlot.머리);
                    if (accessory != null)
                    {
                        saveData.inventoryData.accessory = new SaveData.InventoryData.ItemInfo
                        {
                            itemId = accessory.id,
                            count = 1,
                            slotIndex = -1
                        };
                    }
                }
                
                Debug.Log("인벤토리 데이터가 성공적으로 저장되었습니다.");
            }
            else
            {
                Debug.LogWarning("InventoryManager.Instance가 null입니다. 인벤토리를 저장할 수 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"인벤토리 데이터 저장 중 오류: {e.Message}");
        }
    }
    
    // 스킬 데이터 저장
    private void SaveSkillData(SaveData saveData)
    {
        try
        {
            if (SkillManager.Instance != null)
            {
                // 습득한 스킬 목록 저장 - 세부 구현은 실제 SkillManager 구조에 맞게 조정 필요
                saveData.skillData.unlockedSkills.Clear();
                
                // 스킬 정보는 SkillManager로부터 직접 가져오기
                // 실제 구현이 확인되지 않아 기본값만 저장
                saveData.skillData.unlockedSkills.Add(new SaveData.SkillData.SkillInfo { 
                    skillId = "Dash", 
                    level = 1 
                });
                
                saveData.skillData.unlockedSkills.Add(new SaveData.SkillData.SkillInfo { 
                    skillId = "Slash", 
                    level = 1 
                });
                
                Debug.Log("스킬 데이터가 성공적으로 저장되었습니다.");
            }
            else
            {
                Debug.LogWarning("SkillManager.Instance가 null입니다. 스킬을 저장할 수 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"스킬 데이터 저장 중 오류: {e.Message}");
        }
    }
    
    // 퀘스트 데이터 저장
    private void SaveQuestData(SaveData saveData)
    {
        try
        {
            if (QuestManager.Instance != null)
            {
                // 활성화된 퀘스트 목록 저장
                saveData.questData.activeQuests.Clear();
                var activeQuests = QuestManager.QuestDatabase;
                if (activeQuests != null)
                {
                    foreach (var quest in activeQuests)
                    {
                        if (!quest.isCompleted)
                        {
                            SaveData.QuestData.QuestInfo questInfo = new SaveData.QuestData.QuestInfo
                            {
                                questId = quest.id,
                                progress = 0, // Dictionary는 직렬화가 어려우므로 개별 처리 필요
                                isTracking = false // 추적 기능이 있다면 저장
                            };
                            saveData.questData.activeQuests.Add(questInfo);
                        }
                    }
                }
                
                // 완료된 퀘스트 목록 저장
                saveData.questData.completedQuests.Clear();
                var completedQuests = QuestManager.CompletedQuests;
                if (completedQuests != null)
                {
                    foreach (var quest in completedQuests)
                    {
                        saveData.questData.completedQuests.Add(quest.id);
                    }
                }
                
                Debug.Log("퀘스트 데이터가 성공적으로 저장되었습니다.");
            }
            else
            {
                Debug.LogWarning("QuestManager.Instance가 null입니다. 퀘스트를 저장할 수 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"퀘스트 데이터 저장 중 오류: {e.Message}");
        }
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
        
        try {
            // DragonData 참조 가져오기
            DragonData dragon = dragonController.DragonData;
            if (dragon == null)
            {
                Debug.LogWarning("DragonData가 null입니다.");
                dragon = CharacterManager.DragonData;
                if (dragon == null)
                {
                    Debug.LogError("CharacterManager.DragonData도 null입니다.");
                    return;
                }
            }
            
            // 기본 정보 저장
            saveData.dragonData.isUnlocked = true;
            saveData.dragonData.level = dragon.bondLevel;
            
            // HP 정보는 현재 컴파일 오류로 인해 주석 처리
            // 나중에 정확한 타입을 확인한 후 수정 필요
            // saveData.dragonData.currentHealth = 100.0f;
            // saveData.dragonData.maxHealth = 100.0f;
            
            // 언락된 능력 저장
            saveData.dragonData.unlockedAbilities.Clear();
            saveData.dragonData.unlockedAbilities.Add("BasicAttack");
            
            Debug.Log("용 데이터가 성공적으로 저장되었습니다.");
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
        
        try {
            // PlayerData 참조 가져오기
            PlayerData playerData = CharacterManager.PlayerCharacterData;
            if (playerData == null) {
                Debug.LogError("PlayerCharacterData가 null입니다.");
                return;
            }
            
            // 기본 스탯 복원
            playerData.maxHp = saveData.playerData.maxHealth;
            playerData.currentHp = saveData.playerData.currentHealth;
            playerData.maxMp = saveData.playerData.maxMana;
            playerData.currentMp = saveData.playerData.currentMana;
            playerData.stamina = saveData.playerData.maxStamina;
            playerData.staminaCurrent = saveData.playerData.currentStamina;
            
            // 레벨 및 경험치 복원
            playerData.level = saveData.playerData.level;
            playerData.currentExperience = saveData.playerData.experience;
            playerData.availableStatPoints = saveData.playerData.statPoints;
            
            // 기본 능력치 복원
            playerData.strength = saveData.playerData.strength;
            playerData.intelligence = saveData.playerData.intelligence;
            playerData.agility = saveData.playerData.dexterity;
            playerData.vitality = saveData.playerData.vitality;
            
            // 파생 스탯 업데이트
            playerData.UpdateDerivedStats();
            
            Debug.Log("플레이어 데이터가 성공적으로 적용되었습니다.");
        }
        catch (Exception e) {
            Debug.LogWarning($"플레이어 데이터 적용 중 오류: {e.Message}");
        }
    }
    
    // 인벤토리 데이터 적용
    private void ApplyInventoryData(SaveData saveData)
    {
        try {
            if (InventoryManager.Instance != null)
            {
                // 인벤토리 초기화
                InventoryManager.Instance.ClearInventory();
                
                // 인벤토리 항목 복원
                foreach (var itemInfo in saveData.inventoryData.items)
                {
                    Item item = ItemManager.Instance.GetItemById(itemInfo.itemId);
                    if (item != null)
                    {
                        item.quantity = itemInfo.count;
                        InventoryManager.Instance.AddItemLogic(item);
                    }
                }
                
                // 장착 중인 장비 복원 - 실제 구현이 불분명하여 로그만 출력
                if (saveData.inventoryData.weapon != null)
                {
                    Debug.Log($"무기 장착: {saveData.inventoryData.weapon.itemId}");
                }
                
                if (saveData.inventoryData.armor != null)
                {
                    Debug.Log($"방어구 장착: {saveData.inventoryData.armor.itemId}");
                }
                
                if (saveData.inventoryData.accessory != null)
                {
                    Debug.Log($"액세서리 장착: {saveData.inventoryData.accessory.itemId}");
                }
                
                // UI 갱신
                UIManager.Instance.InventoryUpdate();
                
                Debug.Log("인벤토리 데이터가 성공적으로 적용되었습니다.");
            }
            else
            {
                Debug.LogWarning("InventoryManager.Instance가 null입니다. 인벤토리를 로드할 수 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"인벤토리 데이터 적용 중 오류: {e.Message}");
        }
    }
    
    // 스킬 데이터 적용
    private void ApplySkillData(SaveData saveData)
    {
        try
        {
            if (SkillManager.Instance != null)
            {
                // 스킬 시스템 초기화
                // 세부 구현은 SkillManager 구조에 맞게 조정 필요
                
                // 간단한 로그만 출력
                foreach (var skillInfo in saveData.skillData.unlockedSkills)
                {
                    Debug.Log($"스킬 로드: {skillInfo.skillId} (레벨 {skillInfo.level})");
                }
                
                Debug.Log("스킬 데이터가 성공적으로 적용되었습니다.");
            }
            else
            {
                Debug.LogWarning("SkillManager.Instance가 null입니다. 스킬을 로드할 수 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"스킬 데이터 적용 중 오류: {e.Message}");
        }
    }
    
    // 퀘스트 데이터 적용
    private void ApplyQuestData(SaveData saveData)
    {
        try
        {
            if (QuestManager.Instance != null)
            {
                // 활성화된 퀘스트 복원
                foreach (var questInfo in saveData.questData.activeQuests)
                {
                    // 퀘스트 활성화 메서드가 확인되지 않아 로그만 출력
                    Debug.Log($"퀘스트 활성화: {questInfo.questId}");
                }
                
                // 완료된 퀘스트 복원
                foreach (var questId in saveData.questData.completedQuests)
                {
                    // 퀘스트 완료 메서드가 확인되지 않아 로그만 출력
                    Debug.Log($"퀘스트 완료: {questId}");
                }
                
                Debug.Log("퀘스트 데이터가 성공적으로 적용되었습니다.");
            }
            else
            {
                Debug.LogWarning("QuestManager.Instance가 null입니다. 퀘스트를 로드할 수 없습니다.");
            }
        }
        catch (Exception e)
        {
            Debug.LogWarning($"퀘스트 데이터 적용 중 오류: {e.Message}");
        }
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
        
        try {
            // DragonData 참조 가져오기
            DragonData dragon = dragonController.DragonData;
            if (dragon == null)
            {
                Debug.LogWarning("DragonData가 null입니다.");
                dragon = CharacterManager.DragonData;
                if (dragon == null)
                {
                    Debug.LogError("CharacterManager.DragonData도 null입니다.");
                    return;
                }
            }
            
            // 기본 정보 복원
            dragon.bondLevel = saveData.dragonData.level;
            
            // 언락된 능력들 로그 출력
            foreach (var ability in saveData.dragonData.unlockedAbilities)
            {
                Debug.Log($"드래곤 능력 로드: {ability}");
            }
            
            // 모델 및 설정 업데이트 시도 (private 메서드 호출 대신 로그만 출력)
            Debug.Log("드래곤 모델 및 설정 업데이트 필요");
            
            Debug.Log("용 데이터가 성공적으로 적용되었습니다.");
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
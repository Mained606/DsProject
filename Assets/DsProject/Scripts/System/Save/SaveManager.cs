using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Linq;

// 타입 오류 수정을 위해 SaveableObject 참조 명시적 추가
using DsProject.Scripts.System.Save;

namespace DsProject.Scripts.System.Save
{
    // SaveableObject 클래스가 이 네임스페이스 안에 있다고 선언하여 참조 오류 해결
}

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
            
            // 저장 가능한 오브젝트들의 상태 저장
            SaveSaveableObjects(saveData);
            
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
        
        // 저장 가능한 오브젝트들의 상태 로드
        LoadSaveableObjects(saveData);
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
            
            // 골드 저장
            saveData.playerData.gold = playerData.gold;
            
            // 버프 정보 저장
            saveData.playerData.hpBuffBonus = playerData.hpBuffBonus;
            saveData.playerData.physicalDamageBuffMultiplier = playerData.physicalDamageBuffMultiplier;
            saveData.playerData.magicDamageBuffMultiplier = playerData.magicDamageBuffMultiplier;
            
            // TODO: 버프 지속시간과 쿨타임 정보를 저장하는 코드를 구현
            // SkillManager.Instance를 사용하여 활성화된 버프 목록을 가져온 후
            // saveData.playerData.activeBuffs 리스트에 저장
            
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
            saveData.inventoryData.items.Clear();
            foreach (var item in InventoryManager.InventoryList)
            {
                SaveData.InventoryData.ItemInfo itemInfo = new SaveData.InventoryData.ItemInfo
                {
                    itemId = item.id,
                    count = item.quantity,
                    slotIndex = -1, // 슬롯 인덱스는 필요에 따라 저장
                    itemLevel = item.itemSkill != null ? item.itemSkill.Level : 0 // 아이템 강화 수치 저장
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
                        slotIndex = -1,
                        itemLevel = weapon.itemSkill != null ? weapon.itemSkill.Level : 0 // 아이템 강화 수치 저장
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
                        slotIndex = -1,
                        itemLevel = armor.itemSkill != null ? armor.itemSkill.Level : 0 // 아이템 강화 수치 저장
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
                        slotIndex = -1,
                        itemLevel = accessory.itemSkill != null ? accessory.itemSkill.Level : 0 // 아이템 강화 수치 저장
                    };
                }
            }
            
            // 퀵슬롯에 등록된 아이템 저장
            saveData.inventoryData.quickSlotItems.Clear();
            
            // UIManager에서 아이템 퀵슬롯 참조 가져오기
            if (UIManager.Instance != null && InventoryManager.QuickSlotsUI != null)
            {
                var itemQuickSlotUI = InventoryManager.QuickSlotsUI;
                var slots = itemQuickSlotUI.GetSlots();
                
                if (slots != null)
                {
                    for (int i = 0; i < slots.Length; i++)
                    {
                        var item = slots[i].GetItem();
                        if (item != null)
                        {
                            SaveData.InventoryData.ItemInfo itemInfo = new SaveData.InventoryData.ItemInfo
                            {
                                itemId = item.id,
                                count = 1, // 퀵슬롯은 소모품 참조만 저장
                                slotIndex = i,
                                itemLevel = item.itemSkill != null ? item.itemSkill.Level : 0
                            };
                            saveData.inventoryData.quickSlotItems.Add(itemInfo);
                            Debug.Log($"퀵슬롯 {i}에 등록된 아이템 '{item.id}'을 저장했습니다.");
                        }
                    }
                }
            }
            
            Debug.Log("인벤토리 데이터가 성공적으로 저장되었습니다.");
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
                // 습득한 스킬 목록 저장
                saveData.skillData.unlockedSkills.Clear();
                
                // SkillManager에서 언락된 스킬 정보 가져오기
                foreach (var entry in SkillManager.SkillList)
                {
                    // 플레이어 스킬만 저장
                    if (entry.Key.Item1 == EntityType.Player)
                    {
                        // 새로운 메서드로 언락 상태 확인
                        bool isUnlocked = SkillManager.Instance.IsSkillUnlocked(entry.Key.Item1, entry.Key.Item2);
                        
                        // 언락된 스킬만 저장
                        if (isUnlocked)
                        {
                            Skills skill = entry.Value;
                            saveData.skillData.unlockedSkills.Add(new SaveData.SkillData.SkillInfo { 
                                skillId = skill.skillName, 
                                level = skill.skillLevel 
                            });
                            
                            Debug.Log($"스킬 '{skill.skillName}' (레벨 {skill.skillLevel})을 저장했습니다.");
                        }
                    }
                }
                
                // 퀵슬롯에 등록된 스킬 저장
                saveData.skillData.quickSlotSkills.Clear();
                
                if (UIManager.SkillsQuickSlot != null)
                {
                    for (int i = 0; i < UIManager.SkillsQuickSlot.registedSkillList.Count; i++)
                    {
                        Skills skill = UIManager.SkillsQuickSlot.registedSkillList[i];
                        if (skill != null)
                        {
                            // 스킬 ID 저장 (스킬 이름 사용)
                            saveData.skillData.quickSlotSkills.Add(skill.skillName);
                            Debug.Log($"퀵슬롯 {i}에 등록된 스킬 '{skill.skillName}'을 저장했습니다.");
                        }
                        else
                        {
                            // 빈 슬롯은 빈 문자열로 저장
                            saveData.skillData.quickSlotSkills.Add("");
                        }
                    }
                }
                
                Debug.Log($"스킬 데이터가 성공적으로 저장되었습니다. 총 {saveData.skillData.unlockedSkills.Count}개의 스킬.");
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
    
    // 스킬 데이터 적용
    private void ApplySkillData(SaveData saveData)
    {
        try
        {
            if (SkillManager.Instance != null)
            {
                // 먼저 모든 플레이어 스킬을 잠금 상태로 초기화
                SkillManager.Instance.ResetAllPlayerSkillUnlockStates();
                
                // 저장된 스킬 데이터 적용
                foreach (var skillInfo in saveData.skillData.unlockedSkills)
                {
                    // SkillManager에서 스킬 찾기
                    foreach (var entry in SkillManager.SkillList)
                    {
                        // 스킬 ID 대신 스킬 이름으로 비교 (ID 매핑이 필요하면 추가 로직 구현 필요)
                        if (entry.Key.Item2 == skillInfo.skillId)
                        {
                            // 새로운 메서드로 언락 상태 설정
                            SkillManager.Instance.SetSkillUnlockState(entry.Key.Item1, entry.Key.Item2, true);
                            
                            Skills skill = entry.Value;
                            
                            // 스킬 레벨 복원 (최소 1, 최대 해당 스킬의 최대 레벨 범위 내에서)
                            int levelToRestore = Mathf.Clamp(skillInfo.level, 1, skill.maxSkillLevel);
                            
                            // 현재 레벨과 다르면 레벨 업데이트
                            if (skill.skillLevel != levelToRestore)
                            {
                                // 레벨 1부터 시작해서 목표 레벨까지 하나씩 올림 (가중치 적용을 위해)
                                skill.skillLevel = 1;
                                skill.Initialize(); // 먼저 초기화
                                
                                // 목표 레벨까지 하나씩 레벨업 (가중치 적용을 위해)
                                for (int i = 1; i < levelToRestore; i++)
                                {
                                    skill.LevelUp(true); // 강제 레벨업
                                }
                            }
                            
                            Debug.Log($"스킬 '{skill.skillName}' 잠금 해제 및 레벨 {skill.skillLevel}으로 복원됨");
                            break; // 스킬을 찾았으므로 다음 스킬로 넘어감
                        }
                    }
                }
                
                // 퀵슬롯에 등록된 스킬 복원
                if (UIManager.SkillsQuickSlot != null && saveData.skillData.quickSlotSkills.Count > 0)
                {
                    // 먼저 모든 슬롯 초기화
                    UIManager.SkillsQuickSlot.ClearAllSlots();
                    
                    // 저장된 스킬 슬롯 복원
                    for (int i = 0; i < saveData.skillData.quickSlotSkills.Count; i++)
                    {
                        string skillId = saveData.skillData.quickSlotSkills[i];
                        if (!string.IsNullOrEmpty(skillId))
                        {
                            // SkillManager에서 스킬 찾기
                            Skills skill = null;
                            foreach (var entry in SkillManager.SkillList)
                            {
                                if (entry.Key.Item2 == skillId)
                                {
                                    skill = entry.Value;
                                    break;
                                }
                            }
                            
                            if (skill != null)
                            {
                                // 스킬 아이콘 가져오기
                                Sprite icon = ItemManager.Instance.GetSkillSprite(skill.skillName);
                                
                                // 퀵슬롯에 스킬 할당
                                UIManager.SkillsQuickSlot.AssignSkillToSlot(skill, icon, i);
                                Debug.Log($"퀵슬롯 {i}에 스킬 '{skill.skillName}'을 복원했습니다.");
                            }
                        }
                    }
                    
                    Debug.Log("스킬 퀵슬롯 데이터가 성공적으로 복원되었습니다.");
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
                                isTracking = false // 추적 기능이 있다면 해당 상태 저장
                            };
                            
                            // 진행 상태 저장 (첫 번째 조건만 저장하거나 필요에 따라 확장)
                            if (quest.progress != null && quest.progress.Count > 0)
                            {
                                var firstProgress = quest.progress.FirstOrDefault();
                                questInfo.progress = firstProgress.Value;
                            }
                            
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
        
        // 드래곤 컨트롤러 가져오기
        DragonController dragonController = GameManager.DragonTransform.GetComponent<DragonController>();
        if (dragonController == null) return;
        
        // 위치 및 회전 정보 저장
        saveData.dragonData.position = GameManager.DragonTransform.position;
        saveData.dragonData.rotation = GameManager.DragonTransform.rotation;
        
        // 활성화 상태 저장
        saveData.dragonData.isActive = GameManager.DragonTransform.gameObject.activeSelf;
        
        try {
            DragonData dragonData = CharacterManager.DragonData;
            
            if (dragonData != null) {
                // 기본 정보 저장
                saveData.dragonData.level = dragonData.bondLevel;
                
                // 언락된 능력 저장
                saveData.dragonData.unlockedAbilities.Clear();
                // DragonData의 unlockedAbilities 필드가 존재한다면 아래 코드를 유지
                // 그렇지 않으면 필요한 능력들을 직접 저장
                saveData.dragonData.unlockedAbilities.Add("BasicAttack");
            }
            
            Debug.Log($"용 데이터가 성공적으로 저장되었습니다. 활성화 상태: {saveData.dragonData.isActive}");
        }
        catch (Exception e) {
            Debug.LogWarning($"용 데이터 저장 중 오류: {e.Message}");
        }
    }
    
    // 저장 가능한 오브젝트들의 상태 저장
    private void SaveSaveableObjects(SaveData saveData)
    {
        if (saveData.saveableObjects == null || saveData.saveableObjects.objects == null)
        {
            Debug.Log("저장된 오브젝트 데이터가 없습니다.");
            return;
        }
        
        // SaveableObject 컴포넌트가 있는 모든 오브젝트를 찾기
        // (비활성화된 오브젝트도 찾기 위해 Resources.FindObjectsOfTypeAll 사용)
        SaveableObject[] saveableObjects = Resources.FindObjectsOfTypeAll<SaveableObject>();
        
        Debug.Log($"씬에서 {saveableObjects.Length}개의 SaveableObject 컴포넌트를 찾았습니다.");
        
        // ID로 활성화 상태와 위치를 저장할 수 있는 오브젝트를 찾고 일괄 저장 이벤트 발생
        foreach (var saveableObject in saveableObjects)
        {
            // 각 SaveableObject는 자신의 OnGameSaved 핸들러에서 자신의 상태를 저장함
            // 여기서는 추가적인 작업 필요 없음
            Debug.Log($"SaveableObject: {saveableObject.gameObject.name} (ID: {saveableObject.objectId})");
        }
    }
    
    // 지연 활성화를 위한 코루틴
    private IEnumerator DelayedActivation(GameObject obj, bool activate)
    {
        // 씬 로드 후 객체를 찾는 데 시간이 필요할 수 있으므로 약간의 지연 추가
        yield return new WaitForSeconds(0.2f);
        
        if (obj != null)
        {
            obj.SetActive(activate);
            Debug.Log($"오브젝트 '{obj.name}'의 활성화 상태를 {activate}로 설정했습니다.");
        }
    }
    
    // 지연 드래곤 업데이트를 위한 코루틴
    private IEnumerator DelayedDragonUpdate(DragonController controller)
    {
        // 모든 컴포넌트가 초기화될 시간을 줌
        yield return new WaitForSeconds(0.5f);
        
        if (controller != null)
        {
            // 새로 추가된 공개 메서드 호출
            bool prevActiveState = controller.gameObject.activeSelf;
            
            // 비활성화 상태라면 임시로 활성화
            if (!prevActiveState)
            {
                controller.gameObject.SetActive(true);
            }
            
            controller.UpdateDragonModelPublic();
            
            // 원래 상태로 복원
            if (!prevActiveState)
            {
                controller.gameObject.SetActive(false);
            }
            
            Debug.Log("Dragon 모델 업데이트 완료");
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
            playerData.maxHp = (int)saveData.playerData.maxHealth;
            playerData.currentHp = (int)saveData.playerData.currentHealth;
            playerData.maxMp = (int)saveData.playerData.maxMana;
            playerData.currentMp = (int)saveData.playerData.currentMana;
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
            
            // 골드 복원
            playerData.gold = saveData.playerData.gold;
            
            // 버프 정보 복원
            playerData.hpBuffBonus = saveData.playerData.hpBuffBonus;
            playerData.physicalDamageBuffMultiplier = saveData.playerData.physicalDamageBuffMultiplier;
            playerData.magicDamageBuffMultiplier = saveData.playerData.magicDamageBuffMultiplier;
            
            // TODO: 버프 지속시간과 쿨타임 정보를 복원하는 코드를 구현
            // saveData.playerData.activeBuffs 리스트에서 버프 정보를 가져와서
            // SkillManager.Instance를 사용하여 버프 적용 및 지속시간 설정
            
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
                        
                        // 아이템 강화 수치 적용 (추가)
                        if (item.itemSkill != null && itemInfo.itemLevel > 0)
                        {
                            item.itemSkill.Level = itemInfo.itemLevel;
                            
                            // 강화 수치에 따른 파생 스탯 업데이트
                            if (item.itemStat != null)
                            {
                                // 먼저 초기 스탯으로 초기화
                                item.itemStat.Initialize();
                                
                                // 레벨에 따라 스탯 증가 (여러 번 ApplyItemStat 호출)
                                for (int i = 0; i < item.itemSkill.Level; i++)
                                {
                                    if (item.type == ItemType.무기)
                                    {
                                        // 무기 강화시 공격력 증가 (예시값 5로 설정)
                                        float attackBonus = 5.0f;
                                        item.itemSkill.ApplyItemStat(item, attackBonus, 1);
                                    }
                                    else if (item.type == ItemType.방어구)
                                    {
                                        // 방어구 강화시 방어력 증가 (예시값 3으로 설정)
                                        float defenseBonus = 3.0f;
                                        item.itemSkill.ApplyItemStat(item, defenseBonus, 1);
                                    }
                                }
                                
                                Debug.Log($"아이템 '{item.name}' (레벨: {item.itemSkill.Level})의 파생 스탯이 업데이트되었습니다.");
                            }
                        }
                        
                        InventoryManager.Instance.AddItemLogic(item);
                    }
                }
                
                // 장착 중인 장비 복원
                if (ItemEffectManager.Instance != null)
                {
                    // 무기 장착
                    if (saveData.inventoryData.weapon != null)
                    {
                        // 인벤토리에서 해당 ID를 가진 아이템을 찾음
                        Item weaponInInventory = InventoryManager.Instance.FindInventoryItem(saveData.inventoryData.weapon.itemId);
                        
                        if (weaponInInventory != null)
                        {
                            // 인벤토리에 있는 아이템을 직접 장착
                            // 이미 강화 수치가 적용되어 있으므로 추가 설정 필요 없음
                            ItemEffectManager.Instance.ApplyItemEffect(weaponInInventory);
                            Debug.Log($"무기 장착: {weaponInInventory.name} (인벤토리에서 직접 찾은 아이템, 레벨: {weaponInInventory.itemSkill?.Level ?? 0})");
                        }
                        else
                        {
                            // 인벤토리에 아이템이 없는 경우 새로 생성
                            Item weapon = ItemManager.Instance.GetItemById(saveData.inventoryData.weapon.itemId);
                            if (weapon != null)
                            {
                                // 강화 수치 적용
                                if (weapon.itemSkill != null && saveData.inventoryData.weapon.itemLevel > 0)
                                {
                                    weapon.itemSkill.Level = saveData.inventoryData.weapon.itemLevel;
                                    
                                    // 강화 수치에 따른 파생 스탯 업데이트
                                    if (weapon.itemStat != null)
                                    {
                                        // 먼저 초기 스탯으로 초기화
                                        weapon.itemStat.Initialize();
                                        
                                        // 레벨에 따라 스탯 증가 (여러 번 ApplyItemStat 호출)
                                        for (int i = 0; i < weapon.itemSkill.Level; i++)
                                        {
                                            // 무기 강화시 공격력 증가 (예시값 5로 설정)
                                            float attackBonus = 5.0f;
                                            weapon.itemSkill.ApplyItemStat(weapon, attackBonus, 1);
                                        }
                                    }
                                }
                                
                                ItemEffectManager.Instance.ApplyItemEffect(weapon);
                                Debug.Log($"무기 장착: {weapon.name} (새로 생성한 아이템, 레벨: {weapon.itemSkill?.Level ?? 0})");
                            }
                        }
                    }
                    
                    // 방어구 장착
                    if (saveData.inventoryData.armor != null)
                    {
                        // 인벤토리에서 해당 ID를 가진 아이템을 찾음
                        Item armorInInventory = InventoryManager.Instance.FindInventoryItem(saveData.inventoryData.armor.itemId);
                        
                        if (armorInInventory != null)
                        {
                            // 인벤토리에 있는 아이템을 직접 장착
                            ItemEffectManager.Instance.ApplyItemEffect(armorInInventory);
                            Debug.Log($"방어구 장착: {armorInInventory.name} (인벤토리에서 직접 찾은 아이템, 레벨: {armorInInventory.itemSkill?.Level ?? 0})");
                        }
                        else
                        {
                            // 인벤토리에 아이템이 없는 경우 새로 생성
                            Item armor = ItemManager.Instance.GetItemById(saveData.inventoryData.armor.itemId);
                            if (armor != null)
                            {
                                // 강화 수치 적용
                                if (armor.itemSkill != null && saveData.inventoryData.armor.itemLevel > 0)
                                {
                                    armor.itemSkill.Level = saveData.inventoryData.armor.itemLevel;
                                    
                                    // 강화 수치에 따른 파생 스탯 업데이트
                                    if (armor.itemStat != null)
                                    {
                                        // 먼저 초기 스탯으로 초기화
                                        armor.itemStat.Initialize();
                                        
                                        // 레벨에 따라 스탯 증가 (여러 번 ApplyItemStat 호출)
                                        for (int i = 0; i < armor.itemSkill.Level; i++)
                                        {
                                            // 방어구 강화시 방어력 증가 (예시값 3으로 설정)
                                            float defenseBonus = 3.0f;
                                            armor.itemSkill.ApplyItemStat(armor, defenseBonus, 1);
                                        }
                                    }
                                }
                                
                                ItemEffectManager.Instance.ApplyItemEffect(armor);
                                Debug.Log($"방어구 장착: {armor.name} (새로 생성한 아이템, 레벨: {armor.itemSkill?.Level ?? 0})");
                            }
                        }
                    }
                    
                    // 액세서리 장착
                    if (saveData.inventoryData.accessory != null)
                    {
                        // 인벤토리에서 해당 ID를 가진 아이템을 찾음
                        Item accessoryInInventory = InventoryManager.Instance.FindInventoryItem(saveData.inventoryData.accessory.itemId);
                        
                        if (accessoryInInventory != null)
                        {
                            // 인벤토리에 있는 아이템을 직접 장착
                            ItemEffectManager.Instance.ApplyItemEffect(accessoryInInventory);
                            Debug.Log($"액세서리 장착: {accessoryInInventory.name} (인벤토리에서 직접 찾은 아이템, 레벨: {accessoryInInventory.itemSkill?.Level ?? 0})");
                        }
                        else
                        {
                            // 인벤토리에 아이템이 없는 경우 새로 생성
                            Item accessory = ItemManager.Instance.GetItemById(saveData.inventoryData.accessory.itemId);
                            if (accessory != null)
                            {
                                // 강화 수치 적용
                                if (accessory.itemSkill != null && saveData.inventoryData.accessory.itemLevel > 0)
                                {
                                    accessory.itemSkill.Level = saveData.inventoryData.accessory.itemLevel;
                                    
                                    // 강화 수치에 따른 파생 스탯 업데이트
                                    if (accessory.itemStat != null)
                                    {
                                        // 먼저 초기 스탯으로 초기화
                                        accessory.itemStat.Initialize();
                                        
                                        // 레벨에 따라 스탯 증가 (여러 번 ApplyItemStat 호출)
                                        for (int i = 0; i < accessory.itemSkill.Level; i++)
                                        {
                                            // 장신구 강화시 스탯 증가 (예시값 2로 설정)
                                            float statBonus = 2.0f;
                                            accessory.itemSkill.ApplyItemStat(accessory, statBonus, 1);
                                        }
                                    }
                                }
                                
                                ItemEffectManager.Instance.ApplyItemEffect(accessory);
                                Debug.Log($"액세서리 장착: {accessory.name} (새로 생성한 아이템, 레벨: {accessory.itemSkill?.Level ?? 0})");
                            }
                        }
                    }
                }
                
                // 퀵슬롯에 등록된 아이템 복원
                if (InventoryManager.QuickSlotsUI != null && saveData.inventoryData.quickSlotItems.Count > 0)
                {
                    // 먼저 모든 슬롯 초기화
                    InventoryManager.QuickSlotsUI.ClearAll();
                    
                    // 저장된 아이템 슬롯 복원
                    var slots = InventoryManager.QuickSlotsUI.GetSlots();
                    
                    foreach (var itemInfo in saveData.inventoryData.quickSlotItems)
                    {
                        if (itemInfo.slotIndex >= 0 && itemInfo.slotIndex < slots.Length)
                        {
                            // 인벤토리에서 해당 아이템 찾기
                            var item = InventoryManager.Instance.FindInventoryItem(itemInfo.itemId);
                            if (item != null)
                            {
                                // 퀵슬롯에 아이템 할당
                                slots[itemInfo.slotIndex].SetItem(item);
                                Debug.Log($"퀵슬롯 {itemInfo.slotIndex}에 아이템 '{item.name}'을 복원했습니다.");
                            }
                        }
                    }
                    
                    Debug.Log("아이템 퀵슬롯 데이터가 성공적으로 복원되었습니다.");
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
    
    // 퀘스트 데이터 적용
    private void ApplyQuestData(SaveData saveData)
    {
        try
        {
            if (QuestManager.Instance != null)
            {
                // 기존 퀘스트 데이터 초기화
                QuestManager.Instance.ClearQuestData();
                
                // 활성화된 퀘스트 복원
                foreach (var questInfo in saveData.questData.activeQuests)
                {
                    Quest quest = QuestManager.Instance.GetQuestById(questInfo.questId);
                    if (quest != null)
                    {
                        // 메인퀘스트가 아니거나, 메인퀘스트이지만 완료된 퀘스트 목록에 없는 경우에만 추가
                        bool isMainQuest = quest.questType == "메인퀘스트";
                        bool isAlreadyCompleted = saveData.questData.completedQuests.Contains(quest.id);
                        
                        // 메인퀘스트이면서 이미 완료된 퀘스트인 경우 추가하지 않음
                        if (isMainQuest && isAlreadyCompleted)
                        {
                            Debug.Log($"[SaveManager] 완료된 메인퀘스트 '{quest.name}'({quest.id})는 QuestDatabase에 추가하지 않습니다.");
                            continue;
                        }
                        
                        QuestManager.Instance.AddQuest(quest);
                        
                        // 퀘스트 진행 상태 복원
                        foreach (var condition in quest.requiredConditions)
                        {
                            if (questInfo.progress > 0)
                            {
                                // 진행 상태 초기화
                                quest.progress[condition.Key] = questInfo.progress;
                                
                                // 조건 완료 상태 확인
                                if (quest.progress[condition.Key] >= condition.Value.requiredQuantity)
                                {
                                    condition.Value.isCompleted = true;
                                }
                            }
                        }
                        
                        // 추적 상태 복원
                        if (questInfo.isTracking)
                        {
                            QuestManager.Instance.TrackQuest(quest);
                        }
                    }
                }
                
                // 완료된 퀘스트 복원
                foreach (var questId in saveData.questData.completedQuests)
                {
                    Quest quest = QuestManager.Instance.GetQuestById(questId);
                    if (quest != null)
                    {
                        // 직접 완료된 퀘스트 목록에 추가
                        quest.isCompleted = true;
                        QuestManager.Instance.AddCompletedQuest(quest);
                    }
                }
                
                // 로드 후 명시적으로 UI 업데이트 호출 - 퀘스트가 없을 때도 UI 정리하기 위함
                if (UIManager.Instance != null)
                {
                    // UI 업데이트 요청
                    UIManager.Instance.QuestUpdate();
                }
                
                // 활성 퀘스트 상태 로그
                int activeCount = QuestManager.QuestDatabase.Count;
                Debug.Log($"퀘스트 데이터가 성공적으로 적용되었습니다. 활성 퀘스트: {activeCount}개");
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
        try {
            if (GameManager.DragonTransform == null)
            {
                Debug.LogWarning("Dragon Transform이 null입니다. 씬에서 Dragon을 찾는 중...");
                
                // 씬에서 Dragon 찾기 시도 (활성화 상태와 관계없이 찾기)
                var dragons = Resources.FindObjectsOfTypeAll<DragonController>();
                
                if (dragons.Length > 0)
                {
                    Debug.Log($"씬에서 Dragon을 찾았습니다. 총 {dragons.Length}개");
                    GameManager.DragonTransform = dragons[0].transform;
                }
                else
                {
                    Debug.LogError("씬에서 Dragon을 찾을 수 없습니다.");
                    
                    // Dragon 프리팹을 Resources 폴더에서 찾아 인스턴스화 시도
                    var dragonPrefab = Resources.Load<GameObject>("Prefabs/Dragon");
                    if (dragonPrefab != null)
                    {
                        var dragonInstance = Instantiate(dragonPrefab);
                        GameManager.DragonTransform = dragonInstance.transform;
                        Debug.Log("Dragon 프리팹을 인스턴스화했습니다.");
                    }
                    else
                    {
                        Debug.LogError("Dragon 프리팹을 찾을 수 없습니다.");
                        return;
                    }
                }
            }
            
            // 용 컨트롤러 가져오기
            DragonController dragonController = GameManager.DragonTransform.GetComponent<DragonController>();
            if (dragonController == null)
            {
                Debug.LogError("Dragon 오브젝트에 DragonController 컴포넌트가 없습니다.");
                return;
            }
            
            // 위치 및 회전 복원
            GameManager.DragonTransform.position = saveData.dragonData.position;
            GameManager.DragonTransform.rotation = saveData.dragonData.rotation;
            
            // 활성화 상태 복원 (코루틴으로 지연 적용)
            StartCoroutine(DelayedActivation(GameManager.DragonTransform.gameObject, saveData.dragonData.isActive));
            
            // DragonData 참조 가져오기
            DragonData dragon = CharacterManager.DragonData;
            if (dragon == null)
            {
                Debug.LogError("CharacterManager.DragonData가 null입니다.");
                return;
            }
            
            // 기본 정보 복원
            dragon.bondLevel = saveData.dragonData.level;
            
            // 언락된 능력들 로그 출력
            foreach (var ability in saveData.dragonData.unlockedAbilities)
            {
                Debug.Log($"드래곤 능력 로드: {ability}");
            }
            
            // 드래곤 모델 업데이트 로직
            if (dragonController != null)
            {
                // 활성화 상태에 관계없이 드래곤 모델 업데이트 시도
                StartCoroutine(DelayedDragonUpdate(dragonController));
            }
            
            Debug.Log($"용 데이터가 성공적으로 적용되었습니다. 활성화 상태: {saveData.dragonData.isActive}");
        }
        catch (Exception e) {
            Debug.LogWarning($"용 데이터 적용 중 오류: {e.Message}");
        }
    }
    
    // 저장 가능한 오브젝트들의 상태 로드
    private void LoadSaveableObjects(SaveData saveData)
    {
        if (saveData.saveableObjects == null || saveData.saveableObjects.objects == null)
        {
            Debug.Log("저장된 오브젝트 데이터가 없습니다.");
            return;
        }
        
        // SaveableObject 컴포넌트가 있는 모든 오브젝트를 찾기
        // (비활성화된 오브젝트도 찾기 위해 Resources.FindObjectsOfTypeAll 사용)
        SaveableObject[] saveableObjects = Resources.FindObjectsOfTypeAll<SaveableObject>();
        
        Debug.Log($"씬에서 {saveableObjects.Length}개의 SaveableObject 컴포넌트를 찾았습니다.");
        
        // ID로 활성화 상태와 위치를 복원할 수 있는 오브젝트를 찾고 일괄 로드 이벤트 발생
        foreach (var saveableObject in saveableObjects)
        {
            // 각 SaveableObject는 자신의 OnGameLoaded 핸들러에서 자신의 상태를 복원함
            // 여기서는 추가적인 작업 필요 없음
            Debug.Log($"SaveableObject: {saveableObject.gameObject.name} (ID: {saveableObject.objectId})");
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
        
        // 스킬 초기화 로직 수정
        if (SkillManager.Instance != null)
        {
            // 새로운 메서드 사용
            SkillManager.Instance.ResetAllPlayerSkillUnlockStates();
            Debug.Log("스킬 언락 상태가 초기화되었습니다.");
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
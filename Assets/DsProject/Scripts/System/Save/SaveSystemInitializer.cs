using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveSystemInitializer : MonoBehaviour
{
    private static SaveSystemInitializer instance;
    
    // 새 게임인지 여부를 나타내는 플래그 추가
    public static bool isNewGame = false;
    
    [Header("세이브 시스템 설정")]
    [Tooltip("타이틀 씬 이름")]
    public string titleSceneName = "TitleScene";
    
    [Tooltip("메인 게임 씬 이름")]
    public string mainGameSceneName = "MainScene";
    
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
            
            // 타이틀 씬에서는 항상 커서가 보이도록 설정
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        // 게임 씬에서는 필요에 따라 저장 데이터 로드
        else
        {
            // 이전 상태가 게임 오버였을 경우 커서 잠금 상태 해제
            if (GameStateMachine.Instance != null && 
                GameStateMachine.Instance.CurrentState == GameSystemState.GameOver)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            // 일반 게임 씬에서는 기본 상태로 설정
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            
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
            // 새 게임이 아니고 저장 데이터가 있는 경우에만 로드
            if (!isNewGame && SaveManager.Instance.HasSaveData())
            {
                Debug.Log($"[SaveSystemInitializer] 저장된 게임 데이터를 로드합니다. (isNewGame: {isNewGame})");
                SaveManager.Instance.LoadGame();
            }
            else
            {
                Debug.Log($"[SaveSystemInitializer] 게임 데이터를 로드하지 않습니다. (isNewGame: {isNewGame}, HasSaveData: {SaveManager.Instance.HasSaveData()})");
            }
            // 새 게임이 시작된 후에는 플래그 초기화
            isNewGame = false;
        }
    }
    
    // 게임 시작 (새 게임)
    public void StartNewGame()
    {
        // 새 게임 플래그 설정
        isNewGame = true;
        
        // 저장 데이터 초기화
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetSaveData();
        }
        
        // 아이템 강화 수치 초기화
        ResetAllItemEnhancements();
        
        // 플레이어 스킬 언락 상태 초기화
        ResetAllPlayerSkills();
        
        // 메인 게임 씬 로드
        SceneManager.LoadScene(mainGameSceneName);
    }
    
    // 아이템 강화 수치 초기화 메서드 추가
    private void ResetAllItemEnhancements()
    {
        // 1. 아이템 템플릿 초기화 (ItemManager.ItemDatabase)
        if (ItemManager.Instance != null)
        {
            // 새로 추가한 메서드 호출
            ItemManager.Instance.ResetAllItemInstances();
        }
        else
        {
            Debug.LogWarning("ItemManager.Instance가 null입니다. 아이템 템플릿 강화 수치를 초기화할 수 없습니다.");
        }
        
        // 2. 인벤토리 내 아이템 초기화
        if (InventoryManager.Instance != null)
        {
            // 인벤토리의 모든 아이템 초기화
            foreach (var item in InventoryManager.InventoryList)
            {
                if (item != null)
                {
                    // 아이템 강화 수치 초기화
                    item.ResetEnhancement();
                    //Debug.Log($"인벤토리 아이템 '{item.id}'의 강화 수치가 초기화되었습니다.");
                }
            }
            
            // 장착 중인 아이템도 초기화
            var equippedItems = new List<Item>();
            // 여러 장비 슬롯에서 장착된 아이템을 가져옵니다
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                equippedItems.AddRange(InventoryManager.GetSlotItem(slot));
            }
            if (equippedItems != null)
            {
                foreach (var item in equippedItems)
                {
                    if (item != null)
                    {
                        // 아이템 강화 수치 초기화
                        item.ResetEnhancement();
                        //Debug.Log($"장착 중인 아이템 '{item.id}'의 강화 수치가 초기화되었습니다.");
                    }
                }
            }
            
            //Debug.Log("인벤토리 및 장착 아이템의 강화 수치가 초기화되었습니다.");
        }
        else
        {
            Debug.LogWarning("InventoryManager.Instance가 null입니다. 인벤토리 아이템 강화 수치를 초기화할 수 없습니다.");
        }
    }
    
    // 플레이어 스킬 언락 상태 초기화 메서드 추가
    private void ResetAllPlayerSkills()
    {
        if (SkillManager.Instance != null)
        {
            // 새로 추가된 메서드 호출
            SkillManager.Instance.ResetAllPlayerSkillUnlockStates();
        }
        else
        {
            Debug.LogWarning("SkillManager.Instance가 null입니다. 스킬 언락 상태를 초기화할 수 없습니다.");
        }
    }
    
    // 저장된 게임 로드
    public void LoadSavedGame()
    {
        // 새 게임이 아님을 표시
        isNewGame = false;
        Debug.Log("[SaveSystemInitializer] LoadSavedGame 호출됨 - isNewGame = false로 설정");
        
        // 메인 게임 씬 로드
        SceneManager.LoadScene(mainGameSceneName);
    }
} 
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIManager : BaseManager<UIManager>
{
    [SerializeField] private GameObject questTextPrefab;
    [SerializeField] private GameObject mainQuestTextPrefab;
    [SerializeField] private GameObject damagePopUpPrefab;
    [SerializeField] private GameObject mainTitleButton;
    [SerializeField] private GameObject baseCanvas;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject dialogWindow;
    [SerializeField] private GameObject questWindow;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject shopUI;
    [SerializeField] private GameObject interactTextUI;
    [SerializeField] private GameObject historyLog;
    [SerializeField] private GameObject historyWindow;
    
    [SerializeField] private GameObject infoMessageWindow;
    [SerializeField] private Button infoMessageCloseButton; // 04.01 테스트용

    [SerializeField] private GameObject InventorytooltipWindow;
    [SerializeField] private GameObject characterStaus;
    [SerializeField] private GameObject quickSlot;
    // ========== 250312 SH 추가 ==========
    [SerializeField] private GameObject skillQuickSlot;
    // ========== 250312 SH 추가 ==========
    [SerializeField] private Transform questListParent;
    [SerializeField] private GameObject bossHud;
    [SerializeField] private GameObject levelUpEffect;
    
    // 03.13 C
    [SerializeField] private GameObject cookingUI;
    [SerializeField] private GameObject skillUI;
    [SerializeField] private GameObject enhanceUI;

    // UI 요소를 타입별로 관리하기 위한 딕셔너리
    private Dictionary<GameSystemState, GameObject> uiStateMap;

    private PickUpItemTextDisplay pickUpItemTextDisplay;
    public GameObject DisplaySpeechWindow => dialogWindow;
    public GameObject MainTitleButton => mainTitleButton;
    public static HistoryManager HistoryManager;

    public PickUpItemTextDisplay PickUpItemTextDisplay => pickUpItemTextDisplay;
    public static InventoryUI InventoryUI;
    public static QuestUI Quest_UI;
    public static MainButtonUI MainButtonUI;
    public static InteractPopupUI InteractPopupUI;
    public static HistoryUI HistoryUI;
    public static ShopUI ShopUI;
    public static DialogUI dialogUI;
    public static HistoryWindowUI HistoryWindowUI;

    // 03.13 C
    public static CookingUI CookingUI;
    public static SkillUI SkillUI;

    // ========== 250312 SH 추가 ==========
    public static SkillQuickSlotUI SkillsQuickSlot;
    public static EnhanceUI EnhanceUI;
    // ========== 250312 SH 추가 ==========

    private Coroutine infoMessageCoroutine; // 코루틴 추가부분

    protected override void OnEnable()
    {
        base.OnEnable();
        
        // 컴포넌트 참조 초기화
        InitializeComponents();
        
        // UI 상태 맵 초기화
        InitializeUIStateMap();
        
        // 초기 UI 상태 설정
        SetInitialUIState();
    }

    private void InitializeComponents()
    {
        pickUpItemTextDisplay = GetComponent<PickUpItemTextDisplay>();
        InventoryUI = inventoryUI.GetComponent<InventoryUI>();
        Quest_UI = questWindow.GetComponent<QuestUI>();
        MainButtonUI = mainTitleButton.GetComponent<MainButtonUI>();
        InteractPopupUI = interactTextUI.GetComponent<InteractPopupUI>();
        HistoryManager = new HistoryManager();
        HistoryUI = historyLog.GetComponent<HistoryUI>();
        HistoryWindowUI = historyWindow.GetComponent<HistoryWindowUI>();
        ShopUI = shopUI.GetComponent<ShopUI>();
        dialogUI = dialogWindow.GetComponent<DialogUI>();
        SkillsQuickSlot = skillQuickSlot.GetComponent<SkillQuickSlotUI>();
        CookingUI = cookingUI.GetComponent<CookingUI>();
        SkillUI = skillUI.GetComponent<SkillUI>();
        EnhanceUI = enhanceUI.GetComponent<EnhanceUI>();
    }

    private void InitializeUIStateMap()
    {
        uiStateMap = new Dictionary<GameSystemState, GameObject>
        {
            { GameSystemState.Inventory, inventoryUI },
            { GameSystemState.QuestReview, questWindow },
            { GameSystemState.StatusUI, characterStaus },
            { GameSystemState.Shopping, shopUI },
            { GameSystemState.DialogueState, dialogWindow },
            { GameSystemState.Cook, cookingUI },
            { GameSystemState.Skill, skillUI },
            { GameSystemState.Enhance,enhanceUI}
        };
    }

    private void SetInitialUIState()
    {
        mainCanvas.SetActive(true);
        mainTitleButton.SetActive(true);
        
        // 모든 UI 요소 비활성화
        dialogWindow.SetActive(false);
        questWindow.SetActive(false);
        shopUI.SetActive(false);
        infoMessageWindow.SetActive(false);
        inventoryUI.gameObject.SetActive(false);
        historyWindow.gameObject.SetActive(false);
        InventorytooltipWindow.SetActive(false);
        characterStaus.SetActive(false);
        bossHud.SetActive(false);
        cookingUI.SetActive(false);
        skillUI.SetActive(false);
        enhanceUI.SetActive(false);
    }

    private void Update()
    {
        CheckInputStat();
    }

    private void CheckInputStat()
    {
        if (IsUIWindowOpen())
        {
            InputManager.InputActions.actions["Interact"].Disable();
        }
        else
        {
            InputManager.InputActions.actions["Interact"].Enable();
            InputManager.InputActions.actions["Move"].Enable();
        }
        
        if (infoMessageWindow.activeSelf && Input.GetMouseButtonDown(1))
        {
            ToggleinfoMessageWindow("");
            GameStateMachine.Instance.ChangeState(GameSystemState.Play);
        }
    }

    private bool IsPointerOverUI()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);
        return results.Count > 0;
    }

    // UI 창 토글 메서드 통합
    public void ToggleUIWindow(GameSystemState uiState, object additionalData = null)
    {
        if (!uiStateMap.TryGetValue(uiState, out GameObject uiWindow))
        {
            Debug.LogWarning($"UI 창을 찾을 수 없음: {uiState}");
            return;
        }

        bool activateUI = !uiWindow.activeSelf;

        // 활성화/비활성화 설정
        uiWindow.SetActive(activateUI);

        // 특정 UI에 대한 추가 처리
        if (activateUI && uiState == GameSystemState.Shopping)
        {
            NPCData npcData = additionalData as NPCData;
            if (npcData != null) ShopUI.SetShopInfo(npcData);
        }
        
        
        
        // 메인 캔버스 및 관련 UI 상태 업데이트
        UpdateMainCanvasState(activateUI, uiState);
        
        // 퀵슬롯 상태 업데이트
        UpdateQuickSlotState(activateUI, uiState);
    }

    private void UpdateMainCanvasState(bool activateUI, GameSystemState uiState)
    {
        // 대화창은 메인 캔버스와 상호 배타적
        if (uiState == GameSystemState.DialogueState)
        {
            mainCanvas.SetActive(!activateUI);
            return;
        }
        
        // 기본적으로는 메인 캔버스와 메인 버튼 UI는 UI 창과 상호 배타적
        mainCanvas.SetActive(!activateUI);
        MainButtonUI.gameObject.SetActive(!activateUI);
    }

    private void UpdateQuickSlotState(bool activateUI, GameSystemState uiState)
    {
        // 쇼핑 UI는 퀵슬롯 비활성화
        if (uiState == GameSystemState.Shopping)
        {
            quickSlot.SetActive(false);
            return;
        }
        
        // 대화창은 퀵슬롯 비활성화
        if (uiState == GameSystemState.DialogueState)
        {
            quickSlot.SetActive(false);
            return;
        }
        
        // 기본적으로는 메인 캔버스가 활성화될 때 퀵슬롯도 활성화
        quickSlot.SetActive(!activateUI ? true : false);
    }

    public void ToggleDialog(bool isOn = false)
    {
        if (isOn)
        {
            dialogWindow.SetActive(true);
            mainCanvas.SetActive(false);
            quickSlot.SetActive(false);
        }
        else
        {
            ToggleUIWindow(GameSystemState.DialogueState);
        }
    }

    public void ToggleHistorylog()
    {
        historyWindow.SetActive(!historyWindow.activeSelf);
        mainCanvas.SetActive(!mainCanvas.activeSelf);
        quickSlot.SetActive(true);
    }

    public void ToggleInventory()
    {
        ToggleUIWindow(GameSystemState.Inventory);
        skillQuickSlot.SetActive(true);
        quickSlot.SetActive(true);
    }

    public void ToggleQuestWindow()
    {
        ToggleUIWindow(GameSystemState.QuestReview);
    }

    public void ToggleStatusWindow()
    {
        ToggleUIWindow(GameSystemState.StatusUI);
    }

    public void ToggleShopWindow(NPCData nPCData)
    {
        ToggleUIWindow(GameSystemState.Shopping, nPCData);
    }

    // 03.13 C
    public void ToggleCookingUIWindow()
    {
        ToggleUIWindow(GameSystemState.Cook);
        skillQuickSlot.SetActive(false);
        quickSlot.SetActive(true);
    }

    public void ToggleEnhanceUIWindow()
    {
        ToggleUIWindow(GameSystemState.Enhance);
        skillQuickSlot.SetActive(false);
        quickSlot.SetActive(false);
    }

    public void ToggleSkillUIWindow()
    {
        ToggleUIWindow(GameSystemState.Skill);
        skillQuickSlot.SetActive(true);
        quickSlot.SetActive(true);
    }

    #region 04.01 기존 메서드
    /*public void ToggleinfoMessageWindow(string message)
    {
        bool isActive = !infoMessageWindow.gameObject.activeSelf;
        infoMessageWindow.gameObject.SetActive(isActive);

        if (isActive)
        {
            infoMessageWindow.transform.GetComponentInChildren<TextMeshProUGUI>().text = message;

            // 기존에 실행 중인 코루틴이 있으면 중지
            if (infoMessageCoroutine != null)
            {
                StopCoroutine(infoMessageCoroutine);
            }

            // 2초 후 창을 자동으로 닫는 코루틴 실행
            infoMessageCoroutine = StartCoroutine(AutoHideInfoMessage());
        }
    }*/
    #endregion

    #region 04.01 테스트용 버튼 추가 메서드
    public void ToggleinfoMessageWindow(string message)
    {
        infoMessageWindow.SetActive(true);
        infoMessageWindow.GetComponentInChildren<TextMeshProUGUI>().text = message;

        infoMessageCloseButton.onClick.RemoveAllListeners();
        infoMessageCloseButton.onClick.AddListener(() =>
        {
            infoMessageWindow.SetActive(false);
            GameStateMachine.Instance.ChangeState(GameSystemState.Exploration); // ← 이거 추가
        });
    }
    #endregion


    // 코루틴 
    private IEnumerator AutoHideInfoMessage()
    {
        yield return new WaitForSeconds(3f);
        infoMessageWindow.gameObject.SetActive(false);
    }

    public void InventoryUpdate()
    {
        if (InventoryUI != null && InventoryUI.gameObject.activeSelf)
        {
            InventoryUI.UpdateUI();
        }
    }

    public void QuestUpdate()
    {
        if (Quest_UI != null && Quest_UI.gameObject.activeSelf)
        {
            Quest_UI.UpdateUI();
        }
        QuestUI();
        InventoryUpdate();
        
        // 현재 활성화된 퀘스트의 QuestDistanceCheck 컴포넌트 업데이트
        var activeQuests = QuestManager.QuestDatabase;
        if (activeQuests.Count > 0)
        {
            // 메인 퀘스트를 우선적으로 표시
            Quest questToDisplay = activeQuests.Find(q => q.questType == "메인퀘스트");
            
            // 메인 퀘스트가 없다면 첫 번째 서브 퀘스트 표시
            if (questToDisplay == null && activeQuests.Count > 0)
            {
                questToDisplay = activeQuests[0];
            }
            
            if (questToDisplay != null)
            {
                // 현재 활성화된 퀘스트 디스플레이를 찾아 업데이트
                Transform questDisplayObj = questListParent.Find("MainQuest(Clone)");
                if (questDisplayObj != null)
                {
                    DisplayQuestTextAtPrefab(questDisplayObj.gameObject, questToDisplay);
                }
            }
        }
        // 활성 퀘스트가 없는데 UI에 MainQuest(Clone)이 있으면 제거
        else if (questListParent.childCount > 0)
        {
            foreach (Transform child in questListParent.transform)
            {
                Destroy(child.gameObject);
            }
        }
    }

    public void QuestUI()
    {
        // 퀘스트가 있는 경우에만 UI 생성
        if (QuestManager.QuestDatabase.Count > 0)
        {
            // 기존 UI 요소 모두 제거
            foreach (Transform child in questListParent.transform)
            {
                Destroy(child.gameObject);
            }

            bool isMainQuestAdded = false;
            List<Quest> completQuest = new List<Quest>();
            foreach (var quest in QuestManager.QuestDatabase)
            {
                GameObject questObjPrefab;
                if (quest.questType == "메인퀘스트")
                {
                    if (isMainQuestAdded) continue;
                    questObjPrefab = mainQuestTextPrefab;
                    isMainQuestAdded = true;
                }
                else
                {
                    questObjPrefab = questTextPrefab;
                }
                var questObj = Instantiate(questObjPrefab, questListParent);
                questObj.transform.SetSiblingIndex(quest.questType == "메인퀘스트" ? 0 : questListParent.childCount);
                DisplayQuestTextAtPrefab(questObj, quest);
                if (quest.isCompleted) completQuest.Add(quest);
            }

            foreach (var quest in completQuest)
            {
                // 메인퀘스트와 서브퀘스트 모두 완료 처리
                QuestManager.Instance.CompleteQuest(quest);
            }
        }
        // 퀘스트가 없는데 UI 요소가 있는 경우, 모두 제거
        else if (questListParent.transform.childCount > 0)
        {
            foreach (Transform child in questListParent.transform)
            {
                Destroy(child.gameObject);
            }
            Debug.Log("[UIManager] 활성 퀘스트가 없어 UI 요소를 모두 제거했습니다.");
        }
    }

    private void DisplayQuestTextAtPrefab(GameObject gameObj, Quest quest)
    {
        QuestDistanceCheck distanceCheck = gameObj.GetComponent<QuestDistanceCheck>();
        TextMeshProUGUI[] subDisplay = gameObj.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
        if (distanceCheck == null)
        {
            distanceCheck = gameObj.AddComponent<QuestDistanceCheck>();
        }
        
        // 퀘스트 지급자 정보가 없는 경우 NPC 정보 검색
        if (string.IsNullOrEmpty(quest.questGiver))
        {
            NPCData foundNPC = null;
            
            // NPC 데이터베이스에서 이 퀘스트를 제공하는 NPC 찾기
            foreach (var npc in QuestManager.NpcDatabase.subQuestNpcLists)
            {
                if (npc.relatedQuestIds != null && npc.relatedQuestIds.Contains(quest.id))
                {
                    quest.questGiver = npc.name;
                    foundNPC = npc;
                    break;
                }
                
                // quests 배열 확인
                if (npc.quests != null)
                {
                    foreach (var q in npc.quests)
                    {
                        if (q != null && q.id == quest.id)
                        {
                            quest.questGiver = npc.name;
                            foundNPC = npc;
                            break;
                        }
                    }
                    
                    if (!string.IsNullOrEmpty(quest.questGiver))
                        break;
                }
            }
            
            // 일반 NPC 목록에서도 검색
            if (string.IsNullOrEmpty(quest.questGiver))
            {
                foreach (var npc in QuestManager.NpcDatabase.npcLists)
                {
                    if (npc.relatedQuestIds != null && npc.relatedQuestIds.Contains(quest.id))
                    {
                        quest.questGiver = npc.name;
                        foundNPC = npc;
                        break;
                    }
                    
                    // quests 배열 확인
                    if (npc.quests != null)
                    {
                        foreach (var q in npc.quests)
                        {
                            if (q != null && q.id == quest.id)
                            {
                                quest.questGiver = npc.name;
                                foundNPC = npc;
                                break;
                            }
                        }
                        
                        if (!string.IsNullOrEmpty(quest.questGiver))
                            break;
                    }
                }
            }
            
            // 찾은 NPC가 있고 아직 QuestConditionPoint에 등록되지 않았다면 등록
            if (foundNPC != null && !string.IsNullOrEmpty(foundNPC.id))
            {
                if (!QuestManager.QuestConditionPoint.ContainsKey(foundNPC.id) && foundNPC.currentNPC != null)
                {
                    QuestManager.QuestConditionPoint[foundNPC.id] = foundNPC.currentNPC.transform;
                    Debug.Log($"[UIManager] 퀘스트 {quest.id}의 지급자 {foundNPC.name}(ID: {foundNPC.id})를 QuestConditionPoint에 등록했습니다.");
                }
                
                // 현재 NPC 게임오브젝트가 없으면 씬에서 찾아보기
                if (foundNPC.currentNPC == null)
                {
                    // NPC ID로 직접 찾기
                    GameObject npcObj = GameObject.Find(foundNPC.id);
                    if (npcObj != null)
                    {
                        QuestManager.QuestConditionPoint[foundNPC.id] = npcObj.transform;
                        Debug.Log($"[UIManager] GameObject.Find로 {foundNPC.name}(ID: {foundNPC.id})를 찾아 QuestConditionPoint에 등록했습니다.");
                    }
                    else
                    {
                        // NPC 이름으로 찾기
                        npcObj = GameObject.Find(foundNPC.name);
                        if (npcObj != null)
                        {
                            QuestManager.QuestConditionPoint[foundNPC.id] = npcObj.transform;
                            Debug.Log($"[UIManager] GameObject.Find로 {foundNPC.name}을 찾아 QuestConditionPoint에 등록했습니다.");
                        }
                        else
                        {
                            // 태그로 찾기
                            GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
                            foreach (var npc in npcs)
                            {
                                if (npc.name.Contains(foundNPC.name) || npc.name.Contains(foundNPC.id))
                                {
                                    QuestManager.QuestConditionPoint[foundNPC.id] = npc.transform;
                                    Debug.Log($"[UIManager] 태그를 통해 {foundNPC.name}을 찾아 QuestConditionPoint에 등록했습니다.");
                                    break;
                                }
                            }
                        }
                    }
                }
            }
        }
        
        distanceCheck.SetQuest(quest);
        if (subDisplay.Length > 0)
        {
            distanceCheck.SetUIText(subDisplay[0]);
        }
        if (quest.isCompleted && quest.questType != "메인퀘스트")
        {
            if (subDisplay.Length > 3)
                subDisplay[3].gameObject.SetActive(true);
        }
        Canvas.ForceUpdateCanvases();
    }

    public void DisplayDialogText(List<string> commentList, DialogType type)
    {
        if (dialogWindow == null) return;
        ToggleDialog(true);
        dialogUI.DisplayDialogText(commentList, type);
    }

    public void DisplayDialogWindow(NPCData nPCData)
    {
        if (dialogWindow == null) return;
        ToggleDialog(true);
        dialogUI.DisplayDialogWindow(nPCData);
    }

    public void DisplayQuestDialogWindow(string title, Quest quest)
    {
        if (dialogWindow == null) return;
        ToggleDialog(true);
        dialogUI.DisplayQuestDialogWindow(title, quest);
    }

    // 완료 가능한 퀘스트 다이얼로그를 표시하는 메서드 추가
    public void DisplayCompletableQuestDialog(NPCData npcData, Quest quest)
    {
        if (dialogWindow == null) return;
        ToggleDialog(true);
        dialogUI.DisplayCompletableQuestDialog(npcData, quest);
    }

    // [추가] 서브 퀘스트도 DialogUI를 통해 받을 수 있도록 메서드 추가
    public void OpenQuestDialogUI(NPCData npcData, Quest quest, bool isMainQuest)
    {
        if (dialogWindow == null) return;
        
        // 게임 상태를 다이얼로그 상태로 먼저 전환
        GameStateMachine.Instance.ChangeState(GameSystemState.DialogueState);
        
        // 다이얼로그 창을 항상 켜지도록 설정
        ToggleDialog(true);
        
        // 기존 DisplayQuestDialogWindow 메서드의 동작을 확장
        if (isMainQuest)
        {
            // 메인 퀘스트는 기존 방식 유지
            dialogUI.DisplayQuestDialogWindow(npcData.name, quest);
        }
        else
        {
            // 서브 퀘스트는 DialogUI에 새 메서드를 추가하여 처리할 수도 있음
            // 현재는 기존 메서드로 처리
            dialogUI.DisplayQuestDialogWindow(npcData.name, quest);
        }
    }

    public void BossHudDisplay(bool isOnOff, BossData bossData = null)
    {
        if (isOnOff)
        {
            bossHud.GetComponent<BossHudUI>().SetBossData(bossData);
        }
        bossHud.SetActive(isOnOff && bossData != null);
    }

    private void HandleQuest(Quest quest, bool state)
    {
        if (state)
        {
            if (!quest.isCompleted)
            {
                QuestManager.Instance.AddQuest(quest);
            }
            else
            {
                QuestManager.Instance.CompleteQuest(quest);
            }
        }
        dialogWindow.SetActive(false);
        mainCanvas.SetActive(true);
    }

    public void DisplayHistory()
    {
        if (historyWindow.activeSelf) HistoryWindowUI.DisplayHistory();
        else HistoryUI.DisplayHistory();
    }

    public static void SystemMessage(string msg)
    {
        HistoryManager.AddHistory(msg, MessageTag.시스템_알림, 1);
    }

    public static void SystemGameMessage(string msg, MessageTag tag)
    {
        HistoryManager.AddHistory(msg, tag, 1);
    }

    public void UIClose()
    {
        EventSystem.current.SetSelectedGameObject(null);
        mainCanvas.SetActive(true);
        quickSlot.SetActive(true);
        MainButtonUI.gameObject.SetActive(true);
        
        // 모든 UI 창 비활성화
        foreach (var uiWindow in uiStateMap.Values)
        {
            uiWindow.SetActive(false);
        }
        
        infoMessageWindow.SetActive(false);
        historyWindow.gameObject.SetActive(false);

        if (InventorytooltipWindow.activeSelf)
        {
            InventorytooltipWindow.SetActive(false);
        }
    }

    public void InteractTextPopup(string keyname, string comment, bool isOn)
    {
        InteractPopupUI.UpdateInteractText(keyname, comment, isOn);
    }

    public static void DisplayPopupText(string text, Vector3 targetPosition, MessageTag msgTag)
    {
        Vector3 finalPosition = targetPosition + new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(0f, 0.5f), Random.Range(-0.3f, 0.3f));
        var go = Instantiate(Instance.damagePopUpPrefab, finalPosition, Quaternion.identity);
        TextMeshProUGUI displayText = go.GetComponentInChildren<TextMeshProUGUI>();
        displayText.text = text;
        if (Instance.tagColors.TryGetValue(msgTag, out string colorCode))
        {
            if (ColorUtility.TryParseHtmlString(colorCode, out Color color))
            {
                displayText.color = color;
            }
        }
    }

    public void UIWindowClose()
    {
        InputManager.InputActions.enabled = !IsUIWindowOpen();
    }

    public bool IsUIWindowOpen()
    {
        // UI 상태 맵의 값들 중 하나라도 활성화되어 있는지 확인
        foreach (var uiWindow in uiStateMap.Values)
        {
            if (uiWindow.activeSelf)
                return true;
        }
       // if (shopUI.activeSelf) return true;
        // 다른 UI 창들도 확인
        return historyWindow.gameObject.activeSelf || infoMessageWindow.gameObject.activeSelf;
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        // 현재 UI 상태를 InputManager에 알려줌
        bool isUIState = InputManager.Instance.IsUIRelatedState(newState);

        // InfoMessage 상태가 아니면 모든 UI 닫기
        if (newState != GameSystemState.InfoMessage && newState != GameSystemState.InventoryChange)
            UIClose();

        // 각 상태에 따른 처리
        switch (newState)
        {
            case GameSystemState.MainMenu:
                // 이미 UIClose()에서 처리됨
                break;
            case GameSystemState.Inventory:
                ToggleInventory();
                break;
            case GameSystemState.QuestReview:
                ToggleQuestWindow();
                break;
            case GameSystemState.StatusUI:
                ToggleStatusWindow();
                break;
            case GameSystemState.Shopping:
                NPCData npcData = additionalData as NPCData;
                ToggleShopWindow(npcData);
                break;
            case GameSystemState.InfoMessage:
                string message = additionalData as string;
                ToggleinfoMessageWindow(message);
                break;
            case GameSystemState.Cook:
                ToggleCookingUIWindow();
                break;
            case GameSystemState.Skill:
                ToggleSkillUIWindow();
                break;
            case GameSystemState.Enhance:
                ToggleEnhanceUIWindow();
                break;
        }
        
        // InputManager에게 UI 상태 변경 알림
        if (isUIState)
        {
            InputManager.Instance.SetGameplayInputsEnabled(false);
        }
        else
        {
            InputManager.Instance.SetGameplayInputsEnabled(true);
        }
    }

    private readonly Dictionary<MessageTag, string> tagColors = new Dictionary<MessageTag, string>
    {
        { MessageTag.퀘스트, "#FFD700" },          // Gold
        { MessageTag.플레이어_피해, "#FF0000" },   // Red
        { MessageTag.적_피해, "#00FF00" },         // Green
        { MessageTag.기타, "#1E90FF" },            // Blue
        { MessageTag.플레이어_회복, "#32CD32" },   // Lime Green
        { MessageTag.플레이어_버프, "#87CEEB" },   // Sky Blue
        { MessageTag.플레이어_디버프, "#8B0000" }, // Dark Red
        { MessageTag.치명타, "#FF4500" },          // Orange Red
        { MessageTag.빗나감, "#708090" },          // Slate Gray
        { MessageTag.회피, "#00CED1" },            // Dark Turquoise
        { MessageTag.적_등장, "#8A2BE2" },         // Purple
        { MessageTag.적_처치, "#FFDAB9" },         // Peach
        { MessageTag.경고, "#FFA500" },            // Orange
        { MessageTag.오류, "#DC143C" },            // Crimson
        { MessageTag.시스템_알림, "#ADD8E6" },     // Light Blue
        { MessageTag.이벤트, "#FFC0CB" },          // Pink
        { MessageTag.아이템_획득, "#FFD700" },     // Gold
        { MessageTag.희귀_아이템, "#9400D3" },     // Dark Violet
        { MessageTag.금화_획득, "#FFFF00" },       // Yellow
        { MessageTag.아군_회복, "#00FA9A" },       // Medium Spring Green
        { MessageTag.아군_피해, "#FF6347" },       // Tomato Red
        { MessageTag.팀_버프, "#4169E1" },         // Royal Blue
        { MessageTag.스킬_사용, "#6495ED" },       // Cornflower Blue
        { MessageTag.마법_사용, "#9932CC" }        // Dark Orchid
    };

    public string GetColorByTag(MessageTag tag)
    {
        if (tagColors.TryGetValue(tag, out string color))
        {
            return color;
        }
        return "#FFFFFF"; // 기본 흰색
    }

    public static void LevelUpEfeect()
    {
        GameObject effect = Instantiate(Instance.levelUpEffect, GameManager.playerTransform.position, Quaternion.identity, GameManager.playerTransform);
        Destroy(effect, 3f);
    }
}

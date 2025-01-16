using System.Collections;
using TMPro;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : BaseManager<UIManager>
{
    [SerializeField] private GameObject questTextPrefab;
    [SerializeField] private GameObject mainQuestTextPrefab;
    [SerializeField] private GameObject mainTitleButton;
    [SerializeField] private GameObject baseCanvas;
    [SerializeField] private GameObject mainCanvas;
    [SerializeField] private GameObject dialogWindow;
    [SerializeField] private GameObject questWindow;
    [SerializeField] private GameObject inventoryUI;
    [SerializeField] private GameObject interactTextUI;
    [SerializeField] private Transform questListParent;
    private PickUpItemTextDisplay pickUpItemTextDisplay;
    public GameObject DisplaySpeechWindow => dialogWindow;
    public GameObject MainTitleButton => mainTitleButton;

    public PickUpItemTextDisplay PickUpItemTextDisplay => pickUpItemTextDisplay;
    public static InventoryUI InventoryUI;
    public static QuestUI Quest_UI;
    public static MainButtonUI MainButtonUI;
    public static InteractPopupUI InteractPopupUI;

    protected override void OnEnable()
    {
        base.OnEnable();
        pickUpItemTextDisplay = GetComponent<PickUpItemTextDisplay>();
        InventoryUI = inventoryUI.GetComponent<InventoryUI>();
        Quest_UI = questWindow.GetComponent<QuestUI>();
        MainButtonUI = mainTitleButton.GetComponent<MainButtonUI>();
        InteractPopupUI = interactTextUI.GetComponent<InteractPopupUI>();
        mainCanvas.SetActive(true);
        dialogWindow.SetActive(false);
        questWindow.SetActive(false);
        inventoryUI.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (IsUIWindowOpen())
        {
            InputManager.InputActions.actions["Attack"].Disable(); // UI가 열려 있을 때 공격 비활성화
            InputManager.InputActions.actions["Interact"].Disable(); // UI가 열려 있을 때 공격 비활성화
        }
        else
        {
            InputManager.InputActions.actions["Interact"].Enable(); // UI가 열려 있을 때 공격 비활성화
            InputManager.InputActions.actions["Attack"].Enable(); // UI가 닫혀 있을 때 공격 활성화
        }
        InputManager.InputActions.actions["Inventory"].Enable();
        InputManager.InputActions.actions["Quest"].Enable();


    }

    public void ToggleDialog()
    {
        dialogWindow.SetActive(!dialogWindow.activeSelf);
        mainCanvas.SetActive(!mainCanvas.activeSelf);
    }

    public void ToggleInventory()
    {
        Debug.Log("인벤 호출");
        inventoryUI.gameObject.SetActive(!inventoryUI.gameObject.activeSelf);
        mainCanvas.SetActive(!mainCanvas.activeSelf);
    }

    public void ToggleQuestWindow()
    {
        questWindow.gameObject.SetActive(!questWindow.gameObject.activeSelf);
        mainCanvas.SetActive(!mainCanvas.activeSelf);
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
    }

    public void QuestUI()
    {
        if (QuestManager.QuestDatabase.Count > 0 || QuestManager.QuestDatabase.Count != questListParent.transform.childCount)
        {
            foreach (Transform child in questListParent.transform)
            {
                Destroy(child.gameObject);
            }

            bool isMainQuestAdded = false;

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
            }
        }
    }

    private void DisplayQuestTextAtPrefab(GameObject gameObj, Quest quest)
    {
        TextMeshProUGUI[] subDisplay = gameObj.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
        string text = quest.description;
        if (text.Length > 20)
        {
            text = text.Substring(0, 20) + "...";
        }
        subDisplay[0].text = text;
        subDisplay[2].text = quest.ToStringTMPro();
        if (quest.isCompleted) { subDisplay[3].gameObject.SetActive(true); }

        //Debug.Log($"[UIManager] {text} 퀘스트 텍스트 표시");
    }

    public void DisplayDialogWindow(NPCData nPCData)
    {
        if (dialogWindow == null) return;
        ToggleDialog();
    }

    public void DisplayQuestDialogWindow(string title, Quest quest)
    {
        if (dialogWindow == null)
        {
            return;
        }

        dialogWindow.SetActive(true);
        mainCanvas.SetActive(false);

        TextMeshProUGUI[] subDisplay = dialogWindow.GetComponentsInChildren<TextMeshProUGUI>(includeInactive: true);
        Button acceptButton = dialogWindow.GetComponentInChildren<Button>(includeInactive: true);

        if (subDisplay.Length < 2)
        {
            Debug.LogError("텍스트 박스가 없습니다.");
            return;
        }

        acceptButton.onClick.RemoveAllListeners();
        subDisplay[0].text = title;

        // Check if it is a completed main quest
        bool isCompletedMainQuest = quest.questType == "main" && QuestManager.CompletedQuests.Exists(q => q.id == quest.id);

        if (isCompletedMainQuest)
        {
            SetupDialog(
                acceptButton,
                subDisplay[1],
                "닫기",
                "완료된 메인 퀘스트입니다. 더 이상 진행할 수 없습니다.",
                ToggleDialog
            );
        }
        else if (quest.isCompleted)
        {
            SetupDialog(
                acceptButton,
                subDisplay[1],
                "보상수령",
                "감사합니다! 퀘스트 완료에 따른 보상을 지급하겠습니다.",
                () => HandleQuest(quest, true)
            );
        }
        else
        {
            bool isQuestInProgress = QuestManager.QuestDatabase.Contains(quest);
            bool canAccept = !isQuestInProgress && quest.acceptCount < 3;

            SetupDialog(
                acceptButton,
                subDisplay[1],
                canAccept ? "수락" : "닫기",
                canAccept ? quest.description : "퀘스트를 완료하고 다시 찾아주세요.",
                () => HandleQuest(quest, canAccept)
            );
        }

        Debug.Log("Quest dialog setup complete.");
    }


    private void SetupDialog(Button button, TextMeshProUGUI display, string buttonText, string message, UnityEngine.Events.UnityAction onClickAction)
    {
        button.GetComponentInChildren<TextMeshProUGUI>().text = buttonText;
        button.onClick.AddListener(onClickAction);
        StartCoroutine(AnimateText(button, display, message));
    }

    private IEnumerator AnimateText(Button button, TextMeshProUGUI display, string message, float delay = 0.05f)
    {
        button.gameObject.SetActive(false);
        display.text = "";
        foreach (char c in message)
        {
            string key = c.ToString().ToUpper();
            display.text += c;
            yield return new WaitForSeconds(delay);
        }
        yield return new WaitForSeconds(0.5f);
        button.gameObject.SetActive(true);
    }

    private void HandleQuest(Quest quest, bool state)
    {
        if (state)
        {
            if (!quest.isCompleted)
            {
                Debug.LogWarning("AddQuest");
                QuestManager.Instance.AddQuest(quest);
            }
            else
            {
                Debug.LogWarning("CompleteQuest");
                QuestManager.Instance.CompleteQuest(quest);
            }
        }
        dialogWindow.SetActive(false);
        mainCanvas.SetActive(true);
    }


    public static void SystemMessage(string msg, MessageTag tag)
    {
        Debug.Log(msg);
    }

    public void UIClose()
    {
        mainCanvas.SetActive(true);
        dialogWindow.SetActive(false);
        questWindow.SetActive(false);
        inventoryUI.gameObject.SetActive(false);
    }

    public void InteractTextPopup(string keyname, string comment, bool isOn)
    {
        InteractPopupUI.UpdateInteractText(keyname, comment, isOn);
    }

    public void UIWindowClose()
    {
        InputManager.InputActions.enabled = !IsUIWindowOpen();
    }

    public bool IsUIWindowOpen()
    {
        return dialogWindow.activeSelf || questWindow.activeSelf || inventoryUI.gameObject.activeSelf;
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        UIClose();
        #region Delete

        switch (newState)
        {
            case GameSystemState.MainMenu:
                UIClose();
                break;
            case GameSystemState.Inventory:

                ToggleInventory();
                break;
            case GameSystemState.QuestReview:

                ToggleQuestWindow();
                break;
        }
        #endregion
    }
}

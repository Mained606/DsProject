using System.Collections.Generic;
using UnityEngine;

public class UIManager : BaseManager<UIManager>
{


     private Dictionary<string,MonoBehaviour> UImodules = new Dictionary<string, MonoBehaviour>();

    //    // 여기에 각 ui 추가 
       #region Plus Modules

        // hud
        public HudUI hudUI => _hudUI;
        [SerializeField] private HudUI _hudUI;

        // pop
        public InventoryUI inventoryUI => _inventoryUI;
        [SerializeField]private InventoryUI _inventoryUI;

        public SkillUI skillUI { get; private set; }

        public StatusUI statusUI => _statusUI;
        [SerializeField]private StatusUI _statusUI;

        public QuestUI questUI => _questUI;
        [SerializeField]private QuestUI _questUI;

        public OptionUI optionUI { get; private set; }

        public DialogUI DialogUI => _dialogUI;
        [SerializeField] private DialogUI _dialogUI;

        #endregion



    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        HideAllUI();

        Debug.Log($"상태 전환{newState} 확인");

        #region 테스트용 추가 스위치문 불필요시 삭제
        switch (newState)
        {
            case GameSystemState.MainMenu:
                ShowHUDUI();
                break;

            case GameSystemState.Inventory:
                ShowInventoryUI();
                break;

            case GameSystemState.QuestReview:
                ShowQuestUI();
                break;

            case GameSystemState.Stat:
                ShowStatusUI();
                break;
            default:
                Debug.Log($"Unhandled GameSystemState: {newState}");
                break;
        }
        #endregion
    }
        #region 테스트용 추가 메서드 불필요시 삭제

        private void ShowHUDUI()
        {
           hudUI.gameObject.SetActive(true);
        }
        private void ShowInventoryUI()
        {
            inventoryUI.gameObject.SetActive(true); // 인벤토리 활성화
        }
        private void ShowQuestUI()
        {
            questUI.gameObject.SetActive(true); // 퀘스트창 활성화
        }
        private void ShowStatusUI()
        {
            statusUI.gameObject.SetActive(true); // 스탯창 활성화 
        }
        private void HideAllUI()
        {
            hudUI.gameObject.SetActive(false);

            inventoryUI.gameObject.SetActive(false); // 인벤토리 비활성화
            questUI.gameObject.SetActive(false);    // 퀘스트 UI 비활성화


            skillUI?.gameObject.SetActive(false);
            statusUI?.gameObject.SetActive(false);
            optionUI?.gameObject.SetActive(false);
        }
        #endregion 

        protected override  void Start()
       {
            base.Start();

            RegisterUIManager();

          //  hudUI = GetComponentInChildren<HudUI>(true);
          //  inventoryUI = GetComponentInChildren<InventoryUI>(true);
            skillUI = GetComponentInChildren<SkillUI>();
         //   statusUI = GetComponentInChildren<StatusUI>();
          //  questUI = GetComponentInChildren<QuestUI>(true);
            optionUI = GetComponentInChildren<OptionUI>();
            //dialogUI = GetComponent<DialogUI>();
        }
        private void RegisterUIManager()
        {
            MonoBehaviour[] modules = GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var module in modules)
            {
                string moduleName = module.GetType().Name;

                if (!UImodules.ContainsKey(moduleName))
                {
                    UImodules.Add(moduleName, module);
                  //  Debug.Log($"UI Module Registered: {moduleName}");
                }
            }
        }
        #region Generic ?
    //    /*
    //    public T GetUIModule<T>() where T : MonoBehaviour
    //    {
    //        string moduleName = typeof(T).Name;
    //        if(UImodules.TryGetValue(moduleName, out MonoBehaviour module))
    //        {
    //            return module as T;
    //        }

    //        return null;
    //    }
    //*/
        #endregion
}

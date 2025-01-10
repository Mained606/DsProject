using System.Collections.Generic;
using UnityEngine;

public class UIManager : BaseManager<UIManager>
{
    
 private Dictionary<string,MonoBehaviour> UImodules = new Dictionary<string, MonoBehaviour>();

    // 여기에 각 ui 추가 
    #region Plus Modules

    public InventoryUI inventoryUI { get; private set; }
    public SkillUI skillUI { get; private set; }
    public StatusUI statusUI { get; private set; }
    public QuestUI questUI { get; private set; }
    public OptionUI optionUI { get; private set; }
    public DialogUI dialogUI { get; private set; }

    #endregion

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
      
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
   protected override  void Start()
   {
        base.Start();

            RegisterUIManager();

        inventoryUI = GetComponent<InventoryUI>();
        skillUI = GetComponent<SkillUI>();
        statusUI = GetComponent<StatusUI>();
        questUI = GetComponent<QuestUI>();
        optionUI = GetComponent<OptionUI>();
        dialogUI = GetComponent<DialogUI>();
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
                Debug.Log($"UI Module Registered: {moduleName}");
            }
        }
    }
    #region Generic ?
    /*
    public T GetUIModule<T>() where T : MonoBehaviour
    {
        string moduleName = typeof(T).Name;
        if(UImodules.TryGetValue(moduleName, out MonoBehaviour module))
        {
            return module as T;
        }

        return null;
    }
*/
    #endregion
}

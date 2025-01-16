using System.Collections.Generic;
using UnityEngine;

public class ShopManager : BaseManager<ShopManager>
{
    #region Variables
    [SerializeField] private ShopData shopData;

    private bool isPlayerInRange;
    #endregion

    protected override void Awake()
    {
        base.Awake();
        shopData.Initialize();
    }

    private void Update()
    {
        if (isPlayerInRange && InputManager.InputActions.actions["Interact"].triggered)
        {
            OpenShop();
        }
    }

    /// <summary>
    /// 상점 오픈
    /// </summary>
    private void OpenShop()
    {
        if(shopData == null || !shopData.isInteractable)
        {
            Debug.Log("상점 비활성화 상태");
        }

        //UI관련 로직
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
        }
    }


    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {

    }
}

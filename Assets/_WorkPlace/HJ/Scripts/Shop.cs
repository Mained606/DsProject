using System.Linq;
using UnityEngine;

public class Shop : MonoBehaviour
{
    #region Variables
    [SerializeField] private string shopId;
    [SerializeField] private ShopData shopData;
    [SerializeField] private float valueReductionRate = 0.7f;

    private bool isPlayerInRange = false;

    private string testItem;  //테스트 변수
    #endregion

    private void Start()
    {
        shopData = ShopManager.shopsDataList.FirstOrDefault(i => i.shopId == shopId);
        shopData.Initialize();

        //테스트용 아이템
        testItem = shopData.availableItems[0].id;
    }

    private void Update()
    {
        if (isPlayerInRange && InputManager.InputActions.actions["Interact"].triggered)
        {
            ShopManager.Instance.OpenShop(shopData);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            UIManager.Instance.InteractTextPopup("F12", shopData.shopName + "상점", true);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            UIManager.Instance.InteractTextPopup("F12", shopData.shopName + "상점", false);
            UIManager.Instance.UIClose();
        }
    }

    public void PurchaseButton()
    {
        ShopManager.Instance.PurchaseItem(testItem, shopData, 1);
    }

    public void SellItemButton()
    {
        ShopManager.Instance.SellItem(testItem, shopData, valueReductionRate, 1);
    }
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuickSlotsUI : MonoBehaviour
{
    [SerializeField] private Animator[] quickSlotsAnimator;
    [SerializeField] private Image[] quickSlotImages;
    [SerializeField] private TextMeshProUGUI[] quickSlotText;
    private PlayerData playerData;
    private Item[] quickSlotItems = new Item[2];
    private BasicTimer[] quickSlotTimer = new BasicTimer[2];
    private string[] condition = { "Consumable001", "Consumable002" };

    private void Start()
    {
        InitQuickSlotItems();
        ItemManager.Instance.AddItemLogic(condition[0], 10);
        ItemManager.Instance.AddItemLogic(condition[1], 10);
        if (playerData == null) { playerData = CharacterManager.PlayerCharacterData; }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            TryUseQuickSlot(0);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            TryUseQuickSlot(1);
        }
        CheckQuickSlotItems();
    }

    private void TryUseQuickSlot(int slotIndex)
    {
        bool isUseQuickSlot = false;
        bool isHpnMpCheck = slotIndex == 0 ? playerData.currentHp == playerData.maxHp : playerData.currentMp == playerData.maxMp;
        if (slotIndex < 0 || slotIndex >= quickSlotTimer.Length || isHpnMpCheck) return;

        string itemType = condition[slotIndex];
        if (!isUseQuickSlot && InventoryManager.Instance.GetItemQuantity(itemType) > 0)
        {
            isUseQuickSlot = true;
            UsedQuickSlot(itemType, slotIndex);
        }
        isUseQuickSlot = false;
    }

    private void UsedQuickSlot(string itemType, int slotIndex)
    {
        Debug.Log("아이템 사용하기 " + quickSlotTimer[slotIndex] == null + " , " + quickSlotTimer[slotIndex].IsRunning);
        if (quickSlotTimer[slotIndex] == null || quickSlotTimer[slotIndex].IsRunning) return;
        Debug.Log("아이템 사용하기 " + quickSlotTimer[slotIndex] == null + " , " + quickSlotTimer[slotIndex].IsRunning);
        TimerManager.Instance.StartTimer(quickSlotTimer[slotIndex]);
        ItemManager.Instance.UseItem(InventoryManager.Instance.FindInventoryItem(itemType));
    }

    private void InitQuickSlotItems()
    {
        for (int i = 0; i < quickSlotItems.Length; i++)
        {
            quickSlotItems[i] = InventoryManager.Instance.FindInventoryItem(condition[i]);
            quickSlotTimer[i] = new BasicTimer(10f);
            UpdateQuickSlotUI(i, quickSlotItems[i] != null, InventoryManager.Instance.GetItemQuantity(condition[i]), quickSlotTimer[i] == null);
        }
    }

    private void CheckQuickSlotItems()
    {
        for (int i = 0; i < quickSlotTimer.Length; i++)
        {
            bool hasItem = InventoryManager.Instance.GetItemQuantity(condition[i]) > 0;
            if (quickSlotItems[i] == null) quickSlotItems[i] = hasItem ? InventoryManager.Instance.FindInventoryItem(condition[i]) : null;
            if (quickSlotsAnimator[i] != null)
            {
                if (quickSlotTimer[i] != null && quickSlotTimer[i].IsRunning)
                {
                    quickSlotsAnimator[i].SetTrigger("Hover");
                    UpdateQuickSlotUI(i, hasItem, InventoryManager.Instance.GetItemQuantity(condition[i]), false);
                    if (!hasItem)
                    {
                        TimerManager.Instance.StopTimer(quickSlotTimer[i]);
                    }
                }
                else
                {
                    UpdateQuickSlotUI(i, hasItem, InventoryManager.Instance.GetItemQuantity(condition[i]), false);
                    quickSlotsAnimator[i].SetTrigger("Normal");
                }
            }
        }
    }

    private void UpdateQuickSlotUI(int slotIndex, bool isActive, int itemQuantity, bool initializeTimer)
    {
        if (isActive)
        {
            quickSlotImages[slotIndex].enabled = true;
            quickSlotImages[slotIndex].fillAmount = quickSlotTimer[slotIndex].IsRunning  ? quickSlotTimer[slotIndex].RemainingPercent : 0;
            quickSlotText[slotIndex].transform.parent.gameObject.SetActive(true);
            quickSlotText[slotIndex].text = itemQuantity.ToString();

            if (initializeTimer)
            {
                quickSlotTimer[slotIndex] = new BasicTimer(10f);
            }
        }
        else
        {
            quickSlotImages[slotIndex].enabled = false;
            quickSlotText[slotIndex].transform.parent.gameObject.SetActive(false);
            quickSlotText[slotIndex].text = string.Empty;
        }
    }

}

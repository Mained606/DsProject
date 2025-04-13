using UnityEngine;

public class QuickSlotsUI : MonoBehaviour
{
    [SerializeField] private DropZone[] itemSlots;

    private void Start()
    {
        InitSlots();
    }

    private void InitSlots()
    {
        foreach (var slot in itemSlots)
        {
            slot.ResetSlot();
        }
    }

    public bool HasItem(Item item)
    {
        foreach (var slot in itemSlots)
        {
            if (slot.GetItem() == item)
                return true;
        }
        return false;
    }

    public void ClearAll()
    {
        foreach (var slot in itemSlots)
        {
            slot.ResetSlot();
        }
    }

    public DropZone[] GetSlots()
    {
        return itemSlots;
    }
}

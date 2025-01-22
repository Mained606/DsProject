using UnityEngine;

public class ItemEffectTest : MonoBehaviour
{
    private void Start()
    {
        foreach (var item in ItemManager.ItemDatabase)
        {
            InventoryManager.Instance.AddItemLogic(item);
        }
    }
}

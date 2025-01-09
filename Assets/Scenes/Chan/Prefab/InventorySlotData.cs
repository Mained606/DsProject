using UnityEngine;

public class InventorySlotData : MonoBehaviour
{
    public int id { get; private set; }


    public void Init(int id)
    {
        this.id = id;
    }
}

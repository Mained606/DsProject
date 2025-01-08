using UnityEngine;

namespace HJ
{
    public class ItemDropTest : MonoBehaviour
    {
        #region Variables
        private PlayerInventory inventory;
        #endregion

        private void Start()
        {
            inventory = GetComponent<PlayerInventory>();
        }

        private void Update()
        {
            if(Input.GetKey(KeyCode.K))
            {
                DropItem(0);
            }
        }

        /// <summary>
        /// 아이템 드랍
        /// </summary>
        private void DropItem(int index)
        {
            if(inventory.items.Count > 0)
            {
                Instantiate(inventory.items[index].itemPrefab, transform.position, Quaternion.identity);
                inventory.RemoveItem(index);
            }
        }
    }
}
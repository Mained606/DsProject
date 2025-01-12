using UnityEngine;

namespace HJ
{
    public class ItemDropTest : MonoBehaviour
    {
        #region Variables
        private PlayerInventory inventory;
        //public Item item = new Item();
        #endregion

        private void Start()
        {
            inventory = GetComponent<PlayerInventory>();
        }

        private void Update()
        {
            if(Input.GetKey(KeyCode.K))
            {
                //DropItem(item);
            }
        }

        /// <summary>
        /// 아이템 드랍
        /// </summary>
        private void DropItem(Item item, int quantity = 1)
        {
            //Instantiate(item.itemPrefab, transform.position, Quaternion.identity);         
        }
    }
}
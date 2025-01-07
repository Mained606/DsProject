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
        /// 프리팹 instantiate할때 하이라키창에 있는 자기자신을 참조하는 오류 발생, 프리팹 경로를 데이터로 가져오는 방식으로 하면 오류 개선될 것으로 파악됨
        /// </summary>
        private void DropItem(int index)
        {
            if(inventory.items.Count > 0)
            {
                //Instantiate(inventory.items[index].itemPrefab, transform.position, Quaternion.identity);
                inventory.RemoveItem(index);
            }
        }
    }
}
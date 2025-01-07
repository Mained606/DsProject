using UnityEngine;

namespace HJ
{
    public class ItemObject : MonoBehaviour
    {
        #region Variables
        [SerializeField] private Transform player;                  //임시로 사용할 플레이어 변수
        [SerializeField] private Item itemData;                     //아이템 정보

        private PlayerInventory inventory;                          //인벤토리
        [SerializeField] private float detectionDistance = 1f;      //플레이어 감지 거리
        #endregion

        private void Start ()
        {
            player = TestManager.Instance.player;
            inventory = player.GetComponent<PlayerInventory>();
        }


        private void Update()
        {
            if (Input.GetKey(KeyCode.F))
            {
                GetItem();
            }
        }

        /// <summary>
        /// 플레이어가 근처에 있는지 감지
        /// 아이템 근처에 가면 UI를 띄운다거나 등등 활용
        /// </summary>
        private bool IsNearPlayer()
        {
            float disatance = Vector3.Distance(transform.position, player.position);

            return disatance <= detectionDistance;
        }

        /// <summary>
        /// 아이템 줍기
        /// </summary>
        public void GetItem()
        {
            if (inventory.items.Count >= inventory.CurrentCapacity)
                return;

            if (IsNearPlayer())
            {
                inventory.SetItem(itemData);
                Destroy(gameObject);
            }
        }
    }
}
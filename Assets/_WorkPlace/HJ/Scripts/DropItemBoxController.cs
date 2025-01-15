using UnityEngine;
using System.Collections.Generic;

namespace JWS
{
    public class DropItemBoxController : MonoBehaviour
    {
        public List<string> dropItemIds = new List<string>();
        [SerializeField] private float detectionDistance = 2f; //플레이어 감지 거리
        public bool isRandomDrop = true;

        private void Start()
        {
        }

        private void Update()
        {
            if (InputManager.InputActions.actions["Interact"].IsPressed())
            {
                OpenBox(dropItemIds);
            }
        }

        public void OpenBox(List<string> items)
        {
            if (!IsNearPlayer())
                return;

            Debug.Log("박스열기");

            if (isRandomDrop)
            {
                Debug.Log("희정이 아이템메니저 이용시");
                // //아이템 데이터 리스트에서 드랍 확률 체크 후 인벤토리에 추가
                // items.ForEach(id =>
                // {
                //     Item item = ItemManager.Instance.FindItemById(id);
                //     //아이템 랜덤 생성
                //     if (item != null && Random.value <= item.itemDropChance)
                //     {
                //         item.Initialize(); //드랍할 아이템 초기화
                //         ItemManager.Instance.AddItemToInventory(item);  //인벤토리에 아이템 추가
                //     }
                // });

            }
            else
            {
                // 추가 필요 고정 아이템
                Debug.Log("고정아이템 : " + items.Count + "아이템 아이디" + items[0]);
                foreach (var itemId in items)
                {
                    ItemManager.Instance.AddItemLogic(itemId);
                }
            }

            Destroy(gameObject); //박스 삭제
        }

        /// <summary>
        /// 플레이어가 근처에 있는지 감지
        /// </summary>
        private bool IsNearPlayer()
        {
            float disatance = Vector3.Distance(transform.position, GameManager.playerTransform.position);
            return disatance <= detectionDistance;
        }
    }
}
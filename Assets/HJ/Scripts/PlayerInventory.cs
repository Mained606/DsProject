using UnityEngine;
using System.Collections.Generic;

namespace HJ
{
    public class PlayerInventory : MonoBehaviour
    {
        #region Variables
        //아이템 수용한도
        public int CurrentCapacity { get; set; }

        //아이템 초기 수용한도
        [SerializeField] private int initialCapacity = 10;
        //최대 수용한도
        [SerializeField] private int maxCapacity = 60;

        //아이템 리스트
        public List<Item> items = new List<Item>();

        //인벤토리 UI
        #endregion

        private void Start()
        {
            CurrentCapacity = initialCapacity;      //초기 수용한도 설정
        }

        /// <summary>
        /// 인벤토리에 아이템 추가
        /// </summary>
        public bool AddItem(Item item, int quantity = 1)
        {
            //이미 소지한 아이템인지 확인
            if (IsAlreadyPossess(item))
            {
                if (IsCanStack(item))
                {
                    IncreaseItemQuantity(item, quantity);
                    return true;
                }
                return false;
            }

            //새 아이템 추가
            if (items.Count < CurrentCapacity)
            {
                AddNewItem(item,quantity);
                return true;
            }

            return false; //인벤토리가 가득 찬 경우
        }

        /// <summary>
        /// 인벤토리에서 아이템 제거
        /// </summary>
        public void RemoveItem(int index, int quantity = 1)
        {
            if (index < 0 || index >= items.Count) return;

            Item targetItem = items[index];

            if (targetItem.isCanStack && targetItem.currentQuantity > quantity)
            {
                targetItem.currentQuantity -= quantity;
            }
            else
            {
                items.RemoveAt(index);
            }
        }

        /// <summary>
        /// 인벤토리 크기 확장
        /// </summary>
        public void ExpandInventory(int expandSize)
        {
            CurrentCapacity = Mathf.Min(CurrentCapacity + expandSize, maxCapacity);
        }

        /// <summary>
        /// 인벤토리의 특정 아이템 정보 가져오기
        /// </summary>
        public Item GetItem(int index)
        {
            return index >= 0 && index < items.Count ? items[index] : null;
        }



        /// <summary>
        /// 이미 소지한 아이템인지 확인
        /// </summary>
        private bool IsAlreadyPossess(Item item)
        {
            foreach (Item possessItem in items)
            {
                if (item.itemId == possessItem.itemId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 아이템 추가할 수 있는지 확인
        /// </summary>
        private bool IsCanStack(Item item)
        {
            return item.isCanStack && item.currentQuantity < item.maxCapacity;
        }

        /// <summary>
        /// 기존 아이템 수량 증가
        /// </summary>
        private void IncreaseItemQuantity(Item item, int quantity = 1)
        {
            foreach (Item existingItem in items)
            {
                if (existingItem.itemId == item.itemId && IsCanStack(existingItem))
                {
                    Debug.Log("기존 아이템 수량 추가");
                    existingItem.currentQuantity += quantity;
                }
            }
        }

        /// <summary>
        /// 새로운 아이템 추가
        /// </summary>
        private void AddNewItem(Item item, int quantity = 1)
        {
            Debug.Log("새로운 아이템 추가");
            item.currentQuantity = Mathf.Min(quantity, item.maxCapacity);
            items.Add(item);
        }
    }
}
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
            if (quantity > item.maxCapacity)
                return false;

            //이미 소지한 아이템인지 확인 및 수량 증가
            Item existingItem = items.Find(c => c.itemId == item.itemId);
            if (existingItem != null && item.isCanStack)
            {
                if (existingItem.currentQuantity >= existingItem.maxCapacity)
                {
                    Debug.Log("기존 아이템 수량 초과");

                    return false;
                }

                Debug.Log("기존 아이템 수량 추가");
                existingItem.currentQuantity = Mathf.Min(existingItem.currentQuantity + quantity, existingItem.maxCapacity);
                return true;
            }

            //새 아이템 추가
            if (items.Count < CurrentCapacity)
            {
                Debug.Log("새로운 아이템 추가");
                item.currentQuantity = quantity;
                items.Add(item);
                return true;
            }

            return false; //인벤토리가 가득 찬 경우
        }

        /// <summary>
        /// 인벤토리에서 아이템 제거
        /// </summary>
        public void RemoveItem(Item item, int quantity = 1)
        {
            Item existingItem = items.Find(c => c.itemId == item.itemId);
            if (existingItem == null)
                return;

            if(existingItem.isCanStack && existingItem.currentQuantity > quantity)
            {
                int currentAmount = existingItem.currentQuantity;
                item.currentQuantity = Mathf.Max(0, currentAmount - quantity);
                return;
            }

            items.Remove(existingItem);
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
        /// 아이템 ID로 아이템 찾기
        /// </summary>
        //private Item FindItemById(int itemId)
        //{
        //    foreach (Item possessItem in items)
        //    {
        //        if (possessItem.itemId == itemId)
        //        {
        //            return possessItem;
        //        }
        //    }
        //    return null;
        //}

        /// <summary>
        /// 기존 아이템 수량 증가
        /// </summary>
        //private bool IncreaseItemQuantity(Item existingItem, int quantity = 1)
        //{
        //    if (existingItem.isCanStack && existingItem.currentQuantity < existingItem.maxCapacity)
        //    {
        //        Debug.Log("기존 아이템 수량 추가");

        //        //추가 가능한 최대 수량 계산
        //        int availableSpace = existingItem.maxCapacity - existingItem.currentQuantity;
        //        int amountToAdd = Mathf.Min(availableSpace, quantity);

        //        existingItem.currentQuantity += amountToAdd;

        //        return true;
        //    }
        //    return false;
        //}

        /// <summary>
        /// 새로운 아이템 추가
        /// </summary>
        //private bool AddNewItem(Item item, int quantity = 1)
        //{
        //    Debug.Log("새로운 아이템 추가");
            
        //    item.currentQuantity = Mathf.Min(quantity, item.maxCapacity);
        //    items.Add(item);

        //    return true;
        //}
    }
}
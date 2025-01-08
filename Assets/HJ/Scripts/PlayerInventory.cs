using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
        public List<Item> items;

        //인벤토리 UI
        #endregion

        private void Start()
        {
            items = new List<Item>();

            CurrentCapacity = initialCapacity;      //초기 수용한도 설정
        }

        /// <summary>
        /// 아이템 여러개 소지 가능한지 여부
        /// </summary>
        private bool IsCanStack(Item item)
        {
            return item.isCanStack && item.currentQuantity < item.maxCapacity;
        }

        /// <summary>
        /// 이미 소지한 아이템인지 확인
        /// </summary>
        private bool IsAlreadyPosses(Item item)
        {
            foreach(Item possesItem in items)
            {
                if(item.itemId == possesItem.itemId)
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 인벤토리 아이템 셋팅
        /// </summary>
        public void SetItem(Item item, int quantity = 1)
        {
            //인벤토리 수용량보다 적은지 확인
            if(items.Count < CurrentCapacity)
            {
                //이미 보유하고 있고 여러개 소지할 수 있는 아이템이면 수량만 추가
                if(IsAlreadyPosses(item) && IsCanStack(item))
                {
                    Debug.Log("이미 소지한 아이템 습득");

                    Item existingItem = items.FirstOrDefault(i => i.itemId == item.itemId);
                    if (existingItem != null)
                    {
                        existingItem.currentQuantity += quantity;
                    }
                }
                else
                {
                    Debug.Log("새로운 아이템 습득");
                    items.Add(item);
                    item.currentQuantity++;
                }
            }
        }

        /// <summary>
        /// 아이템 제거? 버리기?
        /// </summary>
        public void RemoveItem(int index, int quantity = 1)
        {
            if (items.Count > 0)
            {
                if (items[index].isCanStack && items[index].currentQuantity > 1)
                {
                    items[index].currentQuantity -= quantity;
                }
                else
                {
                    items.RemoveAt(index);
                }
            }
        }

        /// <summary>
        /// 아이템 정보 가져오기
        /// </summary>
        public Item GetItem(int index)
        {
            return items.Count > 0 ? items[index] : null;
        }

        /// <summary>
        /// 인벤토리 크기 확장
        /// </summary>
        public void ExpandInventory(int expandSize)
        {
            if(CurrentCapacity + expandSize <= maxCapacity)
            {
                CurrentCapacity += expandSize;
            }
        }
    }
}
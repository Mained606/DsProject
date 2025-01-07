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
        public List<Item> items;

        //인벤토리 UI
        #endregion

        private void Start()
        {
            items = new List<Item>();

            CurrentCapacity = initialCapacity;      //초기 수용한도 설정
        }

        ////아이템 수용 범위 검사
        //private bool IsValidIndex(int index)
        //{
        //    return index >= 0 && index <= CurrentCapacity;
        //}

        ////비어있는 슬롯 앞에서부터 찾기 (잘못된 인덱스는 -1)
        //private int FindEmptySlot()
        //{
        //    for(int i = 0; i < CurrentCapacity; i++)
        //    {
        //        if (items[i] == null)
        //        {
        //            Debug.Log($"{i}번째 슬롯이 비어있음");
        //            return i;
        //        }
        //    }

        //    Debug.Log("빈 슬롯 없음");
        //    return -1;
        //}

        ///// <summary>
        ///// 해당 슬롯의 아이템 여부 확인
        ///// 인벤토리 내에서 아이템 옮길 때, 버릴 때 등 활용 예정
        ///// </summary>
        //public bool HasItem(int index)
        //{
        //   return IsValidIndex(index) && items[index] != null;
        //}




        /// <summary>
        /// 인벤토리 아이템 셋팅
        /// 아이템 습득시 활용
        /// </summary>
        public void SetItem(Item item)
        {
            //int emptySlot = FindEmptySlot();
            if (items.Count < CurrentCapacity)
            {
                items.Add(item);
            }
        }

        /// <summary>
        /// 아이템 제거? 버리기?
        /// </summary>
        public void RemoveItem(int index)
        {
            if (items.Count > 0)
            {
                items.RemoveAt(index);
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
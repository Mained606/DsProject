using UnityEngine;

namespace HJ
{
    public class PlayerInventory : MonoBehaviour
    {
        #region Variables
        //아이템 수용한도
        public int Capacity { get; set; }

        //아이템 초기 수용한도
        [SerializeField] private int initialCapacity = 10;
        //최대 수용한도
        [SerializeField] private int maxCapacity = 64;

        //아이템 배열
        private Item[] items;

        //인벤토리 UI
        #endregion

        //초기화
        private void Start()
        {
            items = new Item[maxCapacity];  //최대 수용량으로 배열 설정
            Capacity = initialCapacity;     //초기 수용한도 설정
        }

        //아이템 수용 범위 검사
        private bool IsValidIndex(int index)
        {
            return index >= 0 && index <= Capacity;
        }

        //비어있는 슬롯 앞에서부터 찾기 (잘못된 인덱스는 -1 리턴)
        private int FindEmptySlot(int startIndex = 0)
        {
            for(int i = startIndex; i < Capacity; i++)
            {
                if(items[i] == null)
                    return i;
            }

            return -1;
        }

        //해당 슬롯에 아이템이 있는지 여부 확인
        private bool HasItem(int index)
        {
           return IsValidIndex(index) && items[index] != null;
        }

        //인벤토리에 아이템 셋팅
        public void SetItem(Item item)
        {

        }
    }
}
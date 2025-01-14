using UnityEngine;
using System.Collections.Generic;

public class DropItemBoxController : MonoBehaviour
{
    [SerializeField] private List<Item> dropItmes = new List<Item>();    //드랍되는 아이템 리스트


    /// <summary>
    /// 아이템 리스트 박스에 설정하기
    /// </summary>
    public void SetItems(List<int> items)
    {
        List<int> itemIds = items;

        itemIds.ForEach(id =>
        {
            Item item = ItemManager.Instance.FindItemById(id);
            if (item != null)
            {
                //아이템 랜덤 드랍
                if(Random.value <= item.itemDropChance)
                {
                    item.Initialize();      //드랍할 아이템 초기화
                    dropItmes.Add(item);    //드랍할 아이템 추가
                }
            }
        });
    }

    /// <summary>
    /// 박스 열기
    /// </summary>
    public bool OpenBox()
    {
        dropItmes.ForEach(item =>
        {
            
        });


        return true;
    }

    /// <summary>
    /// 상자 삭제
    /// </summary>
    private void DestroyBox()
    {
        //상자가 열린적이 있고 상자의 아이템이 모두 비워져 있으면
        if (OpenBox() && dropItmes == null)
        {
            Destroy(gameObject);
        }
    }
}

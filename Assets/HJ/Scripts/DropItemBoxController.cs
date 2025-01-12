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
                //아이템 드랍 확률
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
    /// UI 관련 로직 추가 예정
    /// </summary>
    public void OpenBox()
    {
        //UI

        //박스가 비워지면 삭제하기
        if(dropItmes == null)
        {
            Destroy(gameObject);
        }
    }
}

using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public List<Item> items = new List<Item>();

    //모든 아이템 초기화
    public void InitializeAllItems()
    {
        items.ForEach(item =>
        {
            item.Initialize();
        });
    }
}

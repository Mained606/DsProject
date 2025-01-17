using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class ItemEffectData
{
    public string itemId;
    public Item item;

    public void Initialize()
    {
        if(!string.IsNullOrEmpty(itemId))
        {
            item = ItemManager.Instance.GetItemById(itemId);
        }
    }
}

public class ItemEffectManager : MonoBehaviour
{
    #region Variables
    [SerializeField] private List<ItemEffectData> itemEffectDatas = new List<ItemEffectData>();
    #endregion

    private void Start()
    {
        itemEffectDatas[0].Initialize();
    }
    //이펙트 데이터의 아이디가 아이템 데이터베이스에 있는지 확인
}

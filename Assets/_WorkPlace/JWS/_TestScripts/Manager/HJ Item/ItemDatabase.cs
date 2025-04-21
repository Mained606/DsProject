using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


/// <summary>
/// 01.23 희정 아이템 에디터에 아이템 초기화 버튼을 추가하기 위해서 아이템 초기화 함수 추가
/// 01.24 전체 이펙트 초기화 함수 추가
/// </summary>
[CreateAssetMenu(fileName = "ItemList", menuName = "Ds Project/ItemList")]
public class ItemList : ScriptableObject
{
    public List<Item> itemList = new List<Item>();

    //public void InitGrade()
    //{
    //    foreach (var item in itemList)
    //    {
    //        if (item.type == ItemType.무기 || item.type == ItemType.장신구)
    //            continue;

    //        item.grade = ItemGrade.일반;
    //    }
    //}

    //아이템 초기화 함수
    public void InitializeItem(int index)
    {
        if (index >= 0 && index < itemList.Count)
        {
            Item item = itemList[index];

            item.effect.Initialize(item);

            item.quantity = 1;
            item.maxStack = 1;
            item.isDiscardable = true;
            item.isStackable = false;

            // 아이템 초기화 로직
            if (item.type == ItemType.무기 || item.type == ItemType.방어구)
            {
                item.itemStat = new ItemStat(1, 1, 1, 1); //기본 스탯
                item.durability = new Durability(100);       //기본 내구도
            }
            else if (item.type == ItemType.소모품 || item.type == ItemType.제작재료)
            {
                item.maxStack = 99;  //소모품 기본 최대 스택
                item.isStackable = true;
                item.grade = ItemGrade.일반;
                if (item.type == ItemType.소모품)
                {
                    item.effect.itemSize = ItemSize.Small;
                    item.effect.Initialize(item);
                }                
            }
            else if (item.type == ItemType.퀘스트)
            {
                item.isDiscardable = false; //퀘스트 아이템은 버릴 수 없음
                item.isQuestItem = true;
            }
            else if (item.type == ItemType.요리)
            {
                item.maxStack = 1;
                // item.isStackable = false; // 중첩 가능 여부는 인스펙터 설정을 유지하도록 주석 처리
                item.effect.Initialize(item);
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(this); //변경 사항을 반영
#endif
        }
    }

    public void InitializeAllEffects()
    {
        foreach(var item in itemList)
        {
            item.effect.Initialize(item);
        }
    }
    
    private void OnDisable()
    {
#if UNITY_EDITOR
        // 게임 플레이 중이 아닐 때만 저장
        if (!EditorApplication.isPlaying)
        {
            SaveAsset(); // 에디터에서만 저장
        }
#endif
    }

    public void SaveAsset()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        //Debug.Log($"{name} 자동 저장됨.");
#endif
    }
}



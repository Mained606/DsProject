using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

[CreateAssetMenu(fileName = "CharacterList", menuName = "Ds Project/Character List")]
public class CharacterList : ScriptableObject
{
    public List<PlayerData> players = new List<PlayerData>();
    public List<MonsterData> monsters = new List<MonsterData>();
    public List<DragonData> dragons = new List<DragonData>(); // 드래곤 데이터 리스트

    private void OnEnable()
    {
        GameManager.RegistAsset(this); // 중앙 관리 등록
    }

    private void OnDisable()
    {
        SaveAsset(); // 자동 저장
    }

    public void SaveAsset()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        Debug.Log($"{name} 자동 저장됨.");
#endif
    }
}

using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnnerList", menuName = "Ds Project/Spawnner List", order = 1)]
public class SpawnnerList : ScriptableObject
{
    public List<SpawnData> spawnMonsterDataLists = new List<SpawnData>();
    public List<SpawnData> spawnNPCDataLists = new List<SpawnData>();
    public List<SpawnData> spawnBossDataLists = new List<SpawnData>();

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
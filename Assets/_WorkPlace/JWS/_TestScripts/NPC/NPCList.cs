using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCList", menuName = "Ds Project/NPC List")]
public class NPCList : ScriptableObject
{
    public List<NPCData> mainQuestNpcLists; // 여러 NPC 데이터를 관리하는 리스트
    public List<NPCData> shopNpcLists; // 여러 NPC 데이터를 관리하는 리스트
    public List<NPCData> npcLists; // 여러 NPC 데이터를 관리하는 리스트

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
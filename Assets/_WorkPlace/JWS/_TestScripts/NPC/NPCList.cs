using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "NPCList", menuName = "Ds Project/NPC List")]
public class NPCList : ScriptableObject
{
    public List<NPCData> mainQuestNpcLists; // 메인 퀘스트 NPC 데이터를 관리하는 리스트
    public List<NPCData> subQuestNpcLists; // 서브 퀘스트 NPC 데이터를 관리하는 리스트
    public List<NPCData> shopNpcLists; // 상점 NPC 데이터를 관리하는 리스트
    public List<NPCData> npcLists; // 일반 NPC 데이터를 관리하는 리스트

    private void OnDisable()
    {
        SaveAsset(); // 자동 저장
    }

    public void SaveAsset()
    {
#if UNITY_EDITOR
        EditorUtility.SetDirty(this);
        AssetDatabase.SaveAssets();
        // //Debug.Log($"{name} 자동 저장됨.");
#endif
    }
}
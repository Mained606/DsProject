using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestList", menuName = "Ds Project/QuestList")]
public class QuestList : ScriptableObject
{
    public List<Quest> mainQuestList = new List<Quest>();
    public List<Quest> questList = new List<Quest>();

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
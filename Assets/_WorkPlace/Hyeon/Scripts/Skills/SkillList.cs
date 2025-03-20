using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillList", menuName = "Ds Project/Skill")]
public class SkillList : ScriptableObject
{
    public List<Skills> playerSkills;
    public List<Skills> dragonSkills;
    public List<Skills> bossSkills;
    
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
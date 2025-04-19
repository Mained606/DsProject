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
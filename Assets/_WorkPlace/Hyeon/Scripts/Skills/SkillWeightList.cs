using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillWeightList", menuName = "Ds Project/SkillWeight")]
public class SkillWeightList : ScriptableObject
{
    public List<SkillWeights> playerSkillWeights;
    public List<SkillWeights> dragonSkillWeights;
    public List<SkillWeights> bossSkillWeights;

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
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
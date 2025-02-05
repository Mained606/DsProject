using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "CameraPoseSetting", menuName = "Ds Project/CameraPose")]
public class CameraPoseSetting : ScriptableObject
{
    public List<CameraPose> poseList;
    public List<CameraPoseList> poseTransitionList;
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
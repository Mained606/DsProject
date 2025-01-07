using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class GameStateData
{
    public List<GameStateInfo> States;
}

[Serializable]
public class GameStateInfo
{
    public string StateName;
    public string TransitionCondition;
    public List<SystemLogic> SystemLogics;
}

[Serializable]
public class SystemLogic
{
    public string SystemName; // 실행할 시스템 이름
    public string Logic;      // 실행할 메서드 이름
}

public static class GameStateDataLoader
{
    public static GameStateData LoadGameStateData(string filePath)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError($"JSON 파일을 찾을 수 없습니다: {filePath}");
            return null;
        }

        string json = File.ReadAllText(filePath); // JSON 파일 읽기
        return JsonUtility.FromJson<GameStateData>(json); // JSON -> GameStateData 변환
    }
}
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class SpawnData : ISheetData
{
    public string spwanName;                // 스폰 데이터 이름
    public SpawnnerType spwanType;          // 스폰 타입
    public SpawnStyle spawnStyle;           // 스폰 스타일
    public Vector2 spaawnSize;              // 스폰 범위
    public Vector3 spawnPosition;        // 스폰 위치
    public List<string> spawnObjects;   // 스폰 대상 프리팹 리스트
    public bool autoSpawn = true;           // 자동 스폰 여부
    public float initialDelay = 60f;         // 재 스폰 딜레이
    public float spawnInterval = 0.05f;        // 스폰 주기
    public int maxSpawnCount = 10;           // 최대 스폰 개수
    public float detectionDistance = 100;    // 감지거리
    public TriggerType triggerType;         // 스폰 트리거조건
    public Func<bool> triggerCondition;     // 스폰 조건 (함수)
    public int activeObjectCount;

    public void ParseData(IList<object> row)
    {
        if (row.Count < 10) throw new Exception("스폰 데이터 부족");

        spwanName = row[0].ToString();
        spwanType = Enum.TryParse(row[1].ToString(), out SpawnnerType parsedType) ? parsedType : SpawnnerType.None;
        spawnStyle = Enum.TryParse(row[2].ToString(), out SpawnStyle parsedStyle) ? parsedStyle : SpawnStyle.CircleArea;

        // 스폰 범위 (x,y 값이 쉼표로 구분된 문자열)
        string[] sizeData = row[3].ToString().Split(',');
        if (sizeData.Length >= 2)
        {
            float.TryParse(sizeData[0], out float width);
            float.TryParse(sizeData[1], out float height);
            spaawnSize = new Vector2(width, height);
        }

        // 스폰 위치 (x,y,z 값이 쉼표로 구분된 문자열)
        string[] positionData = row[4].ToString().Split(',');
        if (positionData.Length >= 3)
        {
            float.TryParse(positionData[0], out float x);
            float.TryParse(positionData[1], out float y);
            float.TryParse(positionData[2], out float z);
            spawnPosition = new Vector3(x, y, z);
        }

        // 스폰할 오브젝트 목록 (쉼표로 구분된 리스트)
        spawnObjects = new List<string>(row[5].ToString().Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries));

        autoSpawn = bool.TryParse(row[6].ToString(), out bool auto) ? auto : true;
        initialDelay = float.TryParse(row[7].ToString(), out float delay) ? delay : 60f;
        spawnInterval = float.TryParse(row[8].ToString(), out float interval) ? interval : 0.05f;
        maxSpawnCount = int.TryParse(row[9].ToString(), out int maxCount) ? maxCount : 10;
        detectionDistance = float.TryParse(row[10].ToString(), out float distance) ? distance : 100f;

        // 트리거 타입 변환
        triggerType = Enum.TryParse(row[11].ToString(), out TriggerType parsedTrigger) ? parsedTrigger : TriggerType.Distance;

        // 기본적으로 트리거 조건을 null로 설정 (게임 내에서 조건 할당 필요)
        triggerCondition = null;

        Debug.Log($"[SpawnData] {spwanName} 데이터 로드 완료!");
    }
}

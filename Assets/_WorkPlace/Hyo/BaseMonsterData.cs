using System;
using UnityEngine;

public class BaseMonsterData : MonoBehaviour
{
    // 하나의 퍼블릭 변수만 두고, 타입에 따라 설정
    public object monsterOrBossData;
    public SpawnnerType currentType;

    // 데이터를 설정하는 메서드
    public void SetData(SpawnnerType type, object data)
    {
        monsterOrBossData = data;
        currentType = type;
    }

    // 일반 몬스터 데이터에 접근하는 메서드
    public MonsterData GetMonsterData()
    {
        return monsterOrBossData as MonsterData;  // MonsterData로 캐스팅
    }

    // 보스 데이터에 접근하는 메서드
    public BossData GetBossData()
    {
        return monsterOrBossData as BossData;  // BossData로 캐스팅
    }
}
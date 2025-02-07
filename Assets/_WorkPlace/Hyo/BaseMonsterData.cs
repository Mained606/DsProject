using System;
using UnityEngine;

public class BaseMonsterData : MonoBehaviour
{
    // 하나의 퍼블릭 변수만 두고, 타입에 따라 설정
    public object monsterOrBossData;

    // 데이터를 설정하는 메서드
    public void SetData(SpawnnerType type, object data)
    {
        if (type == SpawnnerType.Monster)
        {
            // 일반 몬스터 데이터 설정
            monsterOrBossData = (MonsterData)data;
        }
        else if (type == SpawnnerType.Boss)
        {
            // 보스 데이터 설정
            monsterOrBossData = (BossData)data;
        }
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
using System;
using UnityEngine;

public class BaseMonsterData : MonoBehaviour
{
    public MonsterData monster;
    public BossData bossData;

    public void SetData(SpawnnerType type, object data)
    {
        if (type == SpawnnerType.Monster)
        {
            monster = (MonsterData)data;
        }
        else if (type == SpawnnerType.Boss)
        {
            bossData = (BossData)data;
        }
    }
}

using System;
using UnityEngine;

[Serializable]
public class SkillWeights
{
    public EntityType entityType;
    public string skillName;
    public float damageIncrease;
    public float cooldownDecrease;
    public float energyCostIncrease;
    public GameObject fullLevelEffectPrefab;

    public float buffDurationIncrease;
    public float buffValueIncrease;

    public float debuffDurationIncrease;
    public float debuffValueIncrease;

    public float effectDurationIncrease;

    public SkillWeights()
    {
    }
}

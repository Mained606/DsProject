using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SkillList", menuName = "Ds Project/Skill")]
public class SkillList : ScriptableObject
{
    public List<Skills> playerSkills;
    public List<Skills> dragonSkills;
    public List<Skills> bossSkills;
}
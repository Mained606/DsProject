using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SpawnnerList", menuName = "Ds Project/Spawnner List", order = 1)]
public class SpawnnerList : ScriptableObject
{
    public List<SpawnData> spawnDataLists = new List<SpawnData>();
}
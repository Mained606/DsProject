using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterList", menuName = "Ds Project/Character List")]
public class CharacterList : ScriptableObject
{
    public List<CharacterData> characters = new List<CharacterData>();
}

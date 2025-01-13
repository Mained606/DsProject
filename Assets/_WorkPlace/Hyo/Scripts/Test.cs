using UnityEngine;

public class Test : MonoBehaviour
{
    public CharacterList characterList;
    public CharacterData[] characterData;

    void OnValidate()
    {
        characterData = new CharacterData[3];
        
        characterData[0] = characterList.characters[0];
        characterData[1] = characterList.characters[1];
        characterData[2] = characterList.characters[2];
    }
}

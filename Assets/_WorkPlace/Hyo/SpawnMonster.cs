using UnityEngine;

public class SpawnMonster : MonoBehaviour
{
    private void Start()
    {
        Spawn();
    }

    private void Update()
    {

    }

    public void Spawn()
    {
        CharacterManager.Instance.SpawnMonsters("Mushroom", 5, new Vector3(15, 1, 0), 5f);
        CharacterManager.Instance.SpawnMonsters("Slime", 2, new Vector3(5, 1, 0), 5f);
        
    }
}

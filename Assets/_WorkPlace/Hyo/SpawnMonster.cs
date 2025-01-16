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
        CharacterManager.Instance.SpawnMonster("Slime", new Vector3(0, 1, 0));
        CharacterManager.Instance.SpawnMonster("Mushroom", new Vector3(4, 1, 0));

    }
}

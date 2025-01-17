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
        CharacterManager.Instance.SpawnMonster("Mushroom", new Vector3(15, 1, 0));
        CharacterManager.Instance.SpawnMonster("Mushroom", new Vector3(18, 1, 0));
        CharacterManager.Instance.SpawnMonster("Mushroom", new Vector3(20, 1, 0));

    }
}

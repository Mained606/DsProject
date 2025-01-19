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
        CharacterManager.Instance.SpawnMonster("Mushroom", new Vector3(
            transform.position.x + Random.Range(-10, 10),
            transform.position.y, 
            transform.position.z + Random.Range(-10, 10))
        );
        CharacterManager.Instance.SpawnMonster("Mushroom", new Vector3(
            transform.position.x + Random.Range(-10, 10),
            transform.position.y,
            transform.position.z + Random.Range(-10, 10))
        );
        CharacterManager.Instance.SpawnMonster("Mushroom", new Vector3(
            transform.position.x + Random.Range(-10, 10),
            transform.position.y,
            transform.position.z + Random.Range(-10, 10))
        );
        CharacterManager.Instance.SpawnMonster("Mushroom", new Vector3(
            transform.position.x + Random.Range(-10, 10),
            transform.position.y,
            transform.position.z + Random.Range(-10, 10))
        );
    }
}

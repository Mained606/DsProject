using UnityEditor.SceneManagement;
using UnityEngine;

public class Test : MonoBehaviour
{
    private BoxCollider boxCollider;
    private NPCSpawner npcSpawner;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        boxCollider = GetComponent<BoxCollider>();
        npcSpawner = GetComponent<NPCSpawner>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {

            npcSpawner.enabled = true;
            
            boxCollider.enabled = false;
        }
    }
}

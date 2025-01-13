using UnityEngine;

public class NPCController : MonoBehaviour
{
    public float interactionRange = 3f; // Range within which interaction is possible
    private Transform playerTransform; // Reference to the player
    private bool isPlayerInRange = false;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player 태그가 없어용");
        }
    }
    void Update()
    {
        if (playerTransform != null)
        {
            float distance = Vector3.Distance(playerTransform.position, transform.position);
            isPlayerInRange = distance <= interactionRange;
            Debug.Log(isPlayerInRange);
            if (isPlayerInRange && InputManager.InputActions.actions["Jump"].triggered)
            {
                Interact();
            }
        }
    }

    void Interact()
    {
        Debug.Log("인터렉트됨");
        // Add your interaction logic here
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a sphere in the editor to visualize the interaction range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

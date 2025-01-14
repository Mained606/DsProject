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
            if (isPlayerInRange && InputManager.InputActions.actions["Interact"].triggered)
            {
                Interact();
            }
        }
    }

    public void Interact()
    {
        Debug.Log("d");
        /* if (!npcData.isInteractable)
        {
            Debug.Log($"{npcData.name}은(는) 상호작용이 불가능합니다.");
            return;
        }

        switch (npcData.npcType)
        {
            case NPCType.상점:
                OpenShop();
                break;
            case NPCType.퀘스트:
                GiveQuest();
                break;
            default:
                HandleState();
                break;
        } */
    }

    private void OnDrawGizmosSelected()
    {
        // Draw a sphere in the editor to visualize the interaction range
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, interactionRange);
    }
}

using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class DropItemEggStrawBerry : MonoBehaviour
{
    public List<string> dropItemIds = new List<string>();
    public GameObject effect;
    public string questId;
    [SerializeField] private float detectionDistance = 2f;
    [SerializeField] private string itemName = "용의알";
    [SerializeField] private int itemAmount = 1;

    private void Start()
    {
        if(effect != null)
            effect.SetActive(false);
    }

    private void Update()
    {
        if (InputManager.InputActions.actions["Interact"].triggered)
        {
            OpenBox(itemName);
        }

        if (effect == null || string.IsNullOrEmpty(questId)) return;

        if (QuestManager.QuestDatabase.Any(q => q.id == questId))
        {
            effect.SetActive(true);
        }
        else if(effect.activeSelf)
        {
            effect.SetActive(false);
        }
    }

    public void OpenBox(string name)
    {
        if (!IsNearPlayer())
            return;

        ItemManager.Instance.AddItemLogic(name, itemAmount);
        Destroy(gameObject);
    }

    private bool IsNearPlayer()
    {
        float disatance = Vector3.Distance(transform.position, GameManager.playerTransform.position);
        return disatance <= detectionDistance;
    }
}

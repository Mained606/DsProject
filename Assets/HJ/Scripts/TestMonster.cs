using System.Collections.Generic;
using UnityEngine;

public class TestMonster : MonoBehaviour
{
    [SerializeField] private List<int> dropItemIds = new List<int>();
    private DropItemBoxController dropItemBoxController;


    private void Start()
    {
        Die();
    }

    private void Update()
    {
        if (InputManager.InputActions.actions["Interact"].IsPressed())
        {
            if (dropItemBoxController)
                dropItemBoxController.OpenBox(dropItemIds);
        }
    }


    private void Die()
    {
        GameObject itemBox = ItemManager.Instance.SpawnItemBox(transform.position);
        dropItemBoxController = itemBox.GetComponent<DropItemBoxController>();
    }
}

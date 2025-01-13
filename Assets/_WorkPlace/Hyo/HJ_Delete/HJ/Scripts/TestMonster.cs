using System.Collections.Generic;
using UnityEngine;

public class TestMonster : MonoBehaviour
{
    [SerializeField] private List<int> dropItemIds = new List<int>();
    private DropItemBoxController dropItemBoxController;


    private void Start()
    {
        Die(true);
    }

    private void Update()
    {
        if (InputManager.InputActions.actions["Interact"].IsPressed())
        {
            if (dropItemBoxController)
                dropItemBoxController.OpenBox(dropItemIds, false);
        }
    }


    private void Die(bool flag)
    {
        GameObject itemBox = ItemManager.Instance.SpawnItemBox(transform.position);
        itemBox.GetComponent<DropItemBoxController>().isRandomDrop = flag;
   
        dropItemBoxController = itemBox.GetComponent<DropItemBoxController>();
    }
}
using UnityEngine;

public class TestCraft : MonoBehaviour
{
    [SerializeField] private string test = "Weapon001";
    [SerializeField] private int index = 0;

    private void Update()
    {
        if (InputManager.InputActions.actions["ESC"].triggered)
        {
            ItemManager.Instance.AddItemLogic(test, 99);
            Debug.Log("ESC");
        }

        if (InputManager.InputActions.actions["Interact"].triggered)
        {
            //CookingManager.Instance.AddIngredient(InventoryManager.InventoryList[index]);
            Debug.Log(InventoryManager.Instance.selectedItem.name);
        }

        if (InputManager.InputActions.actions["SkillUI"].triggered)
        {
            CookingManager.Instance.Craft();
        }

        if (InputManager.InputActions.actions["Jump"].triggered)
        {
            ItemEffectManager.Instance.ApplyItemEffect(InventoryManager.InventoryList[index]);
        }
    }
}

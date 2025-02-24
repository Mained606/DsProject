using UnityEngine;

public class TestCraft : MonoBehaviour
{
    [SerializeField] private string test = "Weapon001";
    [SerializeField] private int index = 0;

    private void Update()
    {
        if (InputManager.InputActions.actions["ESC"].triggered)
        {
            ItemManager.Instance.AddItemLogic(test);
            Debug.Log("ESC");
        }

        if (InputManager.InputActions.actions["Interact"].triggered)
        {
            CookingManager.Instance.AddIngredient(InventoryManager.InventoryList[index]);
        }

        if (InputManager.InputActions.actions["SkillUI"].triggered)
        {
            CookingManager.Instance.Craft();
        }
    }
}

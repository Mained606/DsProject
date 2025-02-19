using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

[Serializable]
public class Recipe
{
    public string itemName;  //제작 아이템 이름 (아이템 ID와 동일)
    public List<string> requiredIngredientIds;  //필요한 재료 ID 리스트
    public RecipeType recipeType;   //레시피타입
    public bool isAlreadyInit = false;

    public List<Item> requiredIngredients = new List<Item>();

    public void Initialize()
    {
        if (isAlreadyInit)
            return;

        foreach (var itemId in requiredIngredientIds)
        {
            Item item = ItemManager.Instance.GetItemById(itemId);
            if (item != null)
            {
                requiredIngredients.Add(item);
            }
        }
        isAlreadyInit = true;
    }

    public bool IsMatch(List<Item> selectedIngredients)
    {
        return requiredIngredients.All(rec => selectedIngredients.Contains(rec)) &&
               selectedIngredients.All(sel => requiredIngredients.Contains(sel));
    }
}

public enum RecipeType
{
    Craft,
    Cook
}

public class CraftManager : BaseManager<CraftManager>
{
    [SerializeField] protected List<Item> selectedIngredients = new List<Item>();
    [SerializeField] protected List<Recipe> recipes = new List<Recipe>();
    [SerializeField] protected int maxIngredients = 5;

    protected override void Awake()
    {
        base.Awake();
        foreach (var recipe in recipes)
        {
            recipe.Initialize();
        }
    }

    public void AddIngredient(Item ingredient)
    {
        if (selectedIngredients.Count < maxIngredients)
        {
            selectedIngredients.Add(ingredient);
            Debug.Log("재료 추가됨: " + ingredient.id);
        }
        else
        {
            Debug.Log("최대 개수 초과");
        }
    }

    public void Craft()
    {
        if (selectedIngredients.Count == 0) return;

        Recipe match = FindMatchingRecipe(selectedIngredients);
        if (match != null)
        {
            CompleteCrafting(match);
        }
        else
        {
            FailedCrafting();
        }

        selectedIngredients.Clear();
    }

    protected virtual void CompleteCrafting(Recipe recipe)
    {
        foreach(string item in recipe.requiredIngredientIds)
        {
            ItemManager.Instance.RemoveItemLogic(item);
        }

        ItemManager.Instance.AddItemLogic(recipe.itemName);

        Debug.Log("제작 성공 인벤토리에 추가: " + recipe.itemName);
    }

    protected virtual void FailedCrafting()
    {
        Debug.Log("제작 실패");
    }

    private Recipe FindMatchingRecipe(List<Item> ingredients)
    {
        return recipes.FirstOrDefault(recipe => recipe.IsMatch(ingredients));
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}

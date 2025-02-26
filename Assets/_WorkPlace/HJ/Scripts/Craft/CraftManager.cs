using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;

[Serializable]
public class Recipe
{
    public string itemId;  //제작 아이템 Id (아이템 데이터의 Id와 동일)
    public List<string> requiredIngredientIds;  //필요한 재료 ID 리스트
    public RecipeType recipeType;   //레시피타입
    public float baseDuration;      //기본 요리 버프 지속시간

    //레시피와 선택된 아이템 비교
    public bool IsMatch(List<Item> selectedIngredients)
    {
        List<string> required = new List<string>(requiredIngredientIds);
        List<string> selected = selectedIngredients.Select(i => i.id).ToList();

        if(recipeType == RecipeType.Craft)  //제작은 레시피와 정확히 일치해야 가능
        {
            var requiredCount = required.GroupBy(r => r).ToDictionary(r => r.Key, r => r.Count());
            var selectedCount = selected.GroupBy(s => s).ToDictionary(s => s.Key, s => s.Count());

            return requiredCount.Count == selectedCount.Count &&    //수가 동일한지 확인
                requiredCount.All(req => selectedCount.TryGetValue(req.Key, out int count) && count == req.Value);  //모든 키값이 존재하는지 확인하고 개수가 동일한지 확인
        }
        else if(recipeType == RecipeType.Cook)  //요리는 레시피에 해당하는 아이템이기만 하면 갯수는 상관없음
        {
            return required.All(rec => selected.Contains(rec)) &&
                   selected.All(sel => required.Contains(sel));
        }

        return false;
    }
}

public enum RecipeType
{
    Craft,
    Cook
}


public class CraftManager : BaseManager<CraftManager>
{
    [SerializeField] protected RecipeList recipeList;
    [SerializeField] protected List<Item> selectedIngredients = new List<Item>();
    [SerializeField] protected int maxIngredients = 5;

    [SerializeField] protected virtual List<Recipe> Recipes { get; set; }

    protected override void Awake()
    {
        base.Awake();

        //레시피 리스트
        if (recipeList != null)
        {
            Recipes = recipeList.recipeList.Where(r => r.recipeType == RecipeType.Craft).ToList();
        }
    }

    public void AddIngredient(Item ingredient)
    {
        if (selectedIngredients.Count >= maxIngredients)
        {
            Debug.Log("최대 갯수 초과");
            return;
        }

        ItemManager.Instance.RemoveItemLogic(ingredient.id);

        selectedIngredients.Add(ingredient);

        Debug.Log("재료 추가됨: " + ingredient.id);
    }

    public void Craft()
    {
        if (selectedIngredients.Count == 0) return;
        if (InventoryManager.Instance.GetRemainingInventory() <= 0) return;

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
        ItemManager.Instance.AddItemLogic(recipe.itemId);

        

        Debug.Log("제작 성공 인벤토리에 추가: " + recipe.itemId);
    }

    protected virtual void FailedCrafting()
    {
        foreach(Item item in selectedIngredients)
        {
            ItemManager.Instance.AddItemLogic(item.id);
        }

        Debug.Log("제작 실패");
    }

    protected Recipe FindMatchingRecipe(List<Item> ingredients)
    {
        if (Recipes == null || Recipes.Count == 0)
        {
            Debug.Log("레시피가 존재하지 않음");
            return null;
        }

        return recipeList.recipeList.FirstOrDefault(recipe => recipe.IsMatch(ingredients));
    }

    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
    }
}

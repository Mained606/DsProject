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
        else if(recipeType == RecipeType.Cook)  //요리는 레시피에 해당하는 아이템이기만 하면 갯수는 상관없음 + 특수 아이템은 레시피에 없어도 성공
        {
            if (!required.All(r => selected.Contains(r))) return false;     //필수재료가 빠지면 실패

            List<string> extraIngredients = selected.Where(i => !required.Contains(i)).ToList();    //필수 재료 외에 들어가는 재료

            bool isAllSpecial = extraIngredients.All(i => CookingManager.Instance.specialIngredients.Contains(i));   //추가 재료가 특수 아이템인지 확인

            return isAllSpecial || extraIngredients.Count == 0;
        }

        return false;
    }
}

public enum RecipeType
{
    Craft,
    Cook
}


public class CraftManager : MonoBehaviour
{
    public static CraftManager Instance { get; private set; }

    [SerializeField] protected RecipeList recipeList;
    [SerializeField] protected List<Item> selectedIngredients = new List<Item>();
    [SerializeField] protected int maxIngredients = 5;

    //public List<string> specialIngredients = new List<string>();    //특수 제작 재료

    private List<Recipe> recipes;
    public virtual List<Recipe> Recipes { get => recipes; set => recipes = value; }

    protected virtual void Awake() 
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this.transform.parent);
        }
        else
        {
            Destroy(gameObject);
        }

        //레시피 리스트
        if (recipeList != null)
        {
            Recipes = recipeList.recipeList.Where(r => r.recipeType == RecipeType.Craft).ToList();
        }
    }
    // 재료 추가 
    public void AddIngredient(Item ingredient)
    {
        if (selectedIngredients.Count >= maxIngredients)
        {
            //Debug.Log("최대 갯수 초과");
            return;
        }

        ItemManager.Instance.RemoveItemLogic(ingredient.id);

        selectedIngredients.Add(ingredient);

        //Debug.Log("재료 추가됨: " + ingredient.id);
        //===============================================
        foreach (var item in selectedIngredients)
        {
            //Debug.Log($"아이템: {item.name}, 개수: {item.quantity}");
        }

    }

    // 재료 제거 -> selectedIngredients 초기화 
    public void ClearIngredients()
    {
        foreach (Item ingredient in selectedIngredients)
        {
            ItemManager.Instance.AddItemLogic(ingredient.id, ingredient.quantity); // 인벤토리에 다시 추가
        }

        selectedIngredients.Clear(); // 리스트 비우기
        //Debug.Log("냄비 초기화");
    }

    // 제작 버튼 
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

        //Debug.Log("제작 성공 인벤토리에 추가: " + recipe.itemId);
    }
    // 제작 실패 메세지
    protected virtual void FailedCrafting()
    {
        foreach(Item item in selectedIngredients)
        {
            ItemManager.Instance.AddItemLogic(item.id);
        }

        //Debug.Log("제작 실패");
    }
    // 없는 아이템 생성 시도시?
    protected Recipe FindMatchingRecipe(List<Item> ingredients)
    {
        if (Recipes == null || Recipes.Count == 0)
        {
            //Debug.Log("레시피가 존재하지 않음");
            return null;
        }

        return recipeList.recipeList.FirstOrDefault(recipe => recipe.IsMatch(ingredients));
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.Collections;
using UnityEngine;


public class CookingManager : CraftManager
{
    public static new CookingManager Instance { get; private set; }
    [SerializeField] private string failedDishId = "실패한 요리";
    [SerializeField] private float maxDuration = 300f;      //최대 버프 지속시간

    public List<string> specialIngredients = new List<string>();    //특수 요리 재료

    [SerializeField] protected override List<Recipe> Recipes { get; set; }

    protected override void Awake()
    {
        if (Instance == null || Instance == this)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }

        if (recipeList != null)
        {
            Recipes = recipeList.recipeList.Where(r => r.recipeType == RecipeType.Cook).ToList();
        }
    }

    //요리 효과 계산
    private Item CalculateDishStat(List<Item> ingredients, Recipe recipe)
    {

        //기본 요리 스탯
        Item totalItem = ItemManager.Instance.GetItemById(recipe.itemId).Clone();
        ItemStat totalStat = totalItem.itemStat;

        //각 재료 갯수
        Dictionary<string, int> ingredientsCount = new Dictionary<string, int>();

        foreach (Item item in ingredients)   //재료
        {
            if (ingredientsCount.ContainsKey(item.id))
            {
                ingredientsCount[item.id]++;    //딕셔너리에 재료의 키값이 이미 있으면 수량만 추가
            }
            else
            {
                ingredientsCount[item.id] = 1;  //딕셔너리에 재료의 키값이 없으면 수량 1
            }
        }

        //베이스 재료 빼기(-1)
        List<string> extraIngredients = selectedIngredients.Select(i => i.id).ToList(); //추가로 넣은 재료 아이디를 담는 리스트

        foreach (string required in recipe.requiredIngredientIds.ToList())
        {
            if (ingredientsCount.ContainsKey(required) && ingredientsCount[required] > 0)    //재료 갯수 세는 딕셔너리에 필수 재료에 대한 키값이 존재하고 0개보다 많으면
            {
                ingredientsCount[required]--;   //재료 갯수에서 한개 차감
                extraIngredients.Remove(required);
            }
        }

        Debug.Log($"extraIngredients.Count: {extraIngredients.Count}");

        //버프 지속시간 설정
        totalItem.effect.duration = CalculateDishDuration(recipe, extraIngredients, maxDuration);

        Debug.Log($"Calculated Duration: {totalItem.effect.duration}");

        //추가되는 재료가 있을 때만
        if (extraIngredients.Count > 0)
        {
            //추가되는 스탯 계산
            ItemStat extraStat = null;

            foreach (string id in extraIngredients)
            {
                Item extra = ItemManager.Instance.GetItemById(id);
                extraStat = extraStat == null ? extra.itemStat.Clone() : extraStat.AddStats(extraStat, extra.itemStat);
            }

            //스탯 총합 계산
            totalStat = totalStat.AddStats(totalStat, extraStat);
            totalItem.itemStat = totalStat;

            //아이템 id, 설명 수정
            EditId(totalItem, ingredientsCount);
            EditDescription(totalItem, extraStat);
        }

        return totalItem;
    }

    //요리 id 수정
    private void EditId(Item item, Dictionary<string, int> ingredientsCount)
    {
        string mostIngredientId = null;
        int maxCount = 0;

        foreach (var pair in ingredientsCount)
        {
            if (pair.Value > maxCount)
            {
                if (specialIngredients.Contains(pair.Key)) continue;

                mostIngredientId = pair.Key;
                maxCount = pair.Value;
            }
        }
        
        Item mostIngredient = ItemManager.Instance.GetItemById(mostIngredientId);

        if (mostIngredient != null)
        {
            item.id = $"{mostIngredient.id} 듬뿍 {item.id}";
        }

        if(ingredientsCount.ContainsKey(specialIngredients.FirstOrDefault()))
        {
            item.id = $"강화 {item.id}";
        }
    }

    //요리 설명 수정
    private void EditDescription(Item item, ItemStat stat)
    {
        string newDescription = stat.GetEffectDescription();

        item.description += $"\n추가효과: {newDescription}";
    }

    //요리 버프 지속시간 설정
    private float CalculateDishDuration(Recipe recipe, List<string> extraIngredients, float maxDuration)
    {
        Debug.Log("요리 지속시간 설정 함수 실행됨!");

        float totalDuration = recipe.baseDuration;
        Debug.Log($"초기 지속시간: {totalDuration}");

        foreach (string ingredient in extraIngredients)
        {
            float bonus = ItemManager.Instance.GetItemById(ingredient).itemStat.durationBonus;
            totalDuration += bonus;
            Debug.Log($"보너스 지속시간 : {bonus}");
        }

        Debug.Log($"최종 지속시간: {totalDuration}");
        return Mathf.Clamp(totalDuration, recipe.baseDuration, maxDuration);
    }

    //요리 완성
    private Item CookedDish(Recipe recipe)
    {
        Item cookedDish = CalculateDishStat(selectedIngredients, recipe);

        return cookedDish;
    }

    //요리 실패
    private Item FailedDish()
    {
        Item failedDish = ItemManager.Instance.GetItemById(failedDishId).Clone();
        ItemStat failedStat = failedDish.itemStat.Clone();

        for (int i = 0; i < selectedIngredients.Count - 1; i++)
        {
            failedDish.itemStat = failedDish.itemStat.AddStats(failedDish.itemStat, failedStat);
        }

        return failedDish;
    }

    protected override void CompleteCrafting(Recipe recipe)
    {
        Item cookedDish = CookedDish(recipe);

        InventoryManager.Instance.AddItemLogic(cookedDish);

        Debug.Log("제작 성공 인벤토리에 추가: " + cookedDish.id);
    }

    protected override void FailedCrafting()
    {
        Item failedDish = FailedDish();

        InventoryManager.Instance.AddItemLogic(failedDish);

        Debug.Log("요리 실패");
    }
}
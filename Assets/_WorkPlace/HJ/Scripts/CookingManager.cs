using System.Collections.Generic;
using UnityEngine;


public class CookingManager : CraftManager
{
    [SerializeField] private float failedEffectAmount = 5f;

    //요리 효과 계산
    private ItemStat CalculateDishStat(List<Item> ingredients, Recipe recipe)
    {
        //기본 요리 스탯
        Item baseItem = ItemManager.Instance.GetItemById(recipe.itemId);  //요리
        ItemStat totalStat = baseItem.itemStat;

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
        List<string> extraIngredients = new List<string>(recipe.requiredIngredientIds); //추가로 넣은 재료 아이디를 담는 리스트
        foreach (string required in extraIngredients)
        {
            if (ingredientsCount.ContainsKey(required) && ingredientsCount[required] > 0)    //재료 갯수 세는 딕셔너리에 필수 재료에 대한 키값이 존재하고 0개보다 많으면
            {
                ingredientsCount[required]--;   //재료 갯수에서 한개 차감
                extraIngredients.Remove(required);  //추가로 넣은 재료 리스트에서 삭제
            }
        }

        //스탯 총합 계산

        return totalStat;
    }

    //요리 완성
    private void CookedDish()
    {

    }
}
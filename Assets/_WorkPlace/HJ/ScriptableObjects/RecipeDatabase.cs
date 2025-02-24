using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "RecipeList", menuName = "Ds Project/RecipeList")]
public class RecipeList : ScriptableObject
{
    public List<Recipe> recipeList = new List<Recipe>();
}
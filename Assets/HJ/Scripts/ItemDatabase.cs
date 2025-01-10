using System.Collections.Generic;
using UnityEngine;

namespace HJ
{
    [CreateAssetMenu(fileName = "ItemDatabase", menuName = "Scriptable Objects/ItemDatabase")]
    public class ItemDatabase : ScriptableObject
    {
        public List<Item> items = new List<Item>();
    }
}
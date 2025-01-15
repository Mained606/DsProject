using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemDatabase))]
public class ItemDatabaseEditor : Editor
{
    private SerializedProperty itemsProperty;   //아이템데이터베이스의 items
    private bool[] foldouts;                    //접기

    private void OnEnable()
    {
        itemsProperty = serializedObject.FindProperty("items");
        foldouts = new bool[itemsProperty.arraySize];
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //레이블 표시
        EditorGUILayout.LabelField("Item Database", EditorStyles.boldLabel);

        //초기화 버튼
        if (GUILayout.Button("Initialize All Items"))
        {
            (target as ItemDatabase)?.InitializeAllItems();
        }

        //아이템 리스트
        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            SerializedProperty itemProperty = itemsProperty.GetArrayElementAtIndex(i);

            // Expand/Collapse per item
            foldouts[i] = EditorGUILayout.Foldout(foldouts[i], $"Item {i + 1}");
            if (foldouts[i])
            {
                DrawItemEditor(itemProperty, i);
            }
        }

        //리스트 추가 버튼
        if (GUILayout.Button("Add New Item"))
        {
            itemsProperty.arraySize++;
            foldouts = new bool[itemsProperty.arraySize];
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawItemEditor(SerializedProperty itemProperty, int index)
    {
        //아이템 공통항목
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemId"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemName"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemDescription"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemType"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("currentQuantity"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("maxCapacity"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("isCanStack"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("value"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("isDiscardable"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemDropChance"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("itemImage"));

        //아이템 타입
        ItemType itemType = (ItemType)itemProperty.FindPropertyRelative("itemType").enumValueIndex;

        //아이템 타입에 따른 동적 속성
        switch (itemType)
        {
            case ItemType.Weapon:
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("attackPower"));
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("durability"));
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("weaponType"));
                break;
            case ItemType.Armor:
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("defensePower"));
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("durability"));
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("equipmentSlot"));
                break;
            case ItemType.Consumable:
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("consumableType"));
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("effectAmount"));
                break;
            case ItemType.QuestItem:
                EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("questId"));
                break;
        }

        //아이템 삭제 버튼
        if (GUILayout.Button("Remove Item"))
        {
            itemsProperty.DeleteArrayElementAtIndex(index);
        }
    }
}

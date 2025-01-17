using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemList))]
public class ItemDatabaseEditor : Editor
{
    private SerializedProperty itemsProperty;   //아이템 데이터베이스의 items
    private bool[] foldouts;                    //토글

    private void OnEnable()
    {
        itemsProperty = serializedObject.FindProperty("itemList");
        foldouts = new bool[itemsProperty.arraySize];
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        //레이블 표시
        EditorGUILayout.LabelField("Item Database", EditorStyles.boldLabel);

        //리스트 추가 버튼
        if (GUILayout.Button("Add New Item"))
        {
            AddNewItem();
        }


        //아이템 리스트를 박스 안에 표시
        EditorGUILayout.BeginVertical("box"); //박스 시작
        GUILayout.Space(10); //박스 위쪽 여백

        for (int i = 0; i < itemsProperty.arraySize; i++)
        {
            SerializedProperty itemProperty = itemsProperty.GetArrayElementAtIndex(i);
            string itemId = itemProperty.FindPropertyRelative("id").stringValue;

            //왼쪽, 오른쪽 여백 추가
            EditorGUILayout.BeginHorizontal();
            GUILayout.Space(15); //왼쪽 여백

            EditorGUILayout.BeginVertical(); //아이템 컨텐츠
            foldouts[i] = EditorGUILayout.Foldout(foldouts[i], itemId); //아이템 id 표시
            if (foldouts[i])
            {
                DrawItemEditor(itemProperty, i);
            }
            EditorGUILayout.EndVertical();

            GUILayout.Space(15); //오른쪽 여백
            EditorGUILayout.EndHorizontal();
        }

        GUILayout.Space(10); //박스 아래쪽 여백
        EditorGUILayout.EndVertical(); //박스를 종료


        serializedObject.ApplyModifiedProperties();
    }

    private void AddNewItem()
    {
        itemsProperty.arraySize++;
        foldouts = new bool[itemsProperty.arraySize];
    }

    //private void ResetTypeSpecificFields(SerializedProperty itemProperty, ItemType itemType)
    //{
    //    switch (itemType)
    //    {
    //        case ItemType.무기:
    //        case ItemType.방어구:
    //        case ItemType.장신구:
    //            itemProperty.FindPropertyRelative("itemStat").objectReferenceValue = null;
    //            itemProperty.FindPropertyRelative("durability").intValue = 0;
    //            itemProperty.FindPropertyRelative("equipmentSlot").enumValueIndex = 0;
    //            break;
    //        case ItemType.소모품:
    //            itemProperty.FindPropertyRelative("consumableType").enumValueIndex = 0;
    //            itemProperty.FindPropertyRelative("effectAmount").floatValue = 0f;
    //            break;
    //        case ItemType.퀘스트:
    //            itemProperty.FindPropertyRelative("questId").intValue = 0;
    //            break;
    //    }
    //}

    private void DrawItemEditor(SerializedProperty itemProperty, int index)
    {
        //아이템 공통항목
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("id"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("name"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("description"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("type"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("grade"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("costValue"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("quantity"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("maxStack"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("isDiscardable"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("isStackable"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("sprite"));
        EditorGUILayout.PropertyField(itemProperty.FindPropertyRelative("dropChance"));

        //아이템 타입
        ItemType itemType = (ItemType)itemProperty.FindPropertyRelative("type").enumValueIndex;

        //아이템 타입에 따른 동적 속성
        switch (itemType)
        {
            case ItemType.무기:
            case ItemType.방어구:
            case ItemType.장신구:
                DrawPropertyIfExists(itemProperty, "itemStat");
                DrawPropertyIfExists(itemProperty, "durability");
                DrawPropertyIfExists(itemProperty, "equipmentSlot");
                break;
            case ItemType.소모품:
                DrawPropertyIfExists(itemProperty, "consumableType");
                DrawPropertyIfExists(itemProperty, "effectAmount");
                break;
            case ItemType.퀘스트:
                DrawPropertyIfExists(itemProperty, "questId");
                break;
        }

        GUILayout.Space(10);

        //아이템 삭제 버튼
        if (GUILayout.Button("Remove Item"))
        {
            itemsProperty.DeleteArrayElementAtIndex(index);
        }
    }

    private void DrawPropertyIfExists(SerializedProperty parentProperty, string propertyName)
    {
        SerializedProperty property = parentProperty.FindPropertyRelative(propertyName);
        if (property != null)
        {
            EditorGUILayout.PropertyField(property);
        }
    }
}

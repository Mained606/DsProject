// using UnityEditor;
// using UnityEngine;

// // [CustomEditor(typeof(NonePlayerCharacter))]
// public class NPCDataEditor : Editor
// {
//     public override void OnInspectorGUI()
//     {
//         // SerializedObject 업데이트
//         serializedObject.Update();

//         // 기본 NPC 설정
//         SerializedProperty npcTypeProperty = serializedObject.FindProperty("npcType");
//         SerializedProperty npcIndexProperty = serializedObject.FindProperty("npcIndex");    //04.08 HJ 추가
//         SerializedProperty shopItemTypeProperty = serializedObject.FindProperty("shopItemType");
//         SerializedProperty shopIndexProperty = serializedObject.FindProperty("shopIndex");
        
//         // 추가된 퀘스트 NPC 필드 참조
//         SerializedProperty isMainQuestNpcProperty = serializedObject.FindProperty("isMainQuestNpc");
//         SerializedProperty isSubQuestNpcProperty = serializedObject.FindProperty("isSubQuestNpc");
//         SerializedProperty subQuestIdProperty = serializedObject.FindProperty("subQuestId");

//         EditorGUILayout.LabelField("▣ NPC 설정");
        
//         // NPC 타입 표시 및 변경 감지
//         EditorGUI.BeginChangeCheck();
//         EditorGUILayout.PropertyField(npcTypeProperty, new GUIContent("      NPC Type"));
//         bool npcTypeChanged = EditorGUI.EndChangeCheck();
        
//         // NPC 타입이 변경되었을 때 추가 처리
//         if (npcTypeChanged)
//         {
//             // 타입이 퀘스트가 아니면 퀘스트 관련 옵션 초기화
//             if (npcTypeProperty.enumValueIndex != (int)NPCType.퀘스트)
//             {
//                 isMainQuestNpcProperty.boolValue = false;
//                 isSubQuestNpcProperty.boolValue = false;
//                 subQuestIdProperty.stringValue = "";
//             }
//         }

//         if (npcTypeProperty.enumValueIndex == (int)NPCType.상점)
//         {
//             EditorGUILayout.PropertyField(shopItemTypeProperty, new GUIContent("      Shop ItemType"));
//             string[] indexOptions = new string[4];
//             for (int i = 0; i <= 3; i++) indexOptions[i] = i.ToString();
//             EditorGUILayout.PropertyField(shopIndexProperty, new GUIContent("      Shop Index"));
//         }
//         else if (npcTypeProperty.enumValueIndex == (int)NPCType.퀘스트) 
//         {
//             EditorGUILayout.PropertyField(npcIndexProperty, new GUIContent("      NPC Index"));
            
//             // 추가된 퀘스트 NPC 설정 필드 표시
//             EditorGUILayout.Space(10);
//             EditorGUILayout.LabelField("▣ 퀘스트 NPC 설정");
            
//             // 메인 퀘스트 NPC 옵션 변경 감지
//             EditorGUI.BeginChangeCheck();
//             EditorGUILayout.PropertyField(isMainQuestNpcProperty, new GUIContent("      메인 퀘스트 NPC"));
//             if (EditorGUI.EndChangeCheck() && isMainQuestNpcProperty.boolValue)
//             {
//                 // 메인 퀘스트 NPC가 선택되면 서브 퀘스트 NPC 옵션 해제
//                 isSubQuestNpcProperty.boolValue = false;
//             }
            
//             // 서브 퀘스트 NPC 옵션 변경 감지
//             EditorGUI.BeginChangeCheck();
//             EditorGUILayout.PropertyField(isSubQuestNpcProperty, new GUIContent("      서브 퀘스트 NPC"));
//             if (EditorGUI.EndChangeCheck() && isSubQuestNpcProperty.boolValue)
//             {
//                 // 서브 퀘스트 NPC가 선택되면 메인 퀘스트 NPC 옵션 해제
//                 isMainQuestNpcProperty.boolValue = false;
//             }
            
//             // 서브 퀘스트 NPC인 경우 서브 퀘스트 ID 필드 표시
//             if (isSubQuestNpcProperty.boolValue)
//             {
//                 // [추가] 서브퀘스트 ID 입력 필드
//                 EditorGUILayout.PropertyField(subQuestIdProperty, new GUIContent("      서브 퀘스트 ID"));
                
//                 // [추가] 서브퀘스트 NPC 역할 설명
//                 bool hasQuestId = !string.IsNullOrEmpty(subQuestIdProperty.stringValue);
//                 string npcRoleInfo = hasQuestId ? 
//                     "이 NPC는 퀘스트 지급자와 활성자 역할을 모두 수행합니다." : 
//                     "이 NPC는 퀘스트 활성자(만남 조건)만 수행합니다.";
                
//                 EditorGUILayout.HelpBox(npcRoleInfo, MessageType.Info);
//             }
//             else
//             {
//                 // 서브 퀘스트 NPC가 아니면 서브 퀘스트 ID 초기화
//                 if (!string.IsNullOrEmpty(subQuestIdProperty.stringValue))
//                 {
//                     subQuestIdProperty.stringValue = "";
//                 }
//             }
            
//             // 둘 다 선택되지 않은 경우 안내 메시지 표시
//             if (!isMainQuestNpcProperty.boolValue && !isSubQuestNpcProperty.boolValue)
//             {
//                 EditorGUILayout.HelpBox("퀘스트 NPC 타입을 선택하지 않으면 일반 NPC로 처리됩니다.", MessageType.Info);
//             }
//         }
//         else
//         {
//             EditorGUILayout.PropertyField(npcIndexProperty, new GUIContent("      NPC Index"));
//         }
        
//         serializedObject.ApplyModifiedProperties();

//         // 읽기 전용 처리
//         GUI.enabled = false;

//         // NonePlayerCharacter와 NPCData 참조 가져오기
//         NonePlayerCharacter holder = (NonePlayerCharacter)target;
//         SerializedProperty npcDataProperty = serializedObject.FindProperty("currentNPCData");

//         if (npcDataProperty == null || string.IsNullOrEmpty(npcDataProperty.FindPropertyRelative("id").stringValue))
//         {
//             GUIStyle style = new GUIStyle
//             {
//                 normal = { textColor = Color.red },
//                 fontSize = 20,
//                 fontStyle = FontStyle.Bold,
//                 alignment = TextAnchor.MiddleCenter
//             };
//             GUILayout.Space(10);
//             EditorGUILayout.LabelField("▣ 할당된 NPC 정보가 없습니다!", style);
//             GUILayout.Space(10);
//             return;
//         }

//         // NPCData 내부 속성 접근
//         SerializedProperty idProperty = npcDataProperty.FindPropertyRelative("id");
//         SerializedProperty nameProperty = npcDataProperty.FindPropertyRelative("name");
//         SerializedProperty descriptionProperty = npcDataProperty.FindPropertyRelative("description");
//         SerializedProperty npcStateProperty = npcDataProperty.FindPropertyRelative("currentState");
//         SerializedProperty reputationRequirementProperty = npcDataProperty.FindPropertyRelative("reputationRequirement");
//         SerializedProperty spriteProperty = npcDataProperty.FindPropertyRelative("sprite");
//         SerializedProperty gameobjectProperty = npcDataProperty.FindPropertyRelative("currentNPC");
//         SerializedProperty shopDataProperty = npcDataProperty.FindPropertyRelative("shopData");
//         SerializedProperty itemsProperty = npcDataProperty.FindPropertyRelative("items");
//         SerializedProperty questsProperty = npcDataProperty.FindPropertyRelative("quests");
//         SerializedProperty patrolPointsProperty = npcDataProperty.FindPropertyRelative("patrolPoints");
//         SerializedProperty voiceLinesProperty = npcDataProperty.FindPropertyRelative("voiceLines");
//         SerializedProperty interactionEffectProperty = npcDataProperty.FindPropertyRelative("interactionEffect");

//         // 기본 필드 표시
//         EditorGUILayout.LabelField("▣ Base Settings");
//         EditorGUILayout.PropertyField(idProperty, new GUIContent("      ID"));
//         EditorGUILayout.PropertyField(nameProperty, new GUIContent("      Name"));
//         EditorGUILayout.PropertyField(descriptionProperty, new GUIContent("      Description"));
//         EditorGUILayout.PropertyField(npcStateProperty, new GUIContent("      NPC State"));
//         EditorGUILayout.PropertyField(reputationRequirementProperty, new GUIContent("      Reputation Requirement"));
//         EditorGUILayout.PropertyField(spriteProperty, new GUIContent("      Sprite"));
//         EditorGUILayout.PropertyField(gameobjectProperty, new GUIContent("      Current NPC"));

//         // 조건부 필드 표시
//         NPCType npcType = (NPCType)npcDataProperty.FindPropertyRelative("npcType").enumValueIndex;

//         if (npcType == NPCType.상점)
//         {
//             EditorGUILayout.LabelField("▣ Shop Settings");
//             SerializedProperty isShopProperty = npcDataProperty.FindPropertyRelative("isShop");
//             EditorGUILayout.PropertyField(isShopProperty, new GUIContent("      Is Shop"));
//             EditorGUILayout.PropertyField(shopDataProperty, new GUIContent("      Shop Data"), true);
//         }

//         if (npcType == NPCType.퀘스트)
//         {
//             EditorGUILayout.LabelField("▣ Quest Settings");
//             SerializedProperty isQuestGiverProperty = npcDataProperty.FindPropertyRelative("isQuestGiver");
//             EditorGUILayout.PropertyField(isQuestGiverProperty, new GUIContent("      Is Quest Giver"));
            
//             // [추가] 퀘스트 활성화자 설정 추가
//             SerializedProperty isQuestActivatorProperty = npcDataProperty.FindPropertyRelative("isQuestActivator");
//             EditorGUILayout.PropertyField(isQuestActivatorProperty, new GUIContent("      Is Quest Activator"));
            
//             EditorGUILayout.PropertyField(questsProperty, new GUIContent("      Quests"), true);
//         }

//         SerializedProperty canMoveProperty = npcDataProperty.FindPropertyRelative("canMove");
//         if (canMoveProperty.boolValue)
//         {
//             EditorGUILayout.LabelField("▣ Movement Settings");
//             SerializedProperty moveSpeedProperty = npcDataProperty.FindPropertyRelative("moveSpeed");
//             EditorGUILayout.PropertyField(moveSpeedProperty, new GUIContent("      Move Speed"));
//             EditorGUILayout.PropertyField(patrolPointsProperty, new GUIContent("      Patrol Points"), true);
//         }

//         SerializedProperty hasScheduleProperty = npcDataProperty.FindPropertyRelative("hasSchedule");
//         if (hasScheduleProperty.boolValue)
//         {
//             EditorGUILayout.LabelField("▣ Schedule Settings");
//             SerializedProperty activeTimeProperty = npcDataProperty.FindPropertyRelative("activeTime");
//             EditorGUILayout.PropertyField(activeTimeProperty, new GUIContent("      Active Time"));
//         }

//         // 효과 관련 필드
//         if (voiceLinesProperty != null && voiceLinesProperty.isArray)
//         {
//             EditorGUILayout.PropertyField(voiceLinesProperty, new GUIContent("      Voice Lines"), true);
//         }

//         if (interactionEffectProperty != null)
//         {
//             EditorGUILayout.PropertyField(interactionEffectProperty, new GUIContent("      Interaction Effect"));
//         }
//     }
// }

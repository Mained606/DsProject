using UnityEngine;
using TMPro;

public class QuestDistanceCheck : MonoBehaviour
{
    private Quest quest;
    private TextMeshProUGUI distanceText;
    private string subMainColor;
    private string mainColor;
    private string distanceColor;
    private Transform questGiverTransform;

    private void Awake()
    {
        mainColor = "<color=yellow>";
        subMainColor = "<color=white>";
    }

    public void SetQuest(Quest newQuest)
    {
        if (quest != null)
        {
            CompassIndicater.ClearAllTargets();
        }
        
        quest = newQuest;
        quest.CheckQuestCondition();
        
        // 퀘스트 지급자가 설정되어 있는 경우 퀘스트 지급자 Transform 찾기
        if (!string.IsNullOrEmpty(quest.questGiver))
        {
            // NPC 데이터베이스에서 퀘스트 지급자 찾기
            foreach (var npc in QuestManager.NpcDatabase.subQuestNpcLists)
            {
                if (npc.name == quest.questGiver)
                {
                    questGiverTransform = QuestManager.GetQuestConditionPoint(npc.id);
                    Debug.Log($"[QuestDistanceCheck] 서브퀘스트 NPC 찾음: {quest.questGiver}, Transform: {(questGiverTransform != null ? "찾음" : "null")}");
                    break;
                }
            }
            
            // 일반 NPC 목록에서도 검색
            if (questGiverTransform == null)
            {
                foreach (var npc in QuestManager.NpcDatabase.npcLists)
                {
                    if (npc.name == quest.questGiver)
                    {
                        questGiverTransform = QuestManager.GetQuestConditionPoint(npc.id);
                        Debug.Log($"[QuestDistanceCheck] 일반 NPC 찾음: {quest.questGiver}, Transform: {(questGiverTransform != null ? "찾음" : "null")}");
                        break;
                    }
                }
            }
            
            // QuestConditionPoint에서 찾지 못한 경우 GameObject.Find로 시도
            if (questGiverTransform == null)
            {
                // NPC ID로 찾기 시도
                GameObject npcObj = GameObject.Find(quest.questGiver);
                if (npcObj != null)
                {
                    questGiverTransform = npcObj.transform;
                    Debug.Log($"[QuestDistanceCheck] GameObject.Find로 NPC 찾음: {quest.questGiver}");
                }
                else
                {
                    // 태그로 찾기 시도
                    GameObject[] npcs = GameObject.FindGameObjectsWithTag("NPC");
                    foreach (var npc in npcs)
                    {
                        if (npc.name.Contains(quest.questGiver))
                        {
                            questGiverTransform = npc.transform;
                            Debug.Log($"[QuestDistanceCheck] Tag로 NPC 찾음: {quest.questGiver}");
                            break;
                        }
                    }
                }
            }
            
            // 퀘스트 지급자의 Transform을 콤파스에 추가
            if (questGiverTransform != null)
            {
                CompassIndicater.AddTarget(questGiverTransform);
                Debug.Log($"[QuestDistanceCheck] 콤파스에 {quest.questGiver} 추가됨");
            }
            else
            {
                Debug.LogWarning($"[QuestDistanceCheck] {quest.questGiver}의 Transform을 찾을 수 없음");
            }
        }
    }

    public void SetUIText(TextMeshProUGUI text)
    {
        distanceText = text;
    }

    private void Update()
    {
        if (quest == null || distanceText == null) return;

        string questDescription = $"{mainColor}{quest.description}</color>";
        
        // 퀘스트 지급자 정보 표시
        if (!string.IsNullOrEmpty(quest.questGiver))
        {
            questDescription += $" - {subMainColor}{quest.questGiver}</color>";
            
            // 퀘스트 지급자와의 거리 표시
            if (questGiverTransform != null && !quest.isCompleted)
            {
                float distance;
                string colorCode = CheckDistance(questGiverTransform, false, out distance);
                questDescription += $" <color={colorCode}>({distance:F1}m)</color>";
            }
        }
        
        questDescription += "\n";
        distanceColor = string.Empty;

        foreach (var condition in quest.requiredConditions)
        {
            string keyWord = condition.Key;
            QuestCondition questCondition = condition.Value;
            Transform targetTransform = QuestManager.GetQuestConditionPoint(keyWord);
            distanceColor += $"<size=36>  " + (questCondition.isCompleted ? $"<color=red>✅ [완료] </color>" : $"🔥");

            switch (questCondition.type)
            {
                case QuestConditionType.Kill:
                    if (!quest.progress.ContainsKey(keyWord))
                    {
                        quest.progress[keyWord] = 0;
                    }
                    int currentProgress = quest.progress[keyWord];
                    
                    distanceColor += $" {subMainColor}{questCondition.targetName} </color>";
                    distanceColor += !questCondition.isCompleted ? $" <color=green>{currentProgress} / {questCondition.requiredQuantity}</color>" : "";
                    
                    if (targetTransform != null)
                    {
                        float killDistance;
                        string killColor = CheckDistance(targetTransform, questCondition.isCompleted, out killDistance);
                        distanceColor += $"  <color={killColor}>{killDistance:F1}m</color>\n";
                    }
                    else
                    {
                        distanceColor += "\n";
                    }
                    break;

                case QuestConditionType.Collect:
                    if (!quest.progress.ContainsKey(keyWord))
                    {
                        quest.progress[keyWord] = 0;
                    }
                    int collectProgress = quest.progress[keyWord];
                    
                    distanceColor += $" {subMainColor}{questCondition.targetName} </color>";
                    distanceColor += !questCondition.isCompleted ? $" <color=green>{collectProgress} / {questCondition.requiredQuantity}</color>\n" : "\n";
                    break;

                case QuestConditionType.Explore:
                case QuestConditionType.Meet:
                    distanceColor += $" {subMainColor}{questCondition.targetName}</color>";
                    
                    if (targetTransform != null)
                    {
                        float exploreDistance;
                        string exploreColor = CheckDistance(targetTransform, questCondition.isCompleted, out exploreDistance);
                        distanceColor += !questCondition.isCompleted ? $"  <color={exploreColor}>{exploreDistance:F1}m</color>\n" : "\n";
                    }
                    else
                    {
                        distanceColor += !questCondition.isCompleted ? "\n" : "\n";
                    }
                    break;
            }
        }

        distanceText.text = questDescription + distanceColor + "</size>";
        Canvas.ForceUpdateCanvases();
    }

    private string CheckDistance(Transform targetTransform, bool iscompelete, out float distance)
    {
        if (!iscompelete) CompassIndicater.AddTarget(targetTransform);
        Vector3 target = new Vector3(targetTransform.position.x, 0, targetTransform.position.z);
        Vector3 player = new Vector3(GameManager.playerTransform.position.x, 0, GameManager.playerTransform.position.z);
        distance = Vector3.Distance(player, target);
        return QuestManager.GetDistanceColor(distance);
    }
}

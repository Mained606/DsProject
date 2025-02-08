using UnityEngine;
using TMPro;

public class QuestDistanceCheck : MonoBehaviour
{
    private Quest quest;
    private TextMeshProUGUI distanceText;
    private string subMainColor;
    private string mainColor;
    private string distanceColor;

    private void Awake()
    {
        mainColor = "<color=yellow>";
        subMainColor = "<color=white>";
    }

    public void SetQuest(Quest newQuest)
    {
        quest = newQuest;
        quest.CheckQuestCondition();
    }

    public void SetUIText(TextMeshProUGUI text)
    {
        distanceText = text;
    }

    private void Update()
    {
        if (quest == null || distanceText == null) return;

        string questDescription = $"{mainColor}{quest.description}</color>\n";
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
                    if (targetTransform == null) continue;
                    float killDistance;
                    string killColor = CheckDistance(targetTransform, questCondition.isCompleted, out killDistance);
                    int currentProgress = quest.progress.ContainsKey(keyWord) ? quest.progress[keyWord] : 0;
                    distanceColor += $" {subMainColor}{questCondition.targetName}</color>";
                    distanceColor += !questCondition.isCompleted ? $"<color=green>{currentProgress} / {questCondition.requiredQuantity}</color>   <color={killColor}>{killDistance:F1}m</color>\n" : "\n";
                    break;

                case QuestConditionType.Collect:
                    int collectProgress = quest.progress.ContainsKey(keyWord) ? quest.progress[keyWord] : 0;
                    distanceColor += $" {subMainColor}{questCondition.targetName} 수집하기</color>";
                    distanceColor += !questCondition.isCompleted ? $"<color=green>{collectProgress} / {questCondition.requiredQuantity}</color>\n" : "\n";
                    break;

                case QuestConditionType.Explore:
                case QuestConditionType.Meet:
                    if (targetTransform == null) continue;
                    float exploreDistance;
                    string exploreColor = CheckDistance(targetTransform, questCondition.isCompleted, out exploreDistance);
                    distanceColor += $" {subMainColor}{questCondition.targetName}</color>";
                    distanceColor += !questCondition.isCompleted ? $"  <color={exploreColor}>{exploreDistance:F1}m</color>\n" : "\n";
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


[Serializable]
public class Quest :ISheetData
{
    [Header ("퀘스트 기본정보")]
    public string id;
    public string questType;
    public string name;
    public string description;
    public string targetID;
    public string questGiver;
    public int acceptCount;
    public Transform questNpcTransform;
    [Header("퀘스트 상태정보")]
    public bool isCompleted;
    public bool isCompletable;
    public bool needsDialog; // 퀘스트 완료 시 대화가 필요한지 여부
    
    [Header("선행 조건")]
    public string prerequisiteQuestId; // 이 퀘스트를 수락하기 위해 먼저 완료해야 하는 퀘스트 ID

    [Header("자동 완료 및 다음 퀘스트 설정")]
    [Tooltip("모든 조건 충족 시 자동으로 퀘스트를 완료합니다")]
    public bool autoComplete; 
    [Tooltip("퀘스트 완료 후 다음 퀘스트를 자동으로 시작합니다")]
    public bool autoStartNextQuest;
    [Tooltip("자동으로 시작할 다음 퀘스트 ID")]
    public string nextQuestId;

    public Dictionary<string, QuestCondition> requiredConditions; // 퀘스트 조건
    public Dictionary<string, int> progress; // 진행 상태
    [Header("퀘스트 보상정보")]
    public List<Reward> rewards; // 보상

    public Quest() { }

    public Quest(string type, string id, string name, string description, Dictionary<string, QuestCondition> requiredConditions, List<Reward> rewards, bool needsDialog = false, string prerequisiteQuestId = "", bool autoComplete = false, bool autoStartNextQuest = false, string nextQuestId = "")
    {
        this.id = id;
        this.questType = type;
        this.name = name;
        this.description = description;
        this.isCompleted = false;
        this.requiredConditions = requiredConditions ?? new Dictionary<string, QuestCondition>(); // null 방지
        this.progress = new Dictionary<string, int>();
        this.rewards = rewards ?? new List<Reward>();
        this.acceptCount = 0;
        foreach (string condition in requiredConditions.Keys)
        {
            this.progress.Add(condition, 0);
        }
        questNpcTransform = null;
        this.needsDialog = needsDialog;
        this.prerequisiteQuestId = prerequisiteQuestId;
        this.autoComplete = autoComplete;
        this.autoStartNextQuest = autoStartNextQuest;
        this.nextQuestId = nextQuestId;
    }

    public void CheckQuestCondition()
    {
        //bool allConditionsMet = true; // 모든 조건을 충족했는지 체크

        foreach (var condition in requiredConditions)
        {
            string keyWord = condition.Key;
            QuestCondition questCondition = condition.Value;

            switch (questCondition.type)
            {
                case QuestConditionType.Collect:
                    // 강화 레벨 체크가 필요한 경우
                    if (questCondition.requiredLevel > 0)
                    {
                        // 해당 아이템의 강화 레벨을 확인하는 메서드 호출
                        int currentQuantity = InventoryManager.Instance.GetItemQuantityWithLevel(questCondition.targetId, questCondition.requiredLevel);
                        progress[keyWord] = currentQuantity;
                        
                        if (currentQuantity >= questCondition.requiredQuantity)
                        {
                            questCondition.isCompleted = true;
                        }
                        else
                        {
                            questCondition.isCompleted = false;
                        }
                    }
                    else
                    {
                        // 기존 로직: 아이템 수량만 체크
                        int currentQuantity = InventoryManager.Instance.GetItemQuantity(questCondition.targetId);
                        progress[keyWord] = currentQuantity;
                        
                        // 중요: 인벤토리의 실제 수량이 요구 수량 이상일 경우에만 완료로 표시
                        if (currentQuantity >= questCondition.requiredQuantity)
                        {
                            questCondition.isCompleted = true;
                        }
                        else
                        {
                            questCondition.isCompleted = false;
                            //allConditionsMet = false;
                        }
                    }
                    break;
            }
        }
        //isCompleted = allConditionsMet;
    }


    public string ToStringTMPro()
    {
        StringBuilder detailedText = new StringBuilder();
        detailedText.AppendLine($"<b><color=#00FF00>{name}</color></b>");
        detailedText.AppendLine($"<b><color=#FFD700>{description}</color></b>");
        detailedText.AppendLine($"");
        detailedText.AppendLine($"<color=yellow>진행 상황:</color>");
        foreach (var condition in requiredConditions)
        {
            int currentProgress = progress.ContainsKey(condition.Key) ? progress[condition.Key] : 0;
            string levelInfo = condition.Value.requiredLevel > 0 ? $" (레벨 {condition.Value.requiredLevel}+)" : "";
            detailedText.AppendLine($"  <color=#00FF00>{condition.Value.targetName}{levelInfo}</color>: {currentProgress}/{condition.Value.requiredQuantity}");
        }
        detailedText.AppendLine($"");
        detailedText.AppendLine("<color=green>보 상:</color>");
        foreach (var reward in rewards)
        {
            if (!string.IsNullOrEmpty(reward.itemId))
            {
                detailedText.AppendLine($"  아이템: <color=#87CEEB>{reward.itemId}</color> x{reward.quantity}");
            }
            if (reward.experience > 0)
            {
                detailedText.AppendLine($"  경험치: <color=#FFD700>{reward.experience}</color>");
            }
            if (reward.gold > 0)
            {
                detailedText.AppendLine($"  골  드: <color=#FFD700>{reward.gold}</color>");
            }
        }

        return detailedText.ToString();
    }

    public string ToStringTMProforList()
    {
        StringBuilder listText = new StringBuilder();
        listText.AppendLine($"<b><color=#00FF00>{name}</color></b>");
        string truncatedDescription = description.Length > 20
            ? description.Substring(0, 20) + "..."
            : description;
        if (requiredConditions.Count > 0)
        {
            var firstCondition = requiredConditions.First();
            int currentProgress = progress.ContainsKey(firstCondition.Key) ? progress[firstCondition.Key] : 0;
            string levelInfo = firstCondition.Value.requiredLevel > 0 ? $" (레벨 {firstCondition.Value.requiredLevel}+)" : "";
            listText.AppendLine($"<i><color=#FFFFFF>{truncatedDescription}</color></i> <color=#FFFF00>({currentProgress}/{firstCondition.Value.requiredQuantity}){levelInfo}</color>");
        }
        else
        {
            listText.AppendLine($"<i><color=#FFFFFF>{truncatedDescription}</color></i> <color=#FF6347>(진행 조건 없음)</color>");
        }
        return listText.ToString();
    }

    public void ParseData(IList<object> row)
    {
        if (row.Count < 8) throw new Exception("퀘스트 데이터 부족");

        id = row[0].ToString();
        questType = row[1].ToString();
        name = row[2].ToString();
        description = row[3].ToString();
        questGiver = row[4].ToString();

        acceptCount = int.TryParse(row[5].ToString(), out int count) ? count : 0;
        isCompleted = bool.TryParse(row[6].ToString(), out bool completed) ? completed : false;

        requiredConditions = new Dictionary<string, QuestCondition>();
        progress = new Dictionary<string, int>();
        rewards = new List<Reward>();

        // 퀘스트 조건 파싱 (쉼표로 구분된 여러 조건)
        if (!string.IsNullOrEmpty(row[7].ToString()))
        {
            string[] conditions = row[7].ToString().Split('|'); // 여러 조건을 '|'로 구분
            foreach (var condition in conditions)
            {
                QuestCondition questCondition = new QuestCondition();
                questCondition.ParseData(condition);
                requiredConditions.Add(questCondition.targetId, questCondition);
                progress.Add(questCondition.targetId, 0);
            }
        }

        // 퀘스트 보상 파싱 (쉼표로 구분된 여러 보상)
        if (row.Count > 8 && !string.IsNullOrEmpty(row[8].ToString()))
        {
            string[] rewardData = row[8].ToString().Split('|'); // 여러 보상을 '|'로 구분
            foreach (var rewardInfo in rewardData)
            {
                Reward reward = new Reward();
                reward.ParseData(rewardInfo);
                rewards.Add(reward);
            }
        }
        
        // 선행 퀘스트 ID 파싱
        if (row.Count > 9 && !string.IsNullOrEmpty(row[9].ToString()))
        {
            prerequisiteQuestId = row[9].ToString();
        }
        
        // 자동 완료 설정 파싱
        if (row.Count > 10 && !string.IsNullOrEmpty(row[10].ToString()))
        {
            autoComplete = bool.TryParse(row[10].ToString(), out bool auto) ? auto : false;
        }
        
        // 자동 시작 설정 파싱
        if (row.Count > 11 && !string.IsNullOrEmpty(row[11].ToString()))
        {
            autoStartNextQuest = bool.TryParse(row[11].ToString(), out bool autoStart) ? autoStart : false;
        }
        
        // 다음 퀘스트 ID 파싱
        if (row.Count > 12 && !string.IsNullOrEmpty(row[12].ToString()))
        {
            nextQuestId = row[12].ToString();
        }

        //Debug.Log($"[Quest] 퀘스트 {name} 데이터 로드 완료!");
    }

}

[Serializable]
public class Reward
{
    public string itemId; // 보상 아이템 ID
    public int quantity; // 아이템 개수
    public int experience; // 경험치 보상
    public int gold; // 골드 보상
    public bool isReward;

    public Reward() { }

    public Reward(string itemId, int quantity, int experience, int gold)
    {
        this.itemId = itemId;
        this.quantity = quantity;
        this.experience = experience;
        this.gold = gold;
        this.isReward = false;
    }

    public void ParseData(string rewardData)
    {
        string[] data = rewardData.Split(',');

        if (data.Length < 4) throw new Exception("퀘스트 보상 데이터 부족");

        itemId = data[0];
        quantity = int.TryParse(data[1], out int qty) ? qty : 1;
        experience = int.TryParse(data[2], out int exp) ? exp : 0;
        gold = int.TryParse(data[3], out int goldAmount) ? goldAmount : 0;
        isReward = false;
    }
}

[Serializable]
public class QuestCondition
{
    [Header("퀘스트 상태정보")]
    public QuestConditionType type; // 조건 유형
    public string targetId;         // 목표 ID (예: 몬스터 ID, 지역 ID, NPC ID)
    public string targetName;       // 목표 이름 (예: 몬스터 이름, 지역 이름, NPC 이름)
    public int requiredQuantity;    // 요구 수량 (예: 처치 수, 수집 수)
    public int requiredLevel;       // 요구 강화 레벨 (아이템 강화 레벨)
    public bool isCompleted;        // 해당 조건 완료 여부

    public QuestCondition() { }

    public QuestCondition(QuestConditionType type, string targetId, string targetName, int requiredQuantity, int requiredLevel = 0)
    {
        this.type = type;
        this.targetId = targetId;
        this.targetName = targetName;
        this.requiredQuantity = requiredQuantity;
        this.requiredLevel = requiredLevel;
        this.isCompleted = false;
    }

    public void ParseData(string conditionData)
    {
        string[] data = conditionData.Split(',');

        if (data.Length < 4) throw new Exception("퀘스트 조건 데이터 부족");

        type = Enum.TryParse(data[0], out QuestConditionType parsedType) ? parsedType : QuestConditionType.Collect;
        targetId = data[1];
        targetName = data[2];
        requiredQuantity = int.TryParse(data[3], out int qty) ? qty : 1;
        
        // 강화 레벨 데이터가 있으면 파싱
        requiredLevel = data.Length > 4 && int.TryParse(data[4], out int level) ? level : 0;
        
        isCompleted = false;
    }
}


public enum QuestConditionType
{
    Collect,  // 아이템 수집
    Explore,  // 지역 탐험
    Kill,     // 몬스터 처치
    Meet,      // NPC 만남
    Enter,     // 지역 진입
    Level      // 레벨 달성
}

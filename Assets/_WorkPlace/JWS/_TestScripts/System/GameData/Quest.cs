using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace JWS
{
    [Serializable]
    public class Quest
    {
        public string id;
        public string questType;
        public string name;
        public string description;
        public string npcid;
        public int acceptCount;
        public bool isCompleted;
        public Dictionary<string, QuestCondition> requiredConditions; // 퀘스트 조건
        public Dictionary<string, int> progress; // 진행 상태
        public List<Reward> rewards; // 보상

        public Quest(string type, string id, string name, string description, Dictionary<string, QuestCondition> requiredConditions, List<Reward> rewards)
        {
            this.id = id;
            this.questType = type;
            this.name = name;
            this.description = description;
            this.isCompleted = false;
            this.requiredConditions = requiredConditions;
            this.progress = new Dictionary<string, int>();
            this.rewards = rewards;
            this.acceptCount = 0;
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
                detailedText.AppendLine($"  <color=#00FF00>{condition.Key}</color>: {currentProgress}/{condition.Value.requiredQuantity}");
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
                listText.AppendLine($"<i><color=#FFFFFF>{truncatedDescription}</color></i> <color=#FFFF00>({currentProgress}/{firstCondition.Value.requiredQuantity})</color>");
            }
            else
            {
                listText.AppendLine($"<i><color=#FFFFFF>{truncatedDescription}</color></i> <color=#FF6347>(진행 조건 없음)</color>");
            }
            return listText.ToString();
        }

    }

    [Serializable]
    public class Reward
    {
        public string itemId; // 보상 아이템 ID
        public int quantity; // 아이템 개수
        public int experience; // 경험치 보상
        public int gold; // 골드 보상

        public Reward(string itemId, int quantity, int experience, int gold)
        {
            this.itemId = itemId;
            this.quantity = quantity;
            this.experience = experience;
            this.gold = gold;
        }
    }

    [Serializable]
    public class QuestCondition
    {
        public QuestConditionType type;
        public int requiredQuantity;

        public QuestCondition(QuestConditionType type, int requiredQuantity)
        {
            this.type = type;
            this.requiredQuantity = requiredQuantity;
        }
    }

    public enum QuestConditionType
    {
        Collect, // 아이템 수집
        Explore, // 지역 탐험
        Kill     // 몬스터 처치
    }
}
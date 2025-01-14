using UnityEngine;
using System.Collections.Generic;

namespace JWS
{
    public class QuestManager : BaseManager<QuestManager>
    {
        [SerializeField] private NPCList npcDataList;
        [SerializeField] private List<Quest> baseDatabase = new List<Quest>();
        [SerializeField] private List<Quest> questDatabase = new List<Quest>();
        [SerializeField] private List<Quest> completedQuests = new List<Quest>();
        [SerializeField] private List<NPCData> npcDatabase = new List<NPCData>();

        public static List<Quest> QuestDatabase => Instance.questDatabase;
        public static List<Quest> CompletedQuests => Instance.completedQuests;
        public static List<NPCData> NpcDatabase => Instance.npcDatabase;


        protected override void OnEnable()
        {
            base.OnEnable();
        }

        protected override void Start()
        {
            base.Start();
            baseDatabase = GenerateQuestLists();
            npcDatabase = GenerateRandomNPCs(50, baseDatabase, ItemManager.ItemDatabase);
            npcDataList.npcLists = npcDatabase;
        }


        public Quest GiveQuests()
        {
            Quest quest = GenerateQuests();
            return quest;
        }

        public void AssignQuestToNPC(string questId, string npcId)
        {
            var quest = questDatabase.Find(q => q.id == questId);
            var npc = npcDatabase.Find(n => n.id == npcId.ToString());
            if (quest != null && npc != null)
            {
                quest.npcid = npcId;
                Debug.Log($"퀘스트 '{quest.name}'가 NPC '{npc.name}'에게 할당되었습니다.");
            }
        }

        public void AddQuest(Quest quest)
        {
            if (questDatabase.Exists(q => q.questType == "메인퀘스트"))
            {
                Debug.LogWarning($"[QuestManager] 이미 진행 중인 메인 퀘스트가 있습니다.");
                return;
            }
            if (questDatabase.Exists(q => q.id == quest.id))
            {
                Debug.LogWarning($"[QuestManager] 퀘스트 '{quest.id}'는 이미 등록되어 있습니다.");
                return;
            }
            if (completedQuests.Exists(q => q.id == quest.id) && quest.questType == "메인퀘스트")
            {
                Debug.LogWarning($"[QuestManager] 완료된 메인 퀘스트 '{quest.id}'는 추가할 수 없습니다.");
                return;
            }
            questDatabase.Add(quest);
            UIManager.Instance.QuestUpdate();
        }

        public void RemoveQuest(string questId)
        {
            Quest quest = questDatabase.Find(q => q.id == questId);
            if (quest != null)
            {
                questDatabase.Remove(quest);
                Debug.Log($"[QuestManager] 퀘스트 '{quest.name}' 삭제됨");
            }
            else
            {
                Debug.LogWarning($"[QuestManager] ID: {questId} 퀘스트를 찾을 수 없습니다.");
            }
        }

        public Quest GetQuestById(string questId)
        {
            Quest quest = questDatabase.Find(q => q.id == questId);
            if (quest == null)
            {
                Debug.LogWarning($"[QuestManager] ID: {questId} 퀘스트를 찾을 수 없습니다.");
            }
            return quest;
        }

        public void UpdateQuestProgress(Item item)
        {
            for (int i = questDatabase.Count - 1; i >= 0; i--)
            {
                if (i < 0 || i >= questDatabase.Count) continue;
                Quest quest = questDatabase[i];
                if (quest.isCompleted) continue;
                if (quest.requiredConditions.ContainsKey(item.id))
                {
                    var condition = quest.requiredConditions[item.id];
                    if (condition.type == QuestConditionType.Collect)
                    {
                        if (!quest.progress.ContainsKey(item.id))
                        {
                            quest.progress[item.id] = 0;
                        }
                        quest.progress[item.id] += item.quantity;
                        if (quest.progress[item.id] >= condition.requiredQuantity)
                        {
                            if (IsQuestCompleted(quest))
                            {
                                quest.isCompleted = true;
                                UIManager.Instance.QuestUpdate();
                            }
                        }
                    }
                }
            }
        }

        private bool IsQuestCompleted(Quest quest)
        {
            foreach (var condition in quest.requiredConditions)
            {
                string conditionId = condition.Key;
                QuestCondition questCondition = condition.Value;

                if (!quest.progress.ContainsKey(conditionId) || quest.progress[conditionId] < questCondition.requiredQuantity)
                {
                    return false;
                }
            }
            return true;
        }

        public void CompleteQuest(Quest quest)
        {
            if (questDatabase.Contains(quest))
            {
                questDatabase.Remove(quest);
            }
            completedQuests.Add(quest);
            UIManager.Instance.QuestUpdate();

            Debug.Log($"[QuestManager] 퀘스트 '{quest.name}' 완료됨!");

            foreach (var condition in quest.requiredConditions)
            {
                if (condition.Value.type == QuestConditionType.Collect)
                {
                    InventoryManager.Instance.RemoveItemLogic(condition.Key, condition.Value.requiredQuantity);
                }
            }

            foreach (Reward reward in quest.rewards)
            {
                if (!string.IsNullOrEmpty(reward.itemId))
                {
                    InventoryManager.Instance.AddItem(reward.itemId, reward.quantity);
                    Debug.Log($"[QuestManager] 보상 아이템 '{reward.itemId}' {reward.quantity}개 지급됨.");
                }

                if (reward.experience > 0)
                {
                    //PlayerStats.Instance.AddExperience(reward.experience);
                    Debug.Log($"[QuestManager] 경험치 {reward.experience} 지급됨.");
                }

                if (reward.gold > 0)
                {
                    //PlayerStats.Instance.AddGold(reward.gold);
                    Debug.Log($"[QuestManager] 골드 {reward.gold} 지급됨.");
                }
            }
            quest.isCompleted = false;
            Debug.Log($"[QuestManager] 퀘스트 '{quest.name}' 보상이 지급되었습니다.");
        }

        public List<Quest> GenerateQuestLists()
        {
            List<Quest> questList = new List<Quest>
        {
            new Quest("메인퀘스트", "quest001", "회복 포션 수집", "회복 포션 10개를 수집하세요.",
            new Dictionary<string, QuestCondition> { { "item001", new QuestCondition(QuestConditionType.Collect, 10) } },
            new List<Reward> { new Reward("item001", 5, 100, 50) }),

            new Quest("서브퀘스트", "quest002", "철검 제작 재료 수집", "철검 제작을 위한 재료를 수집하세요.",
            new Dictionary<string, QuestCondition> { { "item021", new QuestCondition(QuestConditionType.Collect, 1) } },
            new List<Reward> { new Reward("item002", 1, 200, 100) }),

            new Quest("메인퀘스트", "quest003", "가죽 갑옷 제작", "가죽 갑옷 제작을 위한 재료를 모으세요.",
            new Dictionary<string, QuestCondition> { { "item025", new QuestCondition(QuestConditionType.Collect, 3) } },
            new List<Reward> { new Reward("item005", 1, 150, 80) }),

            new Quest("서브퀘스트", "quest004", "수정 조각 탐사", "숲에서 수정 조각을 찾아보세요.",
            new Dictionary<string, QuestCondition> { { "item022", new QuestCondition(QuestConditionType.Collect, 2) } },
            new List<Reward> { new Reward("", 0, 300, 200) }),

            new Quest("메인퀘스트", "quest005", "유령의 눈 회수", "유령의 눈 2개를 회수하세요.",
            new Dictionary<string, QuestCondition> { { "item023", new QuestCondition(QuestConditionType.Collect, 2) } },
            new List<Reward> { new Reward("", 0, 250, 150) }),

            new Quest("서브퀘스트", "quest006", "늑대 사냥", "숲 속에서 늑대 5마리를 처치하세요.",
            new Dictionary<string, QuestCondition> { { "monster002", new QuestCondition(QuestConditionType.Kill, 5) } },
            new List<Reward> { new Reward("", 0, 100, 50) }),

            new Quest("메인퀘스트", "quest007", "마나 포션 전달", "상인에게 마나 포션 5개를 전달하세요.",
            new Dictionary<string, QuestCondition> { { "item002", new QuestCondition(QuestConditionType.Collect, 5) } },
            new List<Reward> { new Reward("item004", 2, 200, 100) }),

            new Quest("서브퀘스트", "quest008", "강철 방패 제작", "강철 방패를 제작하기 위한 재료를 모으세요.",
            new Dictionary<string, QuestCondition> { { "item026", new QuestCondition(QuestConditionType.Collect, 1) } },
            new List<Reward> { new Reward("item008", 1, 300, 150) }),

            new Quest("메인퀘스트", "quest009", "불꽃의 결정 탐사", "불꽃의 결정을 수집하세요.",
            new Dictionary<string, QuestCondition> { { "item026", new QuestCondition(QuestConditionType.Collect, 3) } },
            new List<Reward> { new Reward("", 0, 400, 200) }),

            new Quest("서브퀘스트", "quest010", "고블린의 손도끼 회수", "고블린의 손도끼를 회수하세요.",
            new Dictionary<string, QuestCondition> { { "item049", new QuestCondition(QuestConditionType.Collect, 1) } },
            new List<Reward> { new Reward("item001", 2, 100, 50) }),

            new Quest("메인퀘스트", "quest011", "마법 검 제작", "마법 검 제작 재료를 모으세요.",
            new Dictionary<string, QuestCondition> { { "item022", new QuestCondition(QuestConditionType.Collect, 2) } },
            new List<Reward> { new Reward("item029", 1, 500, 300) }),

            new Quest("서브퀘스트", "quest012", "황금 가루 수집", "황금 가루를 2개 수집하세요.",
            new Dictionary<string, QuestCondition> { { "item030", new QuestCondition(QuestConditionType.Collect, 2) } },
            new List<Reward> { new Reward("", 0, 200, 100) }),

            new Quest("메인퀘스트", "quest013", "룬검 강화", "룬검을 강화하기 위한 재료를 모으세요.",
            new Dictionary<string, QuestCondition> { { "item017", new QuestCondition(QuestConditionType.Collect, 1) } },
            new List<Reward> { new Reward("item014", 1, 600, 400) }),

            new Quest("서브퀘스트", "quest014", "늑대의 송곳니 회수", "늑대의 송곳니를 4개 수집하세요.",
            new Dictionary<string, QuestCondition> { { "item025", new QuestCondition(QuestConditionType.Collect, 4) } },
            new List<Reward> { new Reward("", 0, 120, 70) }),

            new Quest("메인퀘스트", "quest015", "전설의 검 탐사", "전설의 검을 회수하세요.",
            new Dictionary<string, QuestCondition> { { "location_003", new QuestCondition(QuestConditionType.Explore, 1) } },
            new List<Reward> { new Reward("item020", 1, 700, 500) }),

            new Quest("서브퀘스트", "quest016", "유령 몬스터 처치", "유령 몬스터 10마리를 처치하세요.",
            new Dictionary<string, QuestCondition> { { "monster006", new QuestCondition(QuestConditionType.Kill, 10) } },
            new List<Reward> { new Reward("", 0, 150, 50) }),

            new Quest("메인퀘스트", "quest017", "전설의 증표 회수", "전설의 증표를 회수하세요.",
            new Dictionary<string, QuestCondition> { { "item050", new QuestCondition(QuestConditionType.Collect, 1) } },
            new List<Reward> { new Reward("", 0, 800, 500) }),

            new Quest("서브퀘스트", "quest018", "마법의 잉크 회수", "마법의 잉크를 회수하세요.",
            new Dictionary<string, QuestCondition> { { "item027", new QuestCondition(QuestConditionType.Collect, 1) } },
            new List<Reward> { new Reward("", 0, 150, 80) }),

            new Quest("메인퀘스트", "quest019", "마을 방어", "몬스터의 공격으로부터 마을을 방어하세요.",
            new Dictionary<string, QuestCondition> { { "monster_005", new QuestCondition(QuestConditionType.Kill, 10) } },
            new List<Reward> { new Reward("", 0, 400, 200) }),

            new Quest("서브퀘스트", "quest020", "약초 수집", "치료제를 만들기 위한 약초를 수집하세요.",
            new Dictionary<string, QuestCondition> { { "item_101", new QuestCondition(QuestConditionType.Collect, 5) } },
            new List<Reward> { new Reward("", 0, 50, 20) })
        };
            return questList;
        }

        public Quest GenerateQuests()
        {
            return baseDatabase[Random.Range(0, baseDatabase.Count)];
        }

        public List<NPCData> GenerateRandomNPCs(int numberOfNPCs, List<Quest> availableQuests, List<Item> availableItems)
        {
            List<NPCData> mainNpcList = new List<NPCData>();
            List<NPCData> npcList = new List<NPCData>();

            for (int i = 0; i < numberOfNPCs; i++)
            {
                string npcName = $"NPC_{i + 1}";
                NPCType npcType = (NPCType)Random.Range(0, System.Enum.GetValues(typeof(NPCType)).Length);
                NPCState npcState = NPCState.중립;
                NPCData npc = new NPCData
                {
                    id = $"NPC_{i + 1}",
                    name = npcName,
                    npcType = npcType,
                    currentState = npcState
                };
                switch (npcType)
                {
                    case NPCType.상점:
                        npc.isShop = true;
                        npc.isInteractable = true;
                        npc.items = GenerateRandomItems(availableItems, 3, 6); // 3~6개의 랜덤 아이템
                        npc.description = "아이템을 판매하는 상점 NPC입니다.";
                        break;

                    case NPCType.퀘스트:
                        npc.isQuestGiver = true;
                        npc.isInteractable = true;
                        npc.quests = GenerateRandomQuests(availableQuests, 1, 3); // 1~3개의 랜덤 퀘스트
                        npc.description = "퀘스트를 제공하는 NPC입니다.";
                        break;

                    case NPCType.정보제공:
                        npc.isInteractable = true;
                        npc.description = "게임 정보를 제공하는 NPC입니다.";
                        npc.dialogue = new[] { "저기 동굴에 보물이 있습니다.", "이 지역의 몬스터는 불에 약합니다." };
                        break;

                    case NPCType.상호작용:
                        npc.description = "특정 장치를 작동시키는 NPC입니다.";
                        npc.isInteractable = true;
                        npc.interactionCondition = "특정 아이템 필요";
                        break;
                    case NPCType.힐러:
                        npc.isInteractable = true;
                        npc.description = "플레이어를 회복시켜주는 NPC입니다.";
                        npc.dialogue = new[] { "필요한 치료가 있으신가요?", "안전을 기원합니다." };
                        // npc.items = GenerateRandomItems(availableItems.Where(item => item.type == ItemType.회복형).ToList(), 2, 5);
                        break;
                    case NPCType.적NPC:
                        npc.currentState = NPCState.적;
                        npc.description = "플레이어를 공격할 수 있는 적대적 NPC입니다.";
                        break;

                    case NPCType.동료:
                        npc.currentState = NPCState.동료;
                        npc.description = "플레이어의 동료가 될 수 있는 NPC입니다.";
                        break;

                    default:
                        npc.isInteractable = true;
                        npc.description = "일반 대화만 가능한 NPC입니다.";
                        break;
                }
                npcList.Add(npc);
            }

            return npcList;
        }

        private Item[] GenerateRandomItems(List<Item> availableItems, int min, int max)
        {
            if (availableItems == null || availableItems.Count == 0)
            {
                Debug.LogWarning("아이템 목록이 비어 있습니다. 아이템을 생성할 수 없습니다.");
                return new Item[0]; // 빈 배열 반환
            }
            int itemCount = Mathf.Clamp(Random.Range(min, max + 1), 0, availableItems.Count);
            Item[] items = new Item[itemCount];

            for (int i = 0; i < itemCount; i++)
            {
                items[i] = availableItems[Random.Range(0, availableItems.Count)];
            }

            return items;
        }

        //private Item[] GenerateRandomItems(List<Item> availableItems, int min, int max)
        //{
        //    int itemCount = Random.Range(min, max + 1);
        //    Item[] items = new Item[itemCount];
        //    for (int i = 0; i < itemCount; i++)
        //    {
        //        Item randomItem;
        //        do
        //        {
        //            randomItem = availableItems[Random.Range(0, availableItems.Count)];
        //        }
        //        while (randomItem.type != ItemType.소비형); // 소비형 아이템만 선택
        //        items[i] = randomItem;
        //    }
        //    return items;
        //}


        private Quest[] GenerateRandomQuests(List<Quest> availableQuests, int min, int max)
        {
            int questCount = Random.Range(min, max + 1);
            Quest[] quests = new Quest[questCount];

            bool hasMainQuest = false;

            for (int i = 0; i < questCount; i++)
            {
                Quest randomQuest = availableQuests[Random.Range(0, availableQuests.Count)];
                if (randomQuest.questType == "메인퀘스트")
                {
                    if (hasMainQuest)
                    {
                        i--;
                        continue;
                    }
                    else
                    {
                        hasMainQuest = true;
                    }
                }
                quests[i] = randomQuest;
            }
            return quests;
        }

        private Quest[] GenerateRandomQuests(List<Quest> availableQuests, int min, int max, string questTypeFilter = null)
        {
            int questCount = Random.Range(min, max + 1);
            Quest[] quests = new Quest[questCount];

            for (int i = 0; i < questCount; i++)
            {
                Quest randomQuest = availableQuests[Random.Range(0, availableQuests.Count)];

                if (!string.IsNullOrEmpty(questTypeFilter) && randomQuest.questType != questTypeFilter)
                {
                    i--; // 필터에 맞지 않는 퀘스트를 다시 선택
                    continue;
                }

                quests[i] = randomQuest;
            }
            return quests;
        }

        protected override void HandleGameStateChange(global::GameSystemState newState, object additionalData)
        {

        }
    }
}
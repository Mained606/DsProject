using System.Collections.Generic;
using UnityEngine;

public enum QuestType
{
    Main,
    Sub,
    History
}

public class TestQuestList : MonoBehaviour
{
    [SerializeField] public List<QuestTable> questList = new List<QuestTable>();

    private void Awake()
    {
        InitializeQuests();
    }

    /// <summary>
    /// 퀘스트 데이터 초기화
    /// </summary>
    private void InitializeQuests()
    {
        questList.Add(new QuestTable
        {
            quest_index = 1,
            quest_chapter = 1, // 메인 퀘스트
            quest_stringIdx = 1001,
            quest_levelCondition = 1,
            quest_previousQuestCompleted = 0,
            quest_requirement = new List<RequireData>
            {
                new RequireData { RequireDataTypetype = 1, targetID = 101, targetQuantity = 5, isComplete = false }
            },
            quest_gold = 100,
            quest_exp = 200,
            quest_reward = new RewardItemData[]
            {
                new RewardItemData { itemId = 301, itemQuantity = 1 }
            }
        });

        questList.Add(new QuestTable
        {
            quest_index = 2,
            quest_chapter = 2, // 서브 퀘스트
            quest_stringIdx = 1002,
            quest_levelCondition = 3,
            quest_previousQuestCompleted = 1,
            quest_requirement = new List<RequireData>
            {
                new RequireData { RequireDataTypetype = 2, targetID = 401, targetQuantity = 3, isComplete = false }
            },
            quest_gold = 200,
            quest_exp = 300,
            quest_reward = new RewardItemData[]
            {
                new RewardItemData { itemId = 302, itemQuantity = 2 }
            }
        });

        questList.Add(new QuestTable
        {
            quest_index = 3,
            quest_chapter = 3, // 히스토리 퀘스트
            quest_stringIdx = 1003,
            quest_levelCondition = 3,
            quest_previousQuestCompleted = 1,
            quest_requirement = new List<RequireData>
            {
                new RequireData { RequireDataTypetype = 2, targetID = 401, targetQuantity = 3, isComplete = false }
            },
            quest_gold = 200,
            quest_exp = 300,
            quest_reward = new RewardItemData[]
            {
                new RewardItemData { itemId = 302, itemQuantity = 2 }
            }
        });
    }

    /// <summary>
    /// 전체 퀘스트 리스트를 반환
    /// </summary>
    public List<QuestTable> GetQuestList()
    {
        return questList;
    }
}

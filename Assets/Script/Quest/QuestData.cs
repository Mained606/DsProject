using System.Collections.Generic;

[System.Serializable]
public class QuestTable
{
    public int quest_index;
    public int quest_chapter;
    public int quest_stringIdx;
    public int quest_levelCondition;
    public int quest_previousQuestCompleted;
    public List<RequireData> quest_requirement;
    public int quest_reward;
}
[System.Serializable]
public class RequireDataWrapper
{
    public RequireData[] RequireDatas; // Use an array to wrap the list
}
[System.Serializable]
public class RequireData
{
    public RequireDataType type;
    public int targetID;
    public int targetQuantity;
    public int rewardItem;
    public int rewardItemQuantity;
    public bool isComplete;

    
}


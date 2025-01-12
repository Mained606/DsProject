using System.Collections.Generic;

[System.Serializable]
public class QuestTable
{
    public int quest_index; // 키 값
    public int quest_chapter; // 챕터 구분
    public int quest_stringIdx;// 퀘스트 스트링 데이터
    public int quest_levelCondition; // 퀘스트 레벨 요구 조건
    public int quest_previousQuestCompleted; // 이전 퀘스트 클리어 확인
    public List<RequireData> quest_requirement; // 퀘스트 수행 데이터
    public int quest_gold; // 클리어 시 골드
    public int quest_exp; // 클리어 시 경험치
    public List<RewardItemData> quest_reward; // 클리어 시 아이템
    public List<string> quest_diologue; // 퀘스트 진행 대사
}
[System.Serializable]
public class RequireData
{
    public RequireDataType type; // 수행 타입
    public int targetID; // 타겟 아이디
    public int targetQuantity; // 타겟 카운트
    public int rewardItem; // 요구 조건 수행 시 지급 아이템
    public int rewardItemQuantity; // 요구 조건 수행 시 지급 아이템 개수
    public bool isComplete; // 클리어 확인
}


[System.Serializable]
public class RewardItemData
{
    public int itemId; // 아이템 아이디
    public int itemQuantity; // 아이템 개수
}
[System.Serializable]
public class QuestDiologue
{
    public int context; // 대사 대상 아이디 1 : Player, 나머지 Npc
    public string dialogue; // 대사
}

[System.Serializable]
public class RequireDataWrapper
{
    public RequireData[] RequireDatas; // Use an array to wrap the list
}


[System.Serializable]
public class RewardDataWrapper
{
    public RewardItemData[] RewardItemDatas; // Use an array to wrap the list
}
[System.Serializable]
public class QuestDiologueWrapper
{
    public QuestDiologue[] QuestDiologueData; // Use an array to wrap the list
}





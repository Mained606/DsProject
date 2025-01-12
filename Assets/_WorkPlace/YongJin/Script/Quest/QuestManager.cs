using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class QuestManager : BaseManager<QuestManager>
{
    private string googleSheetUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vSHXZuSvLw5TT-985Q3AN_Z28JRTikJr6uiAoJBAlWxKcic0ROz-5wRpNRBAxEPVw/pub?gid=1391226435&single=true&output=csv";
    
    
    public List<QuestTable> currentQuests{get; private set;} = new List<QuestTable>();
    public List<QuestTable> prevQuests{get; private set;} = new List<QuestTable>();

    public static QuestManager instance;
    #region Init
    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(LoadGoogleSheet());  
    }
    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        switch (newState)
        {
        
            default:
                Debug.Log("QuestManager: 특별한 UI 작업 없음.");
                break;
        }
    }
    IEnumerator LoadGoogleSheet()
    {
        UnityWebRequest request = UnityWebRequest.Get(googleSheetUrl);
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string csvData = request.downloadHandler.text;
            ParseCSV(csvData);
        }
        else
        {
            Debug.LogError($"Failed to load Google Sheet: {request.error}");
        }
    }

    private void ParseCSV(string csvData)
    {
        string[] rows = csvData.Split('\n');
        for (int i = 1; i < rows.Length; i++) // Skip header row
        {
            string[] cells = rows[i].Split(',');
            if (cells.Length < 9) continue; // Skip malformed rows
            QuestTable quest = new QuestTable
            {
                quest_index = int.Parse(cells[0]),
                quest_chapter = int.Parse(cells[1]),
                quest_stringIdx = int.Parse(cells[2]),
                quest_levelCondition = int.Parse(cells[3]),
                quest_previousQuestCompleted = int.Parse(cells[4]),
                quest_requirement = ParseQuestRequirements(cells[5]),
                quest_exp = int.Parse(cells[6]),
                quest_gold = int.Parse(cells[7]),
                quest_reward = ParseReward(cells[8]),
            };

            
            currentQuests.Add(quest);
        }
    }
    
    private List<RequireData> ParseQuestRequirements(string json)
    {
        string fixedJson = FixSlashesInJSON(json);
        string cleanedJson = CleanJSON(fixedJson);
        string wrappedJson = $"{{\"RequireDatas\":{cleanedJson}}}";
        RequireDataWrapper wrapper = JsonUtility.FromJson<RequireDataWrapper>(wrappedJson);
        return new List<RequireData>(wrapper.RequireDatas);
    }
    private List<RewardItemData> ParseReward(string json)
    {
        string fixedJson = FixSlashesInJSON(json);
        string cleanedJson = CleanJSON(fixedJson);
        string wrappedJson = $"{{\"RewardItemDatas\":{cleanedJson}}}";
        Debug.Log(wrappedJson);
        RewardDataWrapper rewardWrapper = JsonUtility.FromJson<RewardDataWrapper>(wrappedJson);
        return new List<RewardItemData>(rewardWrapper.RewardItemDatas);
    }
    private string FixSlashesInJSON(string rawJson) => rawJson.Replace("/", ",");
   
    private string CleanJSON(string rawJson)
    {
        rawJson = rawJson.Trim();
        if (rawJson.StartsWith("\"") && rawJson.EndsWith("\""))
        {
            rawJson = rawJson.Substring(1, rawJson.Length - 2);
        }
        rawJson = rawJson.Replace("\"\"", "\"");
        return rawJson;
    }
    #endregion

    #region Function

    public QuestTable GetQuestIdToQuestTable(int id) => currentQuests.Find(x => x.quest_index == id);
    public QuestTable FindPrevQuest(int id) => prevQuests.Find(x => x.quest_index == id);
    public void SuccessQuest(int clearedQuestId)
    {
        currentQuests.Remove(GetQuestIdToQuestTable(clearedQuestId));
        prevQuests.Add(GetQuestIdToQuestTable(clearedQuestId));
    }
    #endregion
    
}
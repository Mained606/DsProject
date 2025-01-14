using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

public class QuestManager : BaseManager<QuestManager>
{

    #region Varient
    private int completedLoads = 0;
    private const int TOTAL_LOADED = 2;
    private string QuestTableUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vSHXZuSvLw5TT-985Q3AN_Z28JRTikJr6uiAoJBAlWxKcic0ROz-5wRpNRBAxEPVw/pub?gid=1391226435&single=true&output=csv";
    private string QuestStringTableUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRRAtTzgMnM5T6Lecvm3TvZlkoZQ8ksdrSRoFDqCY_CRNwnwfIC-ORnfsUjQTJ2bw/pub?gid=1391226435&single=true&output=csv";

    public int nextMainQuestId = 100000;
    
    
    public List<QuestTable> questData{get; private set;} = new List<QuestTable>();
    public List<QuestStringTable> questStringData{get; private set;} = new List<QuestStringTable>();
    public List<QuestTable> currnetQuests = new List<QuestTable>();
    public List<QuestTable> prevQuests = new List<QuestTable>();
    public event EventHandler OnInitQuestManager;
    //{get; private set;} = new List<QuestTable>();
    #endregion

    #region Init
    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(LoadQuestTableSheet());  
        StartCoroutine(LoadQuestStringSheet());
        Debug.Log(Instance);
    }
    
    private IEnumerator LoadQuestTableSheet()
    {
        yield return FetchGoogleSheet(QuestTableUrl, ParseQuestTableCSV);
        CheckDataLoadCompletion();
    }

    private IEnumerator LoadQuestStringSheet()
    {
        yield return FetchGoogleSheet(QuestStringTableUrl, ParseQuestStringTableCSV);
        CheckDataLoadCompletion();
    }
    private void CheckDataLoadCompletion()
    {
        completedLoads++;
        if (completedLoads >= TOTAL_LOADED)
        {
            Debug.Log("모든 퀘스트 데이터 이니셜라이즈 성공");
            OnInitQuestManager?.Invoke(this, EventArgs.Empty);
            
        }
    }
   
    private IEnumerator FetchGoogleSheet(string url, System.Action<string> onSuccess)
    {
        int retries = 3;
        while (retries > 0)
        {
            UnityWebRequest request = UnityWebRequest.Get(url);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
                yield break;
            }
            else
            {
                Debug.LogError($"스프라이트 시트 로드 실패 {url}: {request.error}");
                retries--;
                if (retries > 0)
                {
                    Debug.Log("Retrying...");
                    yield return new WaitForSeconds(1f);
                }
                else
                {
                    Debug.LogError("로드 실패");
                }
            }
        }
    }



    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        switch (newState)
        {
        
            default:
                Debug.Log("QuestManager: 특별한 작업 없음.");
                break;
        }
    }

    private void ParseQuestTableCSV(string csvData)
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
            
            questData.Add(quest);
        }
    }
    private void ParseQuestStringTableCSV(string csvData)
    {
        string[] rows = csvData.Split('\n');
        for (int i = 1; i < rows.Length; i++) // Skip header row
        {
            string[] cells = rows[i].Split(',');
            if (cells.Length < 3) continue; // Skip malformed rows
            QuestStringTable quest = new QuestStringTable
            {
                questString_index = int.Parse(cells[0]),
                questString_title = cells[1],
                questString_desc = cells[2],
                questString_diologue = ParseQuestDialogue(cells[3]),
                
            };
            questStringData.Add(quest);
        }
    }
    
    private List<RequireData> ParseQuestRequirements(string json)
    {
        string wrappedJson = CleanAndWrapJSON(json, "RequireDatas");
        if (string.IsNullOrEmpty(wrappedJson)) return null;
        RequireDataWrapper wrapper = JsonUtility.FromJson<RequireDataWrapper>(wrappedJson);
        return new List<RequireData>(wrapper.RequireDatas);
    }
    private List<QuestDialogue> ParseQuestDialogue(string json)
    {    
        string wrappedJson = CleanAndWrapJSON(json, "QuestDialogueData");
        if (string.IsNullOrEmpty(wrappedJson)) return null;
        QuestDialogueWrapper dialogueWrapper = JsonUtility.FromJson<QuestDialogueWrapper>(wrappedJson);
        return new List<QuestDialogue>(dialogueWrapper.QuestDialogueData);
    }
    private List<RewardItemData> ParseReward(string json)
    {
        string wrappedJson = CleanAndWrapJSON(json, "RewardItemDatas");
        if (string.IsNullOrEmpty(wrappedJson)) return new List<RewardItemData>();
        RewardDataWrapper rewardWrapper = JsonUtility.FromJson<RewardDataWrapper>(wrappedJson);
        return new List<RewardItemData>(rewardWrapper.RewardItemDatas);
    }
    

    private string CleanAndWrapJSON(string json, string wrapperKey)
    {
        try
        {
            string fixedJson = FixSlashesInJSON(json);
            string cleanedJson = CleanJSON(fixedJson);
            return $"{{\"{wrapperKey}\":{cleanedJson}}}";
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"제이손 클리어 애러: {json} - 애러 메시지: {ex.Message}");
            return string.Empty;
        }
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

    public QuestTable GetQuestIdToQuestTable(int id) => questData.Find(x => x.quest_index == id);
    //public QuestTable FindPrevQuest(int id) => prevQuests.Find(x => x.quest_index == id);
    public QuestStringTable GetQuestStringData(int id) => questStringData.Find(x => x.questString_index == id);
    
    public void SuccessQuest(int clearedQuestId)
    {
        currnetQuests.Remove(GetQuestIdToQuestTable(clearedQuestId));
        prevQuests.Add(GetQuestIdToQuestTable(clearedQuestId));
    }
    
    
    #endregion
    
}
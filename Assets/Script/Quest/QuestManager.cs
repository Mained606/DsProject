using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class QuestManager : MonoBehaviour
{
    private string googleSheetUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vSHXZuSvLw5TT-985Q3AN_Z28JRTikJr6uiAoJBAlWxKcic0ROz-5wRpNRBAxEPVw/pub?gid=1391226435&single=true&output=csv";
    public List<QuestTable> quests{get; private set;} = new List<QuestTable>();

    public static QuestManager instance;
    private void Awake() 
    {
        instance = this;
        StartCoroutine(LoadGoogleSheet());  
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
            if (cells.Length < 7) continue; // Skip malformed rows
            QuestTable quest = new QuestTable
            {
                quest_index = int.Parse(cells[0]),
                quest_chapter = int.Parse(cells[1]),
                quest_stringIdx = int.Parse(cells[2]),
                quest_levelCondition = int.Parse(cells[3]),
                quest_previousQuestCompleted = int.Parse(cells[4]),
                quest_requirement = ParseQuestRequirements(cells[5])
            };
            quests.Add(quest);
        }
    }
    
    private List<RequireData> ParseQuestRequirements(string json)
    {
        string fixedJson = FixSlashesInJSON(json);
        // Clean the JSON string further if needed (e.g., for escaped quotes)
        string cleanedJson = CleanJSON(fixedJson);
        // Wrap the JSON array into an object
        string wrappedJson = $"{{\"RequireDatas\":{cleanedJson}}}";
        // Parse the JSON
        RequireDataWrapper wrapper = JsonUtility.FromJson<RequireDataWrapper>(wrappedJson);
        return new List<RequireData>(wrapper.RequireDatas);
    }
    private string FixSlashesInJSON(string rawJson)
    {
        // Replace all slashes with commas
        return rawJson.Replace("/", ",");
    }
    private string CleanJSON(string rawJson)
    {
        // Remove the outer quotes
        rawJson = rawJson.Trim();

        if (rawJson.StartsWith("\"") && rawJson.EndsWith("\""))
        {
            rawJson = rawJson.Substring(1, rawJson.Length - 2);
        }

        // Replace escaped quotes with actual quotes
        rawJson = rawJson.Replace("\"\"", "\"");
        return rawJson;
    }
    
}
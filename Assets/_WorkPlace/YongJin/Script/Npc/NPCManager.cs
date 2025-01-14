using UnityEngine;
using System;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class NPCManager : BaseManager<NPCManager>
{
    #region Varient
    private int completedLoads = 0;
    private const int TOTAL_LOADED = 2;
    private string NPCTableUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRJIS8cTXNKARaeGUWbkbkrGiwCDnPjpStyOvnPJ8LnSFBUb9Sb8CQAYgtvx3Qzxw/pub?gid=1391226435&single=true&output=csv";
    private string NPCStringTableUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQOgtk2B8kDnyVUP4Hw6aDTw7J9ozsL9OkVAe1iP_6lTgNOhGopb8FbrKQSUQ3wJA/pub?gid=1391226435&single=true&output=csv";
    public List<NPCTable> NPCData{get; private set;} = new List<NPCTable>();
    public List<NPCStringTable> NPCStringData{get; private set;} = new List<NPCStringTable>();

    public event EventHandler OnInitNpcManager;
    #endregion 
    
    protected override void Awake()
    {
        base.Awake();
        StartCoroutine(LoadNPCTableSheet());  
        StartCoroutine(LoadNPCStringSheet());  
      
    }
    private IEnumerator LoadNPCTableSheet()
    {
        yield return FetchGoogleSheet(NPCTableUrl, ParseNPCTableCSV);
        CheckDataLoadCompletion();
    }

    private IEnumerator LoadNPCStringSheet()
    {
        yield return FetchGoogleSheet(NPCStringTableUrl, ParseNPCStringTableCSV);
        CheckDataLoadCompletion();
    }
    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {  
        switch (newState)
        {
            default:
                Debug.Log("NPCManager: 특별한 작업 없음.");
                break;
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
    private void ParseNPCTableCSV(string csvData)
    {
        
        string[] rows = csvData.Split('\n');
        
        for (int i = 1; i < rows.Length; i++) // Skip header row
        {
            string[] cells = rows[i].Split(',');
            
            if (cells.Length < 4) continue; // Skip malformed rows
            NPCTable NPC = new NPCTable
            {
                NPC_index = int.Parse(cells[0].Trim()),
                NPC_stringIdx = int.Parse(cells[1].Trim()),
                NPC_type = (NPCType)int.Parse(cells[2].Trim()),
                NPC_questIds = ParseQuestIds(cells[3]),
                NPC_isInteractable = false,
                NPC_isMainQuestGiver = false,
                NPC_isShop = false,
            };
            NPCData.Add(NPC);
        }
    }
    private List<int> ParseQuestIds(string json)
    {
        string wrappedJson = CleanAndWrapJSON(json, "NPCQuestIdDatas");
        if (string.IsNullOrEmpty(wrappedJson)) return null;
        NPCQuestIdsWrapper wrapper = JsonUtility.FromJson<NPCQuestIdsWrapper>(wrappedJson);
        return new List<int>(wrapper.NPCQuestIdDatas);
    }
    private void ParseNPCStringTableCSV(string csvData)
    {
        string[] rows = csvData.Split('\n');
        for (int i = 1; i < rows.Length; i++) // Skip header row
        {
            
            string[] cells = rows[i].Split(',');
            
            if (cells.Length < 4) continue; // Skip malformed rows
            NPCStringTable NPCString = new NPCStringTable
            {
                NPCString_index = int.Parse(cells[0]),
                NPCString_name = cells[1],
                NPCString_desc = cells[2],
                NPCString_dialogue = ParseNPCStringDialogues(cells[3]),
            };
            
            NPCStringData.Add(NPCString);
        }
    }
    private List<string> ParseNPCStringDialogues(string json)
    {
        
        string wrappedJson = CleanAndWrapJSON(json, "NPCStringDialogueDatas");
        if (string.IsNullOrEmpty(wrappedJson)) return null;
        
        NPCStringDialogueWrapper wrapper = JsonUtility.FromJson<NPCStringDialogueWrapper>(wrappedJson);
        return new List<string>(wrapper.NPCStringDialogueDatas);
    }

    
    private void CheckDataLoadCompletion()
    {
        completedLoads++;
        if (completedLoads >= TOTAL_LOADED)
        {
            Debug.Log("모든 NPC 데이터 이니셜라이즈 성공");
            OnInitNpcManager?.Invoke(this, EventArgs.Empty);
        }
    }
    private string CleanAndWrapJSON(string json, string wrapperKey)
    {
        try
        {
            string fixedJson = FixSlashesInJSON(json);
            string cleanedJson = CleanJSON(fixedJson);
            return $"{{\"{wrapperKey}\":{cleanedJson}}}";
        }
        catch (Exception ex)
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
    public NPCTable GetNPCIdToNPCTable(int id) => NPCData.Find(x => x.NPC_index == id);
    //public QuestTable FindPrevQuest(int id) => prevQuests.Find(x => x.quest_index == id);
    public NPCStringTable GetNPCStringData(int id) => NPCStringData.Find(x => x.NPCString_index == id);
   
    
}
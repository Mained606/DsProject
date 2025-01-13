using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

public class NPCManager : BaseManager<NPCManager>
{
    public bool isCompletedLoads;
    private int completedLoads = 0;
    private const int TOTAL_LOADED = 2;
    private string NPCTableUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vSHXZuSvLw5TT-985Q3AN_Z28JRTikJr6uiAoJBAlWxKcic0ROz-5wRpNRBAxEPVw/pub?gid=1391226435&single=true&output=csv";
    private string NPCStringTableUrl = "https://docs.google.com/spreadsheets/d/e/2PACX-1vRRAtTzgMnM5T6Lecvm3TvZlkoZQ8ksdrSRoFDqCY_CRNwnwfIC-ORnfsUjQTJ2bw/pub?gid=1391226435&single=true&output=csv";
    public List<NPCTable> NPCData{get; private set;} = new List<NPCTable>();
    public List<NPCStringTable> NPCStringData{get; private set;} = new List<NPCStringTable>();
    
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
        yield return FetchGoogleSheet(NPCStringTableUrl, ParseQuestStringTableCSV);
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
            if (cells.Length < 8) continue; // Skip malformed rows
            NPCTable NPC = new NPCTable
            {
                
            };
            
            this.NPCData.Add(NPC);
        }
    }
    private void ParseQuestStringTableCSV(string csvData)
    {
        string[] rows = csvData.Split('\n');
        for (int i = 1; i < rows.Length; i++) // Skip header row
        {
            string[] cells = rows[i].Split(',');
            if (cells.Length < 4) continue; // Skip malformed rows
            NPCStringTable NPCString = new NPCStringTable
            {
                
            };
            
            NPCStringData.Add(NPCString);
        }
    }
    private void CheckDataLoadCompletion()
    {
        completedLoads++;
        if (completedLoads >= TOTAL_LOADED)
        {
            Debug.Log("모든 NPC 데이터 이니셜라이즈 성공");
            isCompletedLoads = true;
        }
    }
    /* private string CleanAndWrapJSON(string json, string wrapperKey)
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
    } */
   
    
}
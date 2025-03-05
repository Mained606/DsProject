using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Sheets.v4;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

public class GoogleSheetsManager : MonoBehaviour
{
    public static GoogleSheetsManager Instance;

    static readonly string[] Scopes = { SheetsService.Scope.Spreadsheets };
    const string ApplicationName = "NewProject";
    const string SpreadsheetId = "1woj0IW_VZI221kr30cQ3_cU8qdqVEfz2TAHi5x9whnE"; // 구글 시트 ID
    SheetsService service;

    bool isCompleteLoad = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);
    }

    public async void GoogleStart()
    {
        AuthenticateAndInitialize();
        await InitializeQuestData();
    }

    void AuthenticateAndInitialize()
    {
        GoogleCredential credential;
        using (var stream = new FileStream("Assets/Settings/Plugins/NewProject.json", FileMode.Open, FileAccess.Read))
        {
            credential = GoogleCredential.FromStream(stream).CreateScoped(Scopes);
        }

        service = new SheetsService(new BaseClientService.Initializer()
        {
            HttpClientInitializer = credential,
            ApplicationName = ApplicationName,
        });
    }

    async Task InitializeQuestData()
    {
        await LoadRequiredItems(); // 필요 재료 데이터 로드
        await LoadQuestData();     // 퀘스트 데이터 로드
    }

    async Task<IList<IList<object>>> LoadSheetData(string sheetName)
    {
        var range = $"{sheetName}!A2:M"; // 구글 시트 데이터 범위
        var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
        var response = await request.ExecuteAsync();
        return response.Values ?? new List<IList<object>>();
    }

    async Task LoadQuestData()
    {
        var questData = await LoadSheetData("Quest");

        foreach (var row in questData)
        {
            if (row.Count < 6) continue;

            int questId = int.Parse(row[0].ToString());
            string questtype = row[1].ToString();
            string title = row[2].ToString();
            string description = row[3].ToString();
            float limitedTime = float.Parse(row[4].ToString());
            int completionPoints = int.Parse(row[5].ToString());
            int bonusPoints = int.Parse(row[6].ToString());
            //List<QuestRequiredItem> questRequiredItems = GetRequiredItemsForQuest(questId);

            //QuestData quest = new QuestData(questId, questtype, title, description, limitedTime, completionPoints, bonusPoints, questRequiredItems);
            //questDataList.Add(quest);
        }
    }

    async Task LoadRequiredItems()
    {
        var questData = await LoadSheetData("Quest");
    }


    public bool IsCompleteLoad() => isCompleteLoad;
}

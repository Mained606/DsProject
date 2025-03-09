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
        else Destroy(gameObject);
    }

    public async void GoogleStart()
    {
        AuthenticateAndInitialize();
        await LoadAllData();
    }

    void AuthenticateAndInitialize()
    {
        if (service != null) return; // 이미 초기화된 경우 중복 실행 방지

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

    async Task LoadAllData()
    {
        isCompleteLoad = false;

        // Enum에 정의된 모든 데이터를 로드
        List<Task> loadTasks = new List<Task>();
        foreach (DataTypeList dataType in Enum.GetValues(typeof(DataTypeList)))
        {
            loadTasks.Add(LoadData(dataType));
        }

        await Task.WhenAll(loadTasks);
        isCompleteLoad = true;

        Debug.Log("모든 데이터 로드 완료!");
    }

    async Task LoadData(DataTypeList dataType)
    {
        switch (dataType)
        {
            case DataTypeList.CharacterList:
                List<CharacterData> characters = await LoadSheetData<CharacterData>("CharacterList");
                Debug.Log($"캐릭터 데이터 {characters.Count}개 로드 완료");
                break;

            case DataTypeList.ItemList:
                List<Item> items = await LoadSheetData<Item>("ItemList");
                Debug.Log($"아이템 데이터 {items.Count}개 로드 완료");
                break;

            case DataTypeList.QuestList:
                List<Quest> quests = await LoadSheetData<Quest>("QuestList");
                Debug.Log($"퀘스트 데이터 {quests.Count}개 로드 완료");
                break;

            //case DataTypeList.NPCList:
            //    List<NPCData> npcs = await LoadSheetData<NPCData>("NPCList");
            //    Debug.Log($"NPC 데이터 {npcs.Count}개 로드 완료");
            //    break;

            case DataTypeList.SkillList:
                List<Skills> skills = await LoadSheetData<Skills>("SkillList");
                Debug.Log($"스킬 데이터 {skills.Count}개 로드 완료");
                break;

            case DataTypeList.SpawnList:
                List<SpawnData> spawns = await LoadSheetData<SpawnData>("SpawnList");
                Debug.Log($"스폰 데이터 {spawns.Count}개 로드 완료");
                break;

            default:
                Debug.LogWarning($"[GoogleSheetsManager] 알 수 없는 데이터 타입: {dataType}");
                break;
        }
    }

    async Task<List<T>> LoadSheetData<T>(string sheetName) where T : ISheetData, new()
    {
        var range = $"{sheetName}!A2:M"; // 구글 시트 데이터 범위
        var request = service.Spreadsheets.Values.Get(SpreadsheetId, range);
        var response = await request.ExecuteAsync();
        var values = response.Values ?? new List<IList<object>>();

        List<T> dataList = new List<T>();
        foreach (var row in values)
        {
            try
            {
                T data = new T();
                data.ParseData(row);
                dataList.Add(data);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[{sheetName}] 데이터 파싱 오류: {ex.Message}");
            }
        }
        return dataList;
    }

    public bool IsCompleteLoad() => isCompleteLoad;

    private enum DataTypeList
    {
        CharacterList,
        ItemList,
        QuestList,
        //    NPCList,
        SkillList,
        SpawnList,
    }
}

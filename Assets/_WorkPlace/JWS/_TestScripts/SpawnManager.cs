using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }
    [SerializeField] private SpawnnerList spawnnerLists;    // 관리할 스포너 리스트
    [SerializeField] private Text uiText;                   // UI 텍스트
    [SerializeField] private List<NPCSpawner> spawnerList;

    private float uiUpdateInterval = 0.5f;
    private float uiTimer = 0f;

    public static SpawnManager GetInstance()
    {
        if (Instance == null)
        {
            var instances = Resources.FindObjectsOfTypeAll<SpawnManager>();
            if (instances.Length == 0)
            {
                Debug.LogError("SpawnManager 인스턴스가 존재하지 않습니다.");
                return null;
            }
            else if (instances.Length > 1)
            {
                Debug.LogWarning($"SpawnManager가 여러 개 발견되었습니다. 첫 번째 인스턴스를 사용합니다.");
            }
            Instance = instances[0];
        }
        return Instance;
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(this.gameObject);

        foreach (var spawnData in spawnnerLists.spawnDataLists)
        {
            if (!spawnerList.Exists(spawner => spawner.SpawnData == spawnData))
            {
                CreateSpawnerFromData(spawnData);
            }
        }
    }

    private void CreateSpawnerFromData(SpawnData spawnData)
    {
        GameObject spawnerObject = new GameObject(spawnData.spwanName);
        NPCSpawner newSpawner = spawnerObject.AddComponent<NPCSpawner>();
        newSpawner.SpawnData = spawnData;
        spawnerList.Add(newSpawner);

        Debug.Log($"스폰 데이터 '{spawnData.spwanName}'에서 새로운 스포너 생성.");
    }

    private void Update()
    {
        uiTimer += Time.deltaTime;
        if (uiTimer >= uiUpdateInterval)
        {
            uiTimer = 0f;
            UpdateUIText();
        }
    }

    private void UpdateUIText()
    {
        if (uiText != null)
        {
            string uiStatus = "";
            foreach (var spawner in spawnerList)
            {
                uiStatus += $"{spawner.SpawnType}: {spawner.ActiveObjectCount} ";
            }
            uiText.text = uiStatus;
        }
    }

    public void AddNPCSpawner(NPCSpawner nPCSpawner)
    {
        if (!spawnerList.Contains(nPCSpawner))
        {
            spawnerList.Add(nPCSpawner);
        }

        if (spawnnerLists != null && !spawnnerLists.spawnDataLists.Contains(nPCSpawner.SpawnData))
        {
            spawnnerLists.spawnDataLists.Add(nPCSpawner.SpawnData);
        }

        Debug.Log($"NPCSpawner '{nPCSpawner.SpawnData.spwanName}'가 SpawnManager에 등록되었습니다.");
    }
}

[CreateAssetMenu(fileName = "SpawnnerList", menuName = "Ds Project/Spawnner List", order = 1)]
public class SpawnnerList : ScriptableObject
{
    public List<SpawnData> spawnDataLists = new List<SpawnData>();
}

[Serializable]
public class SpawnData
{
    public string spwanName;                // 스폰 데이터 이름
    public SpawnnerType spwanType;          // 스폰 타입
    public SpawnStyle spawnStyle;           // 스폰 스타일
    public Vector2 spaawnSize;
    public Transform spawnTransform;        // 스폰 위치
    public List<GameObject> spawnObjects;   // 스폰 대상 프리팹 리스트
    public bool autoSpawn = true;           // 자동 스폰 여부
    public float initialDelay = 1f;         // 재 스폰 딜레이
    public float spawnInterval = 5f;        // 스폰 주기
    public int maxSpawnCount = 5;           // 최대 스폰 개수
    public float detectionDistance = 30;    // 감지거리
    public TriggerType triggerType;         // 스폰 트리거조건
    public Func<bool> triggerCondition;     // 스폰 조건 (함수)
    public int activeObjectCount;
}

public enum SpawnnerType
{
    None,
    Npc,
    Monster,
    Boss
}

public enum SpawnStyle
{
    None,
    BoxArea,
    CircleArea,
    FollowPath
}

public enum TriggerType
{
    Distance,
    Time,
    Event
}

// spawnData.triggerCondition = () => Vector3.Distance(player.position, spawnData.spawnTransform.position) < 10f;
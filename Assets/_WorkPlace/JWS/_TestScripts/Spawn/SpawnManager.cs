using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }
    [SerializeField] private SpawnnerList spawnnerLists;    // 관리할 스포너 리스트
    [SerializeField] private TextMeshProUGUI uiText;                   // UI 텍스트
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
        spawnerObject.transform.position = spawnData.spawnPosition;
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
            string uiStatus = $"스폰 정보 ( 스폰갯수 : <color=green>{spawnerList.Count}</color>개 )\n";
            foreach (var spawner in spawnerList)
            {
                float distance = Vector3.Distance(GameManager.playerTransform.position, spawner.SpawnData.spawnPosition);
                uiStatus += $"<color=yellow>{spawner.SpawnData.spwanName}</color>  " +
                            $"-  <color=blue>{spawner.SpawnType}</color>  " +
                            $"-  <color=green>{distance:F1}m</color>\n";
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


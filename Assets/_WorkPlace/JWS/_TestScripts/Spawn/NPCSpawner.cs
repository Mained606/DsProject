using System.Collections;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private string spwanName;
    [SerializeField] private SpawnData spawnData;
    [SerializeField] private bool isSaveNPCSpawner;

    private Transform poolParent;
    private BasicTimer spawnDelayTimer;
    private float checkInterval = 1f;
    private bool isInitialized = false;

    public SpawnnerType SpawnType => spawnData.spwanType;
    public SpawnData SpawnData { get { return spawnData; } set { spawnData = value; } }
    public int ActiveObjectCount { get; private set; }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && !string.IsNullOrEmpty(spwanName) && isSaveNPCSpawner)
        {
            isSaveNPCSpawner = false;
            if (spawnData == null)
            {
                Debug.LogWarning($"{spwanName}: SpawnData가 설정되지 않았습니다.");
                return;
            }
            spawnData.spwanName = spwanName;
            spawnData.spawnPosition = transform.position;
            SpawnManager.GetInstance()?.AddNPCSpawner(this);
        }
    }
#endif

    private void Start()
    {
        Initialize();
    }

    public void Initialize()
    {
        poolParent = new GameObject($"{spawnData.spwanType}_Pool").transform;
        poolParent.transform.position = transform.position;
        poolParent.SetParent(transform);

        // 몬스터 풀 초기화
        for (int i = 0; i < spawnData.maxSpawnCount; i++)
        {
            foreach (var monsterName in spawnData.spawnObjects)
            {
                GameObject obj = CharacterManager.Instance.CreatMonster(monsterName, poolParent);
                obj.SetActive(false);
            }
        }

        StartTriggerCheck();
        spawnDelayTimer = new BasicTimer(spawnData.spawnInterval);
        isInitialized = true;
    }

    private void StartTriggerCheck()
    {
        InvokeRepeating(nameof(CheckPlayerDistance), 0f, checkInterval);
    }

    private void CheckPlayerDistance()
    {
        float distance = Vector3.Distance(transform.position, GameManager.playerTransform.position);

        if (distance < spawnData.detectionDistance)
        {
            // 플레이어가 범위 내에 있을 경우
            if (!spawnDelayTimer.IsRunning && ActiveObjectCount == 0)
            {
                SpawnObjectAction();
            }
        }
        else
        {
            // 플레이어가 범위를 벗어난 경우
            DisableActiveMonsters();
        }
    }

    private void SpawnObjectAction()
    {
        if (!spawnDelayTimer.IsRunning && ActiveObjectCount == 0)
        {
            TimerManager.Instance.StartTimer(spawnDelayTimer);
            StartCoroutine(SpawnRoutine());
        }
    }

    private IEnumerator SpawnRoutine()
    {
        while (ActiveObjectCount < spawnData.maxSpawnCount)
        {
            Spawn();
            yield return new WaitForSeconds(spawnData.spawnInterval);
        }
    }

    private void Spawn()
    {
        if (poolParent.childCount > 0)
        {
            Transform firstChild = poolParent.GetChild(0);
            if (!firstChild.gameObject.activeSelf)
            {
                Vector3 spawnPosition = CalculateSpawnPosition();
                firstChild.position = spawnPosition;
                firstChild.rotation = Quaternion.identity;
                firstChild.gameObject.SetActive(true);
                firstChild.SetAsLastSibling();

                ActiveObjectCount++;
            }
        }
    }

    private Vector3 CalculateSpawnPosition()
    {
        switch (spawnData.spawnStyle)
        {
            case SpawnStyle.BoxArea:
                return spawnData.spawnPosition + new Vector3(
                    Random.Range(-spawnData.spaawnSize.x, spawnData.spaawnSize.x),
                    0,
                    Random.Range(-spawnData.spaawnSize.y, spawnData.spaawnSize.y)
                );
            case SpawnStyle.CircleArea:
                Vector2 randomCircle = Random.insideUnitCircle * (spawnData.spaawnSize.x + spawnData.spaawnSize.y);
                return spawnData.spawnPosition + new Vector3(randomCircle.x, 0, randomCircle.y);
            default:
                return spawnData.spawnPosition;
        }
    }

    private void DisableActiveMonsters()
    {
        TimerManager.Instance.StopTimer(spawnDelayTimer);

        for (int i = 0; i < poolParent.childCount; i++)
        {
            Transform child = poolParent.GetChild(i);
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
                child.SetAsFirstSibling();
            }
        }

        ActiveObjectCount = 0;
    }

    private void OnTransformChildrenChanged()
    {
        if (!isInitialized) return;

        ActiveObjectCount = GetActiveObjectCount();
        if (ActiveObjectCount == 0)
        {
            TimerManager.Instance.StartTimer(spawnDelayTimer);
        }
    }

    private int GetActiveObjectCount()
    {
        int activeCount = 0;
        for (int i = 0; i < poolParent.childCount; i++)
        {
            if (poolParent.GetChild(i).gameObject.activeSelf)
            {
                activeCount++;
            }
        }
        return activeCount;
    }
}

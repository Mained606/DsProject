using System.Collections;
using UnityEngine;

public class NPCSpawner : MonoBehaviour
{
    [SerializeField] private string spwanName;
    [SerializeField] private SpawnData spawnData; // 스폰 데이터
    [SerializeField] bool isSaveNPCSpawner; // 스폰 데이터
    private Transform poolParent;                // 풀링을 위한 부모 트랜스폼
    private BasicTimer spawnDelayTimer;          // 스폰 딜레이 타이머
    private float checkInterval = 1f;            // 거리 체크 간격

    public SpawnnerType SpawnType => spawnData.spwanType; // 스폰 타입
    public SpawnData SpawnData { get { return spawnData; } set { spawnData = value; } }  // 스폰 데이터 접근
    public int ActiveObjectCount => GetActiveObjectCount(); // 활성화된 오브젝트 수

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && !string.IsNullOrEmpty(spwanName) && isSaveNPCSpawner)
        {
            isSaveNPCSpawner = false;
            // SpawnData 검증
            if (spawnData == null)
            {
                Debug.LogWarning($"{spwanName}: SpawnData가 설정되지 않았습니다.");
                return;
            }
            spawnData.spwanName = spwanName;
            spawnData.spawnTransform = transform;
            // 스폰 매니저에 등록
            SpawnManager.GetInstance()?.AddNPCSpawner(this);
        }
    }
#endif


    private void Awake()
    {
        Initialize();
    }

    public void Initialize()
    {
        poolParent = new GameObject($"{spawnData.spwanType}_Pool").transform;
        poolParent.SetParent(transform);

        for (int i = 0; i < spawnData.maxSpawnCount; i++)
        {
            foreach (var prefab in spawnData.spawnObjects)
            {
                GameObject obj = Instantiate(prefab, poolParent);
                obj.SetActive(false);
            }
        }
        StartTriggerCheck();
        spawnDelayTimer = new BasicTimer(spawnData.spawnInterval);
    }

    private void StartTriggerCheck()
    {
        InvokeRepeating(nameof(CheckTriggerCondition), 0f, checkInterval);
    }

    private void CheckTriggerCondition()
    {
        switch (spawnData.triggerType)
        {
            case TriggerType.Distance:
                CheckPlayerDistance();
                break;
            case TriggerType.Time:
                if (!spawnDelayTimer.IsRunning)
                {
                    SpawnObjectAction();
                }
                break;
            case TriggerType.Event:

                break;
        }
    }

    private void CheckPlayerDistance()
    {
        float distance = Vector3.Distance(transform.position, GameManager.playerTransform.position);
        if (distance < spawnData.detectionDistance)
        {
            SpawnObjectAction();
        }
        else
        {
            DisableActiveMonsters();
        }
    }

    private void SpawnObjectAction()
    {
        if (!spawnDelayTimer.IsRunning && ActiveObjectCount == 0)
        {
            StartCoroutine(SpawnRoutine());
        }
    }

    private IEnumerator SpawnRoutine()
    {
        TimerManager.Instance.StartTimer(spawnDelayTimer);

        while (ActiveObjectCount < spawnData.maxSpawnCount)
        {
            Spawn();
            yield return new WaitForSeconds(spawnData.spawnInterval);
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
                firstChild.rotation = spawnData.spawnTransform.rotation;
                firstChild.gameObject.SetActive(true);
                firstChild.SetAsLastSibling();
            }
        }
    }

    private Vector3 CalculateSpawnPosition()
    {
        switch (spawnData.spawnStyle)
        {
            case SpawnStyle.BoxArea:
                return spawnData.spawnTransform.position + new Vector3(
                    Random.Range(-spawnData.spaawnSize.x / 2, spawnData.spaawnSize.x / 2),
                    0,
                    Random.Range(-spawnData.spaawnSize.y / 2, spawnData.spaawnSize.y / 2)
                );
            case SpawnStyle.CircleArea:
                Vector2 randomCircle = Random.insideUnitCircle * spawnData.spaawnSize.x;
                return spawnData.spawnTransform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            default:
                return spawnData.spawnTransform.position;
        }
    }

    public void ReturnObjectToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(poolParent);
        obj.transform.SetAsFirstSibling();
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

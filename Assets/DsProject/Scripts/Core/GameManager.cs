using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [SerializeField] private int totalManagers = 10;
    private HashSet<string> readyManagers = new HashSet<string>();
    private bool allManagersReady = false;
    
    public static Transform playerTransform;
    public static Transform DragonTransform;
    
    public static event Action OnAllManagersReadyEvent;
    private static List<ScriptableObject> scriptableObjects = new List<ScriptableObject>();

    //=========================
    [SerializeField] private GameObject memo;
    //=========================

    private void Start()
    {
        StartCoroutine(DelayChangeStateToInventoryChange());
    }
    private IEnumerator DelayChangeStateToInventoryChange()
    {
        yield return new WaitForSeconds(2f);
        memo.SetActive(true);
        GameStateMachine.Instance.ChangeState(GameSystemState.Event);
      
    }
    private void OnDisable()
    {
        SaveAllAssets();
    }


    public static void SaveAllAssets()
    {
#if UNITY_EDITOR
        foreach (var asset in scriptableObjects)
        {
            EditorUtility.SetDirty(asset);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("모든 ScriptableObject 자동 저장 완료.");
#endif
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        Debug.Log($"GameManager 초기화 완료. 총 필요 매니저 수: {totalManagers}");

        ////////////////////////////////////////////////////////////
        //  플레이어 이동 범위를 Terrain안으로 고정하기
        /// JWS 2025.02.02 13:00 추가
        ////////////////////////////////////////////////////////////
        InitTerrainInfo();
        ////////////////////////////////////////////////////////////
        ////////////////////////////////////////////////////////////
    }

    // 각 매니저가 준비 상태를 등록할 때 필요한 메서드
    public void RegisterManager(string managerName)
    {
        if (!readyManagers.Contains(managerName))
        {
            readyManagers.Add(managerName);
            Debug.Log($"{managerName} 준비 완료 ({readyManagers.Count}/{totalManagers})");

            if (readyManagers.Count == totalManagers)
            {
                OnAllManagersReady();
            }
        }
    }
    
    //모든 매니저가 준비되었을 때 호출되는 후속 작업
    private void OnAllManagersReady()
    {
        Debug.Log("모든 매니저가 준비 완료되었습니다. 게임 시작!");
        allManagersReady = true;
        OnAllManagersReadyEvent?.Invoke();
        //GameStateMachine.Instance.ChangeState(GameSystemState.MainQuestPlay);
    }
    
    public bool CanChangeState()
    {
        return allManagersReady;
    }


    ////////////////////////////////////////////////////////////
    //  플레이어 숨기기 위한 코드 추가
    /// JWS 2025.01.28 10:00 수정
    ////////////////////////////////////////////////////////////
    public static bool PlayerVisible(bool active)
    {
        if (playerTransform == null) return false;
        playerTransform.GetChild(0).GetChild(1).gameObject.SetActive(active);
        return true;
    }
    ////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////



    ////////////////////////////////////////////////////////////
    //  플레이어 이동 범위를 Terrain안으로 고정하기
    /// JWS 2025.02.02 13:00 추가
    ////////////////////////////////////////////////////////////
    private float borderMargin = 10f;  // 경계에서 플레이어가 접근할 수 있는 최소 거리
    private Terrain terrain;
    private float minX, maxX, minZ, maxZ;

    private void InitTerrainInfo()
    {
        terrain = Terrain.activeTerrain;
        if (terrain == null)
        {
            Debug.LogError("Terrain이 씬에 없습니다!");
            return;
        }

        Vector3 terrainPos = terrain.transform.position;
        Vector3 terrainSize = terrain.terrainData.size;

        // 플레이어 이동 가능 범위 계산
        minX = terrainPos.x + borderMargin;
        maxX = terrainPos.x + terrainSize.x - borderMargin;
        minZ = terrainPos.z + borderMargin;
        maxZ = terrainPos.z + terrainSize.z - borderMargin;
        // Debug.LogWarning("Terrain정보를 확인했습니다.");
    }

    private void Update()
    {
        RestrictPlayerMovement();
    }

    private void RestrictPlayerMovement()
    {
        Vector3 pos = playerTransform.position;

        // X축 및 Z축 범위 제한
        pos.x = Mathf.Clamp(pos.x, minX, maxX);
        pos.z = Mathf.Clamp(pos.z, minZ, maxZ);

        playerTransform.position = pos; // 위치 업데이트
    }
    ////////////////////////////////////////////////////////////
    ////////////////////////////////////////////////////////////
}
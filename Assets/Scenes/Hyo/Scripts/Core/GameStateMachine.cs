using System;
using UnityEngine;

public class GameStateMachine : MonoBehaviour
{
    // 싱글턴 인스턴스
    public static GameStateMachine Instance { get; private set; }

    // 현재 상태와 이벤트
    // public GameSystemState CurrentState { get; private set; }
    
    // 테스트용 ===============================================================
    [SerializeField]
    private GameSystemState currentState; // 인스펙터에서 변경 가능
    public GameSystemState CurrentState
    {
        get => currentState;
        private set => currentState = value;
    }
    //========================================================================
    
    public static event Action<GameSystemState> OnGameStateChanged;

    private void Awake()
    {
        // 싱글턴 패턴 설정
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 상태 유지
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 상태 변경 메서드
    public void ChangeState(GameSystemState newState, bool forceTrigger = false)
    {
        if (!forceTrigger && CurrentState == newState)
        {
            Debug.Log($"이미 상태가 {newState} 입니다.");
            return;
        }

        Debug.Log($"게임 상태 변경: {CurrentState} → {newState}");
        CurrentState = newState;

        // 이벤트 발행
        OnGameStateChanged?.Invoke(newState);
    }
    
    // 인스펙터 값 변경 시 이벤트 호출 디버그용 =================================
    private void OnValidate()
    {
        if (Application.isPlaying && Instance == this)
        {
            ChangeState(currentState, true);
        }
    }
    //=====================================================================
}
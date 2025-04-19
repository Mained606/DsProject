using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : BaseManager<TimerManager>
{
    private Dictionary<ITimer, Coroutine> runningTimers = new Dictionary<ITimer, Coroutine>();
        
    // 게임이 일시정지 상태인지 확인
    public bool IsGamePaused { get; private set; } = false;

    protected override void Awake()
    {
        base.Awake();
        // 기본 타임스케일 설정
        Time.timeScale = 1f;
    }


    public void StartTimer(ITimer timer, bool autoStart = true)
    {
        if (runningTimers.ContainsKey(timer))
        {
            StopTimer(timer);
        }

        if (autoStart)
        {
            Coroutine timerCoroutine = StartCoroutine(TimerRoutine(timer));
            runningTimers[timer] = timerCoroutine;
        }
    }

    private IEnumerator TimerRoutine(ITimer timer)
    {
        BasicTimer basicTimer = timer as BasicTimer;
        basicTimer.Start();

        while (!timer.IsCompleted)
        {
            if (basicTimer.IsRunning && !basicTimer.IsPaused)
            {
                basicTimer.AddTime(Time.deltaTime);
            }
            yield return null;
        }
        runningTimers.Remove(timer);
    }

    public void StopTimer(ITimer timer)
    {
        if (runningTimers.TryGetValue(timer, out Coroutine timerCoroutine))
        {
            StopCoroutine(timerCoroutine);
            runningTimers.Remove(timer);
            timer.Stop();
        }
    }
    
    // 게임 상태 변경 이벤트 핸들러
    protected override void HandleGameStateChange(GameSystemState newState, object additionalData)
    {
        // InputManager에 있는 IsUIRelatedState 메서드를 활용하여 UI 관련 상태인지 확인
        if (InputManager.Instance != null && InputManager.Instance.IsUIRelatedState(newState))
        {
            // UI 관련 상태일 경우 게임 일시정지
            PauseGame();
        }
        else if (newState == GameSystemState.Play || newState == GameSystemState.Exploration || 
                 newState == GameSystemState.Combat || newState == GameSystemState.BossBattle || newState == GameSystemState.MainMenu)
        {
            // 게임 플레이 상태로 돌아갈 경우 게임 재개
            ResumeGame();
        }
    }

    // 게임 일시정지 메서드
    public void PauseGame()
    {
        if (!IsGamePaused)
        {
            // 현재 타임스케일을 저장하고 0으로 설정
            Time.timeScale = 0f;
            IsGamePaused = true;
            
            //Debug.Log("게임 일시정지: 타임스케일 0으로 설정");
        }
    }

    // 게임 재개 메서드
    public void ResumeGame()
    {
        if (IsGamePaused)
        {
            // 이전 타임스케일로 복원
            Time.timeScale = 1f;
            IsGamePaused = false;
            
            //Debug.Log("게임 재개");
        }
    }
}

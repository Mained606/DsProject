using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;

    // 현재 실행 중인 타이머들을 관리하는 딕셔너리
    private Dictionary<ITimer, Coroutine> runningTimers = new Dictionary<ITimer, Coroutine>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartTimer(ITimer timer, bool autoStart = true)
    {
        if (runningTimers.ContainsKey(timer))
        {
            // 이미 실행 중인 타이머가 있을 경우 기존 코루틴을 중지
            StopTimer(timer);
        }

        if (autoStart)
        {
            Coroutine timerCoroutine = StartCoroutine(TimerRoutine(timer));
            runningTimers[timer] = timerCoroutine; // 타이머를 실행 중으로 등록
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

        // 타이머가 완료되면 딕셔너리에서 제거
        runningTimers.Remove(timer);
    }

    public void StopTimer(ITimer timer)
    {
        if (runningTimers.TryGetValue(timer, out Coroutine timerCoroutine))
        {
            StopCoroutine(timerCoroutine);
            runningTimers.Remove(timer);
            timer.Stop(); // 타이머 상태를 중지로 설정
        }
    }
}

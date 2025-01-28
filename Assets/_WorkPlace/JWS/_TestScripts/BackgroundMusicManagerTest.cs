using UnityEngine;
using System.Collections;

public class BackgroundMusicManager : MonoBehaviour
{
    public AudioSource audioSource1; // 첫 번째 AudioSource
    public AudioSource audioSource2; // 두 번째 AudioSource
    private AudioSource currentSource; // 현재 재생 중인 AudioSource
    private AudioSource nextSource; // 전환될 AudioSource

    public float transitionDuration = 2f; // 디졸브 지속 시간 (초)

    private void Start()
    {
        // 초기 설정: audioSource1을 메인 배경음으로 설정
        currentSource = audioSource1;
        nextSource = audioSource2;

        // 첫 번째 배경음 재생
        currentSource.Play();
    }

    // 배경음 변경 메서드
    public void ChangeBackgroundMusic(AudioClip newClip)
    {
        if (!currentSource.isPlaying) return;

        // 새로운 클립 설정
        nextSource.clip = newClip;
        nextSource.Play();

        // 디졸브 전환 시작
        StartCoroutine(TransitionMusic());
    }

    private IEnumerator TransitionMusic()
    {
        float time = 0f;

        // 디졸브 아웃-디졸브 인 진행
        while (time < transitionDuration)
        {
            time += Time.deltaTime;
            float progress = time / transitionDuration;

            // 현재 소스는 점점 볼륨 감소
            currentSource.volume = Mathf.Lerp(1f, 0f, progress);

            // 새로운 소스는 점점 볼륨 증가
            nextSource.volume = Mathf.Lerp(0f, 1f, progress);

            yield return null; // 한 프레임 대기
        }

        // 전환 완료 후 상태 업데이트
        currentSource.Stop(); // 이전 음악 정지
        currentSource.volume = 1f; // 볼륨 초기화

        // 소스 스왑
        AudioSource temp = currentSource;
        currentSource = nextSource;
        nextSource = temp;
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorySceneManager : MonoBehaviour
{
    public StorySequenceCollection storySequenceCollection;
    private List<SequenceData> sequenceList;
    private int currentSequenceIndex = 0;
    public Text dialogText;
    public Image fadeImage;

    private bool isProcessing = false;
    private StoryState currentState = StoryState.Idle;
    private AudioSource bgmSource;
    private AudioSource sfxSource;

    private void Start()
    {
        bgmSource = gameObject.AddComponent<AudioSource>();
        sfxSource = gameObject.AddComponent<AudioSource>();
        sequenceList = new List<SequenceData>(storySequenceCollection.sequences);
        StartCoroutine(RunStoryScene());
    }

    private void SetState(StoryState newState)
    {
        Debug.Log($"스토리 상태 변경: {currentState} -> {newState}");
        currentState = newState;
    }

    private IEnumerator RunStoryScene()
    {
        if (isProcessing) yield break;
        isProcessing = true;

        while (currentSequenceIndex < sequenceList.Count)
        {
            SequenceData currentSequence = sequenceList[currentSequenceIndex];
            SetState(StoryState.Playing);

            // 1. 화면 전환 (페이드인)
            if (currentSequence.screenEffect.enableFade)
            {
                yield return StartCoroutine(FadeScreen(true, currentSequence.screenEffect.fadeDuration, currentSequence.screenEffect.fadeColor));
            }

            // 2. 사운드 재생
            PlaySound(currentSequence.soundData);

            // 3. 다이얼로그 출력
            yield return StartCoroutine(DisplayDialog(currentSequence.sequencesDialog));

            // 4. 캐릭터 이동
            if (currentSequence.moveData.shouldMove)
            {
                yield return StartCoroutine(MoveCharacter(currentSequence.moveData));
            }

            // 5. 카메라 효과 적용
            if (currentSequence.cameraEffect.enableShake)
            {
                yield return StartCoroutine(CameraShake(currentSequence.cameraEffect.shakeIntensity, currentSequence.cameraEffect.shakeDuration));
            }

            currentSequence.isComplete = true;
            currentSequenceIndex++;

            // 6. 다음 시퀀스 대기 (Space 키 입력 대기)
            SetState(StoryState.Waiting);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
        }

        SetState(StoryState.Completed);
        Debug.Log("스토리씬 완료!");
        OnStoryComplete();
    }

    private IEnumerator DisplayDialog(List<DialogData> dialogs)
    {
        foreach (var dialog in dialogs)
        {
            if (isProcessing) yield break;
            isProcessing = true;
            dialogText.text = $"{dialog.speakerName}: {dialog.dialogText}";
            SetState(StoryState.Waiting);
            yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space));
            isProcessing = false;
        }
    }

    private IEnumerator MoveCharacter(CharacterMoveData moveData)
    {
        if (isProcessing) yield break;
        isProcessing = true;
        SetState(StoryState.Playing);
        Transform character = GameObject.Find("Character").transform;
        character.position = moveData.startPosition;
        float elapsedTime = 0f;
        while (elapsedTime < 1f)
        {
            character.position = Vector3.Lerp(moveData.startPosition, moveData.endPosition, elapsedTime);
            elapsedTime += Time.deltaTime * moveData.moveSpeed;
            yield return null;
        }
        character.position = moveData.endPosition;
        isProcessing = false;
        SetState(StoryState.Waiting);
    }

    private IEnumerator CameraShake(float intensity, float duration)
    {
        if (isProcessing) yield break;
        isProcessing = true;
        SetState(StoryState.Playing);
        Vector3 originalPosition = Camera.main.transform.position;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            float shakeAmount = Mathf.Sin(t * Mathf.PI) * intensity;
            Camera.main.transform.position = originalPosition + Random.insideUnitSphere * shakeAmount;
            yield return null;
        }
        Camera.main.transform.position = originalPosition;
        isProcessing = false;
        SetState(StoryState.Waiting);
    }

    private IEnumerator FadeScreen(bool fadeIn, float duration, Color color)
    {
        if (isProcessing) yield break;
        isProcessing = true;
        SetState(StoryState.Transition);
        float alpha = fadeIn ? 1 : 0;
        float time = 0;
        fadeImage.color = new Color(color.r, color.g, color.b, alpha);
        while (time < duration)
        {
            time += Time.deltaTime;
            alpha = fadeIn ? (1 - (time / duration)) : (time / duration);
            fadeImage.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }
        isProcessing = false;
        SetState(StoryState.Waiting);
    }

    private void PlaySound(SoundData soundData)
    {
        if (soundData.bgmClip != null)
        {
            bgmSource.clip = soundData.bgmClip;
            bgmSource.volume = soundData.bgmVolume;
            bgmSource.loop = soundData.loopBgm;
            bgmSource.Play();
        }
        if (soundData.sfxClip != null)
        {
            sfxSource.PlayOneShot(soundData.sfxClip, soundData.sfxVolume);
        }
    }

    private void OnStoryComplete()
    {
        Debug.Log("스토리씬이 끝났습니다.");
        //SceneManager.LoadScene("NextScene");
    }
}

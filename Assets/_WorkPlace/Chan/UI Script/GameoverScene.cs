using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using TMPro;

public class GameoverScene : MonoBehaviour
{
    [SerializeField] private GameObject Buttons;                   // 버튼 부모
    [SerializeField] private TextMeshProUGUI Description; // 설명 텍스트
    [SerializeField] private SceneFader fader;
    private Button[] buttons;
    private Image[] hoverImages;
    private Coroutine[] fadeCoroutines;

    private readonly string[] hoverDescriptions = new string[]
    {
        "마지막 저장시점으로 돌아갑니다.",
        "메인 메뉴 화면으로 돌아갑니다.",
        "게임을 종료합니다."
        // 버튼 개수에 따라 이 배열 늘리면 됨
    };

    private void OnEnable()
    {
        // 씬 로드 이벤트에 리스너 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 씬 로드 이벤트에서 리스너 제거
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 시 호출되는 메서드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 씬 로드 시 커서 상태 초기화
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void Start()
    {
        buttons = Buttons.GetComponentsInChildren<Button>(true);
        hoverImages = new Image[buttons.Length];
        fadeCoroutines = new Coroutine[buttons.Length];

        for (int i = 0; i < buttons.Length; i++)
        {
            // 버튼 자식 중 텍스트가 아닌 이미지 하나 추출
            var images = buttons[i].GetComponentsInChildren<Image>(true);
            foreach (var img in images)
            {
                if (img.GetComponent<TextMeshProUGUI>() != null) continue;
                if (img.gameObject == buttons[i].gameObject) continue;
                hoverImages[i] = img;
                break;
            }

            SetAlpha(hoverImages[i], 0f);
            AddEventTriggers(buttons[i].gameObject, i);
            
            // 버튼 클릭 이벤트 추가
            int buttonIndex = i;
            buttons[i].onClick.AddListener(() => OnButtonClick(buttonIndex));
        }

        if (Description != null)
        {
            Description.text = ""; // 시작 시 빈 상태
        }
    }

    private void OnButtonClick(int buttonIndex)
    {
        switch (buttonIndex)
        {
            case 0: // RESTART 버튼
                RestartGame();
                break;
            case 1: // TITLE 버튼
                GoToTitleScene();
                break;
            case 2: // EXIT 버튼
                ExitGame();
                break;
        }
    }

    private void RestartGame()
    {
        TimerManager.Instance.ResumeGame();
        GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
        fader.FadeOutOnly(2f);
        // 커서 상태 명시적 설정 - 씬 전환 전에 커서를 보이게 하고 언락함
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // 세이브 데이터 존재 여부 확인
        if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
        {
            //Debug.Log("저장된 게임 데이터가 있습니다. 메인 씬을 재로드하여 저장된 게임을 불러옵니다.");
            
            // 새 게임이 아님을 표시
            SaveSystemInitializer saveSystem = FindFirstObjectByType<SaveSystemInitializer>();
            if (saveSystem != null)
            {
                SaveSystemInitializer.isNewGame = false;
            }
            
            // UI 창 비활성화
            // gameObject.SetActive(false);
            
            // 현재 씬 재로드
            Scene currentScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(currentScene.name);
            
            // 씬이 로드되면 SaveSystemInitializer의 OnSceneLoaded에서 자동으로 저장 데이터를 로드함
        }
        else
        {
            //Debug.Log("저장된 게임 데이터가 없습니다. 새 게임을 시작합니다.");
            
            // UI 창 비활성화
            // gameObject.SetActive(false);
            
            // SaveSystemInitializer를 찾아서 새 게임 시작
            SaveSystemInitializer saveSystem = FindFirstObjectByType<SaveSystemInitializer>();
            if (saveSystem != null)
            {
                saveSystem.StartNewGame();
            }
            else
            {
                Debug.LogError("SaveSystemInitializer를 찾을 수 없습니다.");
                // 타이틀 씬으로 이동 (대안)
                SceneManager.LoadScene("TitleScene");
            }
        }
    }

    private void GoToTitleScene()
    {
        TimerManager.Instance.ResumeGame();
        GameStateMachine.Instance.ChangeState(GameSystemState.MainMenu);
        fader.FadeOutOnly(2f);
        // 커서 상태 명시적 설정 - 씬 전환 전에 커서를 보이게 하고 언락함
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 타이틀 씬으로 이동
        SaveSystemInitializer saveSystem = FindFirstObjectByType<SaveSystemInitializer>();
        if (saveSystem != null)
        {
            SceneManager.LoadScene(saveSystem.titleSceneName);
        }
        else
        {
            SceneManager.LoadScene("TitleScene");
        }
    }

    private void ExitGame()
    {
        // 게임 종료
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private IEnumerator FadeInOnce(Image img, float duration = 1f)
    {
        float elapsed = 0f;
        float targetAlpha = 33f / 255f;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(0f, targetAlpha, elapsed / duration);
            SetAlpha(img, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetAlpha(img, targetAlpha); // 정확하게 고정
    }

    private void AddEventTriggers(GameObject obj, int index)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        // Pointer Enter
        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((_) =>
        {
            if (fadeCoroutines[index] != null)
                StopCoroutine(fadeCoroutines[index]);

            fadeCoroutines[index] = StartCoroutine(FadeInOnce(hoverImages[index]));

            if (Description != null && index < hoverDescriptions.Length)
                Description.text = hoverDescriptions[index];
        });
        trigger.triggers.Add(enterEntry);

        // Pointer Exit
        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((_) =>
        {
            if (fadeCoroutines[index] != null)
            {
                StopCoroutine(fadeCoroutines[index]);
                fadeCoroutines[index] = null;
            }

            SetAlpha(hoverImages[index], 0f);

            if (Description != null)
                Description.text = "";
        });
        trigger.triggers.Add(exitEntry);
    }

}

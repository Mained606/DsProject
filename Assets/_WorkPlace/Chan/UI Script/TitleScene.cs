using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.SceneManagement;

public class TitleScene : MonoBehaviour
{
    [SerializeField] private GameObject Buttons;
    private Button[] buttons;
    private Image[][] buttonSideImages;
    private Coroutine[][] fadeCoroutines;

    [SerializeField] private GameObject Option;

    // SaveSystemInitializer 참조 추가
    private SaveSystemInitializer saveSystem;

    private void OnEnable()
    {
        // 씬 로드 이벤트에 리스너 등록
        SceneManager.sceneLoaded += OnSceneLoaded;
        
        // 활성화 시 즉시 커서 상태 설정
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        // 씬 로드 이벤트에서 리스너 제거
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 시 호출되는 메서드
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 이 스크립트가 타이틀 씬에 있을 때만 실행
        if (scene.name.Contains("Title") || scene.name == "TitleScene")
        {
            // 씬 로드 시 커서 상태 초기화
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Start()
    {
        // 타이틀 씬 시작 시 항상 커서가 보이도록 설정
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        buttons = Buttons.GetComponentsInChildren<Button>(true);
        int buttonCount = buttons.Length;

        buttonSideImages = new Image[buttonCount][];
        fadeCoroutines = new Coroutine[buttonCount][];
        
        // SaveSystemInitializer 찾기 또는 생성
        saveSystem = FindFirstObjectByType<SaveSystemInitializer>();
        if (saveSystem == null)
        {
            GameObject saveSystemObj = new GameObject("SaveSystemInitializer");
            saveSystem = saveSystemObj.AddComponent<SaveSystemInitializer>();
        }

        for (int i = 0; i < buttonCount; i++)
        {
            // 텍스트 제외한 이미지 수집
            var allImages = buttons[i].GetComponentsInChildren<Image>(true);
            List<Image> sideImgs = new();

            foreach (var img in allImages)
            {
                if (img.GetComponentInChildren<TextMeshProUGUI>() != null) continue;
                if (img.GetComponent<TextMeshProUGUI>() != null) continue;
                if (img.gameObject == buttons[i].gameObject) continue;
                sideImgs.Add(img);
            }

            buttonSideImages[i] = sideImgs.ToArray();
            fadeCoroutines[i] = new Coroutine[sideImgs.Count];

            foreach (var img in buttonSideImages[i])
            {
                SetAlpha(img, 0f);
            }
        }

        // 트리거 및 클릭 리스너 분리 실행
        for (int i = 0; i < buttonCount; i++)
        {
            AddEventTriggers(buttons[i].gameObject, i);
            AddClickListener(buttons[i], i);
        }
        
        // 저장된 게임 데이터 확인 및 계속하기 버튼 활성화 설정
        UpdateContinueButtonState();
    }
    
    // 저장된 게임 데이터 유무에 따라 계속하기 버튼 상태 업데이트
    public void UpdateContinueButtonState()
    {
        bool hasSaveData = SaveManager.Instance != null && SaveManager.Instance.HasSaveData();
        
        // 계속하기 버튼 찾기 (버튼의 이름이나 인덱스에 따라 수정 필요)
        // 예시: 두 번째 버튼이 계속하기라면
        if (buttons.Length >= 2)
        {
            buttons[1].interactable = hasSaveData;
            
            // 저장 데이터가 없을 경우 비활성화된 상태 표시
            TextMeshProUGUI buttonText = buttons[1].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = hasSaveData ? 
                    new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 1f) : 
                    new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 0.5f);
            }
        }
    }

    private void SetAlpha(Image img, float alpha)
    {
        if (img == null) return;
        Color c = img.color;
        c.a = alpha;
        img.color = c;
    }

    private IEnumerator FadeInOut(Image img, float duration)
    {
        float time = 0f;
        while (true)
        {
            time += Time.deltaTime;
            float alpha = Mathf.PingPong(time, duration) / duration;
            SetAlpha(img, alpha);
            yield return null;
        }
    }

    private void AddEventTriggers(GameObject obj, int index)
    {
        EventTrigger trigger = obj.GetComponent<EventTrigger>() ?? obj.AddComponent<EventTrigger>();
        trigger.triggers.Clear();

        var enterEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerEnter };
        enterEntry.callback.AddListener((_) =>
        {
            for (int i = 0; i < buttonSideImages[index].Length; i++)
            {
                if (fadeCoroutines[index][i] != null)
                    StopCoroutine(fadeCoroutines[index][i]);

                fadeCoroutines[index][i] = StartCoroutine(FadeInOnce(buttonSideImages[index][i]));
            }
        });
        trigger.triggers.Add(enterEntry);

        var exitEntry = new EventTrigger.Entry { eventID = EventTriggerType.PointerExit };
        exitEntry.callback.AddListener((_) =>
        {
            for (int i = 0; i < buttonSideImages[index].Length; i++)
            {
                if (fadeCoroutines[index][i] != null)
                {
                    StopCoroutine(fadeCoroutines[index][i]);
                    fadeCoroutines[index][i] = null;
                }

                SetAlpha(buttonSideImages[index][i], 0f);
            }
        });
        trigger.triggers.Add(exitEntry);
    }

    private IEnumerator FadeInOnce(Image img, float duration = 0.3f)
    {
        float elapsed = 0f;
        float targetAlpha = 1f; // ✅ 이제 1까지

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(0f, targetAlpha, elapsed / duration);
            SetAlpha(img, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetAlpha(img, targetAlpha); // 완전하게 1로 고정
    }

    private IEnumerator FadeClickFeedback(TextMeshProUGUI text, float duration = 0.2f)
    {
        if (text == null) yield break;

        float elapsed = 0f;
        float originalAlpha = text.color.a;
        float midAlpha = originalAlpha * 0.5f;

        // 0.5로 서서히 감소
        while (elapsed < duration / 2f)
        {
            float alpha = Mathf.Lerp(originalAlpha, midAlpha, elapsed / (duration / 2f));
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        // 0.5 → 원래대로 복구
        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            float alpha = Mathf.Lerp(midAlpha, originalAlpha, elapsed / (duration / 2f));
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        text.color = new Color(text.color.r, text.color.g, text.color.b, originalAlpha);
    }

    private void AddClickListener(Button button, int index)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            // 1. 텍스트 피드백
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
            {
                StartCoroutine(FadeClickFeedback(text));
            }

            // 2. 사이드 이미지 제거
            for (int i = 0; i < buttonSideImages[index].Length; i++)
            {
                if (fadeCoroutines[index][i] != null)
                {
                    StopCoroutine(fadeCoroutines[index][i]);
                    fadeCoroutines[index][i] = null;
                }

                SetAlpha(buttonSideImages[index][i], 0f);
            }

            // 3. 버튼 액션 실행
            ExecuteButtonAction(index);
        });
    }

    // 버튼 인덱스에 따른 동작 실행
    private void ExecuteButtonAction(int buttonIndex)
    {
        if (saveSystem == null) return;
        
        // 버튼 인덱스에 따라 다른 동작 실행 (실제 버튼 순서에 맞게 수정 필요)
        switch (buttonIndex)
        {
            case 0: // 새 게임 버튼
                //Debug.Log("새 게임 시작");
                // 저장 데이터 삭제
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.ResetSaveData();
                    //Debug.Log("기존 저장 데이터가 삭제되었습니다.");
                }
                saveSystem.StartNewGame();
                break;
                
            case 1: // 계속하기 버튼
                if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
                {
                    //Debug.Log("저장된 게임 불러오기");
                    saveSystem.LoadSavedGame();
                }
                else
                {
                    Debug.LogWarning("저장된 게임이 없습니다.");
                }
                break;
                
            case 2: // 설정 버튼
                Option.SetActive(true);
                // 설정 메뉴 관련 코드
                break;
                
            case 3: // 종료 버튼
                //Debug.Log("게임 종료");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
                break;
                
            default:
                //Debug.Log($"버튼 {buttonIndex} 클릭됨");
                break;
        }
    }
    public void opDown()
    {
        Option.SetActive(false);
    }
   
}

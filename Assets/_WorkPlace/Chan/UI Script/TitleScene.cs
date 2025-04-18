using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class TitleScene : MonoBehaviour
{
    [SerializeField] private GameObject Buttons;
    private Button[] buttons;
    private Image[][] buttonSideImages;
    private Coroutine[][] fadeCoroutines;
    
    // SaveSystemInitializer 참조 추가
    private SaveSystemInitializer saveSystem;

    private void Start()
    {
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
                var img = buttonSideImages[index][i];
                if (fadeCoroutines[index][i] != null)
                    StopCoroutine(fadeCoroutines[index][i]);

                fadeCoroutines[index][i] = StartCoroutine(FadeInOut(img, 1.5f));
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

    private void AddClickListener(Button button, int index)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() =>
        {
            // 1. 모든 버튼 텍스트 복원
            for (int i = 0; i < buttons.Length; i++)
            {
                if (i == index) continue;

                var otherText = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
                if (otherText != null)
                {
                    Color c = otherText.color;
                    c.a = 1f;
                    otherText.color = c;
                }
            }

            // 2. 현재 버튼 텍스트 페이드
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null && text.color.a > 0.95f)
                StartCoroutine(FadeOutText(text));

            // 3. 현재 버튼 이미지 즉시 제거
            for (int i = 0; i < buttonSideImages[index].Length; i++)
            {
                if (fadeCoroutines[index][i] != null)
                {
                    StopCoroutine(fadeCoroutines[index][i]);
                    fadeCoroutines[index][i] = null;
                }

                SetAlpha(buttonSideImages[index][i], 0f);
            }
            
            // 4. 버튼 액션 실행
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
                Debug.Log("새 게임 시작");
                // 저장 데이터 삭제
                if (SaveManager.Instance != null)
                {
                    SaveManager.Instance.ResetSaveData();
                    Debug.Log("기존 저장 데이터가 삭제되었습니다.");
                }
                saveSystem.StartNewGame();
                break;
                
            case 1: // 계속하기 버튼
                if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
                {
                    Debug.Log("저장된 게임 불러오기");
                    saveSystem.LoadSavedGame();
                }
                else
                {
                    Debug.LogWarning("저장된 게임이 없습니다.");
                }
                break;
                
            case 2: // 설정 버튼
                Debug.Log("설정 메뉴 열기");
                // 설정 메뉴 관련 코드
                break;
                
            case 3: // 종료 버튼
                Debug.Log("게임 종료");
                #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
                #else
                Application.Quit();
                #endif
                break;
                
            default:
                Debug.Log($"버튼 {buttonIndex} 클릭됨");
                break;
        }
    }

    private IEnumerator FadeOutText(TextMeshProUGUI text, float duration = 1.5f)
    {
        float elapsed = 0f;
        Color originalColor = text.color;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(1f, 0f, elapsed / duration);
            text.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        text.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
    }
}

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

    private TextMeshProUGUI[] buttonTexts;
    private Color[] originalTextColors;

    [SerializeField] private GameObject Option;
    [SerializeField] private SceneFader fader;

    private SaveSystemInitializer saveSystem;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name.Contains("Title") || scene.name == "TitleScene")
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    private void Start()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        buttons = Buttons.GetComponentsInChildren<Button>(true);
        int buttonCount = buttons.Length;

        buttonSideImages = new Image[buttonCount][];
        fadeCoroutines = new Coroutine[buttonCount][];
        buttonTexts = new TextMeshProUGUI[buttonCount];
        originalTextColors = new Color[buttonCount];

        saveSystem = FindFirstObjectByType<SaveSystemInitializer>();
        if (saveSystem == null)
        {
            GameObject saveSystemObj = new GameObject("SaveSystemInitializer");
            saveSystem = saveSystemObj.AddComponent<SaveSystemInitializer>();
        }

        for (int i = 0; i < buttonCount; i++)
        {
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

            buttonTexts[i] = buttons[i].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonTexts[i] != null)
            {
                Color original = buttonTexts[i].color;
                if (!buttons[i].interactable)
                    original.a = 0.5f; // 비활성화 상태면 반투명으로 저장
                originalTextColors[i] = original;
            }
        }

        for (int i = 0; i < buttonCount; i++)
        {
            AddEventTriggers(buttons[i].gameObject, i);
            AddClickListener(buttons[i], i);
        }

        UpdateContinueButtonState();
    }

    public void UpdateContinueButtonState()
    {
        bool hasSaveData = SaveManager.Instance != null && SaveManager.Instance.HasSaveData();
        if (buttons.Length >= 2)
        {
            buttons[1].interactable = hasSaveData;
            TextMeshProUGUI buttonText = buttons[1].GetComponentInChildren<TextMeshProUGUI>();
            if (buttonText != null)
            {
                buttonText.color = hasSaveData ?
                    new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 1f) :
                    new Color(buttonText.color.r, buttonText.color.g, buttonText.color.b, 0.5f);

                originalTextColors[1] = buttonText.color; // 복원용 색도 갱신
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

            if (buttonTexts[index] != null)
            {
                buttonTexts[index].color = Color.white;
                buttonTexts[index].fontSize *= 1.2f; // 사이즈 20% 증가
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

            if (buttonTexts[index] != null)
            {
                buttonTexts[index].color = originalTextColors[index];
                buttonTexts[index].fontSize /= 1.2f; // 원래 사이즈로 복원
            }
        });
        trigger.triggers.Add(exitEntry);
    }

    private IEnumerator FadeInOnce(Image img, float duration = 0.3f)
    {
        float elapsed = 0f;
        float targetAlpha = 1f;

        while (elapsed < duration)
        {
            float alpha = Mathf.Lerp(0f, targetAlpha, elapsed / duration);
            SetAlpha(img, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

        SetAlpha(img, targetAlpha);
    }

    private IEnumerator FadeClickFeedback(TextMeshProUGUI text, float duration = 0.2f)
    {
        if (text == null) yield break;

        float elapsed = 0f;
        float originalAlpha = text.color.a;
        float midAlpha = originalAlpha * 0.5f;

        while (elapsed < duration / 2f)
        {
            float alpha = Mathf.Lerp(originalAlpha, midAlpha, elapsed / (duration / 2f));
            text.color = new Color(text.color.r, text.color.g, text.color.b, alpha);
            elapsed += Time.deltaTime;
            yield return null;
        }

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
            var text = button.GetComponentInChildren<TextMeshProUGUI>();
            if (text != null)
                StartCoroutine(FadeClickFeedback(text));

            for (int i = 0; i < buttonSideImages[index].Length; i++)
            {
                if (fadeCoroutines[index][i] != null)
                {
                    StopCoroutine(fadeCoroutines[index][i]);
                    fadeCoroutines[index][i] = null;
                }

                SetAlpha(buttonSideImages[index][i], 0f);
            }

            ExecuteButtonAction(index);
        });
    }

    private void ExecuteButtonAction(int buttonIndex)
    {
        if (saveSystem == null) return;

        switch (buttonIndex)
        {
            case 0:
                if (SaveManager.Instance != null)
                    SaveManager.Instance.ResetSaveData();
                StartCoroutine(FadeThenStartNewGame());
                break;

            case 1:
                if (SaveManager.Instance != null && SaveManager.Instance.HasSaveData())
                {
                    Debug.Log("[TitleScene] 저장된 게임 불러오기 시작. isNewGame = false로 설정합니다.");
                    saveSystem.LoadSavedGame();
                }
                else
                {
                    Debug.LogWarning("[TitleScene] 저장된 게임이 없습니다.");
                }
                break;

            case 2:
                Option.SetActive(true);
                break;

            case 3:
#if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
#else
                Application.Quit();
#endif
                break;
        }
    }

    public void opDown()
    {
        Option.SetActive(false);
    }
    private IEnumerator FadeThenStartNewGame()
    {
        yield return fader.FadeOutOnly(1f);
        yield return new WaitForSeconds(1f);
        saveSystem.StartNewGame();
    }
}

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

    private void Start()
    {
        buttons = Buttons.GetComponentsInChildren<Button>(true);
        int buttonCount = buttons.Length;

        buttonSideImages = new Image[buttonCount][];
        fadeCoroutines = new Coroutine[buttonCount][];

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
        });
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

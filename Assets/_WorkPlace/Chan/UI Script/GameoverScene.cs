using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class GameoverScene : MonoBehaviour
{
    [SerializeField] private GameObject Buttons;                   // 버튼 부모
    [SerializeField] private TextMeshProUGUI Description; // 설명 텍스트

    private Button[] buttons;
    private Image[] hoverImages;
    private Coroutine[] fadeCoroutines;

    private readonly string[] hoverDescriptions = new string[]
    {
        "마지막 저장시점으로 돌아갑니다.",
        "메인 메뉴 화면으로 돌아갑니다."
        // 버튼 개수에 따라 이 배열 늘리면 됨
    };

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
        }

        if (Description != null)
        {
            Description.text = ""; // 시작 시 빈 상태
        }
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

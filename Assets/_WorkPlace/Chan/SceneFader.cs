using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private Image fadeImage;  // 검은색 이미지
    [SerializeField] private float fadeDuration = 1f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void FadeTo(string sceneName)
    {
        StartCoroutine(FadeAndLoad(sceneName));
    }

    private IEnumerator FadeAndLoad(string sceneName)
    {
        // Fade Out (검은 화면)
        yield return StartCoroutine(Fade(0f, 1f));

        SceneManager.LoadScene(sceneName);

        // 씬 바뀌고 나서 약간 기다려야 함
        yield return new WaitForSeconds(0.1f);

        // Fade In (다시 밝아짐)
        yield return StartCoroutine(Fade(1f, 0f));
    }

    private IEnumerator Fade(float startAlpha, float endAlpha)
    {
        float elapsed = 0f;

        Color color = fadeImage.color;
        while (elapsed < fadeDuration)
        {
            float t = elapsed / fadeDuration;
            color.a = Mathf.Lerp(startAlpha, endAlpha, t);
            fadeImage.color = color;
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // 끝났을 때 정확히 맞춰줌
        color.a = endAlpha;
        fadeImage.color = color;
    }
}

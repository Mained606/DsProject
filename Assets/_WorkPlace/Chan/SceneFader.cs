using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    public Image image;

    private void Awake()
    {
        image.color = new Color(0f, 0f, 0f, 1f); // 시작은 검정
        FromFade(2f);
    }

    public void FromFade(float delayTime = 0f)
    {
        StartCoroutine(FadeIn(delayTime));
        Debug.Log("페이더 시작");
    }

    IEnumerator FadeIn(float delayTime)
    {
        if (delayTime > 0f)
            yield return new WaitForSeconds(delayTime);

        float duration = 1f;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, t / duration); // 1 → 0
            image.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        image.color = new Color(0f, 0f, 0f, 0f); // 완전 투명 보정
    }

    public void FadeTo(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    public void FadeTo(int sceneNumber)
    {
        StartCoroutine(FadeOutAndLoad(sceneNumber));
    }

    IEnumerator FadeOutAndLoad(string sceneName)
    {
        yield return FadeOut(1f);
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator FadeOutAndLoad(int sceneNumber)
    {
        yield return FadeOut(1f);
        SceneManager.LoadScene(sceneNumber);
    }

    public IEnumerator FadeOut(float duration = 1f)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / duration); // 0 → 1
            image.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        image.color = new Color(0f, 0f, 0f, 1f); // 완전 불투명 보정
    }

    public IEnumerator FadeOutOnly(float duration = 1f)
    {
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, t / duration); // 밝음 → 어둠
            image.color = new Color(0f, 0f, 0f, alpha);
            yield return null;
        }

        image.color = new Color(0f, 0f, 0f, 1f); // 완전 어둡게 고정
    }

}

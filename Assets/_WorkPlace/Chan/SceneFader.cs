using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    #region Variable

    // Fader 이미지
    public Image image;
    public AnimationCurve curve;
    #endregion

    private void Start()
    {
        // 초기화: 시작시 화면을 검정색으로 시작
        image.color = new Color(0f, 0f, 0f, 1f);
        FromFade(2f);
    }

    public void FromFade(float delayTime = 0f)
    {
        // 씬 시작시 페이드 인 효과 
        StartCoroutine(FadeIn(delayTime));
    }

    IEnumerator FadeIn(float delayTime)
    {
        if (delayTime > 0f)
        {
            yield return new WaitForSeconds(delayTime);

        }

        // 1초 동안 이미지의 알파값이 a 1->0으로

        float t = 1;

        while (t > 0)
        {
            t -= Time.deltaTime;
            float a = curve.Evaluate(t);
            image.color = new Color(0f, 0f, 0f, a); // (r ,g ,b 는 검은색)

            yield return 0f;
        }
    }

    public void FadeTo(string sceneName)
    {
        StartCoroutine(FadeOut(sceneName));
    }

    public void FadeTo(int sceneNumber)
    {
        StartCoroutine(FadeOut(sceneNumber));
    }

    IEnumerator FadeOut(int sceneNumber)
    {
        //1초동안 image a 0-> 1
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            float a = curve.Evaluate(t);
            image.color = new Color(0f, 0f, 0f, a);
            yield return 0f;
        }

        //다음씬 로드
        SceneManager.LoadScene(sceneNumber);
    }

    public IEnumerator FadeOutOnly(float duration = 1f)
    {
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            float a = curve.Evaluate(t);
            image.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }

        // 마지막 보정
        image.color = new Color(0f, 0f, 0f, 1f);
    }

    IEnumerator FadeOut(string sceneName)
    {
        // 1초동안 image a 0-> 1
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime;
            float a = curve.Evaluate(t);
            image.color = new Color(0f, 0f, 0f, a);
            yield return null;
        }

        //다음씬 로드
        SceneManager.LoadScene(sceneName);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

public class ObjectFadeController : MonoBehaviour
{
    [Header("设置")]
    [SerializeField] private GameObject fadeObject;
    [SerializeField] private float fadeDuration = 1f;
    [SerializeField] private float waitTime = 5f;

    [Header("事件配置")]
    public UnityEvent OnFadeComplete = new UnityEvent();
    public UnityEvent OnFadeComplete2 = new UnityEvent();
    public UnityEvent OnFadeComplete3 = new UnityEvent();

    public bool isAuto = false;
    private void Awake()
    {
        if (isAuto)
        {
            Fade(false);
        }
    }
    public void Fade(bool isFadeIn)
    {
        Image image = fadeObject.GetComponent<Image>();
        Text text = fadeObject.GetComponent<Text>();
        TMP_Text tmpText = fadeObject.GetComponent<TMP_Text>();
        CanvasGroup canvasGroup = fadeObject.GetComponent<CanvasGroup>();
        if (isFadeIn)
        {
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
            }
            else if (image != null)
            {
                Color color = image.color;
                color.a = 0f;
                image.color = color;
            }
            else if (text != null || tmpText != null)
            {
                Color color = (text != null) ? text.color : tmpText.color;
                color.a = 0f;
                if (text != null) text.color = color;
                if (tmpText != null) tmpText.color = color;
            }
            StartCoroutine(FadeIn());
        }
        else
        {
            StartCoroutine(FadeOut());
        }
    }
    public void FadeInAndOut()
    {
        StartCoroutine(FadeInOut());
    }
    IEnumerator FadeInOut()
    {
        Image image = fadeObject.GetComponent<Image>();
        Text text = fadeObject.GetComponent<Text>();
        TMP_Text tmpText = fadeObject.GetComponent<TMP_Text>();
        CanvasGroup canvasGroup = fadeObject.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
        }
        else if (image != null)
        {
            Color color = image.color;
            color.a = 0f;
            image.color = color;
        }
        else if (text != null || tmpText != null)
        {
            Color color = (text != null) ? text.color : tmpText.color;
            color.a = 0f;
            if (text != null) text.color = color;
            if (tmpText != null) tmpText.color = color;
        }
        yield return StartCoroutine(FadeIn());
        yield return StartCoroutine(FadeOut());
        OnFadeComplete3?.Invoke();
    }
    IEnumerator FadeIn()
    {
        yield return new WaitForSeconds(waitTime);
        yield return StartCoroutine(FadeObject(fadeObject, 0, 1, fadeDuration));
        OnFadeComplete2?.Invoke();
    }
    IEnumerator FadeOut()
    {
        Debug.Log("fadeout");
        yield return new WaitForSeconds(waitTime);
        yield return StartCoroutine(FadeObject(fadeObject, 1, 0, fadeDuration));
        fadeObject.gameObject.SetActive(false);
        OnFadeComplete?.Invoke();
    }


    IEnumerator FadeObject(GameObject uiObject, float startAlpha, float targetAlpha, float duration)
    {
        // 获取所有UI相关组件
        Image image = uiObject.GetComponent<Image>();
        Text text = uiObject.GetComponent<Text>();
        TMP_Text tmpText = uiObject.GetComponent<TMP_Text>();
        CanvasGroup canvasGroup = uiObject.GetComponent<CanvasGroup>();

        float timer = 0f;

        // 优先使用 CanvasGroup（控制整体透明度）
        if (canvasGroup != null)
        {
            canvasGroup.alpha = startAlpha;
            while (timer < duration)
            {
                canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }
            canvasGroup.alpha = targetAlpha;
        }
        // 处理 Image
        else if (image != null)
        {
            Color startColor = image.color;
            startColor.a = startAlpha;
            Color targetColor = startColor;
            targetColor.a = targetAlpha;

            while (timer < duration)
            {
                image.color = Color.Lerp(startColor, targetColor, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }
            image.color = targetColor;
        }
        // 处理 Text 或 TextMeshPro
        else if (text != null || tmpText != null)
        {
            Color startColor = (text != null) ? text.color : tmpText.color;
            startColor.a = startAlpha;
            Color targetColor = startColor;
            targetColor.a = targetAlpha;

            while (timer < duration)
            {
                if (text != null) text.color = Color.Lerp(startColor, targetColor, timer / duration);
                if (tmpText != null) tmpText.color = Color.Lerp(startColor, targetColor, timer / duration);
                timer += Time.deltaTime;
                yield return null;
            }
            if (text != null) text.color = targetColor;
            if (tmpText != null) tmpText.color = targetColor;
        }
        else
        {
            Debug.LogError("未找到支持的UI组件: " + uiObject.name);
        }
    }

}
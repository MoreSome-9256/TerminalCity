using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// 用于 PlotPoint 的渐入/渐出黑屏或 UI 面板
/// </summary>
public class FadePoint : MonoBehaviour
{
    [Header("Fade 设置")]
    [Tooltip("需要渐变的物体")]
    public GameObject fadeObject;

    [Tooltip("渐变时长")]
    public float fadeDuration = 1f;

    [Tooltip("延迟执行")]
    public float waitTime = 0f;

    [Header("PlotPoint 自动触发 (可选)")]
    public bool autoFadeOnEnter = false;
    public bool autoFadeOnExit = false;

    [Header("事件")]
    public UnityEvent onFadeInComplete;
    public UnityEvent onFadeOutComplete;

    // 内部缓存组件
    private CanvasGroup canvasGroup;
    private Image image;
    private TMP_Text tmpText;

    private void Awake()
    {
        if (fadeObject == null)
        {
            Debug.LogError("FadePoint: fadeObject 未绑定！");
            return;
        }

        // 尝试获取常用组件
        canvasGroup = fadeObject.GetComponent<CanvasGroup>();
        image = fadeObject.GetComponent<Image>();
        tmpText = fadeObject.GetComponent<TMP_Text>();

        // 确保对象 active
        fadeObject.SetActive(true);

        // 初始化 alpha 为 0
        SetAlpha(0f);
    }

    /// <summary>
    /// 在事件里调用
    /// </summary>
    public void Fade(bool isFadeIn)
    {
        StopAllCoroutines();
        if (isFadeIn) StartCoroutine(FadeCoroutine(0f, 1f, onFadeInComplete));
        else StartCoroutine(FadeCoroutine(1f, 0f, onFadeOutComplete));
    }

    /// <summary>
    /// PlotPoint Enter 时调用
    /// </summary>
    public void OnPointEnter()
    {
        if (autoFadeOnEnter) Fade(true);
    }

    /// <summary>
    /// PlotPoint Exit 时调用
    /// </summary>
    public void OnPointExit()
    {
        if (autoFadeOnExit) Fade(false);
    }

    private IEnumerator FadeCoroutine(float startAlpha, float targetAlpha, UnityEvent callback)
    {
        if (waitTime > 0f) yield return new WaitForSeconds(waitTime);

        float timer = 0f;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float alpha = Mathf.Lerp(startAlpha, targetAlpha, timer / fadeDuration);
            SetAlpha(alpha);
            yield return null;
        }

        SetAlpha(targetAlpha);
        callback?.Invoke();
    }

    private void SetAlpha(float alpha)
    {
        if (canvasGroup != null) canvasGroup.alpha = alpha;
        if (image != null) { var c = image.color; c.a = alpha; image.color = c; }
        if (tmpText != null) { var c = tmpText.color; c.a = alpha; tmpText.color = c; }
    }
    /// <summary>
    /// 执行一次 Fade
    /// isFadeIn = true 表示从透明→黑屏
    /// isFadeIn = false 表示从黑屏→透明
    /// </summary>
    public IEnumerator PlayFade(bool isFadeIn)
    {
        fadeObject.SetActive(true);

        CanvasGroup cg = fadeObject.GetComponent<CanvasGroup>();
        if (cg == null) cg = fadeObject.AddComponent<CanvasGroup>();

        float start = isFadeIn ? 0f : 1f;
        float end = isFadeIn ? 1f : 0f;

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(start, end, t / fadeDuration);
            yield return null;
        }
        cg.alpha = end;

        if (!isFadeIn)
            fadeObject.SetActive(false);
    }
}

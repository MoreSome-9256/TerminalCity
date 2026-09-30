using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(CanvasGroup))]
public class ScreenFader : MonoBehaviour
{
    public static ScreenFader Instance { get; private set; }

    [Header("绑定黑屏子物体 Panel")]
    [Tooltip("直接把子物体 Panel 拖到这里；不拖的话会自动寻找第一个子物体")]
    [SerializeField] private GameObject blackPanel;

    [Header("默认参数")]
    [SerializeField] private float defaultFadeDuration = 0.5f;
    [SerializeField] private float defaultStayDuration = 0.2f;

    [Header("全局渐变事件配置")]
    [Tooltip("画面达到全黑瞬间触发（如切换场景、重置坐标等）")]
    public UnityEvent OnFadeToBlackComplete = new UnityEvent();

    [Tooltip("渐变完全结束（画面完全亮起/恢复透明）时触发，可在此挂载 DialogueManager.StartDialogue")]
    public UnityEvent OnFadeOutComplete = new UnityEvent();

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        canvasGroup = GetComponent<CanvasGroup>();

        if (blackPanel == null && transform.childCount > 0)
        {
            blackPanel = transform.GetChild(0).gameObject;
        }

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (blackPanel != null)
        {
            blackPanel.SetActive(false);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    /// <summary>
    /// 黑屏闪一下（淡入到黑 -> 停留 -> 淡出恢复）
    /// </summary>
    public void FadeInOut(float fadeDuration = -1f, float stayDuration = -1f)
    {
        float fDuration = fadeDuration > 0 ? fadeDuration : defaultFadeDuration;
        float sDuration = stayDuration >= 0 ? stayDuration : defaultStayDuration;

        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeInOutRoutine(fDuration, sDuration));
    }

    /// <summary>
    /// 单纯变黑
    /// </summary>
    public void FadeToBlack(float duration = -1f)
    {
        float d = duration > 0 ? duration : defaultFadeDuration;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(1f, d, () =>
        {
            OnFadeToBlackComplete?.Invoke();
        }));
    }

    /// <summary>
    /// 从黑恢复透明
    /// </summary>
    public void FadeToClear(float duration = -1f)
    {
        float d = duration > 0 ? duration : defaultFadeDuration;
        if (fadeCoroutine != null) StopCoroutine(fadeCoroutine);
        fadeCoroutine = StartCoroutine(FadeRoutine(0f, d, () =>
        {
            OnFadeOutComplete?.Invoke();
        }));
    }

    private IEnumerator FadeInOutRoutine(float fDuration, float sDuration)
    {
        if (blackPanel != null) blackPanel.SetActive(true);

        // 1. 变黑
        yield return StartCoroutine(FadeRoutine(1f, fDuration, null));

        // 2. 全黑事件触发
        OnFadeToBlackComplete?.Invoke();

        // 3. 全黑短暂停留
        if (sDuration > 0)
        {
            yield return new WaitForSeconds(sDuration);
        }

        // 4. 变透明
        yield return StartCoroutine(FadeRoutine(0f, fDuration, null));

        if (blackPanel != null) blackPanel.SetActive(false);

        // 5. 恢复完毕事件触发（调用接下来的对话）
        OnFadeOutComplete?.Invoke();

        fadeCoroutine = null;
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, Action onComplete)
    {
        if (targetAlpha > 0.01f && blackPanel != null)
        {
            blackPanel.SetActive(true);
        }

        canvasGroup.blocksRaycasts = targetAlpha > 0.01f;

        float startAlpha = canvasGroup.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
        canvasGroup.blocksRaycasts = targetAlpha > 0.01f;

        if (targetAlpha <= 0.01f && blackPanel != null)
        {
            blackPanel.SetActive(false);
        }

        onComplete?.Invoke();
    }
}
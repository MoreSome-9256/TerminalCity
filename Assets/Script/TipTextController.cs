using UnityEngine;
using TMPro;
using System.Collections;

public class TipTextController : MonoBehaviour
{
    [Header("提示文字设置")]
    [SerializeField] private TMP_Text tipText;
    [SerializeField] private float fadeDuration = 1f; // 淡入淡出时长
    [SerializeField] private float stayDuration = 3f;  // 保持显示时长
    [SerializeField] private float waitTime = 5f;

    private void Start()
    {
        // 初始状态透明
        tipText.alpha = 0;
        // 启动显示流程
        StartCoroutine(ShowTipRoutine());
    }

    IEnumerator ShowTipRoutine()
    {
        // 等待对话系统的延迟时间（与对话同步）
        yield return new WaitForSeconds(waitTime); 

        // 淡入效果
        yield return StartCoroutine(FadeText(0, 1, fadeDuration));

        // 保持显示
        yield return new WaitForSeconds(stayDuration);

        // 淡出效果
        yield return StartCoroutine(FadeText(1, 0, fadeDuration));

        // 禁用文字（可选）
        tipText.gameObject.SetActive(false);
    }

    IEnumerator FadeText(float startAlpha, float targetAlpha, float duration)
    {
        float timer = 0;
        while (timer < duration)
        {
            float progress = timer / duration;
            tipText.alpha = Mathf.Lerp(startAlpha, targetAlpha, progress);
            timer += Time.deltaTime;
            yield return null;
        }
        tipText.alpha = targetAlpha;
    }
}
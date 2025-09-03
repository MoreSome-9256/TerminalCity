using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlinkWithFade : MonoBehaviour
{
    [SerializeField] private Image Blink;
    [SerializeField] private float fadeDuration = 0.1f; // 每次淡入/淡出时长

    private Coroutine fadeRoutine;

    public void PlayBlink()
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);
        fadeRoutine = StartCoroutine(FadeBlinkCoroutine());
    }

    private IEnumerator FadeBlinkCoroutine()
    {
        // 淡入
        float t = 0f;
        Color c = Blink.color;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(0f, 1f, t / fadeDuration);
            Blink.color = c;
            yield return null;
        }

        // 播放逐帧眨眼动画（这里用 Animator 控制）
        Animator anim = Blink.GetComponent<Animator>();
        if (anim != null)
            anim.Play("Blink", -1, 0f);

        // 等待动画时长
        yield return new WaitForSeconds(anim.GetCurrentAnimatorStateInfo(0).length);

        // 淡出
        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            c.a = Mathf.Lerp(1f, 0f, t / fadeDuration);
            Blink.color = c;
            yield return null;
        }

        // 确保最后透明
        c.a = 0f;
        Blink.color = c;
    }
}

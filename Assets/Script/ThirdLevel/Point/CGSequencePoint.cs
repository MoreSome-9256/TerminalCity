using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CGSequencePoint : PlotPoint
{
    [Header("CG 显示")]
    public Image cgImage;          // 用来显示 CG 的 Image
    public CGFrame[] frames;

    [Header("淡入淡出（可选）")]
    public bool fadeBetweenFrames = false;
    public float fadeDuration = 0.5f;

    public override IEnumerator Execute()
    {
        if (cgImage == null)
        {
            Debug.LogError("CGSequencePoint: cgImage 未绑定！");
            yield break;
        }
        Debug.Log("CGSequencePoint");
        cgImage.gameObject.SetActive(true);

        CanvasGroup cgGroup = cgImage.GetComponent<CanvasGroup>();
        if (fadeBetweenFrames && cgGroup == null)
            cgGroup = cgImage.gameObject.AddComponent<CanvasGroup>();

        foreach (var frame in frames)
        {
            if (frame.cgSprite == null) continue;

            // 切换 CG
            if (fadeBetweenFrames)
                yield return FadeOut(cgGroup);

            cgImage.sprite = frame.cgSprite;
            cgImage.SetNativeSize();

            if (fadeBetweenFrames)
                yield return FadeIn(cgGroup);

            // 等待逻辑
            if (frame.waitForInput && frame.validKeys != null && frame.validKeys.Length > 0)
            {
                yield return WaitForKey(frame.validKeys);
            }
            else if (frame.duration > 0f)
            {
                yield return new WaitForSeconds(frame.duration);
            }
        }

        cgImage.gameObject.SetActive(false);
    }

    private IEnumerator WaitForKey(KeyCode[] keys)
    {
        while (true)
        {
            foreach (var key in keys)
            {
                if (Input.GetKeyDown(key))
                    yield break;
            }
            yield return null;
        }
    }

    private IEnumerator FadeIn(CanvasGroup cg)
    {
        cg.alpha = 0f;
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 1f;
    }

    private IEnumerator FadeOut(CanvasGroup cg)
    {
        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            cg.alpha = Mathf.Lerp(1f, 0f, t / fadeDuration);
            yield return null;
        }
        cg.alpha = 0f;
    }
}

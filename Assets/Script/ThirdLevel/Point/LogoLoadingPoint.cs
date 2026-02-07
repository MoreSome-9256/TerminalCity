using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LogoLoadingPoint : PlotPoint
{
    [Header("=== UI References ===")]
    [SerializeField] private RectTransform logoRoot;
    [SerializeField] private Image contentImage;
    [SerializeField] private TMP_Text loadingTextComp;

    [Header("Frame")]
    [SerializeField] private RectTransform frameTopMask;
    [SerializeField] private RectTransform frameLeftMask;

    [Header("阶段一：填充加载")]
    public float fillDuration = 1.5f;
    public AnimationCurve fillCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public float loadingDuration = 1.2f;

    [Header("阶段二：完成提示")]
    public float welcomeHoldTime = 1.0f;
    public string loadingText = "Loading...";
    public string welcomeText = "Welcome to the Terminal";

    [Header("阶段三：Logo 就位")]
    public float moveDuration = 0.8f;
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public Vector2 targetAnchoredPos = new Vector2(80, -80);
    public float targetScale = 0.4f;

    [Header("阶段四：边框动画")]
    public float frameDuration = 0.6f;
    public AnimationCurve frameCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public override IEnumerator Execute()
    {
        // ===== 安全校验 =====
        if (!logoRoot || !contentImage || !loadingTextComp || !frameTopMask || !frameLeftMask)
        {
            Debug.LogError("[LogoLoadingPoint] UI reference missing");
            yield break;
        }

        // ===== Frame 初始化 =====
        var frameTop = frameTopMask.GetChild(0) as RectTransform;
        var frameLeft = frameLeftMask.GetChild(0) as RectTransform;

        Vector2 frameTopTargetSize = frameTop.sizeDelta;
        Vector2 frameLeftTargetSize = frameLeft.sizeDelta;

        frameTopMask.sizeDelta = new Vector2(0, frameTopTargetSize.y);
        frameLeftMask.sizeDelta = new Vector2(frameLeftTargetSize.x, 0);

        // ===== 初始化 =====
        contentImage.fillAmount = 0f;
        loadingTextComp.text = "";

        Vector2 startPos = logoRoot.anchoredPosition;
        Vector3 startScale = logoRoot.localScale;

        // ========= 阶段一：Logo 填充 + Loading 打字 =========
        float logoTime = 0f;
        float textTime = 0f;
        int textLength = loadingText.Length;

        while (logoTime < fillDuration || textTime < loadingDuration)
        {
            float delta = Time.deltaTime;

            if (logoTime < fillDuration)
            {
                logoTime += delta;
                float n = Mathf.Clamp01(logoTime / fillDuration);
                contentImage.fillAmount = fillCurve.Evaluate(n);
            }

            if (textTime < loadingDuration)
            {
                textTime += delta;
                float n = Mathf.Clamp01(textTime / loadingDuration);
                int visible = Mathf.FloorToInt(n * textLength);
                loadingTextComp.text = loadingText.Substring(0, visible);
            }

            yield return null;
        }

        contentImage.fillAmount = 1f;
        loadingTextComp.text = loadingText;

        // Loading 消失
        yield return TypeTextReverse(loadingTextComp, loadingText, 0.5f);

        // ========= Welcome =========
        yield return TypeText(loadingTextComp, welcomeText, 0.8f);
        yield return new WaitForSeconds(welcomeHoldTime);
        yield return TypeTextReverse(loadingTextComp, welcomeText, 0.5f);

        // ========= Logo 移动 + 缩放 =========
        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / moveDuration);
            float eased = moveCurve.Evaluate(n);

            logoRoot.anchoredPosition =
                Vector2.Lerp(startPos, targetAnchoredPos, eased);
            logoRoot.localScale =
                Vector3.Lerp(startScale, Vector3.one * targetScale, eased);

            yield return null;
        }

        logoRoot.anchoredPosition = targetAnchoredPos;
        logoRoot.localScale = Vector3.one * targetScale;

        // ========= 边框延伸 =========
        t = 0f;
        while (t < frameDuration)
        {
            t += Time.deltaTime;
            float n = Mathf.Clamp01(t / frameDuration);
            float eased = frameCurve.Evaluate(n);

            frameTopMask.sizeDelta = new Vector2(
                Mathf.Lerp(0, frameTopTargetSize.x, eased),
                frameTopTargetSize.y
            );

            frameLeftMask.sizeDelta = new Vector2(
                frameLeftTargetSize.x,
                Mathf.Lerp(0, frameLeftTargetSize.y, eased)
            );

            yield return null;
        }

        frameTopMask.sizeDelta = frameTopTargetSize;
        frameLeftMask.sizeDelta = frameLeftTargetSize;
    }

    // ===== 打字机 =====
    private IEnumerator TypeText(TMP_Text textComp, string content, float duration)
    {
        textComp.text = "";
        int length = content.Length;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            int visible = Mathf.FloorToInt((t / duration) * length);
            textComp.text = content.Substring(0, visible);
            yield return null;
        }

        textComp.text = content;
    }

    private IEnumerator TypeTextReverse(TMP_Text textComp, string content, float duration)
    {
        int length = content.Length;
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;
            int visible = length - Mathf.FloorToInt((t / duration) * length);
            textComp.text = content.Substring(0, Mathf.Max(0, visible));
            yield return null;
        }

        textComp.text = "";
    }
}

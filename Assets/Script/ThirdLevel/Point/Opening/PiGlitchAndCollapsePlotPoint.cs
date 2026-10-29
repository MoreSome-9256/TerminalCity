using System.Collections;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PiGlitchAndCollapsePlotPoint : PlotPoint
{
    [Header("UI 基础组件")]
    [SerializeField] private TextMeshProUGUI fullScreenText;
    [SerializeField] private CanvasGroup openingCanvasGroup; // 整个 Opening 节点的 CanvasGroup
    [Tooltip("需要黑屏时一并隐藏的背景和多边形")]
    [SerializeField] private GameObject backgroundObj;
    [SerializeField] private GameObject centerGraphicObj;

    [Header("故障扭曲与老旧显示器熄屏")]
    [Tooltip("挂在 Opening 根节点上的 RoomGlitchController")]
    [SerializeField] private RoomGlitchController glitchController;
    [Tooltip("老旧电视熄屏时的刺眼白线/光点（Image，颜色设为纯白）")]
    [SerializeField] private RectTransform crtFlashLine;
    [Tooltip("纯黑过渡遮罩层（用于关机后的死黑与淡出）")]
    [SerializeField] private CanvasGroup fadeBlackCover;

    [Header("音频设置")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip staticNoiseClip;      // 电流噪杂音
    [SerializeField] private AudioClip crtTurnOffClip;        // CRT 关机断电声/消磁爆音（可空）

    [Header("打字机极速效果")]
    [SerializeField] private int charactersPerStep = 6;
    [SerializeField] private float stepInterval = 0.01f;
    [SerializeField] private int zeroCount = 2500;

    [Header("时间与节奏控制")]
    [Tooltip("打字完成至定格的全长（秒）")]
    [SerializeField] private float freezeDuration = 1.8f;
    [Tooltip("故障爆发抖动的时长（秒）")]
    [SerializeField] private float glitchBurstDuration = 0.45f;
    [Tooltip("CRT关机动画时长（秒）")]
    [SerializeField] private float crtAnimDuration = 0.28f;
    [Tooltip("关机后纯黑死寂停留时间（秒）")]
    [SerializeField] private float blackHoldDuration = 0.8f;
    [Tooltip("黑屏淡出回到场景的过渡时长（秒）")]
    [SerializeField] private float fadeToSceneDuration = 1.2f;

    // [SerializeField] private BGMManager openingBGM;

    private const string PI_128_DIGITS =
        "3.14159265358979323846264338327950288419716939937510582097494459230781640628620899862803482534211706798214808651328230664709384460955058223172535940812848111745028410270193852110555964462294895493038196";

    public static event System.Action OnOpeningFinished;

    public override IEnumerator Execute()
    {
        // 初始状态隐藏特效与重置
        if (crtFlashLine != null) crtFlashLine.gameObject.SetActive(false);
        if (fadeBlackCover != null)
        {
            fadeBlackCover.gameObject.SetActive(true); // 👈 同样确保它是开启的
            fadeBlackCover.alpha = 0f; // 只是让它变透明，而不是关掉它
        }
        if (glitchController != null) glitchController.SetGlitchIntensity(0f);

        if (fullScreenText != null) fullScreenText.gameObject.SetActive(true);

        // 1. 播放电流持续杂音
        if (audioSource != null && staticNoiseClip != null)
        {
            audioSource.clip = staticNoiseClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        // 2. 灌满纯数字
        string fullContent = BuildTargetPiString();
        if (fullScreenText != null)
        {
            fullScreenText.text = fullContent;
            fullScreenText.ForceMeshUpdate();
            fullScreenText.maxVisibleCharacters = 0;
        }

        // 3. 极速打字机灌入（直接使用字符串长度，避免 TMP textInfo 未就绪导致死锁）
        int totalChars = fullContent.Length;
        int currentVisible = 0;

        while (currentVisible < totalChars)
        {
            currentVisible += charactersPerStep;
            if (fullScreenText != null)
            {
                fullScreenText.maxVisibleCharacters = Mathf.Min(currentVisible, totalChars);
            }

            if (stepInterval > 0.001f)
                yield return new WaitForSeconds(stepInterval);
            else
                yield return null; // 每帧灌入
        }

        if (fullScreenText != null)
        {
            fullScreenText.maxVisibleCharacters = totalChars;
        }

        //Debug.Log("<color=green>[PiGlitch]</color> 打字完成，进入定格倒计时！");

        // ✨ 修改点 2：直接通过单例调用全局淡出
        if (BGMManager.Instance != null)
        {
            BGMManager.Instance.FadeOutBGM(0.3f);
        }

        // 4. 定格停留
        yield return new WaitForSeconds(freezeDuration);

        Debug.Log("<color=yellow>[PiGlitch]</color> 步骤1：开始 Glitch 爆发！");
        // 4. 【阶段一】Shader 故障剧烈爆发
        yield return StartCoroutine(PlayShaderGlitchBurst());

        Debug.Log("<color=yellow>[PiGlitch]</color> 步骤2：Glitch 结束，停止音频，准备 CRT 关机！");
        if (audioSource != null)
        {
            audioSource.Stop();
            if (crtTurnOffClip != null) audioSource.PlayOneShot(crtTurnOffClip);
        }

        Debug.Log("<color=yellow>[PiGlitch]</color> 步骤3：开始播放 CRT 关机动画！");
        // 5. 【阶段二】老旧 CRT 显示器关机动画
        yield return StartCoroutine(PlayCrtTurnOffRoutine());

        Debug.Log("<color=yellow>[PiGlitch]</color> 步骤4：CRT 结束，全屏置黑，隐藏所有底层内容！");
        // 6. 【阶段三】纯黑死寂停留
        if (glitchController != null) glitchController.SetGlitchIntensity(0f);

        if (fadeBlackCover != null)
        {
            fadeBlackCover.gameObject.SetActive(true);
            fadeBlackCover.alpha = 1f; // 黑屏拉满
        }

        // ✨ 核心操作：彻底黑屏瞬间，把底下的背景、多边形、文字全部关掉！
        if (fullScreenText != null) fullScreenText.gameObject.SetActive(false);
        if (backgroundObj != null) backgroundObj.SetActive(false);
        if (centerGraphicObj != null) centerGraphicObj.SetActive(false);

        // 死寂纯黑停留（默认 0.8s）
        yield return new WaitForSeconds(blackHoldDuration);

        Debug.Log("<color=yellow>[PiGlitch]</color> 步骤5：开始纯黑幕布淡出，回到大场景！");
        // 7. 【阶段四】整体淡出（此时只有纯黑遮罩在淡出）
        yield return StartCoroutine(FadeOutToGameScene());

        Debug.Log("<color=green>[PiGlitch]</color> 全部演出完毕！");
    }

    private string BuildTargetPiString()
    {
        StringBuilder sb = new StringBuilder();
        sb.Append(PI_128_DIGITS.Substring(0, Mathf.Min(130, PI_128_DIGITS.Length)));
        for (int i = 0; i < zeroCount; i++)
        {
            sb.Append('0');
        }
        return sb.ToString();
    }

    /// <summary>
    /// 阶段一：调用 RoomGlitchController 爆发扭曲（使用绝对时间，防死锁）
    /// </summary>
    private IEnumerator PlayShaderGlitchBurst()
    {
        // ✨ 核心改动：直接计算结束时间，不管中间 yield 怎么跑，时间一到必出循环！
        float endTime = Time.time + glitchBurstDuration;

        while (Time.time < endTime)
        {
            float currentIntensity = Random.Range(0.3f, 0.6f);

            if (glitchController != null)
            {
                try
                {
                    glitchController.SetGlitchIntensity(currentIntensity);
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[PiGlitch] 忽略异常: {e.Message}");
                }
            }

            yield return new WaitForSeconds(0.04f);
        }

        yield return null;
    }

    /// <summary>
    /// 阶段二：老旧 CRT 关机收缩动画
    /// </summary>
    private IEnumerator PlayCrtTurnOffRoutine()
    {
        if (crtFlashLine == null) yield break;

        crtFlashLine.gameObject.SetActive(true);
        crtFlashLine.localScale = Vector3.one;

        float t = 0f;
        float halfDuration = crtAnimDuration * 0.5f;

        // 1. 垂直压缩：上下迅速坍缩成一条极窄的高亮水平白线（Y 轴缩小，X 保持全屏）
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / halfDuration);
            float curvedP = Mathf.Pow(p, 2); // 电子束断电的二次加速骤缩感
            crtFlashLine.localScale = new Vector3(1f, Mathf.Lerp(1f, 0.003f, curvedP), 1f);
            yield return null;
        }

        // 2. 水平收缩：白线向中央疾速坍缩为一个光点随之熄灭（X 轴缩小）
        t = 0f;
        while (t < halfDuration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / halfDuration);
            crtFlashLine.localScale = new Vector3(Mathf.Lerp(1f, 0f, p), 0.003f, 1f);
            yield return null;
        }

        crtFlashLine.gameObject.SetActive(false);
    }

    /// <summary>
    /// 阶段四：整个 Opening Prefab 整体平滑淡出，彻底回归主场景
    /// </summary>
    private IEnumerator FadeOutToGameScene()
    {
        // 核心改动：直接淡出整个 Opening Prefab 的根节点 CanvasGroup
        if (openingCanvasGroup != null)
        {
            float elapsed = 0f;
            float startAlpha = openingCanvasGroup.alpha;

            while (elapsed < fadeToSceneDuration)
            {
                elapsed += Time.deltaTime;
                openingCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / fadeToSceneDuration);
                yield return null;
            }

            openingCanvasGroup.alpha = 0f;

            // ✨ 收尾：彻底关闭整个 Opening Prefab，不留任何背景或残留 UI
            openingCanvasGroup.gameObject.SetActive(false);
            //Debug.Log("<color=green>[PiGlitch]</color> Opening Prefab 已完全淡出并关闭，场景无缝交接！");
            // 广播开场完毕
            OnOpeningFinished?.Invoke();
        }
    }
}
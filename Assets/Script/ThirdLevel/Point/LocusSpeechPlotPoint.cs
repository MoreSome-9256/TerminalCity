using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LocusSpeechPlotPoint : PlotPoint
{
    [Header("Speech Data")]
    public SpeechSegment[] segments;

    [Header("UI References")]
    public TextMeshProUGUI officialText;

    [Header("Choice UI")]
    public GameObject choicePanel;
    public Button[] choiceButtons;   // 3个按钮

    public RawImage backgroundImage;
    private Material runtimeMat;
    public TMPGlitch[] glitchTexts;
    public Image overlayImage; // 半透明叠加层

    public float breakdown;
    float maxBreakdown = 30f;

    private int selectedIndex = -1;

    Coroutine glitchRoutine;

    public override IEnumerator Execute()
    {
        breakdown = 0f;
        officialText.text = "";
        runtimeMat = Instantiate(backgroundImage.material);
        backgroundImage.material = runtimeMat;
        // 清零 Shader 崩坏参数
        runtimeMat.SetVector("_ScanLineJitter", Vector2.zero);
        runtimeMat.SetVector("_VerticalJump", Vector2.zero);
        runtimeMat.SetFloat("_HorizontalShake", 0f);
        runtimeMat.SetVector("_ColorDrift", Vector2.zero);
        UpdateBreakdownVisual();
        // 启动 overlay 持续闪烁 Coroutine
        StartCoroutine(OverlayBlinkLoop());

        for (int i = 0; i < segments.Length; i++)
        {
            SpeechSegment segment = segments[i];

            if (segment.hasChoices)
            {
                yield return ShowChoices(segment);
            }

            yield return ShowOfficialLine(segment.officialLine);

            // 等待点击进入下一句
            yield return new WaitUntil(() => Input.GetMouseButtonDown(0));
        }
    }
    IEnumerator OverlayBlinkLoop()
    {
        while (true)
        {
            // 计算当前闪烁基础间隔
            float minInterval = 0.05f;
            float maxInterval = 5f;
            float normalized = Mathf.Clamp01(breakdown / maxBreakdown);
            float baseWait = Mathf.Lerp(maxInterval, minInterval, normalized);

            // 加一点随机性：±50%
            float waitTime = baseWait * Random.Range(0.5f, 1.5f);

            yield return new WaitForSeconds(waitTime);

            // 高于阈值才闪
            if (breakdown > 10f)
            {
                // 随机颜色黑或红
                Color c = Random.value < 0.5f ? Color.black : Color.red;
                // 随机透明度 0.2~0.4，随 breakdown 调整
                float alpha = Mathf.Lerp(0.1f, 0.3f, normalized) * Random.Range(0.5f, 1f);
                c.a = alpha;
                overlayImage.color = c;

                // 短暂闪一下
                yield return new WaitForSeconds(0.05f);
                overlayImage.color = new Color(0, 0, 0, 0);
            }
        }
    }
    IEnumerator ShowChoices(SpeechSegment segment)
    {
        selectedIndex = -1;

        choicePanel.SetActive(true);
        officialText.gameObject.SetActive(false);

        SpeechSegment currentSegment = segment;

        // ⭐启动乱码刷新
        glitchRoutine =
            StartCoroutine(UpdateChoiceGlitchLoop(currentSegment));

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            int index = i;

            TMPGlitch glitch =
                choiceButtons[i]
                .GetComponentInChildren<TMPGlitch>();

            if (glitch != null)
                glitch.intensity = breakdown * 0.25f;

            choiceButtons[i].onClick.RemoveAllListeners();

            choiceButtons[i].onClick.AddListener(() =>
            {
                selectedIndex = index;

                // 去掉前缀再比较
                string chosen = currentSegment.options[index];
                if (chosen.StartsWith(">> "))
                    chosen = chosen.Substring(3);

                if (chosen != currentSegment.officialLine)
                {
                    breakdown += 1f;
                    UpdateBreakdownVisual();
                }
            });
        }

        yield return new WaitUntil(() => selectedIndex != -1);

        // ⭐停止乱码刷新
        if (glitchRoutine != null)
            StopCoroutine(glitchRoutine);

        choicePanel.SetActive(false);

        yield return new WaitForSeconds(0.2f);
    }
    IEnumerator UpdateChoiceGlitchLoop(SpeechSegment segment)
    {
        while (selectedIndex == -1)
        {
            for (int i = 0; i < choiceButtons.Length; i++)
            {
                TextMeshProUGUI tmp =
                    choiceButtons[i]
                    .GetComponentInChildren<TextMeshProUGUI>();

                tmp.text =
                    GenerateDynamicCorruption(
                        segment.options[i]);
            }

            yield return new WaitForSeconds(0.1f);
        }
    }

    IEnumerator ShowOfficialLine(string line)
    {
        choicePanel.SetActive(false);
        officialText.gameObject.SetActive(true);
        officialText.text = "";

        foreach (char c in line)
        {
            officialText.text += c;

            yield return new WaitForSeconds(0.02f);
        }
    }
    void UpdateBreakdownVisual()
    {
        // 背景崩坏阈值
        float threshold = 5f;

        // 文字崩坏保持原来逻辑
        foreach (var gt in glitchTexts)
        {
            if (gt != null)
                gt.intensity = breakdown * 0.2f;
        }

        // 只有当 breakdown 超过阈值才开始背景崩坏
        if (breakdown < threshold)
        {
            if (runtimeMat != null)
            {
                runtimeMat.SetVector("_ScanLineJitter", Vector2.zero);
                runtimeMat.SetVector("_VerticalJump", Vector2.zero);
                runtimeMat.SetFloat("_HorizontalShake", 0f);
                runtimeMat.SetVector("_ColorDrift", Vector2.zero);
            }
            return;
        }

        float normalized = Mathf.Clamp01((breakdown - threshold) / (maxBreakdown - threshold));

        if (runtimeMat != null)
        {
            // 缩小幅度
            float scanBase = 0.02f;    // 原来 0.02 是正常 UI 可见
            float verticalBase = 0.05f;
            float shakeBase = 0.02f;
            float driftBase = 0.03f;

            float normalizedCurve = Mathf.Pow(normalized, 2f); // 二次曲线
            runtimeMat.SetVector("_ScanLineJitter", new Vector2(scanBase * normalizedCurve, 0.1f));
            runtimeMat.SetVector("_VerticalJump", new Vector2(verticalBase * normalizedCurve, 0));
            runtimeMat.SetFloat("_HorizontalShake", shakeBase * normalizedCurve);
            runtimeMat.SetVector("_ColorDrift", new Vector2(driftBase * normalizedCurve, 0));
        }
        // 文字崩坏
        foreach (var gt in glitchTexts)
        {
            if (gt != null)
                gt.intensity = breakdown * 0.2f;
        }
    }
    string GenerateDynamicCorruption(string original)
    {
        if (breakdown <= 5)
            return original;

        char[] chars = original.ToCharArray();

        float corruptionChance =
            Mathf.Clamp01((breakdown-5) * 0.01f);

        for (int i = 0; i < chars.Length; i++)
        {
            if (i < 3 && chars[0] == '>' && chars[1] == '>' && chars[2] == ' ')
                continue;

            if (chars[i] == ' ')
                continue;

            if (Random.value < corruptionChance)
            {
                chars[i] = RandomGarbageChar();
            }
        }

        return new string(chars);
    }
    char RandomGarbageChar()
    {
        string gar = "@#$%&01ZX?/";
        return gar[Random.Range(0, gar.Length)];
    }
}
[System.Serializable]
public class SpeechSegment
{
    [TextArea(2, 4)]
    public string officialLine;

    public bool hasChoices;

    [TextArea(2, 4)]
    public string[] options = new string[3];
    // 三个按钮显示的文本（其中一个应该是officialLine）
}

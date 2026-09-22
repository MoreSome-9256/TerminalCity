using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerminalChaosUI : MonoBehaviour
{
    [Header("认知混乱度主显示")]
    [SerializeField] private TMP_Text percentText;         // 显示 "30%"
    [SerializeField] private TMP_Text labelText;           // 显示 "认知混乱度"
    [SerializeField] private Image barFillImage;           // 长条形填充图 (Type 设为 Filled)
    [SerializeField] private Image baseLineImage;          // 细线底轨 (可选)

    [Header("右侧状态等级文本")]
    [SerializeField] private TMP_Text statusTagText;       // 显示 "Normal" / "Warning" / "Danger"

    [Header("三档颜色配置")]
    [SerializeField] private Color normalColor = new Color32(0, 229, 255, 255);    // < 0.7 科技青蓝
    [SerializeField] private Color warningColor = new Color32(255, 170, 0, 255);   // 0.7 ~ 0.9 警示橙黄
    [SerializeField] private Color dangerColor = new Color32(255, 50, 50, 255);     // >= 0.9 高危纯红

    [Header("平滑过渡")]
    [SerializeField] private float smoothSpeed = 8f;       // 进度条跟随插值速度

    private float currentDisplayedChaos = -1f;

    private void Start()
    {
        if (PlayerChaos.Instance != null)
        {
            currentDisplayedChaos = PlayerChaos.Instance.chaos;
            UpdateUI(currentDisplayedChaos);
        }
    }

    private void Update()
    {
        if (PlayerChaos.Instance == null) return;

        float targetChaos = PlayerChaos.Instance.chaos;

        // 平滑跟随
        currentDisplayedChaos = Mathf.Lerp(currentDisplayedChaos, targetChaos, Time.deltaTime * smoothSpeed);

        UpdateUI(currentDisplayedChaos);
    }

    private void UpdateUI(float chaosValue)
    {
        // 1. 更新百分比文本 (例如 30%)
        if (percentText != null)
        {
            int percentInt = Mathf.RoundToInt(chaosValue * 100f);
            percentText.text = $"{percentInt}%";
        }

        // 2. 更新长条形进度槽 (0 ~ 1)
        if (barFillImage != null)
        {
            barFillImage.fillAmount = chaosValue;
        }

        // 3. 计算三档状态、对应文字及颜色
        string statusText;
        Color themeColor;

        if (chaosValue >= 0.9f)
        {
            statusText = "Danger";
            themeColor = dangerColor;
        }
        else if (chaosValue >= 0.7f)
        {
            statusText = "Warning";
            themeColor = warningColor;
        }
        else
        {
            statusText = "Normal";
            themeColor = normalColor;
        }

        // 4. 应用右侧状态文本与颜色
        if (statusTagText != null)
        {
            statusTagText.text = statusText;
            statusTagText.color = themeColor;
        }

        // 5. 主百分比与进度槽同步染色联动
        if (percentText != null) percentText.color = themeColor;
        if (barFillImage != null) barFillImage.color = themeColor;
    }
}
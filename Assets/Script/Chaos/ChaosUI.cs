using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChaosUI : MonoBehaviour
{
    public static ChaosUI Instance { get; private set; } // 便于外部获取

    [Header("UI 元素")]
    public TMP_Text chaosText;       // 显示混乱度百分比
    public Image barFill;            // 填充条
    public Image barBackground;      // 背景条（可选）
    [Range(0f, 1f)]
    public float backgroundAlpha = 0.5f; // 固定透明度

    [Header("动画设置")]
    public float fillSpeed = 0.5f; // 进度条动画速度（每秒变化比例）

    private float currentFill = 0f;

    // ✨ 新增：纯表现层覆盖标志与目标值
    private bool isOverridden = false;
    private float overrideChaosValue = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }
    private void Update()
    {
        // 🌟 核心：如果是覆盖模式，直接使用虚假的演出数值；否则读真实的 PlayerChaos
        float chaos = 0f;
        if (isOverridden)
        {
            chaos = overrideChaosValue;
        }
        else
        {
            if (PlayerChaos.Instance == null) return;
            chaos = PlayerChaos.Instance.chaos; // 0~1
        }

        int percent = Mathf.RoundToInt(chaos * 100f); 

        // 更新文本
        if (chaosText != null)
        {
            chaosText.text = $"{percent}%";
        }

        // 平滑更新填充条
        if (barFill != null)
        {
            currentFill = Mathf.MoveTowards(currentFill, chaos, fillSpeed * Time.deltaTime);
            barFill.fillAmount = currentFill;

            // 颜色渐变
            Color low = Color.white;
            Color high = Color.red;
            barFill.color = Color.Lerp(low, high, currentFill);
        }

        if (barBackground != null)
        {
            Color low = Color.white;
            Color high = Color.red;
            Color c = Color.Lerp(low, high, currentFill);
            c.a = backgroundAlpha;
            barBackground.color = c;
        }
    }
    /// <summary>
    /// ✨ 开启前端数值覆盖（仅演出用，不改动真实混乱度）
    /// </summary>
    public void SetOverrideChaos(float targetValue)
    {
        isOverridden = true;
        overrideChaosValue = Mathf.Clamp01(targetValue);
    }

    /// <summary>
    /// ✨ 解除覆盖，恢复读取真实 PlayerChaos
    /// </summary>
    public void ClearOverride()
    {
        isOverridden = false;
    }
}

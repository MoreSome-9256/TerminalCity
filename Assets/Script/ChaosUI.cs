using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChaosUI : MonoBehaviour
{
    [Header("UI 元素")]
    public TMP_Text chaosText;       // 显示混乱度百分比
    public Image barFill;            // 填充条
    public Image barBackground;      // 背景条（可选）
    [Range(0f, 1f)]
    public float backgroundAlpha = 0.5f; // 固定透明度

    private void Update()
    {
        if (PlayerChaos.Instance == null) return;

        float chaos = PlayerChaos.Instance.chaos; // 0~1
        int percent = Mathf.RoundToInt(chaos * 100f);

        // 更新文本
        if (chaosText != null)
        {
            chaosText.text = $"{percent}%";
        }

        // 更新进度条
        if (barFill != null)
        {
            barFill.fillAmount = chaos;
        }

        if (barFill != null)
        {
            Color low = Color.white;
            Color high = Color.red;
            barFill.color = Color.Lerp(low, high, chaos);
        }
        if (barBackground != null)
        {
            Color low = Color.white;
            Color high = Color.red;
            Color c = Color.Lerp(low, high, chaos);
            c.a = backgroundAlpha;
            barBackground.color = c;
        }
    }
}

using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class UILightFlicker : MonoBehaviour
{
    private Image lightImage;
    private Color originalColor;

    [Header("透明度范围")]
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.2f;   // 最暗时的透明度
    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 0.8f;   // 最亮时的透明度

    [Header("平滑闪烁设置")]
    [Tooltip("基础呼吸/忽明忽暗的速率")]
    [SerializeField] private float flickerSpeed = 5f;

    [Header("骤闪/坏灯接触不良效果")]
    [Tooltip("是否开启骤暗/突闪效果")]
    [SerializeField] private bool enableSpike = true;
    [Tooltip("每帧触发突闪的概率 (0 - 1)")]
    [Range(0f, 0.2f)]
    [SerializeField] private float spikeChance = 0.05f;

    private float noiseOffset;

    void Awake()
    {
        lightImage = GetComponent<Image>();
        originalColor = lightImage.color;
        // 随机种子，防止多个相同脚本的物体闪烁频率完全一致
        noiseOffset = Random.Range(0f, 100f);
    }

    void Update()
    {
        if (lightImage == null) return;

        // 1. 使用一维柏林噪声计算平滑过渡的亮度基础值 (范围 0~1)
        float noise = Mathf.PerlinNoise(noiseOffset, Time.time * flickerSpeed);
        float targetAlpha = Mathf.Lerp(minAlpha, maxAlpha, noise);

        // 2. 模拟接触不良的瞬间骤降/骤闪 (Spike)
        if (enableSpike && Random.value < spikeChance)
        {
            // 瞬间掉到极暗或极亮
            targetAlpha = Random.value > 0.5f ? minAlpha * 0.5f : maxAlpha;
        }

        // 3. 应用回 Image 的 Alpha 通道
        Color currentColor = lightImage.color;
        currentColor.a = targetAlpha;
        lightImage.color = currentColor;
    }

    /// <summary>
    /// 外部重置或停用灯光时恢复原始颜色
    /// </summary>
    private void OnDisable()
    {
        if (lightImage != null)
        {
            lightImage.color = originalColor;
        }
    }
}
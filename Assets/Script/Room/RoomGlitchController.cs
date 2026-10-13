using UnityEngine;
using UnityEngine.UI;

public class RoomGlitchController : MonoBehaviour
{
    [Header("UI 原生故障强度")]
    [Range(0f, 1f)] public float glitchIntensity = 0f;

    [Header("指定需要抖动的 UI (直接拖入 RectTransform)")]
    [SerializeField] private RectTransform[] targetRects;

    private Vector2[] originalPositions;
    private CanvasGroup[] canvasGroups;

    private void Awake()
    {
        if (targetRects == null) return;

        originalPositions = new Vector2[targetRects.Length];
        canvasGroups = new CanvasGroup[targetRects.Length];

        for (int i = 0; i < targetRects.Length; i++)
        {
            if (targetRects[i] != null)
            {
                // 记录初始位置
                originalPositions[i] = targetRects[i].anchoredPosition;

                // 自动挂载 CanvasGroup 用于控制电平闪烁
                canvasGroups[i] = targetRects[i].GetComponent<CanvasGroup>();
                if (canvasGroups[i] == null)
                {
                    canvasGroups[i] = targetRects[i].gameObject.AddComponent<CanvasGroup>();
                }
            }
        }
    }

    private void Update()
    {
        UpdateGlitchEffect();
    }

    public void SetGlitchIntensity(float intensity)
    {
        glitchIntensity = Mathf.Clamp01(intensity);
        UpdateGlitchEffect();
    }

    private void UpdateGlitchEffect()
    {
        if (targetRects == null) return;

        for (int i = 0; i < targetRects.Length; i++)
        {
            if (targetRects[i] == null) continue;

            if (glitchIntensity <= 0.001f)
            {
                // 恢复原状
                targetRects[i].anchoredPosition = originalPositions[i];
                if (canvasGroups[i] != null) canvasGroups[i].alpha = 1f;
                continue;
            }

            // 1. 模拟 Horizontal Shake & ScanLine Jitter (高频水平与垂直撕裂)
            float shakeX = Random.Range(-45f, 45f) * glitchIntensity;
            float shakeY = Random.Range(-10f, 10f) * glitchIntensity;

            // 模拟 Vertical Jump (偶尔出现大幅度跳跃位移)
            if (Random.value < 0.15f * glitchIntensity)
            {
                shakeX *= 2.5f;
                shakeY += Random.Range(-40f, 40f) * glitchIntensity;
            }

            targetRects[i].anchoredPosition = originalPositions[i] + new Vector2(shakeX, shakeY);

            // 2. 模拟 CRT 电压不稳引起的闪烁感
            if (canvasGroups[i] != null)
            {
                // 强度越大，透明度跌落得越狠
                canvasGroups[i].alpha = Random.Range(1f - glitchIntensity * 0.8f, 1f);
            }
        }
    }
}
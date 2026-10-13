using System.Collections;
using UnityEngine;

[RequireComponent(typeof(InscribedPolygonRenderer))]
public class CenterGraphicVisualController : MonoBehaviour
{
    [Header("旋转设置")]
    [Tooltip("每秒自转的角度（负值顺时针，正值逆时针，建议 -10 到 -15 度慢速旋转）")]
    [SerializeField] private float rotationSpeed = -12f;
    [SerializeField] private bool isRotating = false;

    [Header("开场绘制动画参数")]
    [Tooltip("勾勒完成耗时（秒）")]
    [SerializeField] private float drawDuration = 1.4f;
    [Tooltip("绘制动画曲线（推荐缓入缓出）")]
    [SerializeField] private AnimationCurve drawCurve = AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private RectTransform rectTransform;
    private InscribedPolygonRenderer polygonRenderer;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        polygonRenderer = GetComponent<InscribedPolygonRenderer>();

        // 初始设为 0（隐藏未画状态）
        if (polygonRenderer != null)
        {
            polygonRenderer.DrawProgress = 0f;
        }
    }

    private void Update()
    {
        // 慢速旋转图形
        if (isRotating && rectTransform != null)
        {
            rectTransform.Rotate(Vector3.forward, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// 开场画圆动画（协程，画完后才会返回继续对话）
    /// </summary>
    public IEnumerator PlayDrawAnimation()
    {
        if (polygonRenderer == null) yield break;

        // 归零角度与进度
        rectTransform.localEulerAngles = Vector3.zero;
        polygonRenderer.DrawProgress = 0f;

        float elapsed = 0f;
        while (elapsed < drawDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / drawDuration);
            polygonRenderer.DrawProgress = drawCurve.Evaluate(t);
            yield return null;
        }

        polygonRenderer.DrawProgress = 1f;

        // 画完后立即开启慢速旋转！
        isRotating = true;
    }

    public void StopRotation()
    {
        isRotating = false;
    }
}
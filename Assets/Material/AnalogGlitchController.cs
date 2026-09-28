using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class AnalogGlitchController : MonoBehaviour
{
    [Header("Glitch Settings")]
    [Range(0f, 1f)] public float scanLineJitter = 0.05f;
    [Range(0f, 1f)] public float verticalJump = 0.05f;
    [Range(0f, 0.1f)] public float horizontalShake = 0.02f;
    [Range(0f, 0.2f)] public float colorDrift = 0.03f;

    private Material instancedMat;
    private Image img;

    // Shader 属性 ID（缓存以提升性能）
    private static readonly int ScanLineJitterID = Shader.PropertyToID("_ScanLineJitter");
    private static readonly int VerticalJumpID = Shader.PropertyToID("_VerticalJump");
    private static readonly int HorizontalShakeID = Shader.PropertyToID("_HorizontalShake");
    private static readonly int ColorDriftID = Shader.PropertyToID("_ColorDrift");

    void Awake()
    {
        img = GetComponent<Image>();
        // 实例化材质，避免修改污染 Project 里的全局材质资源
        if (img.material != null)
        {
            instancedMat = new Material(img.material);
            img.material = instancedMat;
        }
    }

    void Update()
    {
        if (instancedMat == null) return;

        // 实时更新参数到材质
        instancedMat.SetVector(ScanLineJitterID, new Vector4(scanLineJitter, 0.8f, 0, 0));
        instancedMat.SetVector(VerticalJumpID, new Vector4(verticalJump, Time.time * 2f, 0, 0));
        instancedMat.SetFloat(HorizontalShakeID, horizontalShake);
        instancedMat.SetVector(ColorDriftID, new Vector4(colorDrift, Time.time * 4f, 0, 0));
    }

    void OnDestroy()
    {
        // 销毁实例化的材质防止内存泄漏
        if (instancedMat != null)
        {
            Destroy(instancedMat);
        }
    }
}
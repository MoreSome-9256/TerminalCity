using UnityEngine;
using UnityEngine.UI;

public class RoomGlitchController : MonoBehaviour
{
    [Header("Shader 参数")]
    [Range(0f, 1f)] public float glitchIntensity = 0f;

    [Header("基础材质球（使用上面的 UIAnalogGlitch_Multi）")]
    [SerializeField] private Material baseGlitchMaterial;

    private Material runtimeMaterial;
    private Graphic[] childGraphics;

    private void Awake()
    {
        if (baseGlitchMaterial == null) return;

        // 创建一个共享的运行时材质实例，避免污染 Project 资源
        runtimeMaterial = new Material(baseGlitchMaterial);

        // 批量分发给房间 Prefab 下的所有 Image / RawImage
        childGraphics = GetComponentsInChildren<Graphic>(true);
        foreach (var graphic in childGraphics)
        {
            graphic.material = runtimeMaterial;
        }

        UpdateShaderProperties();
    }

    private void Update()
    {
        // 外部改动 intensity 时实时同步到 Shader
        UpdateShaderProperties();
    }

    public void SetGlitchIntensity(float intensity)
    {
        glitchIntensity = Mathf.Clamp01(intensity);
        UpdateShaderProperties();
    }

    private void UpdateShaderProperties()
    {
        if (runtimeMaterial == null) return;

        if (glitchIntensity <= 0.001f)
        {
            runtimeMaterial.SetVector("_ScanLineJitter", Vector4.zero);
            runtimeMaterial.SetVector("_VerticalJump", Vector4.zero);
            runtimeMaterial.SetFloat("_HorizontalShake", 0f);
            runtimeMaterial.SetVector("_ColorDrift", Vector4.zero);
            return;
        }

        // 按照传入强度动态缩放各项故障参数[cite: 8]
        runtimeMaterial.SetVector("_ScanLineJitter", new Vector2(0.04f * glitchIntensity, 0.1f));
        runtimeMaterial.SetVector("_VerticalJump", new Vector2(0.08f * glitchIntensity, 0f));
        runtimeMaterial.SetFloat("_HorizontalShake", 0.05f * glitchIntensity);
        runtimeMaterial.SetVector("_ColorDrift", new Vector2(0.06f * glitchIntensity, 0f)); 
    }

    private void OnDestroy()
    {
        if (runtimeMaterial != null)
        {
            Destroy(runtimeMaterial);
        }
    }
}
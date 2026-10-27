using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomVisualGlitch : MonoBehaviour
{
    [Header("故障总强度 (0 ~ 1)")]
    [Range(0f, 1f)][SerializeField] private float intensity = 0f;

    [Header("目标列表 (可拖入多个 Image / RawImage)")]
    [SerializeField] private List<Graphic> targetGraphics = new List<Graphic>();

    [Header("微调参数")]
    [Range(0f, 0.08f)][SerializeField] private float maxChromaticDisp = 0.025f;

    // 缓存每个 UI 物体独立的运行时材质
    private List<Material> runtimeMaterials = new List<Material>();

    private static readonly int IntensityProp = Shader.PropertyToID("_Intensity");
    private static readonly int ChromaticDispProp = Shader.PropertyToID("_ChromaticDisp");

    private void Awake()
    {
        InitializeMaterials();
    }

    private void InitializeMaterials()
    {
        runtimeMaterials.Clear();

        if (targetGraphics == null || targetGraphics.Count == 0) return;

        for (int i = 0; i < targetGraphics.Count; i++)
        {
            var graphic = targetGraphics[i];
            if (graphic != null && graphic.material != null)
            {
                // 实例化一份运行时材质，避免改动 Project 资产文件
                Material matInstance = new Material(graphic.material);
                graphic.material = matInstance;
                runtimeMaterials.Add(matInstance);
            }
            else
            {
                runtimeMaterials.Add(null);
            }
        }

        ApplyIntensity();
    }

    private void Update()
    {
        // 允许在 Inspector 实时拖动强度滑条进行预览
        ApplyIntensity();
    }

    public void SetIntensity(float value)
    {
        intensity = Mathf.Clamp01(value);
        ApplyIntensity();
    }

    private void ApplyIntensity()
    {
        if (runtimeMaterials.Count == 0) return;

        for (int i = 0; i < runtimeMaterials.Count; i++)
        {
            Material mat = runtimeMaterials[i];
            if (mat != null)
            {
                mat.SetFloat(IntensityProp, intensity);
                mat.SetFloat(ChromaticDispProp, maxChromaticDisp);

                // 通知对应 Graphic 重新提交渲染状态
                if (targetGraphics[i] != null)
                {
                    targetGraphics[i].SetMaterialDirty();
                }
            }
        }
    }

    private void OnDisable()
    {
        // 禁用时将所有材质的强度安全归零
        foreach (var mat in runtimeMaterials)
        {
            if (mat != null)
            {
                mat.SetFloat(IntensityProp, 0f);
            }
        }
    }

    private void OnDestroy()
    {
        // 销毁时释放实例化的材质内存
        foreach (var mat in runtimeMaterials)
        {
            if (mat != null)
            {
                Destroy(mat);
            }
        }
        runtimeMaterials.Clear();
    }
}
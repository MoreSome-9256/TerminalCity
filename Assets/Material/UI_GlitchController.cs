using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Graphic))]
public class UI_GlitchController : MonoBehaviour
{
    [Header("故障触发间隔（秒）")]
    [SerializeField] private float glitchInterval = 3f;

    [Header("单次故障持续时间（秒）")]
    [SerializeField] private float glitchDuration = 0.15f;

    [Header("色差分离强度")]
    [SerializeField] private float rgbSplit = 0.015f;

    private Material instancedMat;
    private Graphic targetGraphic;

    private void Awake()
    {
        targetGraphic = GetComponent<Graphic>();
        if (targetGraphic.material == null) return;

        // 实例化材质，确保每个窗口参数独立、不互相污染
        instancedMat = new Material(targetGraphic.material);
        targetGraphic.material = instancedMat;

        // 根据物体世界坐标和物体 Hash 生成该窗口独有的 Seed，彻底打乱各窗口时间节奏
        float seed = (transform.position.x * 0.17f + transform.position.y * 0.31f + GetInstanceID() % 100) * 1.5f;
        instancedMat.SetFloat("_Seed", Mathf.Abs(seed));

        ApplyParameters();
    }

    public void ApplyParameters()
    {
        if (instancedMat == null) return;

        instancedMat.SetFloat("_GlitchInterval", glitchInterval);
        instancedMat.SetFloat("_GlitchDuration", glitchDuration);
        instancedMat.SetFloat("_RGBShift", rgbSplit);
    }

    private void OnValidate()
    {
        if (Application.isPlaying && instancedMat != null)
        {
            ApplyParameters();
        }
    }

    private void OnDestroy()
    {
        if (instancedMat != null)
        {
            Destroy(instancedMat);
        }
    }
}
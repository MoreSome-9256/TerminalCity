using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

[RequireComponent(typeof(Image))]
public class DynamicBlurController : MonoBehaviour
{
    [Header("模糊参数")]
    [SerializeField] private float blurSpeed = 2f;
    [SerializeField] private float maxBlurRadius = 15f;

    [Header("事件响应")]
    public UnityEvent onBlurComplete;

    private Material blurMaterial;
    private float targetBlur;
    private float currentBlur;
    private bool isAnimating = false;

    void Start()
    {
        Image image = GetComponent<Image>();
        if (image == null)
        {
            Debug.LogError("未找到 Image 组件！", this);
            enabled = false;
            return;
        }

        if (image.material == null)
        {
            Debug.LogError("Image 的材质未分配！", this);
            enabled = false;
            return;
        }

        // 创建材质实例
        blurMaterial = new Material(image.material);
        image.material = blurMaterial;

        // 初始化参数
        currentBlur = blurMaterial.GetFloat("_BlurRadius");
        targetBlur = currentBlur;
    }
    // 在原有类中添加以下属性
    public float BlurSpeed
    {
        get => blurSpeed;
        set => blurSpeed = value;
    }

    public float MaxBlurRadius
    {
        get => maxBlurRadius;
        set => maxBlurRadius = value;
    }
    public void StartBlurAnimation(float target)
    {
        Debug.Log("startBlurAnimation");
        if (!isAnimating && blurMaterial != null)
            //Debug.Log("startBlurAnimation");
            StartCoroutine(BlurCoroutine(Mathf.Clamp(target, 0, maxBlurRadius)));
    }

    private IEnumerator BlurCoroutine(float target)
    {
        isAnimating = true;
        float startBlur = currentBlur;
        float duration = Mathf.Abs(target - startBlur) / blurSpeed;
        float elapsed = 0;

        while (elapsed < duration)
        {
            currentBlur = Mathf.Lerp(startBlur, target, elapsed / duration);
            blurMaterial.SetFloat("_BlurRadius", currentBlur);
            elapsed += Time.deltaTime;
            yield return null;
        }

        currentBlur = target;
        blurMaterial.SetFloat("_BlurRadius", currentBlur);
        isAnimating = false;
        onBlurComplete.Invoke();
    }
}
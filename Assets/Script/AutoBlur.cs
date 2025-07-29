using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class AutoBlur : MonoBehaviour
{
    [Header("模糊控制")]
    [SerializeField] private DynamicBlurController blurController;
    [SerializeField] private float transitionDuration = 2f; // 过渡时长

    [Header("初始设置")]
    [SerializeField] private float startBlur = 15f;  // 初始模糊度
    [SerializeField] private float endBlur = 0f;     // 目标清晰度

    IEnumerator Start()
    {
        yield return new WaitForEndOfFrame(); // 等待其他组件初始化

        if (blurController == null)
        {
            Debug.LogError("模糊控制器未分配！", this);
            yield break;
        }

        if (!blurController.gameObject.activeInHierarchy)
        {
            Debug.LogError("模糊控制器未激活！", this);
            yield break;
        }

        StartCoroutine(BlurTransition());
    }

    // 强制设置初始模糊状态
    private void SetInitialBlur()
    {
        if (blurController == null) return;
        // 通过反射强制设置当前模糊度（规避动画系统）
        var controllerType = typeof(DynamicBlurController);
        var currentBlurField = controllerType.GetField("currentBlur",
            System.Reflection.BindingFlags.NonPublic |
            System.Reflection.BindingFlags.Instance);

        currentBlurField?.SetValue(blurController, startBlur);
        blurController.GetComponent<Image>().material.SetFloat("_BlurRadius", startBlur);
    }

    IEnumerator BlurTransition()
    {
        if (blurController == null) yield break;

        // 计算实际需要的动画速度
        float requiredSpeed = Mathf.Abs(startBlur - endBlur) / transitionDuration;

        // 临时修改动画速度
        float originalSpeed = blurController.BlurSpeed;
        blurController.BlurSpeed = requiredSpeed;

        // 启动动画
        blurController.StartBlurAnimation(endBlur);

        // 等待动画完成
        yield return new WaitForSeconds(transitionDuration);

        // 还原原始速度（如果需要后续其他操作）
        blurController.BlurSpeed = originalSpeed;
    }

    // 编辑器验证
    void OnValidate()
    {
        if (blurController != null && blurController.MaxBlurRadius < startBlur)
        {
            Debug.LogWarning($"初始模糊度 {startBlur} 不能超过控制器的最大限制 {blurController.MaxBlurRadius}");
            startBlur = blurController.MaxBlurRadius;
        }
    }
}
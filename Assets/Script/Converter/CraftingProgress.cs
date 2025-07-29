using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;

public class CraftingProgress : MonoBehaviour
{
    [Header("进度条参数")]
    [SerializeField] private Image progressFill; // 关联的填充图片
    [SerializeField] private float minShowTime = 0.5f; // 最小显示时间（保持视觉完整性）
    [SerializeField] private AnimationCurve progressCurve; // 进度变化曲线

    public UnityEvent OnProgressComplete1;
    public UnityEvent OnProgressFailed;
    public UnityEvent OnProgressRepete;

    public bool IsRunning { get; private set; }

    private Coroutine _progressCoroutine;

    // 启动进度条动画
    public void ShowProgress(System.Action onComplete)
    {
        if (_progressCoroutine != null) StopCoroutine(_progressCoroutine);
        _progressCoroutine = StartCoroutine(ProgressRoutine(onComplete));
    }

    private IEnumerator ProgressRoutine(System.Action onComplete)
    {
        IsRunning = true;
        progressFill.fillAmount = 0;
        float timer = 0;

        // 第一阶段：基础进度动画
        while (timer < minShowTime)
        {
            timer += Time.deltaTime;
            progressFill.fillAmount = progressCurve.Evaluate(timer / minShowTime);
            yield return null;
        }

        // 第二阶段：保持进度条可见直到回调完成
        progressFill.fillAmount = 1f;
        //OnProgressComplete1?.Invoke();
        onComplete?.Invoke();

        // 延迟隐藏确保视觉完整
        yield return new WaitForSeconds(0.2f);

        IsRunning = false;
        _progressCoroutine = null;
    }

    // 立即隐藏
    public void HideImmediate()
    {
        if (_progressCoroutine != null)
        {
            StopCoroutine(_progressCoroutine);
            _progressCoroutine = null;
        }
        progressFill.fillAmount = 0;
    }
}
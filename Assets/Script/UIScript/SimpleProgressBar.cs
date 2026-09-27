using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Events;

public class SimpleProgressBar : MonoBehaviour
{
    [Header("进度条设置")]
    public Image progressFill;                   // 进度条填充 Image
    public float duration = 1.0f;                // 填充持续时间
    public AnimationCurve progressCurve = AnimationCurve.Linear(0, 0, 1, 1); // 默认线性曲线

    private Coroutine progressRoutine;
    public UnityEvent OnProgressComplete1;
    public GameObject message;

    public void ShowProgress()
    {
        Debug.Log("ShowProgress");
        message.SetActive(false);
        if (progressRoutine != null)
            StopCoroutine(progressRoutine);

        progressRoutine = StartCoroutine(FillProgressBar());
        
    }

    private IEnumerator FillProgressBar()
    {
        float timer = 0f;
        progressFill.fillAmount = 0f;

        while (timer < duration)
        {
            timer += Time.deltaTime;
            float t = Mathf.Clamp01(timer / duration);
            progressFill.fillAmount = progressCurve.Evaluate(t);
            yield return null;
        }

        progressFill.fillAmount = progressCurve.Evaluate(1f); // 确保最终满值
        OnProgressComplete1?.Invoke();
        progressRoutine = null;
    }

    public void ResetProgress()
    {
        if (progressRoutine != null)
        {
            StopCoroutine(progressRoutine);
            progressRoutine = null;
        }
        progressFill.fillAmount = 0f;
    }
}

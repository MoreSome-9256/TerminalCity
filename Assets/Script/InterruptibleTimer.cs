using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class InterruptibleTimer : MonoBehaviour
{
    public float countdownTime = 10f; // 倒计时时间
    public UnityEvent onTimerFinished; // 倒计时未中止时触发
    public UnityEvent onTimerInterrupted; // 被中止时可选触发

    public TextMeshProUGUI countdownText;

    private Coroutine timerCoroutine;
    private bool isInterrupted = false;

    // 启动倒计时
    public void StartCountdown()
    {
        StopCountdown(); // 防止重复启动
        isInterrupted = false;
        timerCoroutine = StartCoroutine(CountdownCoroutine());
    }

    // 中止倒计时（由外部事件调用）
    public void Interrupt()
    {
        if (timerCoroutine != null)
        {
            isInterrupted = true;
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
            onTimerInterrupted?.Invoke();
            if (countdownText != null)
                countdownText.text = ""; // 或者显示 "中止"
            Debug.Log("Countdown interrupted.");
        }
    }

    // 内部协程
    private IEnumerator CountdownCoroutine()
    {
        float timeLeft = countdownTime;

        while (timeLeft > 0f)
        {
            if (isInterrupted) yield break;

            timeLeft -= Time.deltaTime;

            // 更新 UI 文本
            if (countdownText != null)
            {
                int minutes = Mathf.FloorToInt(timeLeft / 60);
                int seconds = Mathf.FloorToInt(timeLeft % 60);
                countdownText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }

            yield return null;
        }

        if (!isInterrupted)
        {
            Debug.Log("Countdown finished.");
            onTimerFinished?.Invoke();
            if (countdownText != null)
                countdownText.text = "0"; // 最终显示为 0
        }
    }

    // 可选：立即停止倒计时
    public void StopCountdown()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;
        }
    }
}

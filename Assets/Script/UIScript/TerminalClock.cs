using UnityEngine;
using TMPro;

public class TerminalClock : MonoBehaviour
{
    [Header("UI 文本组件")]
    [SerializeField] private TMP_Text dateText; // 显示年月日，例如 322.9.21
    [SerializeField] private TMP_Text timeText; // 显示时分，例如 00:00

    [Header("起始时间设定 (游戏世界起点)")]
    [SerializeField] private int startYear = 322;
    [SerializeField] private int startMonth = 9;
    [SerializeField] private int startDay = 21;
    [SerializeField] private int startHour = 0;
    [SerializeField] private int startMinute = 0;

    [Header("流速与计时参数")]
    [Tooltip("虚拟时间相比现实时间的倍率。1 表示 1:1 纯游玩时长；60 表示现实 1 秒等于游戏 1 分钟")]
    [SerializeField] private float timeMultiplier = 1f;

    [Tooltip("是否允许时间流逝")]
    [SerializeField] private bool isRunning = true;

    [Tooltip("冒号是否按秒交替闪烁 (经典终端感)")]
    [SerializeField] private bool blinkColon = true;

    // 累计游玩时间（秒），后续做存档时直接存读该值即可
    [SerializeField] private double elapsedSeconds = 0f;

    private int lastDisplayedMinute = -1;
    private bool colonVisible = true;
    private float blinkTimer = 0f;

    private void Update()
    {
        if (!isRunning) return;

        // 累计时间（不受 Time.timeScale 暂停影响，使用 unscaledDeltaTime 记录实际游玩耗时）
        elapsedSeconds += Time.unscaledDeltaTime * timeMultiplier;

        // 处理冒号闪烁（每 0.5 秒切换一次）
        if (blinkColon)
        {
            blinkTimer += Time.unscaledDeltaTime;
            if (blinkTimer >= 0.5f)
            {
                blinkTimer = 0f;
                colonVisible = !colonVisible;
                UpdateTimeDisplay();
            }
        }

        // 计算当前总分钟数，检查是否需要刷新文字
        int currentTotalMinutes = (int)(elapsedSeconds / 60.0);
        if (currentTotalMinutes != lastDisplayedMinute)
        {
            lastDisplayedMinute = currentTotalMinutes;
            UpdateFullDisplay();
        }
    }

    /// <summary>
    /// 全量计算并更新日期与时间
    /// </summary>
    private void UpdateFullDisplay()
    {
        long totalMinutes = (long)(elapsedSeconds / 60.0) + (startHour * 60 + startMinute);

        // 当天分钟数与小时换算
        long dayMinutes = totalMinutes % (24 * 60);
        int currentHour = (int)(dayMinutes / 60);
        int currentMinute = (int)(dayMinutes % 60);

        // 跨越的天数换算
        int passedDays = (int)(totalMinutes / (24 * 60));
        CalculateCalendar(passedDays, out int curYear, out int curMonth, out int curDay);

        // 格式化日期：322.9.21
        if (dateText != null)
        {
            dateText.text = $"{curYear}.{curMonth}.{curDay}";
        }

        // 格式化时间
        UpdateTimeDisplay(currentHour, currentMinute);
    }

    /// <summary>
    /// 单独更新时分显示（支持冒号闪烁）
    /// </summary>
    private void UpdateTimeDisplay(int hour = -1, int minute = -1)
    {
        if (timeText == null) return;

        if (hour == -1 || minute == -1)
        {
            long totalMinutes = (long)(elapsedSeconds / 60.0) + (startHour * 60 + startMinute);
            long dayMinutes = totalMinutes % (24 * 60);
            hour = (int)(dayMinutes / 60);
            minute = (int)(dayMinutes % 60);
        }

        string colon = colonVisible || !blinkColon ? ":" : "<color=#00000000>:</color>"; // 用全透明占位防止文字宽度跳动
        timeText.text = $"{hour:D2}{colon}{minute:D2}";
    }

    /// <summary>
    /// 简易日历推进计算（按每月 30 天计算；如需标准大小月也可后续扩展）
    /// </summary>
    private void CalculateCalendar(int addedDays, out int outYear, out int outMonth, out int outDay)
    {
        int totalDays = (startDay - 1) + addedDays;
        int totalMonths = (startMonth - 1) + (totalDays / 30);

        outDay = (totalDays % 30) + 1;
        outMonth = (totalMonths % 12) + 1;
        outYear = startYear + (totalMonths / 12);
    }

    #region 外部调用接口

    /// <summary>
    /// 设置/读取游玩总秒数（用于读档）
    /// </summary>
    public void SetElapsedTime(double seconds)
    {
        elapsedSeconds = seconds;
        UpdateFullDisplay();
    }

    public double GetElapsedTime() => elapsedSeconds;

    public void PauseClock(bool pause) => isRunning = !pause;

    #endregion
}
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerminalInstabilityUI : MonoBehaviour
{
    [Header("UI 组件引用")]
    [SerializeField] private TMP_Text valueText;       // 显示 "当前空间逆恒值：XXXXLoci"
    [SerializeField] private TMP_Text statusBadgeText; // 显示 "SAFE", "CAUTION" 等
    [SerializeField] private Image statusBadgeBg;      // 状态底框（联动变色）
    [SerializeField] private Image cubeIcon;           // 左侧立方体图标

    [Header("状态图标配置 (四档)")]
    [SerializeField] private Sprite iconSafe;          // 安全状态图标 (< 9700)
    [SerializeField] private Sprite iconCaution;       // 注意/轻微波动图标 (9700 ~ 9800)
    [SerializeField] private Sprite iconWarning;       // 警告图标 (9800 ~ 9900)
    [SerializeField] private Sprite iconDanger;        // 危险/濒临崩溃图标 (>= 9900)

    private int lastRoomID = -999;
    private int lastInstability = -1;

    private void Update()
    {
        if (RoomManager.Instance == null) return;

        int currentRoomID = RoomManager.Instance.currentRoomID;
        RoomState roomState = RoomManager.Instance.GetRoomState(currentRoomID);

        if (roomState == null) return;

        // 仅在房间切换或数值实际发生变化时刷新一次[cite: 5, 6]
        if (currentRoomID != lastRoomID || roomState.instability != lastInstability)
        {
            lastRoomID = currentRoomID;
            lastInstability = roomState.instability;
            UpdateInstabilityDisplay(roomState.instability, roomState.instabilityThreshold);
        }
    }

    /// <summary>
    /// 刷新数据与外观状态
    /// </summary>
    public void UpdateInstabilityDisplay(int currentInstability, int threshold)
    {
        // 1. 刷新实时数值
        if (valueText != null)
        {
            valueText.text = $"当前空间逆恒值: {currentInstability}Loci";
        }

        // 2. 状态判定与图标、颜色映射（对齐 InstabilityTextProvider 四档梯度）
        string statusTag;
        Color themeColor;
        Sprite targetIcon;

        if (currentInstability < 9700)
        {
            statusTag = "SAFE";
            themeColor = new Color32(185, 217, 255, 255); // 科技青蓝
            targetIcon = iconSafe;
        }
        else if (currentInstability < 9800)
        {
            statusTag = "CAUTION";
            themeColor = new Color32(255, 220, 90, 255); // 警示黄
            targetIcon = iconCaution;
        }
        else if (currentInstability < 9900)
        {
            statusTag = "WARNING";
            themeColor = new Color32(255, 140, 50, 255); // 警告橙
            targetIcon = iconWarning;
        }
        else if (currentInstability < 10000)
        {
            statusTag = "DANGER";
            themeColor = new Color32(255, 60, 60, 255);  // 极度危险红
            targetIcon = iconDanger;
        }
        else
        {
            // 突破极限时沿用危险图标与暗红色
            statusTag = "CRITICAL";
            themeColor = new Color32(220, 20, 60, 255);
            targetIcon = iconDanger;
        }

        // 3. 应用图标贴图
        if (cubeIcon != null && targetIcon != null)
        {
            cubeIcon.sprite = targetIcon;
        }

        // 4. 应用颜色联动
        if (cubeIcon != null)
        {
            cubeIcon.color = themeColor;
        }

        if (statusBadgeText != null)
        {
            statusBadgeText.text = statusTag;
            statusBadgeText.color = themeColor;
        }

        if (statusBadgeBg != null)
        {
            statusBadgeBg.color = new Color(themeColor.r, themeColor.g, themeColor.b, 0.25f);
        }
    }
}
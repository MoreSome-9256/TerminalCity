using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Linq;

public class ClueLightShow : MonoBehaviour
{
    [Header("UI 引用")]
    [SerializeField] private Image[] lightIcons; // 拖入3个子物体Image

    // 缓存当前状态避免频繁刷新
    private bool[] currentStates = new bool[3];
    private int lastCount = -1;

    private void Update()
    {
        UpdateLightDisplay();
    }

    public void UpdateLightDisplay()
    {
        // 获取最新状态
        int newCount = CountLights();

        // 状态无变化时跳过
        if (newCount == lastCount) return;

        // 更新显示
        for (int i = 0; i < lightIcons.Length; i++)
        {
            // 按顺序激活前N个图标
            lightIcons[i].gameObject.SetActive(i < newCount);
        }

        // 强制刷新布局
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            lightIcons[0].transform.parent as RectTransform
        );

        lastCount = newCount;
    }

    // 优化后的计数方法
    public int CountLights()
    {
        Array.Clear(currentStates, 0, 3);

        foreach (Window w in FindObjectsOfType<Window>())
        {
            Level1Data data = w.GetLevel1Data();
            if (data == null) continue;

            currentStates[0] |= !string.IsNullOrEmpty(data.Name);
            currentStates[1] |= !string.IsNullOrEmpty(data.Time);
            currentStates[2] |= !string.IsNullOrEmpty(data.Event);
        }

        return currentStates.Count(b => b);
    }
}
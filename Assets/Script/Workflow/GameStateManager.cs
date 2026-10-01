using System;
using System.Collections.Generic;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    // 存储当前所有已达成的标记（可以是线性阶段名如 "PROLOGUE_BAG_CLOSED"，也可以是物品如 "KEY_BASEMENT"）
    private HashSet<string> activeFlags = new HashSet<string>();

    // 状态变更广播：当任何 Flag 改变时，通知场景内所有带锁的物体自己去核对一次
    public static event Action OnStateChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 点亮某个标记（例如获得钥匙、关完背包、播完某段对话）
    /// </summary>
    public void SetFlag(string flagKey)
    {
        if (string.IsNullOrEmpty(flagKey)) return;

        if (activeFlags.Add(flagKey))
        {
            Debug.Log($"<color=#00FF00>[GameState] 状态更新: {flagKey}</color>");
            OnStateChanged?.Invoke(); // 全场广播
        }
    }

    /// <summary>
    /// 检查某个标记是否存在
    /// </summary>
    public bool HasFlag(string flagKey)
    {
        return !string.IsNullOrEmpty(flagKey) && activeFlags.Contains(flagKey);
    }

    /// <summary>
    /// 检查是否满足一组条件
    /// </summary>
    public bool CheckConditions(List<string> requiredFlags)
    {
        if (requiredFlags == null || requiredFlags.Count == 0) return true;
        foreach (var req in requiredFlags)
        {
            if (!HasFlag(req)) return false;
        }
        return true;
    }
}
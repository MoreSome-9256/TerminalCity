using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Level3Context 用于传递全局公用组件给 SequenceStep 使用
/// Step 内如果需要特定 UI（如 TerminalView、ChoicePanel）则直接从 prefab 根节点获取
/// </summary>
public class Level3Context
{
    // 全局通用组件
    public DialogueManager DialogueManager { get; }
    public InputManager Input { get; }
    public Animator Animator { get; }

    // 可选标记数据，用于不同 Step 之间传递信息
    private Dictionary<string, object> flags = new Dictionary<string, object>();

    /// <summary>
    /// 构造函数
    /// </summary>
    public Level3Context(DialogueManager dialogueManager, InputManager inputManager, Animator animator)
    {
        DialogueManager = dialogueManager;
        Input = inputManager;
        Animator = animator;
    }

    /// <summary>
    /// 设置标记（可用于 Step 间共享状态，如玩家输入的命令）
    /// </summary>
    public void SetFlag(string key, object value)
    {
        flags[key] = value;
    }

    /// <summary>
    /// 获取标记
    /// </summary>
    public T GetFlag<T>(string key)
    {
        if (flags.ContainsKey(key))
            return (T)flags[key];
        return default;
    }

    /// <summary>
    /// 判断标记是否存在
    /// </summary>
    public bool HasFlag(string key)
    {
        return flags.ContainsKey(key);
    }
}

using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// 全局输入管理器，用于 SequenceStep 等待玩家按键或输入
/// Step 可以订阅事件，收到输入时触发
/// </summary>
public class InputManager : MonoBehaviour
{
    /// <summary>
    /// 键盘按下事件
    /// 参数 KeyCode 表示玩家按下的键
    /// </summary>
    public event Action<KeyCode> OnKeyPressed;

    /// <summary>
    /// Update 每帧检测键盘输入
    /// </summary>
    void Update()
    {
        // 这里简单示例只检测常用按键
        if (Input.anyKeyDown)
        {
            foreach (KeyCode kcode in Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(kcode))
                {
                    OnKeyPressed?.Invoke(kcode);
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 等待玩家按下指定按键（协程用）
    /// </summary>
    public IEnumerator WaitForKey(KeyCode key)
    {
        bool pressed = false;
        void Callback(KeyCode k)
        {
            if (k == key) pressed = true;
        }

        OnKeyPressed += Callback;
        yield return new WaitUntil(() => pressed);
        OnKeyPressed -= Callback;
    }

    /// <summary>
    /// 等待玩家按下任意键（协程用）
    /// </summary>
    public IEnumerator WaitForAnyKey()
    {
        bool pressed = false;
        void Callback(KeyCode k)
        {
            pressed = true;
        }

        OnKeyPressed += Callback;
        yield return new WaitUntil(() => pressed);
        OnKeyPressed -= Callback;
    }
}

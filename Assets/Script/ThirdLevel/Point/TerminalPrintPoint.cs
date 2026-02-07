using System.Collections;
using UnityEngine;
using TMPro;

public class TerminalPrintPoint : PlotPoint
{
    [Header("显示文本区域 (TMP)")]
    public TMP_Text textComp;  // 直接拖拽

    [Header("文本行")]
    public TerminalLine[] lines;

    [Header("打字机设置")]
    public float charInterval = 0.05f;  // 每个字显示时间
    public float postLineDelay = 0.3f;  // 每句话输出完的延迟

    [Header("打字音效")]
    public AudioSource typingAudioSource;
    public AudioClip typingClip;

    // 模拟输入的随机性
    public Vector2 userTypingIntervalRange = new Vector2(0.06f, 0.18f);
    public Vector2 typingSoundIntervalRange = new Vector2(0.08f, 0.25f);


    /// <summary>
    /// PlotPoint 接口调用
    /// </summary>
    public override IEnumerator Execute()
    {
        if (textComp == null)
        {
            Debug.LogError("TerminalPrintPoint: TMP_Text 未绑定！");
            yield break;
        }

        textComp.gameObject.SetActive(true);

        foreach (var line in lines)
        {
            yield return TypeLine(line);

            if (line.waitForKey && line.validKeys != null && line.validKeys.Length > 0)
            {
                yield return WaitForPlayerKey(line.validKeys);
            }

            // 自动换行
            textComp.text += "\n";
        }
    }
    private IEnumerator TypeLine(TerminalLine line)
    {
        if (line.clearBefore)
            textComp.text = "";

        // 如果是“模拟用户输入”，启动键盘音
        if (line.simulateUserTyping && typingAudioSource != null && typingClip != null)
        {
            typingAudioSource.clip = typingClip;
            typingAudioSource.Play();
        }

        foreach (char c in line.text)
        {
            textComp.text += c;

            float interval = line.simulateUserTyping
                ? Random.Range(charInterval * 0.7f, charInterval * 1.4f)
                : charInterval;

            yield return new WaitForSeconds(interval);
        }

        // 本行结束，停止键盘音
        if (line.simulateUserTyping && typingAudioSource != null)
        {
            typingAudioSource.Stop();
        }

        if (postLineDelay > 0f)
            yield return new WaitForSeconds(postLineDelay);
    }

    private IEnumerator TypeAuto(string text)
    {
        foreach (char c in text)
        {
            textComp.text += c;
            yield return new WaitForSeconds(charInterval);
        }
    }
    private IEnumerator WaitForPlayerKey(KeyCode[] keys)
    {
        bool pressed = false;
        while (!pressed)
        {
            foreach (var key in keys)
            {
                if (Input.GetKeyDown(key))
                {
                    pressed = true;
                    break;
                }
            }
            yield return null;
        }
    }
}
public enum TerminalInputMode
{
    AutoPrint,   // 系统自动输出（原本那种）
    UserTyping   // 模拟玩家输入
}

[System.Serializable]
public class TerminalLine
{
    [TextArea]
    public string text;

    public bool clearBefore;
    public bool waitForKey;
    public KeyCode[] validKeys;

    [Header("模拟用户输入")]
    public bool simulateUserTyping;
}

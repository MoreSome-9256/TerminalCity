using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MemoItemUI : MonoBehaviour
{
    [Header("UI 绑定")]
    [Tooltip("对应预制体里的 TaskDesc 文本")]
    [SerializeField] private TextMeshProUGUI descText;
    [Tooltip("整条可点击按钮，如果不填会自动获取自身 Button")]
    [SerializeField] private Button clickButton;
    [Tooltip("删除按钮（例如挂在子物体的 DeleteItem）")]
    [SerializeField] private Button deleteButton; // 👈 新增

    private MemoData currentData;
    private Action<MemoData> onClickCallback;
    private Action<MemoData> onDeleteCallback;   // 👈 新增

    private void Awake()
    {
        if (clickButton == null)
            clickButton = GetComponent<Button>();
    }

    /// <summary>
    /// 初始化条目展示（新增 onDelete 回调）
    /// </summary>
    public void Setup(MemoData data, Action<MemoData> onClick, Action<MemoData> onDelete)
    {
        currentData = data;
        onClickCallback = onClick;
        onDeleteCallback = onDelete;

        if (descText != null)
        {
            descText.text = FormatPreviewText(data.content);
        }

        if (clickButton != null)
        {
            clickButton.onClick.RemoveAllListeners();
            clickButton.onClick.AddListener(() => onClickCallback?.Invoke(currentData));
        }

        // 绑定删除按钮
        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(() =>
            {
                // 触发删除事件
                onDeleteCallback?.Invoke(currentData);
            });
        }
    }

    private string FormatPreviewText(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            return "新备忘录";
        }

        string[] lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        string firstLine = lines.Length > 0 ? lines[0] : "";

        bool hasMultipleLines = content.Contains("\n") || content.Contains("\r");

        if (firstLine.Length > 16)
        {
            return firstLine.Substring(0, 16) + "...";
        }
        else if (hasMultipleLines)
        {
            return firstLine + "...";
        }

        return firstLine;
    }
}
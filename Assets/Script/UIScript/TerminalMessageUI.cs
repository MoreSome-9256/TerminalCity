using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[System.Serializable]
public class TerminalMessageData
{
    public string senderName;   // 发送者名字，如 "Aurora", "Locus"
    [TextArea] public string messageContent; // 完整文本内容
    public bool isRead;         // 是否已读
}

public class TerminalMessageUI : MonoBehaviour
{
    [Header("顶部状态栏引用")]
    [SerializeField] private Image mailIcon;                // 信封图标（切换贴图）
    [SerializeField] private TMP_Text summaryText;          // 概要文字（受颜色变化控制）[cite: 1]

    [Header("图标切图配置")]
    [SerializeField] private Sprite iconNoMessage;          // 无未读消息时的信封图标
    [SerializeField] private Sprite iconHasMessage;         // 有新消息时的信封图标[cite: 1]

    [Header("文本颜色风格配置")]
    [SerializeField] private Color noMessageTextColor = new Color32(140, 185, 215, 255); // 暂无消息时的浅蓝灰
    [SerializeField] private Color hasMessageTextColor = new Color32(255, 205, 100, 255); // 有新消息时的金黄色[cite: 1]

    [Header("下方预览列表")]
    [SerializeField] private GameObject listContainer;      // 列表父节点[cite: 1]
    [SerializeField] private MessageItemSlot[] previewSlots;// 固定配置 3 个显示槽位[cite: 1]
    [SerializeField] private int maxCharLimit = 16;         // 摘要最大字数限制

    [Header("测试用消息库 (后续可替换为游戏数据单例)")]
    [SerializeField] private List<TerminalMessageData> messageDatabase = new List<TerminalMessageData>();

    [System.Serializable]
    public class MessageItemSlot
    {
        public GameObject rootObject;
        public TMP_Text contentText; // 文本组件，负责发件人与预览正文[cite: 1]
    }

    private void Start()
    {
        RefreshMessageDisplay();
    }

    /// <summary>
    /// 全局调用：刷新消息通知面板
    /// </summary>
    public void RefreshMessageDisplay()
    {
        // 筛选未读消息
        List<TerminalMessageData> unreadList = new List<TerminalMessageData>();
        for (int i = 0; i < messageDatabase.Count; i++)
        {
            if (!messageDatabase[i].isRead)
            {
                unreadList.Add(messageDatabase[i]);
            }
        }

        int unreadCount = unreadList.Count;

        // 状态 1：无未读消息
        if (unreadCount == 0)
        {
            if (mailIcon != null && iconNoMessage != null)
            {
                mailIcon.sprite = iconNoMessage;
            }

            if (summaryText != null)
            {
                summaryText.text = "暂无未读消息";
                summaryText.color = noMessageTextColor;
            }

            if (listContainer != null)
            {
                listContainer.SetActive(false);
            }
        }
        // 状态 2：有未读消息
        else
        {
            if (mailIcon != null && iconHasMessage != null)
            {
                mailIcon.sprite = iconHasMessage;
            }

            if (summaryText != null)
            {
                summaryText.text = $"{unreadCount}条未读消息";
                summaryText.color = hasMessageTextColor;
            }

            if (listContainer != null)
            {
                listContainer.SetActive(true);
            }

            // 最多渲染 3 条[cite: 1]
            int displayCount = Mathf.Min(unreadCount, previewSlots.Length);

            for (int i = 0; i < previewSlots.Length; i++)
            {
                if (i < displayCount)
                {
                    previewSlots[i].rootObject.SetActive(true);

                    // 截取前 N 个字并加省略号
                    string rawContent = unreadList[i].messageContent;
                    string snippet = rawContent.Length > maxCharLimit
                        ? rawContent.Substring(0, maxCharLimit) + "..."
                        : rawContent;

                    // 组合发信人与内容（固定亮色风格显示）[cite: 1]
                    if (previewSlots[i].contentText != null)
                    {
                        previewSlots[i].contentText.text =
                            $"{unreadList[i].senderName}\n" +
                            $"{snippet}";
                    }
                }
                else
                {
                    previewSlots[i].rootObject.SetActive(false);
                }
            }
        }
    }

    #region 对外接口 / 测试方法

    public void AddNewMessage(string sender, string content)
    {
        TerminalMessageData newMsg = new TerminalMessageData
        {
            senderName = sender,
            messageContent = content,
            isRead = false
        };

        messageDatabase.Insert(0, newMsg);
        RefreshMessageDisplay();
    }

    public void MarkAllAsRead()
    {
        for (int i = 0; i < messageDatabase.Count; i++)
        {
            messageDatabase[i].isRead = true;
        }
        RefreshMessageDisplay();
    }

    [ContextMenu("Test Add Message (测试插入新消息)")]
    private void TestAdd()
    {
        AddNewMessage("Aurora", "关于Basal的那件事，你怎么想...");
    }

    [ContextMenu("Test Clear/Read All (测试全部已读)")]
    private void TestRead()
    {
        MarkAllAsRead();
    }

    #endregion
}
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TerminalHomeTaskSlot : MonoBehaviour
{
    [Header("UI 组件引用")]
    [SerializeField] private TMP_Text taskDescText;
    [SerializeField] private Button checkToggleBtn;
    [SerializeField] private GameObject markObj;       // 勾选标记

    [Header("文本截断设置")]
    [SerializeField] private int maxCharLimit = 14;    // 首页右侧尺寸小，超长字符数阈值

    private TaskData currentData;

    private void Awake()
    {
        if (checkToggleBtn != null)
        {
            checkToggleBtn.onClick.AddListener(OnTaskClicked);
        }
    }

    public void Setup(TaskData data)
    {
        currentData = data;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (currentData == null) return;

        // 1. 文本截断与省略号
        if (taskDescText != null)
        {
            string content = currentData.description;
            if (!string.IsNullOrEmpty(content) && content.Length > maxCharLimit)
            {
                taskDescText.text = content.Substring(0, maxCharLimit) + "...";
            }
            else
            {
                taskDescText.text = content;
            }
        }

        // 2. 勾选图标状态
        if (markObj != null)
        {
            markObj.SetActive(currentData.isCompleted);
        }
    }

    private void OnTaskClicked()
    {
        if (currentData == null || TaskManagerUI.Instance == null) return;

        // 在首页点击直接将该任务设为已完成
        TaskManagerUI.Instance.CompleteTask(currentData.taskID);
    }
}
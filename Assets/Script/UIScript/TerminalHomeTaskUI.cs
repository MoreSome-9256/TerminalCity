using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TerminalHomeTaskUI : MonoBehaviour
{
    [Header("滚动容器引用")]
    [SerializeField] private Transform contentContainer;      // 首页右侧 ScrollView 下的 Content 物体
    [SerializeField] private GameObject homeTaskItemPrefab;   // 首页专用尺寸的 TaskItem 预制体

    private void OnEnable()
    {
        TaskManagerUI.OnTaskListUpdated += RefreshTaskList;
        RefreshTaskList();
    }

    private void OnDisable()
    {
        TaskManagerUI.OnTaskListUpdated -= RefreshTaskList;
    }

    private void Start()
    {
        RefreshTaskList();
    }

    /// <summary>
    /// 刷新首页右侧未完成任务
    /// </summary>
    public void RefreshTaskList()
    {
        if (contentContainer == null || homeTaskItemPrefab == null) return;
        if (TaskManagerUI.Instance == null) return;

        // 只获取未完成的任务
        List<TaskData> activeTasks = TaskManagerUI.Instance.GetActiveTasks();

        // 1. 清理现有子物体
        for (int i = contentContainer.childCount - 1; i >= 0; i--)
        {
            Destroy(contentContainer.GetChild(i).gameObject);
        }

        // 2. 依次生成未完成任务项
        for (int i = 0; i < activeTasks.Count; i++)
        {
            GameObject obj = Instantiate(homeTaskItemPrefab, contentContainer);
            TerminalHomeTaskSlot slot = obj.GetComponent<TerminalHomeTaskSlot>();
            if (slot != null)
            {
                slot.Setup(activeTasks[i]);
            }
        }

        // 3. 强制重绘布局，确保 ScrollView Content 正确撑开
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer.GetComponent<RectTransform>());
    }
}
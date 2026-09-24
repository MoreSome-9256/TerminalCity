using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class TaskData
{
    public string taskID;
    public string description;
    public bool isCompleted;
    public bool isCustom;
    public long createTimestamp;
}

public class TaskManagerUI : MonoBehaviour
{
    public static TaskManagerUI Instance;

    [Header("UI 容器与预制体")]
    [SerializeField] private Transform contentContainer;  // Content 物体
    [SerializeField] private GameObject taskItemPrefab;   // TaskItem Prefab

    [Header("右上角新增按钮")]
    [SerializeField] private Button addNewTaskButton;

    [Header("运行时任务库")]
    [SerializeField] private List<TaskData> taskList = new List<TaskData>();

    private TaskItemSlot activeEditingSlot; // 当前正在原地输入的 Slot

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        if (addNewTaskButton != null)
        {
            addNewTaskButton.onClick.AddListener(OnAddNewButtonClicked);
        }
    }

    private void Start()
    {
        RebuildTaskListUI();
    }

    /// <summary>
    /// 点击右上角“新增”按钮
    /// </summary>
    private void OnAddNewButtonClicked()
    {
        // 如果已经有一个正在编辑的空白项，避免重复生成
        if (activeEditingSlot != null) return;

        // 在最上方生成一个临时编辑条目
        GameObject obj = Instantiate(taskItemPrefab, contentContainer);
        obj.transform.SetAsFirstSibling(); // 放到 Content 的第一位

        activeEditingSlot = obj.GetComponent<TaskItemSlot>();
        if (activeEditingSlot != null)
        {
            activeEditingSlot.EnterEditMode(OnCustomTaskInputFinished);
        }
    }

    /// <summary>
    /// 玩家原地输入完毕回调
    /// </summary>
    private void OnCustomTaskInputFinished(TaskItemSlot slot, string resultText)
    {
        // 销毁临时的编辑项
        if (slot != null)
        {
            Destroy(slot.gameObject);
        }
        activeEditingSlot = null;

        // 如果用户没输入内容直接回车/点开别处，则视为取消新增
        if (string.IsNullOrEmpty(resultText)) return;

        // 生成新任务并加入数据列表
        string customID = "Custom_" + System.Guid.NewGuid().ToString().Substring(0, 6);
        AddTask(customID, resultText, false, isCustom: true);
    }

    public void RebuildTaskListUI()
    {
        if (contentContainer == null || taskItemPrefab == null) return;

        // 排序：未完成在前，已完成在后；时间戳升序
        taskList.Sort((a, b) =>
        {
            if (a.isCompleted != b.isCompleted)
            {
                return a.isCompleted ? 1 : -1;
            }
            return a.createTimestamp.CompareTo(b.createTimestamp);
        });

        // 清理所有旧条目（保留当前正在编辑中的项，如果有的话）
        for (int i = contentContainer.childCount - 1; i >= 0; i--)
        {
            Transform child = contentContainer.GetChild(i);
            if (activeEditingSlot != null && child == activeEditingSlot.transform) continue;
            Destroy(child.gameObject);
        }

        for (int i = 0; i < taskList.Count; i++)
        {
            GameObject obj = Instantiate(taskItemPrefab, contentContainer);
            TaskItemSlot slot = obj.GetComponent<TaskItemSlot>();
            if (slot != null)
            {
                slot.Setup(taskList[i], OnTaskStatusChanged);
            }
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(contentContainer.GetComponent<RectTransform>());
    }

    private void OnTaskStatusChanged(string taskID)
    {
        RebuildTaskListUI();
    }

    #region 外部调用接口

    public void AddTask(string id, string desc, bool completed = false, bool isCustom = false)
    {
        if (taskList.Exists(t => t.taskID == id)) return;

        TaskData newTask = new TaskData
        {
            taskID = id,
            description = desc,
            isCompleted = completed,
            isCustom = isCustom,
            createTimestamp = System.DateTime.UtcNow.Ticks
        };

        taskList.Add(newTask);
        RebuildTaskListUI();
    }

    public void CompleteTask(string id)
    {
        TaskData task = taskList.Find(t => t.taskID == id);
        if (task != null && !task.isCompleted)
        {
            task.isCompleted = true;
            RebuildTaskListUI();
        }
    }

    public List<TaskData> GetActiveTasks()
    {
        return taskList.FindAll(t => !t.isCompleted);
    }

    #endregion
}
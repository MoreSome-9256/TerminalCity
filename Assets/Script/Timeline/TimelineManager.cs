using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TimelineEventData
{
    [Tooltip("全局唯一事件ID，如 EV_001")]
    public string id;

    [Tooltip("界面显示的日期文本，如 恒元216年4月13日")]
    public string dateDisplay;

    [Tooltip("事件具体内容描述")]
    [TextArea(2, 4)]
    public string description;

    [Tooltip("绝对时间标量（用于计算物理间距与排序，如 216.28f）")]
    public float exactYear;
}

[Serializable]
public class TimelineSaveData
{
    public List<string> unlockedIds = new List<string>();
    public List<string> unreadIds = new List<string>();
}

public class TimelineManager : MonoBehaviour
{
    public static TimelineManager Instance { get; private set; }

    [Header("静态数据资产")]
    [Tooltip("拖入 TimelineDatabase.asset")]
    [SerializeField] private TimelineDatabase database;

    [Header("主视图（可选）")]
    [Tooltip("主界面 Scroll View 上的 DynamicTimelineView")]
    [SerializeField] private DynamicTimelineView mainTimelineView;

    // 所有当前活跃的视图列表（主视图、合成页小窗等）
    private readonly List<DynamicTimelineView> registeredViews = new List<DynamicTimelineView>();

    // 状态集合
    private HashSet<string> unlockedEventIds = new HashSet<string>();
    private HashSet<string> unreadEventIds = new HashSet<string>();

    private const string SAVE_KEY = "TC_Timeline_SaveData";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 如果 Inspector 没拖，自动尝试从 Resources 加载
        if (database == null)
        {
            database = Resources.Load<TimelineDatabase>("TimelineDatabase");
        }

        LoadData();

        // 默认将主视图加入管理
        if (mainTimelineView != null)
        {
            RegisterView(mainTimelineView);
        }
    }

    private void OnEnable()
    {
        // 激活时刷新所有活跃视图
        RefreshAllViews();
    }

    private void OnDisable()
    {
        // 离开面板时标记所有红点为已读
        MarkAllAsRead();
    }

    #region 视图注册机制（供合成页小窗等动态挂载）

    /// <summary>
    /// 注册视图并立即刷新该视图
    /// </summary>
    public void RegisterView(DynamicTimelineView view)
    {
        if (view != null && !registeredViews.Contains(view))
        {
            registeredViews.Add(view);
            RefreshSingleView(view);
        }
    }

    /// <summary>
    /// 注销视图
    /// </summary>
    public void UnregisterView(DynamicTimelineView view)
    {
        if (view != null && registeredViews.Contains(view))
        {
            registeredViews.Remove(view);
        }
    }

    #endregion

    #region 核心业务与刷新接口

    /// <summary>
    /// ✨ 外部核心调用：解锁一个新事件并广播刷新所有窗口
    /// </summary>
    public void UnlockEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId)) return;

        if (!unlockedEventIds.Contains(eventId))
        {
            unlockedEventIds.Add(eventId);
            unreadEventIds.Add(eventId);
            SaveData();

            // 广播刷新大屏和小屏
            RefreshAllViews();
        }
    }

    /// <summary>
    /// 保持旧方法名兼容，功能等同于刷新所有视图
    /// </summary>
    public void RefreshView()
    {
        RefreshAllViews();
    }

    /// <summary>
    /// 刷新所有已注册的视图
    /// </summary>
    public void RefreshAllViews()
    {
        for (int i = registeredViews.Count - 1; i >= 0; i--)
        {
            if (registeredViews[i] != null)
            {
                RefreshSingleView(registeredViews[i]);
            }
            else
            {
                registeredViews.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 刷新单个视图渲染
    /// </summary>
    public void RefreshSingleView(DynamicTimelineView view)
    {
        if (view == null || database == null) return;

        var currentUnlocked = database.allEvents
            .Where(e => unlockedEventIds.Contains(e.id))
            .ToList();

        view.BuildTimeline(currentUnlocked, unreadEventIds);
    }

    public void MarkAllAsRead()
    {
        if (unreadEventIds.Count > 0)
        {
            unreadEventIds.Clear();
            SaveData();
        }
    }

    #endregion

    #region 存档与读档

    private void SaveData()
    {
        TimelineSaveData data = new TimelineSaveData
        {
            unlockedIds = new List<string>(unlockedEventIds),
            unreadIds = new List<string>(unreadEventIds)
        };
        PlayerPrefs.SetString(SAVE_KEY, JsonUtility.ToJson(data));
        PlayerPrefs.Save();
    }

    private void LoadData()
    {
        if (PlayerPrefs.HasKey(SAVE_KEY))
        {
            TimelineSaveData data = JsonUtility.FromJson<TimelineSaveData>(PlayerPrefs.GetString(SAVE_KEY));
            if (data != null)
            {
                unlockedEventIds = new HashSet<string>(data.unlockedIds ?? new List<string>());
                unreadEventIds = new HashSet<string>(data.unreadIds ?? new List<string>());
            }
        }
    }

    #endregion

    #region 编辑器测试

    [ContextMenu("🗑️ 清空存档数据")]
    public void ClearTestData()
    {
        unlockedEventIds.Clear();
        unreadEventIds.Clear();
        PlayerPrefs.DeleteKey(SAVE_KEY);
        PlayerPrefs.Save();
        RefreshAllViews();
        Debug.Log("时间轴存档已重置清空！");
    }

    #endregion

    [ContextMenu("🧪 预览当前 Database 中前 3 条事件")]
    public void PreviewDatabaseFirstFew()
    {
        if (database == null || database.allEvents == null || database.allEvents.Count == 0)
        {
            Debug.LogWarning("请先在 TimelineDatabase.asset 里面手动添加至少 1 条事件！");
            return;
        }

        int count = Mathf.Min(3, database.allEvents.Count);
        var previewList = database.allEvents.Take(count).ToList();

        unlockedEventIds.Clear();
        unreadEventIds.Clear();
        foreach (var ev in previewList)
        {
            unlockedEventIds.Add(ev.id);
        }
        unreadEventIds.Add(previewList[previewList.Count - 1].id);

        // 🌟 关键优化：如果是编辑器模式，直接查找场景里所有的 DynamicTimelineView 一起画！
        DynamicTimelineView[] allViewsInScene = FindObjectsOfType<DynamicTimelineView>(true);
        foreach (var view in allViewsInScene)
        {
            if (!registeredViews.Contains(view))
            {
                registeredViews.Add(view);
            }
        }

        RefreshAllViews();
        Debug.Log($"成功在所有时间轴（主轴和小轴）上预览了 Database 中的前 {count} 条数据！");
    }
}
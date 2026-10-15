using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class TimelineEventData
{
    [Tooltip("全局唯一事件ID，如 EV_IRIS_BUILD_TC")]
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

    [Header("UI 视图组件")]
    [SerializeField] private DynamicTimelineView timelineView;

    [Header("静态数据引用")]
    [Tooltip("拖入刚才创建的 TimelineDatabase.asset")]
    [SerializeField] private TimelineDatabase database;

    // 运行时状态管理（存档依然管玩家解锁了哪些）
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

        // 可选：如果忘拖了，自动从 Resources 加载
        if (database == null)
        {
            database = Resources.Load<TimelineDatabase>("TimelineDatabase");
        }

        LoadData();
    }

    private void OnEnable()
    {
        // 打开面板时自动刷新 UI 渲染
        RefreshView();
    }

    /// <summary>
    /// ✨ 外部核心调用：解锁一个新事件
    /// 无论在哪捡到线索/合成成功，直接 TimelineManager.Instance.UnlockEvent("EV_XXX");
    /// </summary>
    public void UnlockEvent(string eventId)
    {
        if (string.IsNullOrEmpty(eventId)) return;

        if (!unlockedEventIds.Contains(eventId))
        {
            unlockedEventIds.Add(eventId);
            unreadEventIds.Add(eventId); // 标记为未读（显示 NewPrompt）
            SaveData();

            // 若当前时间轴界面正开着，立刻重画
            if (gameObject.activeInHierarchy)
            {
                RefreshView();
            }
        }
    }

    public void RefreshView()
    {
        if (timelineView == null || database == null) return;

        // 统一从 ScriptableObject 数据库中读取
        var currentUnlocked = database.allEvents
            .Where(e => unlockedEventIds.Contains(e.id))
            .ToList();

        timelineView.BuildTimeline(currentUnlocked, unreadEventIds);
    }

    public void MarkAllAsRead()
    {
        if (unreadEventIds.Count > 0)
        {
            unreadEventIds.Clear();
            SaveData();
        }
    }

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

    
}
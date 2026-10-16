using UnityEngine;

[RequireComponent(typeof(DynamicTimelineView))]
public class TimelineViewAutoBind : MonoBehaviour
{
    private DynamicTimelineView timelineView;

    private void Awake()
    {
        timelineView = GetComponent<DynamicTimelineView>();
    }

    private void OnEnable()
    {
        // 窗口显示/激活时，自动向单例注册并拉取最新时间轴刷新
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.RegisterView(timelineView);
        }
    }

    private void OnDisable()
    {
        // 窗口隐藏时，自动注销
        if (TimelineManager.Instance != null)
        {
            TimelineManager.Instance.UnregisterView(timelineView);
        }
    }
}
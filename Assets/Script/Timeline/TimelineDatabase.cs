using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "TimelineDatabase", menuName = "TerminalCity/时间轴数据库 (TimelineDatabase)")]
public class TimelineDatabase : ScriptableObject
{
    [Header("全事件总库")]
    public List<TimelineEventData> allEvents = new List<TimelineEventData>();

    /// <summary>
    /// 根据 ID 快速检索事件
    /// </summary>
    public TimelineEventData GetEventById(string id)
    {
        return allEvents.Find(e => e.id == id);
    }
}
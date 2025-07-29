using UnityEngine.Events;
using UnityEngine;

public class EventGroup : MonoBehaviour
{
    public int totalEvents = 3; // 你有多少个并行事件
    private int completedCount = 0;

    public UnityEvent onAllEventsCompleted; // 所有事件完成后触发的事件列表

    // 每个等价事件完成时调用这个方法
    public void NotifyEventCompleted()
    {
        completedCount++;
        if (completedCount >= totalEvents)
        {
            Debug.Log("All events completed. Triggering next events.");
            onAllEventsCompleted?.Invoke();
        }
    }
}

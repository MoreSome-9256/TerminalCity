using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public abstract class PlotPoint : MonoBehaviour
{
    [Header("PlotPoint Events")]
    public UnityEvent onPointEnter;
    public UnityEvent onPointExit;

    /// <summary>
    /// 执行这个剧情节点
    /// </summary>
    public IEnumerator Play()
    {
        // 进入点
        onPointEnter?.Invoke();

        // 核心逻辑
        yield return Execute();

        // 离开点
        onPointExit?.Invoke();
    }

    /// <summary>
    /// 子类只关心“自己要干嘛”
    /// </summary>
    public virtual IEnumerator Execute()
    {
        yield break;
    }
}
